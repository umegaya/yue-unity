using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Yue;

public class NetworkObject : MonoBehaviour {

	Actor _actor;
	float _last_update;

	public string HostName = "192.168.59.103";
	public int HostPort = 8080;
	public string LoginActorId = "srv";

	void Start () {
		//create actor to call server RPC
		_actor = NetworkManager.instance.NewActor("tcp://"+HostName+":"+HostPort+"/"+LoginActorId);
		//register this game object so that method can call from server
		NetworkManager.instance.Register("/sys", this);
		_last_update = 0;
	}

	public string GetUnityVersion(bool test) {
		if (test) {
			return "Unity Test";
		}
		else {
			return "Unity 5.0";
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
					Debug.Log("resp:"+resp.Args(0));
				}
				else if (err is ServerException) {
					Debug.Log((err as ServerException).Message);
				}
				return null;
			}, "echo", "hoge");
		}
	}
}
