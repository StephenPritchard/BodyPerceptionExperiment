using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class UI
{
    public static GameObject TitleScreen = GameObject.Find("TitleScreen");
    public static GameObject CalibrateScreen = GameObject.Find("CalibrateScreen");
    public static GameObject InstructionsScreen = GameObject.Find("InstructionsScreen");
    public static Text InstructText = GameObject.Find("InstructText").GetComponent<Text>();
    public static GameObject BetweenBlocksScreen = GameObject.Find("BetweenBlocksScreen");
    public static Text BetweenBlocksText = GameObject.Find("BetweenBlocksText").GetComponent<Text>();
    public static GameObject ReturnToStartCrossScreen = GameObject.Find("ReturnToStartCrossScreen");
    public static GameObject ReadyForNextTrialScreen = GameObject.Find("ReadyForNextTrialScreen");
    public static GameObject DuringTrialScreen = GameObject.Find("DuringTrialScreen");
    public static GameObject CountDownScreen = GameObject.Find("CountDownScreen");
    public static Text CountDownIntroText = GameObject.Find("CountDownIntroText").GetComponent<Text>();
    public static Text CountDownNumberText = GameObject.Find("CountDownNumberText").GetComponent<Text>();
    public static GameObject ExperimenterScrollView = GameObject.Find("ExperimenterScrollView");
    public static Text ExperimenterTitle = GameObject.Find("ExperimenterTitle").GetComponent<Text>();
    public static Text ExperimenterText = GameObject.Find("ExperimenterText").GetComponent<Text>();
    public static GameObject ApertureDisplay = GameObject.Find("ApertureDisplay");
    public static Text ApertureValue = GameObject.Find("ApertureValue").GetComponent<Text>();
    public static GameObject CalibrationLines = GameObject.Find("CalibrationLines");
    public static GameObject XCalibrationLine = GameObject.Find("XCalibrationLine");

    public static void LoadInstructions(FileInfo fileInstructions)
    {
        var streamR = fileInstructions.ReadFileToStream();
        string line;
        var textBuilder = new StringBuilder();

        do
        {
            line = streamR.ReadLine();
            if (line == null)
                continue;

            textBuilder.AppendLine(line);
        } while (line != null);

        UI.InstructText.text = textBuilder.ToString();
        UI.WriteLineToExperimenterScreen("Instructions Loaded.");
    }

    public static void WriteLineToExperimenterScreen(string line)
    {
        ExperimenterText.text += line + "\n";
        Canvas.ForceUpdateCanvases();
        ExperimenterScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
    }

    public static void WriteLineToExperimenterScreen()
    {
        ExperimenterText.text += "\n";
        Canvas.ForceUpdateCanvases();
        ExperimenterScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
    }
}