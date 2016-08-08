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
    private UnityAction<string> _cb;

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
        _inputField.characterLimit = 7;
        _btnMinus.onClick.AddListener(DecrementValue);
        _btnPlus.onClick.AddListener(IncrementValue);
        Refresh();
    }

    public void Refresh()
    {
        _slider.minValue = Data.min;
        _slider.maxValue = Data.max;
        _slider.onValueChanged.RemoveAllListeners();
        _slider.onValueChanged.AddListener((value) =>
        {
            _inputField.text = value.ToString();
            _cb(value.ToString());
        });
    }

    public void IncrementValue()
    {
        var value = float.Parse(_inputField.text);
        value = (value + Data.variance > Data.max) ? Data.max : value + Data.variance;
        _inputField.text = value.ToString();
        _slider.value = value;
    }

    public void DecrementValue()
    {
        var value = float.Parse(_inputField.text);
        value = (value - Data.variance < Data.min) ? Data.min : value - Data.variance;
        _inputField.text = value.ToString();
        _slider.value = value;
    }

    public void SetCallBackFunc(UnityAction<string> cb)
    {
        _inputField.onValueChanged.RemoveAllListeners();
        _inputField.onValueChanged.AddListener(cb);
        _cb = cb;
    }
}
