using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
	public class BehaviourData
	{
		public MyLib.MonoBehaviour behaviour;
		public bool mainloopBegan;
		public LinkedList<LinkedList<Coroutine>> routineList;

		public BehaviourData(MyLib.MonoBehaviour behaviour)
		{
			this.behaviour = behaviour;
			this.mainloopBegan = false;
			this.routineList = new LinkedList<LinkedList<Coroutine>>();
		}
	}
}