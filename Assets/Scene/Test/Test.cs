using OtherEngine;
using System.Collections;

public class Test : MonoBehaviour
{
	public override void Start()
	{
		//StartCoroutine(RoutineTest());
		StartCoroutine("RoutineTest", "hoge");
	}

	int count;

	public override void Update()
	{
		++count;
		if (count == 300)
		{
			StopAllCoroutines();
		}
	}

	private IEnumerator RoutineTest(string x)
	{
		while (true)
		{
			UnityEngine.Debug.Log("★ Coroutine");
			yield return null;
		}
	}
}