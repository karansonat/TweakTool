using UnityEngine;
using System.Collections;

public class TestClass : MonoBehaviour
{
    public static TestClass Instance;

    public float test1;
    public float test2;
    public float test3;

    // Use this for initialization
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
