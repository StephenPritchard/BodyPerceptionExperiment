using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class UI
{
    public static GameObject TitleScreen;
    public static GameObject CalibrateScreen;
    public static GameObject PostCalibrationScreen;
    public static GameObject InstructionsScreen;
    public static Text InstructText;
    public static GameObject BetweenBlocksScreen;
    public static Text BetweenBlocksText;
    public static GameObject ReturnToStartCrossScreen;
    public static Text ReturnToStartText;
    public static GameObject ReadyForNextTrialScreen;
    public static GameObject DuringTrialScreen;
    public static GameObject CountDownScreen;
    public static Text CountDownIntroText;
    public static Text CountDownNumberText;
    public static GameObject EnterParticipantScreen;
    public static GameObject ConditionChoiceScreen;
    public static GameObject ExperimenterScrollView;
    public static Text ExperimenterTitle;
    public static Text ExperimenterText;
    public static GameObject ApertureDisplay;
    public static Text ApertureValue;
    public static GameObject CalibrationLines;
    public static GameObject XCalibrationLine;

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