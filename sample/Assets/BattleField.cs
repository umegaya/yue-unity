using UnityEngine;
using YueUnityTest;
using System.Collections.Generic;

namespace YueUnityTest {
	class BattleField : Yue.GameField {
		//configurable instance variables
		public bool local = false;
		public bool debug = false;
		public string login_url = null;
		public string field_id = null;

		void Start () {
			if (local) {
				InitLocal(Fixture.FixedData, Fixture.FieldData, debug);
			}
			else if (string.IsNullOrEmpty(field_id)) {
				InitRemoteWithCreateField(login_url, Fixture.FixedData, Fixture.FieldData);
			}
			else {
				InitRemote(login_url, field_id);
			}		
		}
	}
} 
