using UnityEngine;
using System.Collections.Generic;

public class BattleField : MonoBehaviour {

	GameField gf;
	Renderer renderer;
	
	public bool local = true;
	public bool debug = false;
	public string url = "";
	
	void Start () {
		InitFixData();
		gf = new GameField(this.debug);
		if (local) {
			renderer = new Renderer();
			var user_data = TestUserData();
			var field_data = TestFieldData();
			gf.InitLocal(field_data, renderer);
			gf.Enter(renderer, user_data);
		}
		else {
			//TODO : initialize remote game field
			gf.InitRemote(url, renderer);
			gf.Enter(renderer);
		}
	}

	void Update () {
		gf.Update(Time.deltaTime);
	}
	
	// test fixture
	Dictionary<string, object> TestFieldData() {
		return new Dictionary<string, object>() { 
			{
				"Cells", new List<List<string>> {
					new List<string> { "green_cell" }
				}
			},
			{
				"Arrangements", new List<string> {}
			},
			{
				"Objectives", new List<string> {
					"normal_enemy_annihilation",
				}
			},
			{
				"Events", new List<string>{}
			},
			{
				"Teams", new List<string> {
					"normal_battle_user", "normal_battle_enemy",
				}
			}
		};	
	}
	Dictionary<string, object> TestUserData() {
		return new Dictionary<string, object>() { 
			{
				"TeamId", "normal_battle_user"
			},
			{
				"Heroes", new List<Dictionary<string, object>> {
					new Dictionary<string, object> {
						{ "Id", "hero" },
						{ "Exp", 100 }
					}, 
					new Dictionary<string, object> {
						{ "Id", "hero" },
						{ "Exp", 1000 }
					}, 
					new Dictionary<string, object> {
						{ "Id", "hero" },
						{ "Exp", 10000 }
					}, 
					new Dictionary<string, object> {
						{ "Id", "hero" },
						{ "Exp", 100000 }
					}, 
					new Dictionary<string, object> {
						{ "Id", "hero" },
						{ "Exp", 1000000 }
					}
				}
			},
			{
				"ObjectiveId", "normal_enemy_annihilation"
			}
		};	
	}
	void InitFixData() {
		var test_data = new Dictionary<string, Dictionary<string, Dictionary<string, object>>> {
			{
				"CellTypes", new Dictionary<string, Dictionary<string, object>> {
					{
						"green_cell", new Dictionary<string, object> {
							{"Name", "草原"},
							{"Script", "cells/cell_base.lua"}
						}
					},
					{
						"ice_cell", new Dictionary<string, object> {
							{"Name", "雪原"},
							{"TypeClass", "DOTCellType"},
							{"Script", "cells/dot.lua"},
							{"DamagePerTick", 10}
						}
					}
				}
			}, 
			{
				"Objectives", new Dictionary<string, Dictionary<string, object>> {
					{
						"normal_enemy_annihilation", new Dictionary<string, object> {
							{"Name", "敵の殲滅"},
							{"TypeClass", "LossRatioObjectiveType"},
							{"Script", "objectives/loss_ratio.lua"},
							{"AssignedTeam", "normal_battle_user"},
							{"Group", "default"},
							{"LossRatio", 100},
							{"TeamId", "normal_battle_enemy"},
						}
					}
				}
			}, 
			{
				"Skills", new Dictionary<string, Dictionary<string, object>> {
					{
						"attack", new Dictionary<string, object> {
							{"Name", "攻撃"},
							{"TypeClass", "AttackSkillType"},
							{"Script", "skills/attack.lua"},
							{"Ratio", 1.0f}
						}
					},
					{
						"double_attack", new Dictionary<string, object> {
							{"Name", "二段切り"},
							{"TypeClass", "AttackSkillType"},
							{"Script", "skills/attack.lua"},
							{"Ratio", 2.0f}
						}
					}
				}
			}, 
			{
				"Teams", new Dictionary<string, Dictionary<string, object>> {
					{
						"normal_battle_user", new Dictionary<string, object> {
							{"Name", "通常戦闘時ユーザー"},
							{"Script", "teams/team_base.lua"},
							{"FriendlyTeams", new List<string> {"normal_battle_user"} },
							{"HostileTeams", new List<string> {"normal_battle_enemy"} }
						}
					},
					{
						"normal_battle_enemy", new Dictionary<string, object> {
							{"Name", "通常戦闘時エネミー"},
							{"Script", "teams/team_base.lua"},
							{"FriendlyTeams", new List<string> {"normal_battle_enemy"} },
							{"HostileTeams", new List<string> {"normal_battle_user"} }
						}						
					}
				}
			}, 
			{
				"Events", new Dictionary<string, Dictionary<string, object>> {}	
			},
			{
				"Objects", new Dictionary<string, Dictionary<string, object>> {
					{
						"user", new Dictionary<string, object> {
							{"Name", "ユーザー"},
							{"Class", "GameUser"},
							{"Script", "users/game_user.lua"},
							{"DisplaySide", "user"}
						}
					},
					{
						"hero", new Dictionary<string, object> {
							{"Name", "ヒーロー"},
							{"TypeClass", "HeroObjectType"},
							{"Class", "Character"},
							{"Script", "objects/hero.lua"},
							{"DisplaySide", "user"},
							{"MaxHp", 100}
						}					
					},
					{
						"npc", new Dictionary<string, object> {
							{"Name", "NPC"},
							{"TypeClass", "NPCObjectType"},
							{"Class", "Character"},
							{"Script", "objects/npc.lua"},
							{"DisplaySide", "enemy"},
							{"MaxHp", 150}
						}											
					}
				}
			},
		};
		GameField.Initialize(test_data);
	}
}
