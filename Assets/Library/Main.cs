using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class Main : MonoBehaviour
{
	// キーがnullになることがある、かつ登録した順序を保持したいので型はこうなる。
	private static Dictionary<OtherEngine.MonoBehaviour, OtherEngine.BehaviourData> behaviourDict = 
		new Dictionary<OtherEngine.MonoBehaviour, OtherEngine.BehaviourData>();

	/// <summary>
	/// 現在実行されているCoroutine。
	/// </summary>
	private static Stack<OtherEngine.Coroutine> currentRoutine = new Stack<OtherEngine.Coroutine>();

	public static void AddMonoBehaviour(OtherEngine.MonoBehaviour behaviour)
	{
		if (!behaviourDict.ContainsKey(behaviour))
		{
			behaviourDict.Add(behaviour, new OtherEngine.BehaviourData(behaviour));
		}
	}

	/// <summary>
	/// このメソッドの実行中に StartCoroutine() が呼ばれると再入するので注意。
	/// </summary>
	/// <returns>The routine.</returns>
	/// <param name="behaviour">Behaviour.</param>
	/// <param name="methodName">Method name.</param>
	/// <param name="routine">Routine.</param>
	public static OtherEngine.Coroutine AddRoutine(OtherEngine.MonoBehaviour behaviour, string methodName, IEnumerator routine)
	{
		OtherEngine.BehaviourData bdata;

		if (behaviourDict.TryGetValue(behaviour, out bdata))
		{
			var coroutine = new OtherEngine.Coroutine(methodName, routine);

			// 何はともあれまずコルーチンを登録
			var list = new LinkedList<OtherEngine.Coroutine>();
			coroutine.node = list.AddLast(coroutine);
			bdata.routineList.AddLast(list);

			// コルーチンの初回実行を行う。
			ProcessCoroutine(coroutine);

			return coroutine;
		}
		else
		{
			// ここに来ることはない
			return null;
		}
	}

	/// <summary>
	/// コルーチンの実行。コルーチンが既に終了していたらfalseを返す。
	/// </summary>
	/// <param name="routineList">Routine list.</param>
	/// <param name="coroutine">Coroutine.</param>
	private static bool ProcessCoroutine(OtherEngine.Coroutine coroutine)
	{
		// beforeがnullである場合、現在のコンテキストがゲームのメインループであることを
		// 意味するので、coroutineをMoveNext()させると必ずnullが返ってくる(yield return できないため)。
		OtherEngine.Coroutine before = (currentRoutine.Count > 0) ?
			currentRoutine.Peek() :
			null;

		currentRoutine.Push(coroutine);

		bool executed = coroutine.routine.MoveNext();

		// 一回だけ実行
		if (executed && before != null)
		{
			object current = coroutine.routine.Current;

			// ★TODO: とりあえずcurrentがCoroutineだった場合のみ考慮
			// 将来的にはYieldInstructionにも対応する必要あり。

			// current は yield return の戻り値である。
			if (current is OtherEngine.Coroutine)
			{
				var next = (OtherEngine.Coroutine)current;

				// next をbeforeの後ろにくっつける。
				// ただし、next が既に別のコルーチンチェーンに組み込まれていた場合、
				// ログを出すだけで何もしない。
				if (next.isChained)
				{
					UnityEngine.Debug.Log("[エラー] 1つのコルーチンで2つ以上のコルーチンを待機させる事はできません。");
				}
				else
				{
					// nextが登録されているLinkedListからnextを削除。
					next.node.List.Remove(next.node);
					// beforeのリストに改めてnextを登録。
					next.node = before.node.List.AddLast(next);
					// nextはコルーチンチェーンに組み込まれたので、フラグを立てる。
					next.isChained = true;
				}
			}
		}

		currentRoutine.Pop();

		return executed;
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
				LinkedList<OtherEngine.Coroutine> coroutineChain = node.Value;
				ProcessChainedCoroutine(coroutineChain);

				var oldNode = node;
				node = node.Next;

				// コルーチンチェーンが空になったら、チェーンの入れ物自体を破棄。
				if (coroutineChain.Count == 0)
				{
					bdata.routineList.Remove(oldNode);
				}
			}
		}
	}

	private void ProcessChainedCoroutine(LinkedList<OtherEngine.Coroutine> chain)
	{
		// chainの末尾を実行。
		// 実行完了していたら、chainから削除。

		LinkedListNode<OtherEngine.Coroutine> node = chain.Last;
		if (node != null)
		{
			OtherEngine.Coroutine coroutine = node.Value;

			if (ProcessCoroutine(coroutine))
			{
				node = node.Next;
			}
			else
			{
				// 終わったコルーチンはリストから除外
				LinkedListNode<OtherEngine.Coroutine> toRemove = node;
				node = node.Next;
				chain.Remove(toRemove);
			}
		}
	}
}
