using UnityEngine;
using System.Collections;

public class Parameter : MonoBehaviour
{
    private ParameterData _parameter;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetParameter(ParameterData data)
    {
        _parameter = data;
    }
}
