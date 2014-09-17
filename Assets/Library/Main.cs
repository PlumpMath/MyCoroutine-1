using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class Main : MonoBehaviour
{
	// キーがnullになることがある、かつ登録した順序を保持したいので型はこうなる。
	//private static List<OtherEngine.BehaviourData> routineList = new List<OtherEngine.BehaviourData>();
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
			// ここでコルーチンの初回実行。Currentが意味のない値を返すようになるまで再帰的に呼び出し続ける。
			// なお、StartCoroutine() が呼ばれた時点でこのメソッドも再帰的に呼ばれるのに注意。

			ProcessRoutine(routine);

			bdata.routineList.AddLast(new OtherEngine.RoutineData(methodName, routine));
		}

		return new OtherEngine.Coroutine(bdata);
	}

	private static void ProcessRoutine(IEnumerator routine)
	{
		// 一回だけ実行
		if (routine.MoveNext())
		{
			object current = routine.Current;
			// ★とりあえずcurrentがCoroutineだった場合のみ考慮
			if (current is OtherEngine.Coroutine)
			{
				// yield return StartCoroutine(routine)されている。
				// ★routineListへの登録を削除する。
			}
		}
	}

	public static void RemoveRoutine(OtherEngine.MonoBehaviour behaviour, string methodName)
	{
		OtherEngine.BehaviourData bdata;
		if (behaviourDict.TryGetValue(behaviour, out bdata))
		{
			LinkedListNode<OtherEngine.RoutineData> node = bdata.routineList.First;
			while (node != null)
			{
				OtherEngine.RoutineData rdata = node.Value;

				var oldNode = node;
				node = node.Next;
				if (rdata.methodName == methodName)
				{
					bdata.routineList.Remove(oldNode);
				}
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
			if (bdata.mainloopBegan)
			{
				bdata.behaviour.Update();
			}
			else
			{
				bdata.behaviour.Start();
				bdata.mainloopBegan = true;
			}
		}

		//コルーチンはUpdateの後
		foreach (OtherEngine.BehaviourData bdata in behaviourDict.Values)
		{
			LinkedListNode<OtherEngine.RoutineData> node = bdata.routineList.First;
			while (node != null)
			{
				OtherEngine.RoutineData rdata = node.Value;
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
					LinkedListNode<OtherEngine.RoutineData> toRemove = node;
					node = node.Next;
					bdata.routineList.Remove(toRemove);
				}
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
