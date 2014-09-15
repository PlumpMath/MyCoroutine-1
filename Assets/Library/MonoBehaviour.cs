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
			// routineを静的なリストに登録する
			Main.AddRoutine(this, routine);

			return null;
		}

		public Coroutine StartCoroutine(string methodName, object arg = null)
		{
			IEnumerator routine = (IEnumerator)this.GetType().InvokeMember(
				methodName,
				BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
				null, this, new object[] { arg }
			);

			Main.AddRoutine(this, routine);

			return null;
		}

		public void StopCorutine(string methodName)
		{
			Main.RemoveRoutine(this, methodName);
		}

		public void StopAllCoroutines()
		{
			Main.RemoveAllRoutines(this);
		}
	}
}