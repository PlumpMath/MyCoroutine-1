using UnityEngine;
using System.Collections;

namespace OtherEngine
{
	public class Coroutine : YieldInstruction
	{
		// ★このクラスのメンバやメソッドは、実際には internal で実装されることになると思う

		public BehaviourData bdata;

		public Coroutine(BehaviourData bdata)
		{
			this.bdata = bdata;
		}
	}
}