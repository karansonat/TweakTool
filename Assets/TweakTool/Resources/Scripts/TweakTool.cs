using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.UI;
using Newtonsoft.Json;

[Serializable]
public class ProfileData
{
    public string name;
    public List<ParameterData> data;
}

[Serializable]
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

public partial class TweakTool : MonoBehaviour
{
    public static TweakTool Instance;
    [HideInInspector] public GameObject MainPanel;
    [HideInInspector] public GameObject InGamePanel;
    [HideInInspector] public GameObject ParameterHolder;
    [HideInInspector] public GameObject ParameterPrefab;
    [HideInInspector] public GameObject InGameGadgetPrefab;
    private Button _triggerButton;
    private Button _liveDataButton;
    private bool _liveData = false;
    [HideInInspector] public int CurrentSaveSlot;
    [HideInInspector] public List<ParameterData> ParameterList = new List<ParameterData>();
    private readonly List<ParameterData> _defaultParameterList = new List<ParameterData>();

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

        // BinaryFormatter used in ExtensionMethods.DeepCopy requires jit compiler.
        // Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");

        MainPanel = transform.FindChild("MainPanel").gameObject;
        InGamePanel = transform.FindChild("InGamePanel").gameObject;
        ParameterHolder = transform.FindChild("MainPanel/Parameters Scroll View/Viewport/ParametersPanel").gameObject;
        ParameterPrefab = Resources.Load("Prefabs/Parameter") as GameObject;
        InGameGadgetPrefab = Resources.Load("Prefabs/InGameGadget") as GameObject;

        DontDestroyOnLoad(gameObject);
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
	    RefreshProfileSlots();
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
	        foreach (var parameterData in ParameterList)
	        {
	            parameterData.Gadget.GetComponent<GadgetController>().Refresh();
	        }
	    });
	}

    public void RefreshProfileSlots()
    {
        var profileHolder = TweakTool.Instance.MainPanel.transform.FindChild("ProfilePanel/Profiles").gameObject;

        if (File.Exists(Application.persistentDataPath + "//Tweakable//Profiles//Profile1.json"))
        {
            var profile1 = profileHolder.transform.FindChild("Profile1");
            var profile1Data = File.ReadAllText(Application.persistentDataPath + "//Tweakable//Profiles//Profile1.json");
            profile1.FindChild("Name").GetComponent<Text>().text = JsonConvert.DeserializeObject<ProfileData>(profile1Data).name;
        }

        if (File.Exists(Application.persistentDataPath + "//Tweakable//Profiles//Profile2.json"))
        {
            var profile2 = profileHolder.transform.FindChild("Profile2");
            var profile2Data = File.ReadAllText(Application.persistentDataPath + "//Tweakable//Profiles//Profile2.json");
            profile2.FindChild("Name").GetComponent<Text>().text = JsonConvert.DeserializeObject<ProfileData>(profile2Data).name;
        }

        if (File.Exists(Application.persistentDataPath + "//Tweakable//Profiles//Profile3.json"))
        {
            var profile3 = profileHolder.transform.FindChild("Profile3");
            var profile3Data = File.ReadAllText(Application.persistentDataPath + "//Tweakable//Profiles//Profile3.json");
            profile3.FindChild("Name").GetComponent<Text>().text = JsonConvert.DeserializeObject<ProfileData>(profile3Data).name;
        }
    }

    public void SaveProfile(string name)
    {
        var serializeObject = new ProfileData
        {
            name = name,
            data = ParameterList
        };
        var data = JsonConvert.SerializeObject(serializeObject, Formatting.Indented);
        var path = Directory.CreateDirectory(Application.persistentDataPath + "//Tweakable//Profiles");
        File.WriteAllText(path.FullName + "//Profile" + CurrentSaveSlot + ".json", data);
    }

    public void LoadProfile()
    {
        var data = File.ReadAllText(Application.persistentDataPath + "//Tweakable//Profiles//Profile" + CurrentSaveSlot + ".json");
        var profileData = JsonConvert.DeserializeObject<ProfileData>(data);
        var parameters = profileData.data;
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
        gadgetObject.transform.SetParent(InGamePanel.transform, false);

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
        currentInputField.onValueChanged.AddListener((value) =>
        {
            float.TryParse(value, out data.current);
        });

        //Variance
        var varianceInputField =
            parameterObject.transform.FindChild("Variables/Variance/Config/InputField").GetComponent<InputField>();
        varianceInputField.text = variance.ToString();
        varianceInputField.onValueChanged.AddListener((value) =>
        {
            float.TryParse(value, out data.variance);
        });

        //Minimum
        var minInputField =
            parameterObject.transform.FindChild("Variables/Minimum/Config/InputField").GetComponent<InputField>();
        minInputField.text = min.ToString();
        minInputField.onValueChanged.AddListener((value) =>
        {
            float.TryParse(value, out data.min);
        });

        //Maximum
        var maxInputField =
            parameterObject.transform.FindChild("Variables/Maximum/Config/InputField").GetComponent<InputField>();
        maxInputField.text = max.ToString();
        maxInputField.onValueChanged.AddListener((value) =>
        {
            float.TryParse(value, out data.max);
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
            if (property.DeclaringType != typeof(TweakTool)) continue;
            foreach (var parameterData in ParameterList)
            {
                if (parameterData.name != property.Name) continue;
                parameterData.current = (float)property.GetValue(this, null);
                parameterData.Container.GetComponent<Parameter>().Refresh();
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
                var parameterValue = (float) property.GetValue(this, null);
                AddParameter(property.Name, parameterValue, 1, 0, parameterValue * 5,
                    (value) =>
                    {
                        float result;
                        float.TryParse(value, out result);
                        property.SetValue(this, result, null);
                    });
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

    public void SetCurrentSaveSlot(int index)
    {
        CurrentSaveSlot = index;
    }
}
