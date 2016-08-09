using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine.Events;
using UnityEngine.UI;
using Newtonsoft.Json;


[System.Serializable]
public class ParameterData
{
    public bool inGame = false;
    public string name;
    public float current;
    public float variance;
    public float min;
    public float max;
    [JsonIgnore, NonSerialized] public GameObject Gadget;
    [JsonIgnore, NonSerialized] public GameObject Container;
}

public partial class TweakTool : MonoSingleton<TweakTool>
{
    public GameObject MainPanel;
    public GameObject InGamePanel;

    public GameObject ParameterHolder;
    public GameObject InGameGadgetHolder;
    public GameObject ParameterPrefab;
    public GameObject InGameGadgetPrefab;
    private Button _triggerButton;
    private Button _liveDataButton;
    private bool _liveData = false;
    public List<ParameterData> ParameterList = new List<ParameterData>();
    private readonly List<ParameterData> _defaultParameterList = new List<ParameterData>();

    public string test;
    public float testFloat = 5;

    void Awake()
    {
        // BinaryFormatter used in ExtensionMethods.DeepCopy requires jit compiler.
        // Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
    }

    void LateUpdate()
    {
        if (!_liveData) return;
        Refresh();
    }

	// Use this for initialization
	void Start ()
	{
        InitParameters();
	    Refresh();

	    _liveDataButton = transform.Find("MainPanel/btnLiveData").GetComponent<Button>();
	    _triggerButton = transform.FindChild("InvisibleTrigger").GetComponent<Button>();
	    _triggerButton.onClick.AddListener(() =>
	    {
	        MainPanel.SetActive(true);
	        InGamePanel.SetActive(false);
	        Refresh();
	    });

	    MainPanel.transform.FindChild("btnApply").GetComponent<Button>().onClick.AddListener(() =>
	    {
	        MainPanel.SetActive(false);
	        if (InGamePanel.transform.childCount > 0)
	        {
	            InGamePanel.SetActive(true);
	        }
	    });
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SaveProfile(int index)
    {
        var data = JsonConvert.SerializeObject(ParameterList, Formatting.Indented);
        var path = Directory.CreateDirectory(Application.persistentDataPath + "//Tweakable//Profiles");
        File.WriteAllText(path.FullName + "//Profile" + index + ".json", data);
    }

    public void LoadProfile(int index)
    {
        var profileData = File.ReadAllText(Application.persistentDataPath + "//Tweakable//Profiles//Profile" + index + ".json");
        var parameters = JsonConvert.DeserializeObject<List<ParameterData>>(profileData);
        var childIndex = 0;
        foreach (var parameter in parameters)
        {
            //Container
            parameter.Container = ParameterHolder.transform.GetChild(childIndex).gameObject;

            //InGame
            var InGameCheckBox = parameter.Container.transform.FindChild("TitleField/Toggle").GetComponent<Toggle>();
            InGameCheckBox.isOn = parameter.inGame;

            //Current
            var currentInputField = parameter.Container.transform.FindChild("Variables/Current/Config/InputField").GetComponent<InputField>();
            currentInputField.text = parameter.current.ToString();

            //Variance
            var varianceInputField = parameter.Container.transform.FindChild("Variables/Variance/Config/InputField").GetComponent<InputField>();
            varianceInputField.text = parameter.variance.ToString();

            //Minimum
            var minInputField = parameter.Container.transform.FindChild("Variables/Minimum/Config/InputField").GetComponent<InputField>();
            minInputField.text = parameter.min.ToString();

            //Maximum
            var maxInputField = parameter.Container.transform.FindChild("Variables/Maximum/Config/InputField").GetComponent<InputField>();
            maxInputField.text = parameter.max.ToString();

            parameter.Container.GetComponent<Parameter>().Refresh();

            childIndex++;
        }
    }

    public void AddParameter(string name, float initial, float variance, float min, float max, UnityAction<string> cb)
    {
        //Async creation for providing fast loading.
        StartCoroutine(AddParameterAsync(name, initial, variance, min, max, cb));
    }

    private IEnumerator AddParameterAsync(string name, float initial, float variance, float min, float max, UnityAction<string> cb)
    {
        //Instantiate parameter object.
        var parameterObject = Instantiate(ParameterPrefab);

        //Instantiate gadget object.
        var gadgetObject = Instantiate(InGameGadgetPrefab);

        //Set hiearchy
        parameterObject.transform.SetParent(ParameterHolder.transform, false);
        gadgetObject.transform.SetParent(InGameGadgetHolder.transform, false);

        //Create and set parameter data
        var data = new ParameterData
        {
            name = name,
            current = initial,
            variance = variance,
            min = min,
            max = max,
            Gadget = gadgetObject,
            Container = parameterObject
        };

        //Set Gadget Call function
        data.Gadget.GetComponent<GadgetController>().SetCallBackFunc(cb);

        //Hide gadget until user wants to see it.
        data.Gadget.SetActive(false);

        //Add parameter data to list.
        parameterObject.GetComponent<Parameter>().ParameterData = data;
        ParameterList.Add(data);
        _defaultParameterList.Add(data.DeepCopy());

        //Initialize gadget component
        data.Gadget.GetComponent<GadgetController>().Data = data;
        data.Gadget.GetComponent<GadgetController>().Init();

        //Name
        var parameterName = parameterObject.transform.FindChild("TitleField/Name").GetComponent<Text>();
        parameterName.text = name;

        //InGame
        var InGameCheckBox =
            parameterObject.transform.FindChild("TitleField/Toggle").GetComponent<Toggle>();
        InGameCheckBox.onValueChanged.AddListener((value) =>
        {
            data.inGame = value;
            RefreshInGameGadgets(data, value);
        });

        //Current
        var currentInputField =
            parameterObject.transform.FindChild("Variables/Current/Config/InputField").GetComponent<InputField>();
        currentInputField.text = initial.ToString();
        currentInputField.onValueChanged.AddListener(cb);

        //Variance
        var varianceInputField =
            parameterObject.transform.FindChild("Variables/Variance/Config/InputField").GetComponent<InputField>();
        varianceInputField.text = variance.ToString();
        varianceInputField.onValueChanged.AddListener((value) =>
        {
            data.variance = float.Parse(value);
        });

        //Minimum
        var minInputField =
            parameterObject.transform.FindChild("Variables/Minimum/Config/InputField").GetComponent<InputField>();
        minInputField.text = min.ToString();
        minInputField.onValueChanged.AddListener((value) =>
        {
            data.min = float.Parse(value);
        });

        //Maximum
        var maxInputField =
            parameterObject.transform.FindChild("Variables/Maximum/Config/InputField").GetComponent<InputField>();
        maxInputField.text = max.ToString();
        maxInputField.onValueChanged.AddListener((value) =>
        {
            data.max = float.Parse(value);
        });

        yield return null;
    }

    public void RefreshInGameGadgets(ParameterData data, bool visibility)
    {
        data.Gadget.SetActive(visibility);
        data.Gadget.GetComponent<GadgetController>().Refresh();
    }

    /// <summary>
    ///    Read game parameter data and update TweakTool.
    /// </summary>
    public void Refresh()
    {
        var fields = typeof(TweakTool).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in fields)
        {
            if (property.DeclaringType == typeof(TweakTool))
            {
                foreach (var parameterData in ParameterList)
                {
                    if (parameterData.name == property.Name)
                    {
                        parameterData.current = (float)property.GetValue(this, null);
                        parameterData.Container.GetComponent<Parameter>().Refresh();
                    }
                }
            }

        }
    }

