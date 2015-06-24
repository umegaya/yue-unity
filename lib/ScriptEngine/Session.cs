using UnityEngine;
using Yue;
using NLua;
using MiniJSON;
using System.Collections.Generic;

namespace Yue {
	public class Session : MonoBehaviour {
		//constants
		//dummy otp for local mode
		const string DUMMY_OTP = "otp";
		const float MATCHING_WAIT_COOLDOWN_SEC = 3.0f;
		//login progress state for remote mode
		public enum State {
			WAIT_FIELD = -3,
			NOT_ACTIVE = -2,
			ON_REQUEST = -1,
			LOGIN = 0,

			LOCAL_ENTER = -10,

			REMOTE_PUT_USER_DATA = -20,
			REMOTE_REQUEST_OTP = -21,
			REMOTE_REQUEST_ENTER = -22,

			MATCHING_PUT_USER_DATA = -30,
			MATCHING_REQUEST_ENTER_QUEUE = -31,
			MATCHING_WAIT = -32,
			MATCHING_WAIT_COOLDOWN = -33,
			MATCHING_REQUEST_ENTER = -34,
		};
		//login state
		public State _login_state = State.NOT_ACTIVE;
		
		//working variables
		GameField _gf = null; //game field which this session establish to
		Actor _actor = null;
		string _otp = null;
		int _matching_wait_user = 0;
		float _last_matching_state = 0.0f;
		object _user_data = null;
		
		//configurable variables 
		public string user_id = "";
	
		void Start () {
		}
		
		//method
		public void SendCommand(GameField.ScriptResultDelegate d, object command) {
			if (_gf.LocalMode) {
				_gf.Call("SendCommand", d, System.Convert.ToInt32(user_id), Json.Serialize(command));
			}
			else {
				// TODO : call actor
				_actor.Call((resp, err) => {
					if (resp != null) {
						d(resp.ArgsList, null);
					}
					else {
						d(null, (err is System.Exception) ? err : new System.Exception(err.ToString()));
					}
					return null;
				}, "SendCommand", user_id, Json.Serialize(command));
			}
		}
	
		public void Enter(GameField gf, object user_data) {
			_user_data = user_data;
			_gf = gf;
			if (!_gf.Ready) {
				_login_state = State.WAIT_FIELD;
			}
			else { 
				switch (gf.LoginMethod) {
				case GameField.Method.LOCAL:
					_login_state = State.LOCAL_ENTER;
					break;
				case GameField.Method.REMOTE:
					if (string.IsNullOrEmpty(user_id)) {
						throw new System.Exception("user_id should specified for remote mode");
					}
					NetworkManager.instance.Register("/"+user_id, this);
					_login_state = _gf.UseDummyWebsv ? State.REMOTE_PUT_USER_DATA : State.REMOTE_REQUEST_OTP;
					break;
				case GameField.Method.MATCHING:
					if (string.IsNullOrEmpty(user_id)) {
						throw new System.Exception("user_id should specified for matching mode");
					}
					NetworkManager.instance.Register("/"+user_id, this);
					_login_state = _gf.UseDummyWebsv ? State.MATCHING_PUT_USER_DATA : State.MATCHING_REQUEST_ENTER_QUEUE;
					break;
				}
			}
		}
	
