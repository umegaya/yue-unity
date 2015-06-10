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
		//login progress state for remote mode
		public enum State {
			WAIT_FIELD = -7,
			LOCAL_ENTER = -6,
			NOT_ACTIVE = -5,
			ON_REQUEST = -4,
			PUT_USER_DATA = -3,
			REQUEST_OTP = -2,
			REQUEST_ENTER = -1,
			LOGIN = 0
		};
		//login state
		public State _login_state = State.NOT_ACTIVE;
		
		//working variables
		GameField _gf = null; //game field which this session establish to
		Actor _actor = null;
		string _otp = null;
		object _user_data = null;
		
		//configurable variables 
		public string url = "";
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
			if (!gf.Ready) {
				_login_state = State.WAIT_FIELD;
			}
			else if (gf.LocalMode) {
				_login_state = State.LOCAL_ENTER;
			}
			else {
				if (string.IsNullOrEmpty(user_id)) {
					throw new System.Exception("user_id should specified for remote mode");
				}
				NetworkManager.instance.Register("/player", this);
				_login_state = gf.CreateRemoteField ? State.PUT_USER_DATA : State.REQUEST_OTP;
			}
		}
	
		//override behavior
		protected void Update () {
			switch (_login_state) {
			case State.WAIT_FIELD:
				if (_gf.Ready) {
					Enter(_gf, _user_data);
				}
				return;
			case State.LOCAL_ENTER:
				Debug.Log("LOCAL_ENTER");
				_gf.Call("Enter", delegate (object []rvs, object err) {
					if (rvs != null) {
						user_id = ((double)rvs[0]).ToString();
					}
				}, DUMMY_OTP, this, Json.Serialize(_user_data));
				_login_state = State.LOGIN;
				return;
			case State.NOT_ACTIVE:
			case State.ON_REQUEST:
			case State.LOGIN:
				return;
			case State.PUT_USER_DATA:
				Debug.Log("PUT_USER_DATA");
				_gf.ActorCall(_gf.LoginActor, (resp) => {
					_login_state = State.REQUEST_OTP;
				}, "put_user_data", user_id, _user_data);
				break;
			case State.REQUEST_OTP:
				Debug.Log("REQUEST_OTP");
				if (string.IsNullOrEmpty(_gf.FieldId)) {
					return; //wait for field creation completion.
				}
				_gf.ActorCall(_gf.LoginActor, (resp) => {
					_otp = ((string)resp.Args(0));
					_actor = NetworkManager.instance.NewActor(url + _gf.FieldId);
					Debug.Log("generate otp:"+_otp+"@"+_gf.FieldId);
					_login_state = State.REQUEST_ENTER;
				}, "otp", _gf.FieldId, user_id);
				break;
			case State.REQUEST_ENTER:
				Debug.Log("REQUEST_ENTER");
				_gf.ActorCall(_actor, (resp) => {
					_login_state = State.LOGIN;
				}, "login", _otp);
				break;
			}
			_login_state = State.ON_REQUEST;	
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
