using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GetParticipantAndCondition : MonoBehaviour
{
    public static GetParticipantAndCondition Instance;
    public InputField ParticipantIDField;
    private bool _participantIDWasFocused;
    public static string ParticipantID;

    private bool _participantSuccessfullyEntered;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UI.EnterParticipantScreen = GameObject.Find("EnterParticipantScreen");
        UI.ConditionChoiceScreen = GameObject.Find("ConditionChoiceScreen");
        UI.ConditionChoiceScreen.SetActive(false);
        UI.EnterParticipantScreen.SetActive(true);

        if (ParticipantID != null)
            ParticipantIDField.text = ParticipantID;

        ParticipantIDField.Select();
        ParticipantIDField.ActivateInputField();
    }

    public void OnParticipantIDEntered()
    {
        ParticipantID = ParticipantIDField.text;
        if (ParticipantAlreadyExists(ParticipantID))
            ParticipantIDField.text = "Exists! Try another.";
        else
        {
            UI.ConditionChoiceScreen.SetActive(true);
            UI.EnterParticipantScreen.SetActive(false);
            _participantSuccessfullyEntered = true;
        }
    }

    private static bool ParticipantAlreadyExists(string pID)
    {
        var directories = Directory.GetDirectories(".");
        return directories.Any(dirName => dirName.Contains(pID));
    }

    private void Update()
    {
        //if (_participantIDWasFocused && Input.GetKeyDown(KeyCode.Return))
        //{
        //    OnParticipantIDEntered();
        //}

        //_participantIDWasFocused = ParticipantIDField.isFocused;

        if (!_participantSuccessfullyEntered) return;

        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            Parameters.Condition = 0;
            Parameters.ParametersFile = new FileInfo("parameters.txt");
            UI.ConditionChoiceScreen.SetActive(false);
            SceneManager.LoadScene("Experiment");
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            Parameters.Condition = 1;
            Parameters.ParametersFile = new FileInfo("condition1.txt");
            UI.ConditionChoiceScreen.SetActive(false);
            SceneManager.LoadScene("Experiment");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            Parameters.Condition = 2;
            Parameters.ParametersFile = new FileInfo("condition2.txt");
            UI.ConditionChoiceScreen.SetActive(false);
            SceneManager.LoadScene("Experiment");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            Parameters.Condition = 3;
            Parameters.ParametersFile = new FileInfo("condition3.txt");
            UI.ConditionChoiceScreen.SetActive(false);
            SceneManager.LoadScene("Experiment");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            Parameters.Condition = 4;
            Parameters.ParametersFile = new FileInfo("condition4.txt");
            UI.ConditionChoiceScreen.SetActive(false);
            SceneManager.LoadScene("Experiment");
        }
    }
}
