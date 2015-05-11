using UnityEngine;
using System.Collections;
using Yue;

public class NetworkManager : MonoBehaviour {
	//singleton
	private static NetworkManager _instance;
 	public static NetworkManager instance
    {
        get
        {
            if(_instance == null)
                _instance = GameObject.FindObjectOfType<NetworkManager>();
            return _instance;
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		TransportManager.Poll();
	}

	public Actor NewActor(string url, Yue.ConnectionStateDelegate d = null) {
		return new Actor(url, d);
	}

	public void Register(string name, MonoBehaviour g) {
		TransportManager.Register(name, g);
	}
}
