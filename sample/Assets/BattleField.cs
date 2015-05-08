using UnityEngine;
using System.Collections.Generic;

public class BattleField : MonoBehaviour {

	GameField gf;
	Renderer renderer;
	
	public bool local = true;
	public bool debug = false;
	public string url = "";

	void Start () {
		gf = new GameField(this.debug);
		if (local) {
			renderer = new Renderer();
			var user_data = new List<string>() { "foo", "bar", "baz" };
			var field_data = new List<string>() { "hoge", "fuga", "fugu" };
			gf.InitLocal(field_data, renderer);
			gf.Enter(user_data);
		}
		else {
			//TODO : initialize remote game field
			gf.InitRemote(url, renderer);
			gf.Enter();
		}
	}

	void Update () {
		gf.Update(Time.deltaTime);
	}
}
