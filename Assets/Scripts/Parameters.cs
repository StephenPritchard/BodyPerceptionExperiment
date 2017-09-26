using System.IO;
using System.Text;

public static class Parameters
{
    public static int NumberOfPracticeTrials;
    public static int NumberOfTrialsPerBlock;
    public static int NumberOfBlocks;
    public static int CountDownDuration;
    public static int TimeoutForTrial;
    public static float WalkBeforePoles;
    public static float WalkAfterPoles;
    public static bool PolesPositionedViaTracker;
    public static float[] ApertureToBodyRatios;
    public static PoleHeightSetting PoleHeightPreset;
    public static float PoleHeightPresetValue;
    public static bool TrackHmd;
    public static bool TrackLeftHand;
    public static bool TrackRightHand;
    public static bool TrackLeftHip;
    public static bool TrackRightHip;
    public static bool TrackLeftShoulder;
    public static bool TrackRightShoulder;
    
    public static void LoadParameters(FileInfo fileParameters)
    {
        var streamR = fileParameters.ReadFileToStream();
        string line;

        do
        {
            line = streamR.ReadLine();
            if (line == null) continue;

            var splitline = line.Split(' ', ',');

            switch (splitline[0])
            {
                case "NumberOfPracticeTrials:":
                    NumberOfPracticeTrials = int.Parse(splitline[1]);
                    UI.WriteLineToExperimenterScreen("NumberOfPracticeTrials = " + NumberOfPracticeTrials);
                    break;

                case "NumberOfTrialsPerBlock:":
                    NumberOfTrialsPerBlock = int.Parse(splitline[1]);
                    UI.WriteLineToExperimenterScreen("NumberOfTrialsPerBlock = " + NumberOfTrialsPerBlock);
                    break;

                case "NumberOfBlocks:":
                    NumberOfBlocks = int.Parse(splitline[1]);
                    UI.WriteLineToExperimenterScreen("NumberOfBlocks = " + NumberOfBlocks);
                    break;

                case "CountDownDuration:":
                    CountDownDuration = int.Parse(splitline[1]);
                    UI.WriteLineToExperimenterScreen("CountDownDuration = " + CountDownDuration);
                    break;

                case "TimeoutForTrial:":
                    TimeoutForTrial = int.Parse(splitline[1]);
                    UI.WriteLineToExperimenterScreen("TimeoutForTrial = " + TimeoutForTrial);
                    break;

                case "WalkBeforePoles:":
                    WalkBeforePoles = float.Parse(splitline[1]);
                    UI.WriteLineToExperimenterScreen("WalkBeforePoles = " + WalkBeforePoles);
                    break;

                case "WalkAfterPoles:":
                    WalkAfterPoles = float.Parse(splitline[1]);
                    UI.WriteLineToExperimenterScreen("WalkAfterPoles = " + WalkAfterPoles);
                    break;

                case "PolesPositionedViaTracker:":
                    PolesPositionedViaTracker = int.Parse(splitline[1]) == 1;
                    UI.WriteLineToExperimenterScreen("PolesPositionedViaTracker = " + WalkBeforePoles);
                    break;

                case "ApertureToBodyRatios:":
                    ApertureToBodyRatios = new float[splitline.Length - 1];
                    for (var i = 1; i < splitline.Length; i++)
                        ApertureToBodyRatios[i - 1] = float.Parse(splitline[i]);

                    var sBuilder = new StringBuilder();
                    foreach (var element in ApertureToBodyRatios)
                    {
                        sBuilder.Append(element.ToString("F2") + ",");
                    }
                    UI.WriteLineToExperimenterScreen("ApertureToBodyRatios = " + sBuilder);
                    break;

                case "PoleHeightPreset:":
                    string poleHeightPresetString;
                    switch (int.Parse(splitline[1]))
                    {
                        case 1:
                            poleHeightPresetString = "shoulder height";
                            PoleHeightPreset = PoleHeightSetting.Shoulder;
                            break;
                        case 2:
                            poleHeightPresetString = "preset value";
                            PoleHeightPreset = PoleHeightSetting.Preset;
                            break;
                        case 3:
                            poleHeightPresetString = "pole tracker height";
                            PoleHeightPreset = PoleHeightSetting.Tracker;
                            break;
                        default:
                            poleHeightPresetString = "hip height";
                            PoleHeightPreset = PoleHeightSetting.Hip;
                            break;
                    }
                    UI.WriteLineToExperimenterScreen("PoleHeightPreset = " + poleHeightPresetString);
                    break;

                case "PoleHeightPresetValue:":
                    PoleHeightPresetValue = float.Parse(splitline[1]);
                    if (PoleHeightPreset == PoleHeightSetting.Preset)
                    {
                        UI.WriteLineToExperimenterScreen("PoleHeightPresetValue:" + PoleHeightPresetValue);
                    }
                    break;

                case "TrackHMD:":
                    TrackHmd = int.Parse(splitline[1]) == 1;
                    break;

                case "TrackLeftHand:":
                    TrackLeftHand = int.Parse(splitline[1]) == 1;
                    break;

                case "TrackRightHand:":
                    TrackRightHand = int.Parse(splitline[1]) == 1;
                    break;

                case "TrackLeftHip:":
                    TrackLeftHip = int.Parse(splitline[1]) == 1;
                    break;

                case "TrackRightHip:":
                    TrackRightHip = int.Parse(splitline[1]) == 1;
                    break;

                case "TrackLeftShoulder:":
                    TrackLeftShoulder = int.Parse(splitline[1]) == 1;
                    break;

                case "TrackRightShoulder:":
                    TrackRightShoulder = int.Parse(splitline[1]) == 1;
                    break;
            }
        } while (line != null);

        var builderDataToRecord = new StringBuilder();
        builderDataToRecord.Append("DataToRecord: ");
        if (TrackHmd) builderDataToRecord.Append("HMD,");
        if (TrackRightHand) builderDataToRecord.Append("RightHand,");
        if (TrackLeftHand) builderDataToRecord.Append("LeftHand,");
        if (TrackRightHip) builderDataToRecord.Append("RightHip,");
        if (TrackLeftHip) builderDataToRecord.Append("LeftHip,");
        if (TrackRightShoulder) builderDataToRecord.Append("RightShoulder,");
        if (TrackLeftShoulder) builderDataToRecord.Append("LeftShoulder,");
        UI.WriteLineToExperimenterScreen(builderDataToRecord.ToString());

        streamR.Close();
    }
}