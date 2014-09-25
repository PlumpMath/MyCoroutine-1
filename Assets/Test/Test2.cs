using MyLib;
using System.Collections;

public class Test2 : MonoBehaviour {

	// Use this for initialization
	public override void Start()
	{
		StartCoroutine(Routine1());
	}
	
	IEnumerator Routine1()
	{
		UnityEngine.Debug.Log("Routine1 In");

		yield return StartCoroutine(Routine2());

		UnityEngine.Debug.Log("Routine1 Out");
	}

	IEnumerator Routine2()
	{
		UnityEngine.Debug.Log("    Routine2 In");
		yield return StartCoroutine(Routine3());
		UnityEngine.Debug.Log("    Routine2 Out");
	}

	IEnumerator Routine3()
	{
		UnityEngine.Debug.Log("        Routine3!");
		yield break;
		UnityEngine.Debug.Log("    Routine2 Out");
	}
}
