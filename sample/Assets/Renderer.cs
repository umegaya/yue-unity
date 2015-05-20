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
	public string Winner { get; set; }
	public Dictionary<object, object> Enemy() {
		return (Dictionary<object, object>)(
			(Dictionary<object, object>) (
				((Dictionary<object, object>)this.SceneData["EnemySide"])["normal_battle_enemy"]
			)
		);
	}
	public Dictionary<object, object> Hero() {
		return (Dictionary<object, object>)(
			(Dictionary<object, object>) (
				((Dictionary<object, object>)this.SceneData["UserSide"])["normal_battle_user"]
			)
		);
	}
	public Dictionary<object, object> FindObjectById(object idobj) {
		double id = (double)idobj;
		foreach (var e in Enemy()) {
			var d = (Dictionary<object, object>)(e.Value);
			if (((double)d["TargetId"]) == id) {
				return d;
			}
		}
		foreach (var e in Hero()) {
			var d = (Dictionary<object, object>)(e.Value);
			if (((double)d["TargetId"]) == id) {
				return d;
			}
		}
		return null;
	}
	public void SetStateData(Dictionary<object, object> d) {
		Dictionary<object, object> obj = FindObjectById(d["TargetId"]);
		if (obj != null) {
			obj["MaxHp"] = d["MaxHp"];	
			obj["Hp"] = d["Hp"];	
			obj["MaxWp"] = d["MaxWp"];	
			obj["Wp"] = d["Wp"];	
		}
	}
	const int DAMAGE = 1;
	const int HEAL = 2;
	const int EFFECT = 3;
	public void SetActionData(Dictionary<object, object> d) {
		Dictionary<object, object> obj = FindObjectById(d["TargetId"]);
		if (obj != null) {
			var a = (Dictionary<object, object>)d["Action"];
			int type = (int)(double)a["Type"];
			if (type == DAMAGE) {
				double hp = (double)obj["Hp"];
				double dmg = (double)a["Damage"];
				obj["Hp"] = System.Math.Max(0, hp - dmg);
			}
			else if (type == HEAL) {
				double hp = (double)obj["Hp"];
				double mhp = (double)obj["MaxHp"];
				double heal = (double)a["Heal"];
				obj["Hp"] = System.Math.Min(mhp, hp + heal);				
			}
			else if (type == EFFECT) {
				Debug.Log("Effect Added");
			}
		}
	}
	public void SetDeadData(Dictionary<object, object> d) {
		Dictionary<object, object> obj = FindObjectById(d["TargetId"]);
		if (obj != null) {
			obj["IsDead"] = true;
		}
	}
	public string EnemyText(object e) {
		var d = (Dictionary<object, object>)e;
		string text = string.Format("{0}", d["Name"]);
		text = text+string.Format("HP {0}/{1}:", d["Hp"], d["MaxHp"]);
		text = text+string.Format("WP {0}/{1}", d["Wp"], d["MaxWp"]);
		object b;
		if (d.TryGetValue("IsDead", out b)) {
			text = text+":(dead)";
		}
		return text;
	}
	public string HeroText(object e) {
		var text = EnemyText(e);
		var d = (Dictionary<object, object>)e;
		return text;		
	}
	public string BattleFieldText() {
		var text = "field status:";
		if (this.Winner != null) {
			text = text + string.Format("finished Winner({0})", this.Winner);
		}
		else {
			text = text + "ongoing";
		}
		return text;
	}
	
	void Start () {
	}
	
	const int BOX_WIDTH = 460;
	const int BOX_HEIGHT = 540;
	const int BOX_X = 10;
	const int BOX_Y = 120;
	const int MERGIN = 10;
	const int BUTTON_X = BOX_X + MERGIN;
	const int BUTTON_START_Y = BOX_Y + 2 * MERGIN;
	const int BUTTON_WIDTH = BOX_WIDTH - 2 * MERGIN;
	const int BUTTON_HEIGHT = 50;
	void OnGUI () {
        // Make a background box
        GUI.Box(new Rect(BOX_X,BOX_Y,BOX_WIDTH,BOX_HEIGHT), BattleFieldText());
    
		int cnt = 0;
		foreach (var e in Enemy()) {
			GUI.Button(new Rect(BUTTON_X, BUTTON_START_Y + BUTTON_HEIGHT * cnt, BUTTON_WIDTH, BUTTON_HEIGHT), EnemyText(e.Value));
			cnt++;
		}

		foreach (var e in Hero()) {
			GUI.Button(new Rect(BUTTON_X, 10 + BUTTON_START_Y + BUTTON_HEIGHT * cnt, BUTTON_WIDTH, BUTTON_HEIGHT), HeroText(e.Value));
			cnt++;			
		}
    }
	
	//battle event receiver.
	//LuaTable almost acts like Dictionary. 
	//you can get property by data[hoge], and iterate it by foreach (KeyValuePair<object, object> e in data) {}. 
	public void Play(string type, LuaTable data) {
		var dict = GameField.ToDictionary(data);
		Debug.Log("Play:"+type+"|"+Json.Serialize(dict));
		if (type == "init") {
			this.SceneData = dict;
		}
		else if (type == "status_change") {
			this.SetStateData(dict);
		}
		else if (type == "action") {
			this.SetActionData(dict);
		}
		else if (type == "dead") {
			this.SetDeadData(dict);
		}
		else if (type == "end") {
			this.Winner = (string)dict["Winner"];
		}
	}
}
