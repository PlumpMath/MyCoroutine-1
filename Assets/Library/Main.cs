﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class Main : MonoBehaviour
{
	[SerializeField]
	private int framerate;

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

	public static void AddRoutine(OtherEngine.MonoBehaviour behaviour, string methodName, IEnumerator routine)
	{
		OtherEngine.BehaviourData bdata;
		if (behaviourDict.TryGetValue(behaviour, out bdata))
		{
			bdata.routineList.AddLast(new OtherEngine.RoutineData(methodName, routine));
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
		if (instruction == null)
		{
			return;
		}

		if (instruction is Coroutine)
		{

		}
		else if (instruction is YieldInstruction)
		{
			//WaitForSeconds
		}
	}
}
