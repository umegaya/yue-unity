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
	System.Random _prng = new System.Random();
	public Dictionary<double, double> SkillSelection { get; set; }
	public Dictionary<object, object> SceneData { get; set; }
	public string Winner { get; set; }
	public double Cooldown { get; set; }
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
		string text = string.Format("{0}({1}) ", d["Name"], d["TargetId"]);
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
		object b; double idx;
		if (d.TryGetValue("IsDead", out b)) {
		}
		else if (this.SkillSelection.TryGetValue((double)d["TargetId"], out idx)) {
			var skill = (Dictionary<object, object>)(((Dictionary<object, object>)d["Skills"])[idx]);
			text = text + string.Format(": skill={0}", skill["Name"]);
		}
		return text;
	}
	public string BattleFieldText() {
		var text = "Field Status:";
		if (this.Winner != null) {
			text = text + string.Format("finished Winner({0})", this.Winner);
		}
		else {
			text = text + "ongoing";
			if (this.Cooldown <= 0.0) {
				text = text + " command ready";
			}
		}
		return text;
	}
	public void ShuffleSkillSelection() {
		if (this.SkillSelection == null) {
			this.SkillSelection = new Dictionary<double, double>();
		}
		this.SkillSelection.Clear();
		foreach (var e in Hero()) {
			var d = (Dictionary<object, object>)e.Value;
			var skills = (Dictionary<object, object>)d["Skills"];
			var idx = _prng.Next(1, skills.Count + 1);
			this.SkillSelection.Add((double)d["TargetId"], idx);
		}
	}
	public string GetSkillIdByTargetAndIndex(double target_id, double index) {
		var d = FindObjectById(target_id);
		var skills = (Dictionary<object, object>)d["Skills"];
		var skill = (Dictionary<object, object>)skills[index];
		return (string)skill["Id"];
	}
	public Dictionary<object, object> BuildBattleCommand(object target) {
		var d = new Dictionary<object, object>();
		var tdata = (Dictionary<object, object>)target;
		foreach (var e in this.SkillSelection) {
			int target_id = (int)(double)tdata["TargetId"];
			d[e.Key] = new Dictionary<string, object> {
				{ "TargetId", target_id },
				{ "SkillId", GetSkillIdByTargetAndIndex(e.Key, e.Value) }
			};
		}
		return new Dictionary<object, object> {
			{ "Type", "battle" },
			{ "Orders", d }
		};
	}
	
	void Start () {
	}
	
	void Update() {
		this.Cooldown -= Time.deltaTime;
		if (this.Cooldown < 0.0) {
			this.Cooldown = 0.0;
		}
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
		if (this.SceneData == null) {
			return;
		}
        // Make a background box
        GUI.Box(new Rect(BOX_X,BOX_Y,BOX_WIDTH,BOX_HEIGHT), BattleFieldText());
    
		int cnt = 0;
		foreach (var e in Enemy()) {
			if (GUI.Button(new Rect(BUTTON_X, BUTTON_START_Y + BUTTON_HEIGHT * cnt, BUTTON_WIDTH, BUTTON_HEIGHT), EnemyText(e.Value))) {
				BattleField.instance.SendCommand(delegate (object []rvs, object err) {
					if (rvs != null) {
						this.Cooldown = (double)rvs[0];
					}
				}, BuildBattleCommand(e.Value));
				ShuffleSkillSelection();
			}
			cnt++;
		}

		foreach (var e in Hero()) {
			GUI.Label(new Rect(BUTTON_X, 10 + BUTTON_START_Y + BUTTON_HEIGHT * cnt, BUTTON_WIDTH, BUTTON_HEIGHT), HeroText(e.Value));
			cnt++;			
		}
		
		if (GUI.Button(new Rect(BUTTON_X, BUTTON_START_Y + BUTTON_HEIGHT * cnt, BUTTON_WIDTH, BUTTON_HEIGHT), "Shuffle Skill Selection")) {
			ShuffleSkillSelection();
		}		
    }
	
	//battle event receiver.
	//LuaTable almost acts like Dictionary. 
	//you can get property by data[hoge], and iterate it by foreach (KeyValuePair<object, object> e in data) {}. 
	public void Play(string type, LuaTable data) {
		//*
		var dict = GameField.ToDictionary(data);
		//Debug.Log("Play:"+type+"|"+Json.Serialize(dict));
		if (type == "init") {
			this.SceneData = dict;
			ShuffleSkillSelection();
			this.Cooldown = 0;
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
		else if (type == "error") {
			Debug.LogError(Json.Serialize(dict));
		}
		//*/
	}
}
