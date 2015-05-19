using UnityEngine;
using MiniJSON;
using NLua;

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
	public int Id = 1;
	
	void Start () {
	}
	
	//battle event receiver.
	//LuaTable almost acts like Dictionary. 
	//you can get property by data[hoge], and iterate it by foreach (KeyValuePair<object, object> e in data) {}. 
	public void Play(string type, LuaTable data) {
		Debug.Log("Play:"+type+"|"+Json.Serialize(GameField.ToDictionary(data)));
	}
}
