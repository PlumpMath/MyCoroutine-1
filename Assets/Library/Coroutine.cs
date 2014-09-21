using UnityEngine;
using System.Collections;

namespace OtherEngine
{
	public class Coroutine : YieldInstruction
	{
		// ★このクラスのメンバやメソッドは、実際には internal で実装されることになると思う

		public string methodName;
		public IEnumerator routine;

		public Coroutine(string methodName, IEnumerator routine)
		{
			this.methodName = methodName;
			this.routine = routine;
		}
	}
}