using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Parameter : MonoBehaviour
{
    public ParameterData ParameterData;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Refresh()
    {
        //InGame
        var InGameCheckBox = transform.FindChild("TitleField/Toggle").GetComponent<Toggle>();
        InGameCheckBox.isOn = ParameterData.inGame;
        ParameterData.Gadget.GetComponent<GadgetController>().Refresh();

        //Current
        var currentInputField = transform.FindChild("Variables/Current/Config/InputField").GetComponent<InputField>();
        currentInputField.text = ParameterData.current.ToString();

        //Variance
        var varianceInputField = transform.FindChild("Variables/Variance/Config/InputField").GetComponent<InputField>();
        varianceInputField.text = ParameterData.variance.ToString();

        //Minimum
        var minInputField = transform.FindChild("Variables/Minimum/Config/InputField").GetComponent<InputField>();
        minInputField.text = ParameterData.min.ToString();

        //Maximum
        var maxInputField = transform.FindChild("Variables/Maximum/Config/InputField").GetComponent<InputField>();
        maxInputField.text = ParameterData.max.ToString();
    }
}
