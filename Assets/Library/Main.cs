using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class Main : MonoBehaviour
{
	// キーがnullになることがある、かつ登録した順序を保持したいので型はこうなる。
	private static Dictionary<OtherEngine.MonoBehaviour, OtherEngine.BehaviourData> behaviourDict = 
		new Dictionary<OtherEngine.MonoBehaviour, OtherEngine.BehaviourData>();

	public static void AddMonoBehaviour(OtherEngine.MonoBehaviour behaviour)
	{
		if (!behaviourDict.ContainsKey(behaviour))
		{
			behaviourDict.Add(behaviour, new OtherEngine.BehaviourData(behaviour));
		}
	}

	public static OtherEngine.Coroutine AddRoutine(OtherEngine.MonoBehaviour behaviour, string methodName, IEnumerator routine)
	{
		OtherEngine.BehaviourData bdata;

		if (behaviourDict.TryGetValue(behaviour, out bdata))
		{
			var coroutine = new OtherEngine.Coroutine(methodName, routine);

			// 登録前にコルーチンの初回実行を行う。
			// なお、StartCoroutine() が呼ばれた時点でこのメソッドも再帰的に呼ばれるのに注意。

			// 自分自身が実行されているチェーン(LinkedList<Coroutine>)を渡す必要がある。
			// ★そのためには、現在実行されているルーチンがどこに所属するのかを判別する必要がある。
			//ProcessRoutine(bdata.routineList, coroutine);

			//もしyield return されたコルーチンの場合は、そのyield returnが呼ばれたコルーチンのリストの最後にくっつける。
			//そうではない場合、新たにリストを作ってroutineListの最後にくっつける。
			var list = new LinkedList<OtherEngine.Coroutine>();
			list.AddLast(coroutine);
			bdata.routineList.AddLast(list);

			return coroutine;
		}
		else
		{
			// ここに来ることはない
			return null;
		}
	}

	private static void ProcessRoutine(IEnumerator routine)
	{
		// 一回だけ実行
		if (routine.MoveNext())
		{
			object current = routine.Current;
			// ★TODO: とりあえずcurrentがCoroutineだった場合のみ考慮
			if (current is OtherEngine.Coroutine)
			{
				var coroutine = (OtherEngine.Coroutine)current;
				// coroutineが yield return の戻り値として呼び出されたので、
				// このcoroutine をコルーチンの実行リストから外して、コルーチンチェーンに組み込む。

			}
		}
	}

	public static void RemoveRoutine(OtherEngine.MonoBehaviour behaviour, string methodName)
	{
		OtherEngine.BehaviourData bdata;
		if (behaviourDict.TryGetValue(behaviour, out bdata))
		{
			LinkedListNode<LinkedList<OtherEngine.Coroutine>> node = bdata.routineList.First;
			while (node != null)
			{
				LinkedList<OtherEngine.Coroutine> list = node.Value;
				RemoveRoutineSub(list, methodName);

				var oldNode = node;
				node = node.Next;

				// listの要素が空になった場合は、list自体を除去。
				if (list.Count == 0)
				{
					bdata.routineList.Remove(oldNode);
				}
			}
		}
	}

	private static void RemoveRoutineSub(LinkedList<OtherEngine.Coroutine> list, string methodName)
	{
		LinkedListNode<OtherEngine.Coroutine> node = list.First;
		while (node != null)
		{
			var oldNode = node;
			node = node.Next;
			if (oldNode.Value.methodName == methodName)
			{
				list.Remove(oldNode);
			}
		}
	}

	public static void RemoveAllRoutines(OtherEngine.MonoBehaviour behaviour)
	{
		OtherEngine.BehaviourData bdata;
		if (behaviourDict.TryGetValue(behaviour, out bdata))
		{
			bdata.routineList.Clear();
		}
	}

	public void Awake()
	{
		AddMonoBehaviour(new Test());
	}

	public void Update()
	{
		// すべてのMonoBehaviourを実行
		foreach (OtherEngine.BehaviourData bdata in behaviourDict.Values)
		{
			if (!bdata.mainloopBegan)
			{
				bdata.behaviour.Start();
				bdata.mainloopBegan = true;
			}

			bdata.behaviour.Update();
		}

		// すべてのMonoBehaviourが持つコルーチンを実行。
		// コルーチンは Update の後に呼ばれるので、ここで実行。
		foreach (OtherEngine.BehaviourData bdata in behaviourDict.Values)
		{
			LinkedListNode<LinkedList<OtherEngine.Coroutine>> node = bdata.routineList.First;
			while (node != null)
			{
				LinkedList<OtherEngine.Coroutine> list = node.Value;
				ProcessCoroutine(list);

				var oldNode = node;
				node = node.Next;

				if (list.Count == 0)
				{
					bdata.routineList.Remove(oldNode);
				}
			}
		}
	}

	private void ProcessCoroutine(LinkedList<OtherEngine.Coroutine> list)
	{
		LinkedListNode<OtherEngine.Coroutine> node = list.First;
		while (node != null)
		{
			OtherEngine.Coroutine rdata = node.Value;
			if (rdata.routine.MoveNext())
			{
				object current = rdata.routine.Current;
				// ★ここがキモ
				ProcessYieldInstruction(current);
				node = node.Next;
			}
			else
			{
				// 終わったコルーチンはリストから除外
				LinkedListNode<OtherEngine.Coroutine> toRemove = node;
				node = node.Next;
				list.Remove(toRemove);
			}
		}
	}

	private void ProcessYieldInstruction(object instruction)
	{
		// コルーチンの戻り値を見て適切な処理を実行

		if (instruction == null)
		{
			return;
		}

		if (instruction is Coroutine)
		{
			// ここで instruction を実行する必要がある。
			// かつ、次のメインループで実行されるのは、instruction を辿った最後のコルーチン。
		}
		else if (instruction is YieldInstruction)
		{
			//WaitForSeconds
		}
	}
}
