using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Serialization;
using UnityEngine.Events;
using UnityEngine.UI;

public class GadgetController : MonoBehaviour
{
    [HideInInspector] public ParameterData Data;
    private InputField _inputField;
    private Button _btnMinus;
    private Button _btnPlus;
    private Slider _slider;

	// Use this for initialization
	void Awake ()
	{
	    _inputField = transform.FindChild("Config/InputField").GetComponent<InputField>();
	    _btnMinus = transform.FindChild("Config/btnMinus").GetComponent<Button>();
	    _btnPlus = transform.FindChild("Config/btnPlus").GetComponent<Button>();
	    _slider = transform.FindChild("Slider").GetComponent<Slider>();
	}

    public void Init()
    {
        _inputField.text = Data.initial.ToString();
        _btnMinus.onClick.AddListener(DecrementValue);
        _btnPlus.onClick.AddListener(IncrementValue);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void IncrementValue()
    {
        Debug.Log("IncrementValue");
        var value = float.Parse(_inputField.text);
        if (value + Data.variance > Data.max) return;
        value += Data.variance;
        _inputField.text = value.ToString();
    }

    public void DecrementValue()
    {
        Debug.Log("DecrementValue");
        var value = float.Parse(_inputField.text);
        if (value - Data.variance < Data.min) return;
        value -= Data.variance;
        _inputField.text = value.ToString();
    }

    public void SetCallBackFunc(UnityAction<string> cb)
    {
        _inputField.onValueChanged.RemoveAllListeners();
        _inputField.onValueChanged.AddListener(cb);
    }
}
