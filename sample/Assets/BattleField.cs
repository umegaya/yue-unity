using UnityEngine;
using System.Collections.Generic;

public class BattleField : MonoBehaviour {

	GameField gf;
	Renderer renderer;
	
	public bool local = true;

	void Start () {
		if (local) {
			gf = new GameField();
			renderer = new Renderer();
			var user_data = new List<string>() { "foo", "bar", "baz" };
			var field_data = new List<string>() { "hoge", "fuga", "fugu" };
			gf.InitLocal(user_data, field_data, renderer);
		}
		else {
			//TODO : initialize remote game field
		}
	}

	void Update () {
		gf.Update(Time.deltaTime);
	}
}
