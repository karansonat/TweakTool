using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Parameters : MonoBehaviour
{
    public float FloatNum
    {
        get { return TestClass.Instance.testFloat; }
        set { TestClass.Instance.testFloat = value; }
    }


	// Use this for initialization
	void Start ()
	{
	    TweakTool.Instance.AddParameter("FloatNum", FloatNum, 5, 0, 100, OnValueChangedFloatNum);
	}

    private void OnValueChangedFloatNum(string value)
    {
        FloatNum = float.Parse(value);
    }

}