    public void InitParameters()
    {
        var fields = typeof(TweakTool).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        for (var index = 0; index < fields.Length; index++)
        {
            var property = fields[index];
            if (property.DeclaringType == typeof(TweakTool))
            {
                AddParameter(property.Name, (float) property.GetValue(this, null), 1, 0, 100,
                    (value) => { property.SetValue(this, float.Parse(value), null); });
            }
        }
    }

    /// <summary>
    ///    Change all parameter datas with default ones.
    /// </summary>
    public void ResetAllParameters()
    {
        for (var i = 0; i < ParameterList.Count; i++)
        {
            var parameter = ParameterList[i].Container.GetComponent<Parameter>();
            var currentData = parameter.ParameterData;
            var defaultData = _defaultParameterList[i];
            currentData.current = defaultData.current;
            currentData.inGame = defaultData.inGame;
            currentData.variance = defaultData.variance;
            currentData.min = defaultData.min;
            currentData.max = defaultData.max;
            parameter.Refresh();

        }
    }

    public void ToogleLiveData()
    {
        _liveData = !_liveData;
        var textComp = _liveDataButton.gameObject.GetComponentInChildren<Text>();
        textComp.text = _liveData ? "Turn Off Live Data" : "Turn On Live Data";

        var greenColorBlock = _liveDataButton.colors;
        var colorGreen = Utility.HexToUnityColor("25AE2BFF");
        greenColorBlock.normalColor = colorGreen;
        greenColorBlock.highlightedColor = colorGreen;
        greenColorBlock.pressedColor = colorGreen;

        var redColorBlock = _liveDataButton.colors;
        var colorRed = Utility.HexToUnityColor("BD0909FF");
        redColorBlock.normalColor = colorRed;
        redColorBlock.highlightedColor = colorRed;
        redColorBlock.pressedColor = colorRed;

        _liveDataButton.colors = _liveData ? greenColorBlock : redColorBlock;
    }
}
