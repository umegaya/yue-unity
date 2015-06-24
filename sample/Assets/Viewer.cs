using UnityEngine;
using MiniJSON;
using YueUnityTest;
using System.Collections.Generic;

namespace YueUnityTest {
	public class Viewer : MonoBehaviour {
		public string SessionGameObject = "Session1";
			
		void Start () {
		}
		
		void Update() {
		}
		
		//display UI
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
			var go = GameObject.Find(SessionGameObject);
			if (go == null) {
				return;
			}
			BattleSession s = go.GetComponent<BattleSession>();
			if (s == null) { return; }
			if (s.SceneData == null) {
				if (s.MatchingWaitUser >= 0) {
			        GUI.Box(new Rect(BOX_X,BOX_Y,BOX_WIDTH,BOX_HEIGHT), s.MatchingWaitUser.ToString() + " users wait for matching");
				}
				return;
			}
	        // Make a background box
	        GUI.Box(new Rect(BOX_X,BOX_Y,BOX_WIDTH,BOX_HEIGHT), s.BattleFieldText());
	    
			int cnt = 0;
			foreach (var e in s.Enemy()) {
				if (GUI.Button(new Rect(BUTTON_X, BUTTON_START_Y + BUTTON_HEIGHT * cnt, BUTTON_WIDTH, BUTTON_HEIGHT), s.EnemyText(e))) {
					s.SendCommand(delegate (object []rvs, object err) {
						if (rvs != null) {
							s.Cooldown = (double)rvs[0];
						}
						else {
							Debug.Log("cmderr:" + err);
						}
					}, s.BuildBattleCommand(e));
					s.ShuffleSkillSelection();
				}
				cnt++;
			}
	
			foreach (var e in s.Hero()) {
				if (s.IsMyHero(e)) {
					GUI.Label(new Rect(BUTTON_X, 10 + BUTTON_START_Y + BUTTON_HEIGHT * cnt, BUTTON_WIDTH, BUTTON_HEIGHT), s.HeroText(e));
					cnt++;
				}
			}
			
			if (GUI.Button(new Rect(BUTTON_X, BUTTON_START_Y + BUTTON_HEIGHT * cnt, BUTTON_WIDTH, BUTTON_HEIGHT), "Shuffle Skill Selection")) {
				s.ShuffleSkillSelection();
			}		
	    }
	}
}