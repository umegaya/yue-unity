using System.Collections.Generic;

namespace YueUnityTest {
	class Fixture {
		// test fixture
		static public Dictionary<string, object> FieldData = new Dictionary<string, object>() { 
			{ "SizeX", 1 }, { "SizeY", 1 },
			{
				"Cells", new List<List<string>> {
					new List<string> { "green_cell" }
				}
			},
			{
				"Arrangement", "sandbagland"
			},
			{
				"Objectives", new List<string> {
					"normal_enemy_annihilation", "normal_user_annihilation"
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
		static public Dictionary<string, object> UserData = new Dictionary<string, object>() { 
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
		static public Dictionary<string, Dictionary<string, Dictionary<string, object>>> FixedData = 
		new Dictionary<string, Dictionary<string, Dictionary<string, object>>> {
			{
				"Cells", new Dictionary<string, Dictionary<string, object>> {
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
							{"DamageName", "寒波"},
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
							{"TeamId", "normal_battle_enemy"}
						}
					},
					{
						"normal_user_annihilation", new Dictionary<string, object> {
							{"Name", "味方の全滅"},
							{"TypeClass", "LossRatioObjectiveType"},
							{"Script", "objectives/loss_ratio.lua"},
							{"AssignedTeam", "normal_battle_enemy"},
							{"Group", "default"},
							{"LossRatio", 100},
							{"TeamId", "normal_battle_user"}
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
							{"Range", "single"},
							{"Scope", "hostile"},
							{"BonusType", "multiply"},
							{"Bonus", 1.0f}
						}
					},
					{
						"double_attack", new Dictionary<string, object> {
							{"Name", "二段切り"},
							{"TypeClass", "AttackSkillType"},
							{"Script", "skills/attack.lua"},
							{"Range", "single"},
							{"Scope", "hostile"},
							{"Prefix", "二段"},
							{"Postfix", "切り"},
							{"Group", "multiway"},
							{"AcceptGroups", new List<string> {"multiway"} },
							{"BonusType", "multiply"},
							{"Bonus", 2.0f},
							{"Wp", 1}
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
				"Groups", new Dictionary<string, Dictionary<string, object>> {
					{						
						"sandbags", new Dictionary<string, object> {
							{"Script", "arrangements/group_base.lua"},
							{"Size", 4},
							{"RandomList", new List<string> {"npc", "npcplus"} },
							{"FixedList", new List<string> {"npcplus"} }
						}
					}
				}	
			},
			{
				"Arrangements", new Dictionary<string, Dictionary<string, object>> {
					{
						"sandbagland", new Dictionary<string, object> {
							{"Script", "arrangements/arrangement_base.lua"},
							{ 
								"TeamMemberLists", new Dictionary<string, List<string>> { 
									{"normal_battle_enemy", new List<string> {"sandbags"}}
								}
							}
						}
					}
				}	
			},
			{
				"Objects", new Dictionary<string, Dictionary<string, object>> {
					{
						"user", new Dictionary<string, object> {
							{"Name", "ユーザー"},
							{"TypeClass", "GameUserType"},
							{"Class", "GameUser"},
							{"Script", "users/game_user.lua"},
							{"DisplaySide", "user"},
							{"WaitSec", 3.0f}
						}
					},
					{
						"hero", new Dictionary<string, object> {
							{"Name", "ヒーロー"},
							{"TypeClass", "HeroObjectType"},
							{"Class", "Hero"},
							{"Script", "objects/hero.lua"},
							{"DisplaySide", "user"},
							{"MaxHp", 100},
							{"MaxWp", 10},
							{"Attack", 100},
							{"Defense", 80},
							{"Skills", new List<string> {"attack", "double_attack"}}
						}					
					},
					{
						"npc", new Dictionary<string, object> {
							{"Name", "サンドバッグ"},
							{"TypeClass", "NPCObjectType"},
							{"Class", "NPCObject"},
							{"Script", "objects/npc.lua"},
							{"DisplaySide", "enemy"},
							{"MaxHp", 150},
							{"MaxWp", 10},
							{"WaitSec", 5.0f},
							{"Attack", 100},
							{"Defense", 80},
							{"Skills", new List<string> {"attack", "double_attack"}}
						}
					},
					{
						"npcplus", new Dictionary<string, object> {
							{"Name", "強いサンドバッグ"},
							{"TypeClass", "NPCObjectType"},
							{"Class", "NPCObject"},
							{"Script", "objects/npc.lua"},
							{"DisplaySide", "enemy"},
							{"MaxHp", 1500},
							{"MaxWp", 100},
							{"WaitSec", 5.0f},
							{"Attack", 100},
							{"Defense", 80},
							{"Skills", new List<string> {"attack", "double_attack"}}
						}											
					}
				}
			},
		};
	}
}
