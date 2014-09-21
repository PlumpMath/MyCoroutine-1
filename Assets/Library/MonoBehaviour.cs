using System.Collections;
using System.Reflection;

namespace OtherEngine
{
	public class MonoBehaviour
	{
		public virtual void Start()
		{
		}

		public virtual void Update()
		{
		}

		public Coroutine StartCoroutine(IEnumerator routine)
		{
			return StartCoroutineCommon(null, routine);
		}

		public Coroutine StartCoroutine(string methodName, object arg = null)
		{
			object[] param = (arg == null) ?
				null :
				new object[] { arg };

			IEnumerator routine = (IEnumerator)this.GetType().InvokeMember(
				methodName,
				BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
				null, this, param
			);

			return StartCoroutineCommon(methodName, routine);
		}

		private Coroutine StartCoroutineCommon(string methodName, IEnumerator routine)
		{
			// コルーチンの初回実行はStartCoroutineを呼び出したシーケンスで行われるので、
			// このあたりで一回 MoveNext() を呼びたいところ。
			return Main.AddRoutine(this, methodName, routine);
		}

		public void StopCoroutine(string methodName)
		{
			Main.RemoveRoutine(this, methodName);
		}

		public void StopAllCoroutines()
		{
			Main.RemoveAllRoutines(this);
		}
	}
}