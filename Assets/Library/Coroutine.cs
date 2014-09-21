using System.Collections;
using System.Collections.Generic;

namespace OtherEngine
{
	public class Coroutine : YieldInstruction
	{
		// ★このクラスのメンバやメソッドは、実際には internal で実装されることになると思う

		public string methodName;
		public IEnumerator routine;
		public bool isChained;

		public LinkedListNode<Coroutine> node;

		public Coroutine(string methodName, IEnumerator routine)
			: this(methodName, routine, false)
		{
		}

		public Coroutine(string methodName, IEnumerator routine,  bool isChained)
		{
			this.methodName = methodName;
			this.routine = routine;
			this.isChained = isChained;
		}
	}
}