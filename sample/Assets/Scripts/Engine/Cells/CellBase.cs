using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class CellTypeBase : Util.FixData {
	}
	public class Partition {
		// team_id => all objects belongs to team_id
		public Dictionary<string, List<User>> Users { get; set; }
		public Dictionary<string, List<ObjectBase>> Teams { get; set; }
		public Partition() {
			this.Users = new Dictionary<string, List<User>>();
			this.Teams = new Dictionary<string, List<ObjectBase>>();
		}
		public void EnterUser(User u) { Enter<User>(this.Users, u); }
		public void Enter(ObjectBase o) { Enter<ObjectBase>(this.Teams, o); }
		public void Enter<T>(Dictionary<string, List<T>> d, T o) where T : ObjectBase {
			List<T> list;
			if (!d.TryGetValue(o.Team.Type.Id, out list)) {
				list = new List<T>();
				d[o.Team.Type.Id] = list;
			}
			list.Add(o);
			//Debug.Log("object added to part:" + d.Count + "|" + list.Count + "|" + o.Team.Type.Id + "|" + o);
			o.Partition = this;
		}
		public void ExitUser(User u) { Exit<User>(this.Users, u); }
		public void Exit(ObjectBase o) { Exit<ObjectBase>(this.Teams, o); }
		public bool Exit<T>(Dictionary<string, List<T>> d, T o) where T : ObjectBase {
			Debug.Log("Exit:" + o);
			List<T> list;
			if (d.TryGetValue(o.Team.Type.Id, out list)) {
				list.Remove(o);
				o.Partition = null;
				return list.Count <= 0;
			}
			return false;
		}
	}
	public class CellBase {
		// cell type
		public CellTypeBase Type { get; set; }
		public Partition EnemySide { get; set; }
		public List<Partition> UserSide { get; set; }
		public Dictionary<int, ObjectBase> ObjectMap { get; set; }
		public ObjectBase FindObject(int id) { return this.ObjectMap[id]; }
		public CellBase() {
			this.EnemySide = new Partition();
			this.UserSide = new List<Partition>();
			this.UserSide.Add(new Partition());
			this.ObjectMap = new Dictionary<int, ObjectBase>();
		}
	}
	public class CellFactory : Util.Factory<CellTypeBase, CellBase> {}
}
