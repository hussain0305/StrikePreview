using UnityEngine;
using System;
using System.Collections.Generic;

public class InGameContext : IContextProvider
{
    public Transform GetAimTransform()
    {
        return Game.AngleInput.cylinderPivot;
    }

    public float GetLaunchForce()
    {
        return Game.LaunchForce;
    }

    public Quaternion GetLaunchAngle()
    {
        return Game.LaunchAngle;
    }

    public Vector2 GetSpinVector()
    {
        return Game.SpinVector;
    }

    public void SetBallState(BallState newState)
    {
        GameManager.BallState = newState;
    }

    public PinBehaviourPerTurn GetPinResetBehaviour()
    {
        return Game.pinBehaviour;
    }

    public Tee GetTee()
    {
        return Game.tee;
    }

    public int GetTrajectoryDefinition()
    {
        return Game.TRAJECTORY_DEFINITION;
    }

    public float GetGravity()
    {
        return Game.Gravity;
    }
}