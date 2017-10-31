using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

public class IOTesting : MonoBehaviour
{
    private readonly Stopwatch _sw = new Stopwatch();
    private readonly ExperimentBlockData _data = new ExperimentBlockData();

    // Use this for initialization
    private void Start ()
    {

        var pose = new Pose(Vector3.back, Quaternion.identity);
        _sw.Start();
        FillBuffer(pose);
        _sw.Stop();
        var msg = string.Format("time to make data: {0}", _sw.ElapsedMilliseconds);
        UnityEngine.Debug.Log(msg);
        _sw.Reset();
    }

    private void FillBuffer(Pose pose)
    {
        for (var i = 0; i < 100000; i++)
        {
            _data.Buffer.Add(new DataSample(DeviceRole.HandLeft, 1, 1, 0.5f, 0.5f, 0.5f, 0.5f, gameObject.transform,
                gameObject.transform, 1.3D, pose));
        }
    }

    // Update is called once per frame
    private void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(_data.FlushBuffer(new FileInfo("test.csv")));
        }
    }
}
