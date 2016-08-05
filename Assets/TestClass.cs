using UnityEngine;
using System.Collections;

public class TestClass : MonoSingleton<TestClass>
{

    public float testFloat;

	// Use this for initialization
	void Start ()
	{
	    testFloat = 5.0f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
