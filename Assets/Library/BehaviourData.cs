using System.Collections;
using System.Collections.Generic;

namespace OtherEngine
{
	public class BehaviourData
	{
		public OtherEngine.MonoBehaviour behaviour;
		public bool mainloopBegan;
		public LinkedList<LinkedList<RoutineData>> routineList;

		public BehaviourData(OtherEngine.MonoBehaviour behaviour)
		{
			this.behaviour = behaviour;
			this.mainloopBegan = false;
			this.routineList = new LinkedList<LinkedList<RoutineData>>();
		}
	}
}