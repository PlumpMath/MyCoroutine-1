using OtherEngine;
using System.Collections;

public class Test : MonoBehaviour
{
	public override void Start()
	{
		//StartCoroutine(RoutineTest());
		StartCoroutine("RoutineTest", "A");
		StartCoroutine("RoutineTest", "B");
	}

	int count;

	public override void Update()
	{
		++count;

		UnityEngine.Debug.Log("    Update:" + count);

		if (count == 50)
		{
			//StopAllCoroutines();
			StopCoroutine("RoutineTest");
		}
	}

	private IEnumerator RoutineTest(string name)
	{
		int x = 0;

		while (true)
		{
			++x;
			UnityEngine.Debug.Log("★" + name + ", " + x);
			yield return null;
		}
	}
}