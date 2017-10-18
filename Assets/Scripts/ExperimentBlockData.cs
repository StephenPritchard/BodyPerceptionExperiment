using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ExperimentBlockData
{
    public readonly List<DataSample> Buffer = new List<DataSample>();

    public IEnumerator FlushBuffer(FileInfo fileExperimentData)
    {
        UI.ExperimenterTitle.text = "Writing buffer to file";
        const int samplesWrittenPerFrame = 15;

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
        Buffer.Clear();
    }
}