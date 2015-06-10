using UnityEngine;
using Yue;

namespace YueUnityTest {
	public class NetworkObject : MonoBehaviour {
	
		Actor _actor;
		float _last_update;
		double _latency = 0;
	
		public string HostName = "192.168.59.103";
		public int HostPort = 8080;
		public string LoginActorId = "srv";
	
		void Start () {
			//create actor to call server RPC (see Update() for detail usage)
			_actor = NetworkManager.instance.NewActor("tcp://"+HostName+":"+HostPort+"/"+LoginActorId, ConnectionWatcher);
			//register this game object so that method can call from server
			NetworkManager.instance.Register("/sys", this);
			_last_update = 0;
		}
		
		void OnGUI () {
	        // Make a background box
	        GUI.Box(new Rect(10,10,200,110), "Menu");
	    
	        // Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
	        if(GUI.Button(new Rect(20,40,180,20), "Close Connection")) {
				_actor.Call((resp, err) => {
					if (resp != null) {
					}
					else if (err is ServerException) {
						Debug.Log((err as ServerException).Message);
					}
					else {
						Debug.Log("other exception:" + err);
					}
					return null;
				}, "close_me");
			}
	    
	        GUI.Label(new Rect(20,70,180,20), "Latency:" + System.Math.Ceiling(1000 * _latency) + "ms");
	        GUI.Label(new Rect(20,90,180,20), "ScpLatency:" + (System.Math.Ceiling(100000 * GameField.update_latency) / 100) + "ms");
	    }
	
	
		//this function called from server RPC directly. 
		//*public* is important to make it accessible from server
		public string GetUnityVersion(bool test) {
			if (test) {
				return "Unity Test";
			}
			else {
				return "Unity 5.0";
			}
		}
		
		//connection watcher which do something when connection established/closed
		public void ConnectionWatcher(string url, bool opened) {
			if (opened) {
				Debug.Log("connection open:" + url);
			}
			else {
				Debug.Log("connection close:" + url);			
			}
		}
		
		// Update is called once per frame
		void Update () {
			var now = Time.time;
			if ((_last_update + 1) < now) {
				_last_update = now;
				// callback, method_name, arg1, arg2, ...
				_actor.Call((resp, err) => {
					if (resp != null) {
						var ts = ((double)resp.Args(0));
						_latency = (Time.time - ts);
						//Debug.Log("latency:"+_latency);
					}
					else if (err is ServerException) {
						Debug.Log((err as ServerException).Message);
					}
					else {
						Debug.Log("other exception:" + err);
					}
					return null;
				}, "echo", now);
			}
		}
	}
}
