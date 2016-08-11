using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public partial class TweakTool
{
    public float FloatTest1
    {
        get { return TestClass.Instance.test1; }
        set { TestClass.Instance.test1 = value; }
    }

    public float FloatTest2
    {
        get { return TestClass.Instance.test2; }
        set { TestClass.Instance.test2 = value; }
    }

    public float FloatTest3
    {
        get { return TestClass.Instance.test3; }
        set { TestClass.Instance.test3 = value; }
    }
}
