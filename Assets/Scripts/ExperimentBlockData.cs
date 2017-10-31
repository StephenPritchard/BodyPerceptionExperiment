using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

public class ExperimentBlockData
{
    private readonly Stopwatch _sw = new Stopwatch();
    public readonly List<DataSample> Buffer = new List<DataSample>();

    public IEnumerator FlushBuffer(FileInfo fileExperimentData)
    {
        _sw.Start();
        UI.ExperimenterTitle.text = "Writing buffer to file";
        const int samplesWrittenPerFrame = 1000;

        var samplecount = 0;
        using (var streamW = fileExperimentData.StreamToWriteFile())
        {
            streamW.WriteLine(Parameters.BodyWidthByShoulder
                ? "DEVICE,BLOCK,TRIAL,SHOULDERWIDTH*,HIPWIDTH,APERTURE,INTENDED_ATOS, LEFTPOLE_X,LEFTPOLE_Y,LEFTPOLE_Z,RIGHTPOLE_X,RIGHTPOL_Y,RIGHTPOLE_Z,TIME,XPOS,YPOS,ZPOS,XROT,YROT,ZROT"
                : "DEVICE,BLOCK,TRIAL,SHOULDERWIDTH,HIPWIDTH*,APERTURE,INTENDED_ATOS,LEFTPOLE_X,LEFTPOLE_Y,LEFTPOLE_Z,RIGHTPOLE_X,RIGHTPOL_Y,RIGHTPOLE_Z,TIME,XPOS,YPOS,ZPOS,XROT,YROT,ZROT");

            foreach (var sample in Buffer)
            {
                streamW.WriteLine(sample.ToString());
                samplecount++;
                if (samplecount % samplesWrittenPerFrame == 0)
                    yield return null;
            }
        }
        _sw.Stop();
        var msg = string.Format("time to save data: {0}", _sw.ElapsedMilliseconds);
        UnityEngine.Debug.Log(msg);
        _sw.Reset();
        _sw.Start();
        Buffer.Clear();
        _sw.Stop();
        msg = string.Format("time to clear buffer: {0}", _sw.ElapsedMilliseconds);
        UnityEngine.Debug.Log(msg);
        _sw.Reset();
    }
}