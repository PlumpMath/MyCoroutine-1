using System.Collections;

namespace OtherEngine
{
	public class RoutineData
	{
		public string methodName;
		public IEnumerator routine;

		public RoutineData(string methodName, IEnumerator routine)
		{
			this.methodName = methodName;
			this.routine = routine;
		}
	}
}