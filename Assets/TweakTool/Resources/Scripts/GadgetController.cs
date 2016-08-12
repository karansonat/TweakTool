using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Serialization;
using UnityEngine.Events;
using UnityEngine.UI;

public class GadgetController : MonoBehaviour
{
    [HideInInspector] public ParameterData Data;
    private Text _title;
    private InputField _inputField;
    private Button _btnMinus;
    private Button _btnPlus;
    private Slider _slider;
    private UnityAction<string> _cb;

	// Use this for initialization
	void Awake ()
	{
	    _title = transform.FindChild("Title").GetComponent<Text>();
	    _inputField = transform.FindChild("Config/InputField").GetComponent<InputField>();
	    _btnMinus = transform.FindChild("Config/btnMinus").GetComponent<Button>();
	    _btnPlus = transform.FindChild("Config/btnPlus").GetComponent<Button>();
	    _slider = transform.FindChild("Slider").GetComponent<Slider>();
	}

    public void Init()
    {
        _title.text = Data.name;
        _inputField.text = Data.current.ToString();
        _inputField.characterLimit = 7;
        _inputField.onEndEdit.AddListener((value) =>
        {
            float currentValue;
            float.TryParse(value, out currentValue);
            if (currentValue > Data.max) currentValue = Data.max;
            if (currentValue < Data.min) currentValue = Data.min;
            _inputField.text = currentValue.ToString();
        });
        _btnMinus.onClick.AddListener(DecrementValue);
        _btnPlus.onClick.AddListener(IncrementValue);
        _slider.wholeNumbers = true;
        Refresh();
    }

    public void Refresh()
    {
        _inputField.text = Data.current.ToString();
        _slider.minValue = 0;
        var max = (Data.max - Data.min) / Data.variance;
        _slider.maxValue = max;
        _slider.onValueChanged.RemoveAllListeners();
        _slider.onValueChanged.AddListener((value) =>
        {
            _inputField.text = (value * Data.variance + Data.min).ToString();
        });
        _slider.value = (Data.current - Data.min) / Data.variance;
        _slider.Rebuild(CanvasUpdate.Layout);
    }

    public void IncrementValue()
    {
        var value = float.Parse(_inputField.text);
        value = (value + Data.variance > Data.max) ? Data.max : value + Data.variance;
        _inputField.text = value.ToString();
        _slider.value = (value - Data.min) / Data.variance;
    }

    public void DecrementValue()
    {
        var value = float.Parse(_inputField.text);
        value = (value - Data.variance < Data.min) ? Data.min : value - Data.variance;
        _inputField.text = value.ToString();
        _slider.value = (value - Data.min) / Data.variance;
    }

    public void SetCallBackFunc(UnityAction<string> cb)
    {
        _inputField.onValueChanged.RemoveAllListeners();
        _inputField.onValueChanged.AddListener(cb);
        _cb = cb;
    }
}
