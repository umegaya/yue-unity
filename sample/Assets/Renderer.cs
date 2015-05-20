using UnityEngine;
using MiniJSON;
using NLua;
using System.Collections.Generic;

public class Renderer : MonoBehaviour {
	//singleton
	private static Renderer _instance;
 	public static Renderer instance
    {
        get
        {
            if(_instance == null)
                _instance = GameObject.FindObjectOfType<Renderer>();
            return _instance;
        }
    }
	public Dictionary<object, object> SceneData { get; set; }
	
	void Start () {
	}
	
	void OnGUI () {
        // Make a background box
        GUI.Box(new Rect(10,120,460,520), "Users");
    
    }
	
	//battle event receiver.
	//LuaTable almost acts like Dictionary. 
	//you can get property by data[hoge], and iterate it by foreach (KeyValuePair<object, object> e in data) {}. 
	public void Play(string type, LuaTable data) {
		Debug.Log("Play:"+type+"|"+Json.Serialize(GameField.ToDictionary(data)));
		if (type == "init") {
			this.SceneData = GameField.ToDictionary(data);	
		}
	}
}
