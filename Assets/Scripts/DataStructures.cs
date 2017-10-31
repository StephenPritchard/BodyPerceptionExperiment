using System;
using UnityEngine;

public enum DeviceRole
{
    HandRight,
    HandLeft,
    HipLeft,
    HipRight,
    ShoulderLeft,
    ShoulderRight,
    Head,
    PoleLeft,
    PoleRight
}

public enum PoleHeightSetting
{
    Hip,
    Shoulder,
    Preset,
    Tracker
}


public struct Pose
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;

    public Pose(Vector3 position, Quaternion rotation) : this()
    {
        Position = position;
        Rotation = rotation;
    }
}


public struct DataSample
{
    public readonly DeviceRole DeviceRole;
    public readonly int Block;
    public readonly int Trial;
    public readonly float ShoulderWidth;
    public readonly float HipWidth;
    public readonly float ActualAperture;
    public readonly float AtoSRatio;
    public readonly Transform LeftPoleTransform;
    public readonly Transform RightPoleTransform;
    public readonly double Time;
    public readonly Pose Pose;

    public DataSample(DeviceRole deviceRole, int block, int trial, float shoulderWidth, float hipWidth, float aperture, float aTos,
        Transform leftPoleTransform, Transform rightPoleTransform, double time, 
                        Pose pose) : this()
    {
        DeviceRole = deviceRole;
        Block = block;
        Trial = trial;
        ShoulderWidth = shoulderWidth;
        HipWidth = hipWidth;
        ActualAperture = aperture;
        AtoSRatio = aTos;
        LeftPoleTransform = leftPoleTransform;
        RightPoleTransform = rightPoleTransform;
        Time = time;
        Pose = pose;
    }

    public override string ToString()
    {
        float transformedYRot;
        if (Pose.Rotation.eulerAngles.y <= 180)
            transformedYRot = Pose.Rotation.eulerAngles.y;
        else
            transformedYRot = 180 - Pose.Rotation.eulerAngles.y;

        return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}",
            DeviceRole,
            Block,
            Trial,
            ShoulderWidth,
            HipWidth,
            ActualAperture,
            AtoSRatio,
            LeftPoleTransform.position.x,
            LeftPoleTransform.position.y,
            LeftPoleTransform.position.z,
            RightPoleTransform.position.x,
            RightPoleTransform.position.y,
            RightPoleTransform.position.z,
            Time,
            Pose.Position.x,
            Pose.Position.y,
            Pose.Position.z,
            Pose.Rotation.eulerAngles.x,
            transformedYRot,
            Pose.Rotation.eulerAngles.z);
    }
}