using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine.Events;
using UnityEngine.UI;
using Newtonsoft.Json;


[System.Serializable]
public class ParameterData
{
    public bool inGame = false;
    public string name;
    public float initial;
    public float variance;
    public float min;
    public float max;
    [JsonIgnore, NonSerialized] public GameObject Gadget;
}

public class TweakTool : MonoSingleton<TweakTool>
{
    public GameObject MainPanel;
    public GameObject InGamePanel;

    public GameObject ParameterHolder;
    public GameObject InGameGadgetHolder;
    public GameObject ParameterPrefab;
    public GameObject InGameGadgetPrefab;
    private Button _triggerButton;
    public List<ParameterData> ParameterList = new List<ParameterData>();

    public string test;
    public float testFloat = 5;

    void Awake()
    {
        // BinaryFormatter used in ExtensionMethods.DeepCopy requires jit compiler.
        // Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
    }

	// Use this for initialization
	void Start ()
	{
	    _triggerButton = transform.FindChild("InvisibleTrigger").GetComponent<Button>();
	    _triggerButton.onClick.AddListener(() =>
	    {
	        MainPanel.SetActive(true);
	        InGamePanel.SetActive(false);
	    });

	    MainPanel.transform.FindChild("btnExit").GetComponent<Button>().onClick.AddListener(() =>
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
            //Variance
            var varianceInputField =
                ParameterHolder.transform.GetChild(childIndex).FindChild("Parameters/Variance/Config/InputField").GetComponent<InputField>();
            varianceInputField.text = parameter.variance.ToString();

            //Minimum
            var minInputField =
                ParameterHolder.transform.GetChild(childIndex).FindChild("Parameters/Minimum/Config/InputField").GetComponent<InputField>();
            minInputField.text = parameter.min.ToString();

            //Maximum
            var maxInputField =
                ParameterHolder.transform.GetChild(childIndex).FindChild("Parameters/Maximum/Config/InputField").GetComponent<InputField>();
            maxInputField.text = parameter.max.ToString();

            childIndex++;
        }
    }

    public void AddParameter(string name, float initial,float variance, float min, float max, UnityAction<string> cb)
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
            initial = initial,
            variance = variance,
            min = min,
            max = max,
            Gadget = gadgetObject
        };

        //Set Gadget Call function
        data.Gadget.GetComponent<GadgetController>().SetCallBackFunc(cb);

        //Hide gadget until user wants to see it.
        data.Gadget.SetActive(false);

        //Add parameter data to list.
        parameterObject.GetComponent<Parameter>().SetParameter(data);
        ParameterList.Add(data);

        //Initialize gadget component
        data.Gadget.GetComponent<GadgetController>().Data = data;
        data.Gadget.GetComponent<GadgetController>().Init();

        //Name
        var parameterName = parameterObject.transform.FindChild("Name").GetComponent<Text>();
        parameterName.text = "    " + name;

        //InGame
        var InGameCheckBox =
            parameterObject.transform.FindChild("Parameters/Include/Toggle").GetComponent<Toggle>();
        InGameCheckBox.onValueChanged.AddListener((value) =>
        {
            data.inGame = value;
            RefreshInGameGadgets(data, value);
        });

        //Variance
        var varianceInputField =
            parameterObject.transform.FindChild("Parameters/Variance/Config/InputField").GetComponent<InputField>();
        varianceInputField.text = variance.ToString();
        varianceInputField.onValueChanged.AddListener((value) =>
        {
            data.variance = float.Parse(value);
        });

        //Minimum
        var minInputField =
            parameterObject.transform.FindChild("Parameters/Minimum/Config/InputField").GetComponent<InputField>();
        minInputField.text = min.ToString();
        minInputField.onValueChanged.AddListener((value) =>
        {
            data.min = float.Parse(value);
        });

        //Maximum
        var maxInputField =
            parameterObject.transform.FindChild("Parameters/Maximum/Config/InputField").GetComponent<InputField>();
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
    }

}