		//override behavior
		protected void LocalLogin() {
			switch (_login_state) {
			case State.LOCAL_ENTER:
				Debug.Log("LOCAL_ENTER");
				_gf.Call("Enter", delegate (object []rvs, object err) {
					if (rvs != null) {
						user_id = ((double)rvs[0]).ToString();
					}
				}, DUMMY_OTP, this, Json.Serialize(_user_data));
				_login_state = State.LOGIN;
				return;
			}
			_login_state = State.ON_REQUEST;	
		}
		protected void RemoteLogin() {
			switch (_login_state) {
			case State.REMOTE_PUT_USER_DATA:
				Debug.Log("REMOTE_PUT_USER_DATA");
				_gf.ActorCall(_gf.LoginActor, (resp) => {
					_login_state = State.REMOTE_REQUEST_OTP;
				}, "put_user_data", user_id, _user_data);
				break;
			case State.REMOTE_REQUEST_OTP:
				Debug.Log("REMOTE_REQUEST_OTP");
				if (string.IsNullOrEmpty(_gf.FieldId)) {
					return; //wait for field creation completion.
				}
				_gf.ActorCall(_gf.LoginActor, (resp) => {
					_otp = ((string)resp.Args(0));
					_actor = NetworkManager.instance.NewActor(_gf.FieldId);
					Debug.Log("generate otp:"+_otp+"@"+_gf.FieldId);
					_login_state = State.REMOTE_REQUEST_ENTER;
				}, "otp", _gf.FieldId, user_id);
				break;
			case State.REMOTE_REQUEST_ENTER:
				Debug.Log("REMOTE_REQUEST_ENTER");
				_gf.ActorCall(_actor, (resp) => {
					_login_state = State.LOGIN;
				}, "login", _otp);
				break;
			}
			_login_state = State.ON_REQUEST;	
		}
		public int MatchingWaitUser {
			get {
				return ((_login_state == State.MATCHING_WAIT || _login_state == State.MATCHING_WAIT_COOLDOWN) ? _matching_wait_user : -1);
			}
		}
		protected void MatchingLogin() {
			switch (_login_state) {
			case State.MATCHING_PUT_USER_DATA:
				Debug.Log("MATCHING_PUT_USER_DATA");
				_gf.ActorCall(_gf.LoginActor, (resp) => {
					_login_state = State.MATCHING_REQUEST_ENTER_QUEUE;
				}, "put_user_data", user_id, _user_data);
				break;
			case State.MATCHING_REQUEST_ENTER_QUEUE:
				Debug.Log("MATCHING_ENTER_QUEUE");
				_gf.ActorCall(_gf.LoginActor, (resp) => {
					var queue_url = ((string)resp.Args(0));
					Debug.Log("enter queue:"+queue_url);
					_actor = NetworkManager.instance.NewActor(queue_url);
					_login_state = State.MATCHING_WAIT;
				}, "queue", _gf.QueueName, _gf.QueueSettings, user_id);
				break;
			case State.MATCHING_WAIT:
				Debug.Log("MATCHING_WAIT");
				_gf.ActorCall(_actor, (resp) => {
					Debug.Log("MATCHING_WAIT:" + Json.Serialize(resp.Data));
					if (resp.Args(0) is double) {
						_matching_wait_user = (int)(double)resp.Args(0);
						_last_matching_state = Time.time;
						_login_state = State.MATCHING_WAIT_COOLDOWN;
					}
					else if (resp.Args(0) is string) {
						_otp = (string)resp.Args(0);
						_gf.FieldId = (string)resp.Args(1);
						Debug.Log("matching finished:" + _otp + "@" + _gf.FieldId);
						//refresh actor 
						_actor = NetworkManager.instance.NewActor(_gf.FieldId);
						_login_state = State.MATCHING_REQUEST_ENTER;
					}
				}, "stat", user_id);
				break;
			case State.MATCHING_WAIT_COOLDOWN:
				if (Time.time - _last_matching_state > MATCHING_WAIT_COOLDOWN_SEC) {
					_login_state = State.MATCHING_WAIT;
				}
				return;
			case State.MATCHING_REQUEST_ENTER:
				Debug.Log("MATCHING_REQUEST_ENTER");
				_gf.ActorCall(_actor, (resp) => {
					_login_state = State.LOGIN;
				}, "login", _otp);
				break;
			}
			_login_state = State.ON_REQUEST;				
		}
		protected void Update () {
			switch (_login_state) {
			case State.WAIT_FIELD:
				if (_gf.Ready) {
					Enter(_gf, _user_data);
				}
				return; 
			case State.NOT_ACTIVE:
			case State.ON_REQUEST:
			case State.LOGIN:
				return;
			}
			switch (_gf.LoginMethod) {
				case GameField.Method.LOCAL:
					LocalLogin();
					break;
				case GameField.Method.REMOTE:
					RemoteLogin();
					break;
				case GameField.Method.MATCHING:
					MatchingLogin();
					break;
			}
		}
		
		//game field event receiver.
		//local receiver. it converts LuaTable to Dictionary for compatibility with server side payload
		public void PlayLocal(string type, LuaTable data) {
			try {
				var dict = LuaUtil.ToDictionary(data);
				Play(type, dict);
			}
			catch (System.Exception e) {
				Debug.Log("PlayLocal error:" + e);
			}
		}
		//this will be overwritten by child.
		public virtual void Play(string type, Dictionary<object, object> dict) {
			Debug.LogError("Play: don't wanna handle it?:"+type+"|"+Json.Serialize(dict));
		}
		public virtual void Play(string type, object[] list) {
			Debug.LogError("Play: don't wanna handle it?:"+type+"|"+Json.Serialize(list));
		}
	}
}
