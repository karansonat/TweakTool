using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [HideInInspector] public GameObject SaveDialogue;
    [HideInInspector] public GameObject LoadDialogue;
    [HideInInspector] public GameObject ResetDialogue;

    private GameObject _profileHolder;
    private GameObject _profile1;
    private GameObject _profile2;
    private GameObject _profile3;

    void Awake()
    {
        SaveDialogue = transform.FindChild("SavePanel").gameObject;
        LoadDialogue = transform.FindChild("LoadPanel").gameObject;
        ResetDialogue = transform.FindChild("ResetPanel").gameObject;
    }

	// Use this for initialization
	void Start ()
	{
	    _profileHolder = TweakTool.Instance.MainPanel.transform.FindChild("ProfilePanel/Profiles").gameObject;
	    _profile1 = _profileHolder.transform.FindChild("Profile1").gameObject;
	    _profile2 = _profileHolder.transform.FindChild("Profile2").gameObject;
	    _profile3 = _profileHolder.transform.FindChild("Profile3").gameObject;
	}

    public void SaveProfile()
    {
        var inputField = SaveDialogue.transform.FindChild("Dialogue/InputField").GetComponent<InputField>();
        var ProfileTitle = _profileHolder.transform.FindChild("Profile" + TweakTool.Instance.CurrentSaveSlot + "/Name")
            .GetComponent<Text>();
        TweakTool.Instance.SaveProfile(inputField.text);
        ProfileTitle.text = inputField.text;
        inputField.text = "";
        HideSaveProfileDialogue();
    }

    public void LoadProfile()
    {
        TweakTool.Instance.LoadProfile();
        HideLoadDialogue();
    }

    public void ResetParameters()
    {
        TweakTool.Instance.ResetAllParameters();
        HideResetDialogue();
    }

    public void ShowSaveProfileDialogue()
    {
        SaveDialogue.SetActive(true);
    }

    public void HideSaveProfileDialogue()
    {
        SaveDialogue.SetActive(false);
    }

    public void ShowLoadDialogue()
    {
        LoadDialogue.SetActive(true);
    }

    public void HideLoadDialogue()
    {
        LoadDialogue.SetActive(false);
    }

    public void ShowResetDialogue()
    {
        ResetDialogue.SetActive(true);
    }

    public void HideResetDialogue()
    {
        ResetDialogue.SetActive(false);
    }
}
