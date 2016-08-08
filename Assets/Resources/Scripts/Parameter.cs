using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Parameter : MonoBehaviour
{
    private ParameterData _parameter;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetParameter(ParameterData data)
    {
        _parameter = data;
    }

    public void Refresh()
    {
        //InGame
        var InGameCheckBox = transform.FindChild("TitleField/Toggle").GetComponent<Toggle>();
        InGameCheckBox.isOn = _parameter.inGame;
        _parameter.Gadget.GetComponent<GadgetController>().Refresh();

        //Current
        var currentInputField = transform.FindChild("Variables/Current/Config/InputField").GetComponent<InputField>();
        currentInputField.text = _parameter.current.ToString();

        //Variance
        var varianceInputField = transform.FindChild("Variables/Variance/Config/InputField").GetComponent<InputField>();
        varianceInputField.text = _parameter.variance.ToString();

        //Minimum
        var minInputField = transform.FindChild("Variables/Minimum/Config/InputField").GetComponent<InputField>();
        minInputField.text = _parameter.min.ToString();

        //Maximum
        var maxInputField = transform.FindChild("Variables/Maximum/Config/InputField").GetComponent<InputField>();
        maxInputField.text = _parameter.max.ToString();
    }
}
