﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class ExperimentManager : MonoBehaviour
{
    #region FIELDS
    public static ExperimentManager Instance;

    public float PoleDiameter;
    public GameObject CameraRig;

    private readonly FileInfo _fileInstructions = new FileInfo("instructions.txt");
    private readonly FileInfo _fileParameters = new FileInfo("parameters.txt");
    private readonly DirectoryInfo _mainDirectory = new DirectoryInfo(".");
    private DirectoryInfo _workingSubDirectory;
    private int _dirNumber = 1;

    private static readonly Stopwatch Timer = new Stopwatch();
    
    private static GameObject _startMarker;
    private static GameObject _endMarker;
    private static GameObject _poleLeft;
    private static GameObject _poleRight;

    private bool _endColliderTouched;
    private bool _experimentCommenced;
    private bool _poleTrackersAssigned;
    private bool _spacebarDown;
    private bool _recordingData;

    private int _currentTrial;
    private int _currentBlock;
    private float _currentAperture;

    private static float _poleZPosition;
    private float _poleHeightRight;
    private float _poleHeightLeft;
    private const float DefaultPoleHeight = 1.0f;
    private float[] _randomisedAtoBRatios;
    private float _bodyWidth;

    // Buffers data from an experiment block for writing to file.
    // Buffer will be flushed at the end of each block.

    // Fields for referencing the correct identity of each tracked device.
    private readonly Dictionary<SteamVR_TrackedObject.EIndex, SteamVR_TrackedObject>
        _trackedDevices = new Dictionary<SteamVR_TrackedObject.EIndex, SteamVR_TrackedObject>();
    private readonly Dictionary<DeviceRole, SteamVR_TrackedObject.EIndex>
        _trackedDeviceRoles = new Dictionary<DeviceRole, SteamVR_TrackedObject.EIndex>();
    private SteamVR_Controller.Device LeftController { get { return SteamVR_Controller.Input(_leftHandIndex); }}
    private SteamVR_Controller.Device RightController { get { return SteamVR_Controller.Input(_rightHandIndex); }}

    private readonly ExperimentBlockData _experimentBlockData = new ExperimentBlockData();

    private int _rightHandIndex;
    private int _leftHandIndex;

    #endregion

    #region UNITYMETHODS
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        _startMarker = GameObject.Find("StartMarker");
        _endMarker = GameObject.Find("EndMarker");
        _poleLeft = GameObject.Find("PoleLeft");
        _poleRight = GameObject.Find("PoleRight");


        UI.LoadInstructions(_fileInstructions);
        Parameters.LoadParameters(_fileParameters);
        InitialiseUIDisplay();
        RandomiseAtoBRatiosForTrials();
        InitialisePolePositions();
        InitialiseCalibrationLines();
        InitialiseStartAndEndMarkers();

        _rightHandIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
        _leftHandIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);

        UI.WriteLineToExperimenterScreen();
        UI.WriteLineToExperimenterScreen("Press space to begin tracker calibration.");
    }

    private void Update()
    {
        if (_rightHandIndex == -1)
            _rightHandIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
        if (_leftHandIndex == -1)
            _leftHandIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);

        if (Input.GetKeyDown(KeyCode.Space) || (_leftHandIndex != -1 && LeftController.GetHairTriggerDown()))
        {
            _spacebarDown = true;
            if (!_experimentCommenced)
            {
                _experimentCommenced = true;
                StartCoroutine(RunExperiment());
            }
        }
        else
        {
            _spacebarDown = false;
        }

        if (_recordingData)
        {
            RecordPosesDataToBuffer();
        }


        if (Input.GetKeyDown(KeyCode.G))
        {
            UI.CalibrationLines.SetActive(!UI.CalibrationLines.activeInHierarchy);
        }

        // If tracking the pole positions, update the virtual pole position and experimenter display every frame.
        if (!_poleTrackersAssigned) return;
        var rightPolePosition = _trackedDevices[_trackedDeviceRoles[DeviceRole.PoleRight]].transform.position;
        var leftPolePosition = _trackedDevices[_trackedDeviceRoles[DeviceRole.PoleLeft]].transform.position;
        _poleRight.transform.position = new Vector3(rightPolePosition.x, _poleHeightRight / 2f, rightPolePosition.z);
        _poleLeft.transform.position = new Vector3(leftPolePosition.x, _poleHeightRight / 2f, leftPolePosition.z);
        UI.ApertureValue.text = (Vector3.Distance(_poleLeft.transform.position, _poleRight.transform.position) - PoleDiameter).ToString("F2");
    }
    #endregion

    #region INITIALISATION
    private static void InitialiseUIDisplay()
    {
        UI.CalibrateScreen.SetActive(false);
        UI.InstructionsScreen.SetActive(false);
        UI.CountDownScreen.SetActive(false);
        UI.ReturnToStartCrossScreen.SetActive(false);
        UI.ReadyForNextTrialScreen.SetActive(false);
        UI.BetweenBlocksScreen.SetActive(false);
        UI.DuringTrialScreen.SetActive(false);
    }

    private void RandomiseAtoBRatiosForTrials()
    {
        _randomisedAtoBRatios = new float[Parameters.NumberOfBlocks * Parameters.NumberOfTrialsPerBlock];
        var currentRatioIndex = 0;
        for (var i = 0; i < _randomisedAtoBRatios.Length; i++)
        {
            _randomisedAtoBRatios[i] = Parameters.ApertureToBodyRatios[currentRatioIndex];
            currentRatioIndex++;
            if (currentRatioIndex == Parameters.ApertureToBodyRatios.Length)
                currentRatioIndex = 0;
        }
        _randomisedAtoBRatios.Shuffle();
        UI.WriteLineToExperimenterScreen("Aperture to body ratios randomised.");
    }

    private static void InitialisePolePositions()
    {
        // Position the poles and starting/end crosses to be best placed in the vive playspace.
        _poleZPosition = -(-Parameters.WalkBeforePoles + Parameters.WalkAfterPoles) / 2f;
        _poleLeft.transform.position = new Vector3(_poleLeft.transform.position.x, _poleLeft.transform.position.y,
            _poleZPosition);
        _poleRight.transform.position = new Vector3(_poleRight.transform.position.x, _poleRight.transform.position.y,
            _poleZPosition);
        UI.WriteLineToExperimenterScreen("Pole positions initialised. Poles at Z = " + _poleZPosition);
        if (!Parameters.PolesPositionedViaTracker)
        {
            UI.ApertureDisplay.SetActive(false);
        }
    }

    private static void InitialiseCalibrationLines()
    {
        UI.XCalibrationLine.transform.localPosition = new Vector3(0f, 0f, _poleZPosition);
        UI.CalibrationLines.SetActive(false);
    }

    private static void InitialiseStartAndEndMarkers()
    {
        const float crossHeight = 0.0001f;
        _startMarker.transform.position = new Vector3(0f, crossHeight, _poleZPosition - Parameters.WalkBeforePoles);
        _endMarker.transform.position = new Vector3(0f, crossHeight, _poleZPosition + Parameters.WalkAfterPoles);
        UI.WriteLineToExperimenterScreen("Stand and end floor cross positions set.");
    }
    #endregion

    #region EXPERIMENTMANAGEMENT
    private IEnumerator RunExperiment()
    {
        UI.TitleScreen.SetActive(false);
        CreateExperimentDirectory();

        yield return RunTrackerCalibration();
        yield return DisplayInstructions();
        yield return RunPracticeTrials();
        yield return RunExperimentBlocks();

        Application.Quit();
    }

    private void CreateExperimentDirectory()
    {
        while (Directory.Exists(string.Concat("Experiment", _dirNumber.ToString())))
            _dirNumber++;
        try
        {
            _workingSubDirectory = _mainDirectory.CreateSubdirectory(string.Concat("Experiment", _dirNumber.ToString()));
            UI.WriteLineToExperimenterScreen("Experiment directory is: Experiment" + _dirNumber);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("There was a problem trying to create a new subdirectory for the experiment.");
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
            Application.Quit();
        }
    }

    private IEnumerator RunTrackerCalibration()
    {
        UI.CalibrateScreen.SetActive(true);
        UI.ExperimenterTitle.text = "Tracker Calibration. C to calibrate, SPACE to continue.";
        yield return StartCoroutine(AssignTrackersWhenReady());
        while (!_spacebarDown) yield return null;
        UI.CalibrateScreen.SetActive(false);
    }

    private IEnumerator DisplayInstructions()
    {
        UI.ExperimenterTitle.text = "Give instructions to participant. SPACE to continue.";
        UI.InstructionsScreen.SetActive(true);
        // Wait a second so spacebar event not immediately triggered.
        yield return new WaitForSeconds(1);
        while (!_spacebarDown) yield return null;
        UI.InstructionsScreen.SetActive(false);
    }

    private IEnumerator RunPracticeTrials()
    {
        // Between blocks
        UI.BetweenBlocksText.text =
            "Advise the experimenter when you are ready to begin the practice trials.";
        UI.BetweenBlocksScreen.SetActive(true);
        UI.ExperimenterTitle.text = "When participant is ready, SPACE to start practice block.";
        // Wait a second so spacebar event not immediately triggered.
        yield return new WaitForSeconds(1);
        // Wait on ready screen until spacebar is pressed.
        while (!_spacebarDown) yield return null;
        UI.BetweenBlocksScreen.SetActive(false);

        UI.ExperimenterTitle.text = "Countdown";
        yield return RunCountDown("Starting practice trials in...");
        yield return RunExperimentBlock(0, Parameters.NumberOfPracticeTrials);
    }

    private IEnumerator RunExperimentBlocks()
    {
        for (var i = 0; i < Parameters.NumberOfBlocks; i++)
        {
            UI.ExperimenterTitle.text = "Rest before Experiment block" + (i + 1) + ". SPACE to begin.";
            UI.BetweenBlocksText.text =
                string.Concat(
                    "Have a rest, then advise the experimenter when you are ready to begin experiment block #",
                    (i + 1).ToString());
            UI.BetweenBlocksScreen.SetActive(true);

            yield return new WaitForSeconds(1);
            // Wait on ready screen until button one is held down.
            while (!_spacebarDown) yield return null;

            UI.BetweenBlocksScreen.SetActive(false);
            UI.ExperimenterTitle.text = "Countdown.";
            yield return RunCountDown("Starting experiment block in...");
            yield return RunExperimentBlock(i + 1, Parameters.NumberOfTrialsPerBlock);
        }
    }

    private static IEnumerator RunCountDown(string introText)
    {
        UI.ExperimenterTitle.text = "Countdown...";
        UI.CountDownNumberText.text = Parameters.CountDownDuration.ToString();
        UI.CountDownIntroText.text = introText;
        UI.CountDownScreen.SetActive(true);
        for (var i = Parameters.CountDownDuration; i > 0; i--)
        {
            UI.CountDownNumberText.GetComponent<Text>().text = i.ToString();
            yield return new WaitForSeconds(1);
        }
        UI.CountDownScreen.SetActive(false);
    }

    private IEnumerator RunExperimentBlock(int blockNumber, int numberOfTrials)
    {
        for (var i = 0; i < numberOfTrials; i++)
        {
            _currentTrial = i;
            _currentBlock = blockNumber;
            if (blockNumber != 0)
            {
                _currentAperture = _randomisedAtoBRatios[(blockNumber - 1) * numberOfTrials + i] * _bodyWidth;
            }
            else
            {
                _currentAperture = 1f;
            }
            yield return PrepareSingleTrial();
            yield return ExecuteSingleTrial();
            UI.DuringTrialScreen.SetActive(false);
        }

        var logFile = new FileInfo(Path.Combine(_workingSubDirectory.FullName, "log.txt"));
        logFile.WriteTextToFile(UI.ExperimenterText.text);

        var samplecount = _experimentBlockData.Buffer.Count;
        var blockDataFileName = string.Concat("experimentblock",_currentBlock,".csv");
        var fileExperimentData = new FileInfo(Path.Combine(_workingSubDirectory.FullName, blockDataFileName));
        yield return _experimentBlockData.FlushBuffer(fileExperimentData);
        UI.WriteLineToExperimenterScreen(samplecount + " poses written from buffer to file " + blockDataFileName);
    }

    private IEnumerator PrepareSingleTrial()
    {
        UI.ReturnToStartCrossScreen.SetActive(true);
        UI.ExperimenterTitle.text = "Block: " + _currentBlock + "Trial: " + _currentTrial + "Aperture: " + _currentAperture;
        UI.WriteLineToExperimenterScreen("Block: " + _currentBlock + "Trial: " + _currentTrial + "Aperture: " + _currentAperture);
        //Wait until experimenter hits spacebar to move poles.
        while (!_spacebarDown) yield return null;
        // Poles will be moved while participant facing away.
        if (!Parameters.PolesPositionedViaTracker)
        {
            _poleLeft.transform.position = new Vector3(-(_currentAperture + PoleDiameter) / 2, _poleLeft.transform.position.y, _poleLeft.transform.position.z);
            _poleRight.transform.position = new Vector3((_currentAperture + PoleDiameter) / 2, _poleRight.transform.position.y, _poleRight.transform.position.z);
        }
        UI.ReturnToStartCrossScreen.SetActive(false);
        UI.ReadyForNextTrialScreen.SetActive(true);
        yield return new WaitForSeconds(1);
        // Wait on ready screen until spacebar pressed by experimenter.
        while (!_spacebarDown) yield return null;
        UI.ReadyForNextTrialScreen.SetActive(false);
        UI.DuringTrialScreen.SetActive(true);
        _endColliderTouched = false;        
    }

    private IEnumerator ExecuteSingleTrial()
    {
        Timer.Reset();
        Timer.Start();
        _recordingData = true;
        UI.ExperimenterTitle.text = "Recording data to buffer.";
        while (!_endColliderTouched)
        {
            if (Timer.Elapsed.TotalMilliseconds > Parameters.TimeoutForTrial)
            {
                break;
            }

            yield return null;
        }

        _recordingData = false;
        Timer.Stop();
    }
    #endregion

    #region ASSIGNTRACKERS
    private IEnumerator AssignTrackersWhenReady()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.C) || (_rightHandIndex != -1 && RightController.GetHairTriggerDown()))
            {
                AssignTrackers();
                break;
            }
            yield return null;
        }
        yield return null;
    }


    public void AssignTrackers()
    {
        _trackedDeviceRoles.Clear();
        // Note: this only returns SteamVR_TrackedObjects in active children. Any tracker not being
        // used will be inactive.
        RefreshTrackedDevices();
        var devices = GetActiveDevices();
        IdentifyAndRemovePoleTrackers(devices);

        // Sort the (non-pole) devices from smallest position y value (height) to highest.
        // So hips will have lowest index, then shoulders, then hands.
        // Participant should be standing in a star pose, with hands raised higher
        // than their head. Stand on start cross, and face towards end cross.
        devices.Sort((a, b) => a.Value.y.CompareTo(b.Value.y));

        switch (devices.Count)
        {
            case 6:
                IdentifySixBodyTrackers(devices);
                break;
            case 4:
                IdentifyFourBodyTrackers(devices);
                break;
            default:
                Debug.Log("Need to rescan, should be 4 or 6 body trackers.");
                UI.WriteLineToExperimenterScreen("Need to rescan, should be 4 or 6 body trackers.");
                break;
        }
        
        SetPoleHeights();

        _bodyWidth = GetBodyWidth();
        if (_bodyWidth < 0)
        {
            //TODO: rescan prompt
            Debug.Log("Something went wrong calculating shoulder width. Recalibrate.");
            UI.WriteLineToExperimenterScreen("Something went wrong calculating shoulder width. Recalibrate.");
        }

        var strTrackers = _trackedDeviceRoles.Keys.Aggregate("", (current, item) => current + (item + ","));
        Debug.Log("bound body parts: " + strTrackers);
        UI.WriteLineToExperimenterScreen("Bound body parts: " + strTrackers);
        ReportTrackedBodyHeights();
    }

    private void ReportTrackedBodyHeights()
    {
        if (_trackedDeviceRoles.ContainsKey(DeviceRole.ShoulderLeft) && _trackedDeviceRoles.ContainsKey(DeviceRole.ShoulderRight))
        {
            var averageShoulderHeight =
            (_trackedDevices[_trackedDeviceRoles[DeviceRole.ShoulderLeft]].transform.position.y +
             _trackedDevices[_trackedDeviceRoles[DeviceRole.ShoulderRight]].transform.position.y) / 2;
            UI.WriteLineToExperimenterScreen("Shoulder Height: " + averageShoulderHeight);
        }

        if (_trackedDeviceRoles.ContainsKey(DeviceRole.HipLeft) && _trackedDeviceRoles.ContainsKey(DeviceRole.HipRight))
        {
            var averageHipHeight =
            (_trackedDevices[_trackedDeviceRoles[DeviceRole.ShoulderLeft]].transform.position.y +
             _trackedDevices[_trackedDeviceRoles[DeviceRole.ShoulderRight]].transform.position.y) / 2;
            UI.WriteLineToExperimenterScreen("Hip Height: " + averageHipHeight);
        }
    }

    private void RefreshTrackedDevices()
    {
        var trackedDevicesArray = CameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();
        _trackedDevices.Clear();
        foreach (var trackedObject in trackedDevicesArray)
        {
            _trackedDevices.Add(trackedObject.index, trackedObject);
        }
    }

    private List<KeyValuePair<SteamVR_TrackedObject.EIndex, Vector3>> GetActiveDevices()
    {
        var devices = new List<KeyValuePair<SteamVR_TrackedObject.EIndex, Vector3>>();
        for (var i = SteamVR_TrackedObject.EIndex.Device1; i < SteamVR_TrackedObject.EIndex.Device15; i++)
        {
            var device = SteamVR_Controller.Input((int) i);

            if (!device.hasTracking || !device.connected || !device.valid) continue;

            var deviceClass = Valve.VR.OpenVR.System.GetTrackedDeviceClass((uint) i);
            if (deviceClass == Valve.VR.ETrackedDeviceClass.Controller ||
                deviceClass == Valve.VR.ETrackedDeviceClass.GenericTracker)
            {
                devices.Add(new KeyValuePair<SteamVR_TrackedObject.EIndex, Vector3>(i, GetPose(i).Position));
                Debug.Log(i + ", type = " + deviceClass);
            }
            else
            {
                Debug.Log(i + " is a basestation, type = " + deviceClass);
            }
        }

        Debug.Log("deviceRole count = " + devices.Count);
        return devices;
    }

    private Pose GetPose(SteamVR_TrackedObject.EIndex index)
    {
        var trackedObject = _trackedDevices[index];
        var pose = new Pose { Position = trackedObject.transform.position, Rotation = trackedObject.transform.rotation };
        return pose;
    }

    private void IdentifyAndRemovePoleTrackers(List<KeyValuePair<SteamVR_TrackedObject.EIndex, Vector3>> devices)
    {
        var poleDevices = devices.FindAll(IsPoleTracker);
        if (poleDevices.Count != 2)
        {
            UI.WriteLineToExperimenterScreen("Two pole trackers were not found. Attemping to use parameter-specified pole positioning instead.");
            if (Parameters.ApertureToBodyRatios == null)
            {
                UI.WriteLineToExperimenterScreen("No parameter-specified aperture-to-body ratios found. Press Enter to exit...");
                Console.ReadLine();
                var logFile = new FileInfo(Path.Combine(_workingSubDirectory.FullName, "log.txt"));
                logFile.WriteTextToFile(UI.ExperimenterText.text);
                Application.Quit();
            }
            Parameters.PolesPositionedViaTracker = false;
        }
        else
        {
            if (poleDevices[0].Value.x < 0f)
                poleDevices.SwapPositions(0, 1);
            _trackedDeviceRoles[DeviceRole.PoleRight] = poleDevices[0].Key;
            _trackedDeviceRoles[DeviceRole.PoleLeft] = poleDevices[1].Key;
            _poleTrackersAssigned = true;
        }
        devices.RemoveAll(IsPoleTracker);
    }

    private static bool IsPoleTracker(KeyValuePair<SteamVR_TrackedObject.EIndex, Vector3> device)
    {
        // tolerance is how close a tracker's z position has to be to the nominal
        // pole z position to be regarded as a pole tracker not a body tracker.
        const float tolerance = 1f;
        return (Math.Abs(device.Value.z - _poleZPosition) < tolerance);
    }

    private void IdentifySixBodyTrackers(IList<KeyValuePair<SteamVR_TrackedObject.EIndex, Vector3>> devices)
    {
        // devices 0,1 are hips
        // devices 2,3 are shoulders
        // devices 4,5 are hand controllers
        if (devices[0].Value.x < 0f)
            devices.SwapPositions(0, 1);
        if (devices[2].Value.x < 0f)
            devices.SwapPositions(2, 3);
        if (devices[4].Value.x < 0f)
            devices.SwapPositions(4, 5);

        _trackedDeviceRoles[DeviceRole.HipRight] = devices[0].Key;
        _trackedDeviceRoles[DeviceRole.HipLeft] = devices[1].Key;
        _trackedDeviceRoles[DeviceRole.ShoulderRight] = devices[2].Key;
        _trackedDeviceRoles[DeviceRole.ShoulderLeft] = devices[3].Key;
        _trackedDeviceRoles[DeviceRole.HandRight] = devices[4].Key;
        _trackedDeviceRoles[DeviceRole.HandLeft] = devices[5].Key;

    }

    private void IdentifyFourBodyTrackers(IList<KeyValuePair<SteamVR_TrackedObject.EIndex, Vector3>> devices)
    {
        // devices 0,1 are either hips or shoulders
        // devices 2,3 are hand controllers
        if (devices[0].Value.x < 0f)
            devices.SwapPositions(0, 1);
        if (devices[2].Value.x < 0f)
            devices.SwapPositions(2, 3);

        _trackedDeviceRoles[DeviceRole.HandRight] = devices[2].Key;
        _trackedDeviceRoles[DeviceRole.HandLeft] = devices[3].Key;
        // if the hands are more than 1.5x as high as the other trackers, then the
        // other trackers are treated as hips. If not, treat as shoulders.
        if (devices[2].Value.y > 1.5f * devices[0].Value.y)
        {
            _trackedDeviceRoles[DeviceRole.HipRight] = devices[0].Key;
            _trackedDeviceRoles[DeviceRole.HipLeft] = devices[1].Key;
        }
        else
        {
            _trackedDeviceRoles[DeviceRole.ShoulderRight] = devices[0].Key;
            _trackedDeviceRoles[DeviceRole.ShoulderLeft] = devices[1].Key;
        }
    }

    private void SetPoleHeights()
    {
        switch (Parameters.PoleHeightPreset)
        {
            case PoleHeightSetting.Hip:
                if (!_trackedDeviceRoles.ContainsKey(DeviceRole.HipLeft))
                    SetDefaultPoleHeights();
                else
                {
                    _poleHeightRight = _poleHeightLeft =
                    (_trackedDevices[_trackedDeviceRoles[DeviceRole.HipLeft]].transform.position.y +
                     _trackedDevices[_trackedDeviceRoles[DeviceRole.HipRight]].transform.position.y) / 2f;
                }
                break;
            case PoleHeightSetting.Shoulder:
                if (!_trackedDeviceRoles.ContainsKey(DeviceRole.ShoulderLeft))
                    SetDefaultPoleHeights();
                else
                {
                    _poleHeightRight = _poleHeightLeft =
                    (_trackedDevices[_trackedDeviceRoles[DeviceRole.ShoulderLeft]].transform.position.y +
                     _trackedDevices[_trackedDeviceRoles[DeviceRole.ShoulderRight]].transform.position.y) / 2f;
                }
                break;
            case PoleHeightSetting.Preset:
                if (Parameters.PoleHeightPresetValue < 0.1f)
                    SetDefaultPoleHeights();
                else
                {
                    _poleHeightRight = _poleHeightLeft = Parameters.PoleHeightPresetValue;
                }
                break;
            case PoleHeightSetting.Tracker:
                if (!_trackedDeviceRoles.ContainsKey(DeviceRole.PoleLeft) ||
                    !_trackedDeviceRoles.ContainsKey(DeviceRole.PoleRight))
                    SetDefaultPoleHeights();
                else
                {
                    _poleHeightRight = _trackedDevices[_trackedDeviceRoles[DeviceRole.PoleRight]].transform.position.y;
                    _poleHeightLeft = _trackedDevices[_trackedDeviceRoles[DeviceRole.PoleLeft]].transform.position.y;
                }
                break;
        }
        _poleRight.transform.localScale = new Vector3(PoleDiameter, _poleHeightRight / 2f, PoleDiameter);
        _poleLeft.transform.localScale = new Vector3(PoleDiameter, _poleHeightLeft / 2f, PoleDiameter);
    }

    private void SetDefaultPoleHeights()
    {
        UI.WriteLineToExperimenterScreen(string.Format("Error determining pole heights. Using default value of {0}.", DefaultPoleHeight));
        _poleHeightRight = _poleHeightLeft = DefaultPoleHeight;
    }

    private float GetBodyWidth()
    {
        // Get shoulder width if shoulders are tracked, otherwise get hip
        // width.
        var result = -1f;
        if (_trackedDeviceRoles.ContainsKey(DeviceRole.ShoulderLeft) && _trackedDeviceRoles.ContainsKey(DeviceRole.ShoulderRight))
        {
            result = Vector3.Distance(
                _trackedDevices[_trackedDeviceRoles[DeviceRole.ShoulderLeft]].transform.position,
                _trackedDevices[_trackedDeviceRoles[DeviceRole.ShoulderRight]].transform.position);
        }
        else if (_trackedDeviceRoles.ContainsKey(DeviceRole.HipLeft) && _trackedDeviceRoles.ContainsKey(DeviceRole.HipRight))
        {
            result = Vector3.Distance(
                _trackedDevices[_trackedDeviceRoles[DeviceRole.HipLeft]].transform.position,
                _trackedDevices[_trackedDeviceRoles[DeviceRole.HipRight]].transform.position);
        }
        return result;
    }
    #endregion

    #region RECORDINGPOSEDATA
    private void RecordPosesDataToBuffer()
    {
        float currentActualAperture;

        if (_poleTrackersAssigned)
        {
            var rightPolePosition = new Vector2(_poleRight.transform.position.x, _poleRight.transform.position.z);
            var leftPolePostion = new Vector2(_poleLeft.transform.position.x, _poleLeft.transform.position.z);
            currentActualAperture = Vector2.Distance(rightPolePosition, leftPolePostion);
        }
        else
        {
            currentActualAperture = _currentAperture;
        }

        var elapsedTotalMilliseconds = Timer.Elapsed.TotalMilliseconds;

        if (Parameters.TrackHmd)
        {
            var device = GameObject.Find("Camera (eye)");
            _experimentBlockData.Buffer.Add(CreateDataSampleFromDevice(device, DeviceRole.Head, currentActualAperture,
                elapsedTotalMilliseconds));
        }
        if (Parameters.TrackLeftHand && _trackedDeviceRoles.ContainsKey(DeviceRole.HandLeft))
        {
            var device = GetTrackedObjectByRole(DeviceRole.HandLeft).gameObject;
            _experimentBlockData.Buffer.Add(CreateDataSampleFromDevice(device, DeviceRole.HandLeft, currentActualAperture,
                elapsedTotalMilliseconds));
        }
        if (Parameters.TrackRightHand && _trackedDeviceRoles.ContainsKey(DeviceRole.HandRight))
        {
            var device = GetTrackedObjectByRole(DeviceRole.HandRight).gameObject;
            _experimentBlockData.Buffer.Add(CreateDataSampleFromDevice(device, DeviceRole.HandRight, currentActualAperture,
                elapsedTotalMilliseconds));
        }
        if (Parameters.TrackLeftHip && _trackedDeviceRoles.ContainsKey(DeviceRole.HipLeft))
        {
            var device = GetTrackedObjectByRole(DeviceRole.HipLeft).gameObject;
            _experimentBlockData.Buffer.Add(CreateDataSampleFromDevice(device, DeviceRole.HipLeft, currentActualAperture,
                elapsedTotalMilliseconds));
        }
        if (Parameters.TrackRightHip && _trackedDeviceRoles.ContainsKey(DeviceRole.HipRight))
        {
            var device = GetTrackedObjectByRole(DeviceRole.HipRight).gameObject;
            _experimentBlockData.Buffer.Add(CreateDataSampleFromDevice(device, DeviceRole.HipRight, currentActualAperture,
                elapsedTotalMilliseconds));
        }
        if (Parameters.TrackLeftShoulder && _trackedDeviceRoles.ContainsKey(DeviceRole.ShoulderLeft))
        {
            var device = GetTrackedObjectByRole(DeviceRole.ShoulderLeft).gameObject;
            _experimentBlockData.Buffer.Add(CreateDataSampleFromDevice(device, DeviceRole.ShoulderLeft, currentActualAperture,
                elapsedTotalMilliseconds));
        }
        if (Parameters.TrackRightShoulder && _trackedDeviceRoles.ContainsKey(DeviceRole.ShoulderRight))
        {
            var device = GetTrackedObjectByRole(DeviceRole.ShoulderRight).gameObject;
            _experimentBlockData.Buffer.Add(CreateDataSampleFromDevice(device, DeviceRole.ShoulderRight, currentActualAperture,
                elapsedTotalMilliseconds));
        }
    }

    private SteamVR_TrackedObject GetTrackedObjectByRole(DeviceRole role)
    {
        return _trackedDevices[_trackedDeviceRoles[role]];
    }

    private DataSample CreateDataSampleFromDevice(GameObject device, DeviceRole role, float currentActualAperture, double elapsedTotalMilliseconds)
    {
        return new DataSample(role,
            _currentBlock,
            _currentTrial, _bodyWidth,
            currentActualAperture, _poleLeft.transform, _poleRight.transform,
            elapsedTotalMilliseconds,
            new Pose { Position = device.transform.position, Rotation = device.transform.rotation });
    }
    #endregion

    #region OTHER
    public void EndPointReached()
    {
        _endColliderTouched = true;
    }

    #endregion
}