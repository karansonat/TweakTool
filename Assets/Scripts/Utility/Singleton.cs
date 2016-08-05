// Simplifies Singleton pattern code.
// Tim Johnson (c) PlayQ

using System;
using UnityEngine;

/// <summary>
/// Prefab attribute. Use this on child classes
/// to define if they have a prefab associated or not
/// By default will attempt to load a prefab
/// that has the same name as the class,
/// otherwise [Prefab("path/to/prefab")]
/// to define it specifically.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PrefabAttribute : Attribute
{
    string _name;

    public string Name
    {
        get { return _name; }
    }

    public PrefabAttribute()
    {
        _name = "";
    }

    public PrefabAttribute(string name)
    {
        _name = name;
    }
}

/// <summary>
///     Be aware this will not prevent a non singleton constructor
///     such as `T myT = new T();`
///     To prevent that, add `protected T () {}` to your singleton class.
///
/// </summary>
public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    //public static T Instance { get; private set; }
    public static T main
    {
        get { return Instance; }
    }

/*
            protected virtual void OnDestroy()
            {
            }

            protected virtual void Awake()
            {
                if (Instance == null)
                {
                    Instance = (T)this;
                }
                else
                {
                    Log.Error("Got a second instance of the class " + this.GetType());
                }
            }*/

    private static T _instance;

    public static bool IsAwake
    {
        get { return (_instance != null); }
    }

    /// <summary>
    /// gets the instance of this Singleton
    /// use this for all instance calls:
    /// MyClass.Instance.MyMethod();
    /// or make your public methods static
    /// and have them use Instance internally
    /// for a nice clean interface
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            Debug.Log("Initializing singleton: " + typeof(T));
            // 1. Look for instance in scene
            var mytype = typeof(T);
            _instance = (T) FindObjectOfType(mytype);
            var count = FindObjectsOfType(mytype).Length;
            if (count > 1)
            {
                Debug.LogError("Singleton: there are " + count + " of " + mytype.Name);
                throw new Exception("Too many (" + count + ") prefab singletons of type: " + mytype.Name);
            }
            if (_instance != null)
                return _instance;

            //Debug.Log("initializing instance of: " + mytype.Name);
            var goName = mytype.ToString();
            var go = GameObject.Find(goName);
            if (go == null) // try again searching for a cloned object
            {
                go = GameObject.Find(goName + "(Clone)");
                if (go != null)
                {
                    //Debug.Log("found clone of object using it!");
                }
            }

            if (go == null && Attribute.IsDefined(mytype, typeof(PrefabAttribute)))
                //if still not found try prefab or create
            {
                var name = typeof(T).ToString();
                // checks if the [Prefab] attribute is set and pulls that if it can

                var attr = (PrefabAttribute) Attribute.GetCustomAttribute(mytype, typeof(PrefabAttribute));
                name = attr.Name == "" ? typeof(T).ToString() : attr.Name;

                name = name.Contains("/") ? name : "Scripts/" + name;

                //Debug.LogWarning(goName + " not found attempting to instantiate prefab... either: " + goName + " or: " + prefabname);
                try
                {
                    go = Resources.Load(name) as GameObject;
                    if (go)
                    {
                        go = Instantiate(go);
                        go.name = go.name.Replace("(Clone)", "");
                    }
                    else
                    {
                        Debug.LogError("Failed to load prefab '" + name + "' for singleton " + typeof(T));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(mytype + ": could not instantiate prefab '" + name +
                                   "' even though prefab attribute was set: " + e.Message + "\n" + e.StackTrace);
                }
            }
            if (go == null)
            {
                //Debug.LogWarning(goName + " not found creating...");
                go = new GameObject
                {
                    name = goName
                };
            }

            _instance = go.GetComponent<T>() ?? go.GetComponentInChildren<T>() ?? go.AddComponent<T>();
            if (_instance == null)
                Debug.LogError(
                    "Could not instantiate prefab (no script on loaded object). Your prefab should have the script attatched");


            return _instance;
        }
    }


    protected virtual void Awake()
    {
        _instance = (T) this;

    }

    protected virtual void OnDestroy()
    {

    }

    /// <summary>
    /// for garbage collection
    /// </summary>
    public virtual void OnApplicationQuit()
    {
        // release reference on exit
        _instance = null;
    }
}


public class Singleton<T>
{
    private static T _instance;

    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = default(T);
                }

                return _instance;
            }
        }
    }
}