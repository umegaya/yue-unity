using UnityEngine;
using YueUnityTest;
using System.Collections.Generic;

namespace YueUnityTest {
	class BattleField : Yue.GameField {
		//configurable instance variables
		public Method login_method = Method.LOCAL;
		public bool debug = false;
		public string login_url = null;
		public string field_id = null;
		public string queue_name = null;
		public int group_size = 5;

		void Start () {
			switch (login_method) {
			case Method.LOCAL:
				InitLocal(Fixture.FixedData, Fixture.FieldData, debug);
				break;
			case Method.REMOTE:
				if (debug) {
					InitRemoteWithCreateField(login_url, Fixture.FixedData, Fixture.FieldData);
				}
				else {
					InitRemote(login_url, field_id);
				}
				break;
			case Method.MATCHING:
				if (debug) {
					InitMatchingWithConfigure(login_url, queue_name, group_size, Fixture.FixedData, Fixture.FieldData);
				}
				else {
					InitMatching(login_url, queue_name, group_size, Fixture.FieldData);
				}
				break;
			}
		}
	}
} 
