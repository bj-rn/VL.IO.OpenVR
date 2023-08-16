﻿//========= Copyright 2015, Valve Corporation, All rights reserved. ===========
//
// Purpose: Wrapper for working with SteamVR controller input
//
// Example usage:
//
//	var deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
//	if (deviceIndex != -1 && SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
//		SteamVR_Controller.Input(deviceIndex).TriggerHapticPulse(1000);
//
//=============================================================================

using Valve.VR;
using Stride.Core.Mathematics;
using System.Runtime.InteropServices;

namespace VL.IO.ValveOpenVR
{
    public class OpenVRController
    {
        public class ButtonMask
        {
            public const ulong System = (1ul << (int)EVRButtonId.k_EButton_System); // reserved
            public const ulong ApplicationMenu = (1ul << (int)EVRButtonId.k_EButton_ApplicationMenu);
            public const ulong Grip = (1ul << (int)EVRButtonId.k_EButton_Grip);
            public const ulong Axis0 = (1ul << (int)EVRButtonId.k_EButton_Axis0);
            public const ulong Axis1 = (1ul << (int)EVRButtonId.k_EButton_Axis1);
            public const ulong Axis2 = (1ul << (int)EVRButtonId.k_EButton_Axis2);
            public const ulong Axis3 = (1ul << (int)EVRButtonId.k_EButton_Axis3);
            public const ulong Axis4 = (1ul << (int)EVRButtonId.k_EButton_Axis4);
            public const ulong Touchpad = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad);
            public const ulong Trigger = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Trigger);
        }

        public class Device
        {
            public Device(uint i) { index = i; }
            public uint index { get; private set; }

            public bool valid { get; private set; }
            public bool connected { get { UpdateInternal(); return pose.bDeviceIsConnected; } }
            public bool hasTracking { get { UpdateInternal(); return pose.bPoseIsValid; } }

            public bool outOfRange { get { UpdateInternal(); return pose.eTrackingResult == ETrackingResult.Running_OutOfRange || pose.eTrackingResult == ETrackingResult.Calibrating_OutOfRange; } }
            public bool calibrating { get { UpdateInternal(); return pose.eTrackingResult == ETrackingResult.Calibrating_InProgress || pose.eTrackingResult == ETrackingResult.Calibrating_OutOfRange; } }
            public bool uninitialized { get { UpdateInternal(); return pose.eTrackingResult == ETrackingResult.Uninitialized; } }

            // These values are only accurate for the last controller state change (e.g. trigger release), and by definition, will always lag behind
            // the predicted visual poses that drive SteamVR_TrackedObjects since they are sync'd to the input timestamp that caused them to update.
            public Matrix transform { get { UpdateInternal(); return pose.mDeviceToAbsoluteTracking.ToMatrix(); } }
            public Vector3 velocity { get { UpdateInternal(); return new Vector3(pose.vVelocity.v0, pose.vVelocity.v1, -pose.vVelocity.v2); } }
            public Vector3 angularVelocity { get { UpdateInternal(); return new Vector3(-pose.vAngularVelocity.v0, -pose.vAngularVelocity.v1, pose.vAngularVelocity.v2); } }

            public VRControllerState_t GetState() { UpdateInternal(); return state; }
            public VRControllerState_t GetPrevState() { UpdateInternal(); return prevState; }
            public TrackedDevicePose_t GetPose() { UpdateInternal(); return pose; }

            VRControllerState_t state, prevState;
            TrackedDevicePose_t pose;
            int frameCount = 0;
            int prevFrameCount = -1;

            public void Update(int frame)
            {
                frameCount = frame;
                UpdateInternal();
            }

            public void UpdateInternal()
            {
                
                if (frameCount != prevFrameCount)
                {
                    prevFrameCount = frameCount;
                    prevState = state;

                    var system = OpenVR.System;
                    if (system != null)
                    {
                        valid = system.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseSeated, index, ref state, (uint)Marshal.SizeOf(typeof(VRControllerState_t)), ref pose);
                        UpdateHairTrigger();
                    }
                }
            }

            public bool GetPress(ulong buttonMask) { UpdateInternal(); return (state.ulButtonPressed & buttonMask) != 0; }
            public bool GetPressDown(ulong buttonMask) { UpdateInternal(); return (state.ulButtonPressed & buttonMask) != 0 && (prevState.ulButtonPressed & buttonMask) == 0; }
            public bool GetPressUp(ulong buttonMask) { UpdateInternal(); return (state.ulButtonPressed & buttonMask) == 0 && (prevState.ulButtonPressed & buttonMask) != 0; }

            public bool GetPress(EVRButtonId buttonId) { return GetPress(1ul << (int)buttonId); }
            public bool GetPressDown(EVRButtonId buttonId) { return GetPressDown(1ul << (int)buttonId); }
            public bool GetPressUp(EVRButtonId buttonId) { return GetPressUp(1ul << (int)buttonId); }

            public bool GetTouch(ulong buttonMask) { UpdateInternal(); return (state.ulButtonTouched & buttonMask) != 0; }
            public bool GetTouchDown(ulong buttonMask) { UpdateInternal(); return (state.ulButtonTouched & buttonMask) != 0 && (prevState.ulButtonTouched & buttonMask) == 0; }
            public bool GetTouchUp(ulong buttonMask) { UpdateInternal(); return (state.ulButtonTouched & buttonMask) == 0 && (prevState.ulButtonTouched & buttonMask) != 0; }

            public bool GetTouch(EVRButtonId buttonId) { return GetTouch(1ul << (int)buttonId); }
            public bool GetTouchDown(EVRButtonId buttonId) { return GetTouchDown(1ul << (int)buttonId); }
            public bool GetTouchUp(EVRButtonId buttonId) { return GetTouchUp(1ul << (int)buttonId); }

            public Vector2 GetAxis(EVRButtonId buttonId = EVRButtonId.k_EButton_SteamVR_Touchpad)
            {
                UpdateInternal();
                var axisId = (uint)buttonId - (uint)EVRButtonId.k_EButton_Axis0;
                switch (axisId)
                {
                    case 0: return new Vector2(state.rAxis0.x, state.rAxis0.y);
                    case 1: return new Vector2(state.rAxis1.x, state.rAxis1.y);
                    case 2: return new Vector2(state.rAxis2.x, state.rAxis2.y);
                    case 3: return new Vector2(state.rAxis3.x, state.rAxis3.y);
                    case 4: return new Vector2(state.rAxis4.x, state.rAxis4.y);
                }
                return Vector2.Zero;
            }

            public void TriggerHapticPulse(ushort durationMicroSec = 500, EVRButtonId buttonId = EVRButtonId.k_EButton_SteamVR_Touchpad)
            {
                var system = OpenVR.System;
                if (system != null)
                {
                    var axisId = (uint)buttonId - (uint)EVRButtonId.k_EButton_Axis0;
                    system.TriggerHapticPulse(index, axisId, (char)durationMicroSec);
                }
            }

            public float hairTriggerDelta = 0.1f; // amount trigger must be pulled or released to change state
            public float hairTriggerValue = 0;
            float hairTriggerLimit;
            bool hairTriggerState, hairTriggerPrevState;
            void UpdateHairTrigger()
            {
                hairTriggerPrevState = hairTriggerState;
                var value = state.rAxis1.x; // trigger
                hairTriggerValue = value;
                if (hairTriggerState)
                {
                    if (value < hairTriggerLimit - hairTriggerDelta || value <= 0.0f)
                        hairTriggerState = false;
                }
                else
                {
                    if (value > hairTriggerLimit + hairTriggerDelta || value >= 1.0f)
                        hairTriggerState = true;
                }
                hairTriggerLimit = hairTriggerState ? Math.Max(hairTriggerLimit, value) : Math.Min(hairTriggerLimit, value);
               
            }

            public bool GetHairTrigger() { UpdateInternal(); return hairTriggerState; }
            public bool GetHairTriggerDown() { UpdateInternal(); return hairTriggerState && !hairTriggerPrevState; }
            public bool GetHairTriggerUp() { UpdateInternal(); return !hairTriggerState && hairTriggerPrevState; }
        }

        private static Device[] devices;

        public static Device Input(int deviceIndex)
        {
            if (devices == null)
            {
                devices = new Device[OpenVR.k_unMaxTrackedDeviceCount];
                for (uint i = 0; i < devices.Length; i++)
                    devices[i] = new Device(i);
            }

            return devices[deviceIndex];
        }

        public static void Update(int frame)
        {
            for (int i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
                Input(i).Update(frame);
        }

        // This helper can be used in a variety of ways.  Beware that indices may change
        // as new devices are dynamically added or removed, controllers are physically
        // swapped between hands, arms crossed, etc.
        public enum DeviceRelation
        {
            First,
            // radially
            Leftmost,
            Rightmost,
            // distance - also see vr.hmd.GetSortedTrackedDeviceIndicesOfClass
            FarthestLeft,
            FarthestRight,
        }
        public static int GetDeviceIndex(DeviceRelation relation,
            ETrackedDeviceClass deviceClass = ETrackedDeviceClass.Controller,
            int relativeTo = (int)OpenVR.k_unTrackedDeviceIndex_Hmd) // use -1 for absolute tracking space
        {
            var result = -1;

            var invXform = ((uint)relativeTo < OpenVR.k_unMaxTrackedDeviceCount) ?
                Matrix.Invert(Input(relativeTo).transform) : Matrix.Identity;

            var system = OpenVR.System;
            if (system == null)
                return result;

            var best = -float.MaxValue;
            for (int i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (i == relativeTo || system.GetTrackedDeviceClass((uint)i) != deviceClass)
                    continue;

                var device = Input(i);
                if (!device.connected)
                    continue;

                if (relation == DeviceRelation.First)
                    return i;

                float score;

                var pos = Vector3.Transform(device.transform.Pos(), invXform);
                if (relation == DeviceRelation.FarthestRight)
                {
                    score = pos.X;
                }
                else if (relation == DeviceRelation.FarthestLeft)
                {
                    score = -pos.X;
                }
                else
                {
                    var dir = Vector3.Normalize(new Vector3(pos.X, 0.0f, pos.Z));
                    var dot = Vector3.Dot(dir, Vector3.UnitZ);
                    var cross = Vector3.Cross(dir, Vector3.UnitZ);
                    if (relation == DeviceRelation.Leftmost)
                    {
                        score = (cross.Y > 0.0f) ? 2.0f - dot : dot;
                    }
                    else
                    {
                        score = (cross.Y < 0.0f) ? 2.0f - dot : dot;
                    }
                }

                if (score > best)
                {
                    result = i;
                    best = score;
                }
            }

            return result;
        }
    }
}
