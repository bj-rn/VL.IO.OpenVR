using Stride.Core.Mathematics;
using System.Text;
using Valve.VR;


namespace VL.IO.ValveOpenVR
{
    public abstract class OpenVRBase
    {
        public string Error
        {
            get;
            private set;
        }

        public abstract void Update();

        protected void SetStatus(object toString)
        {
            if (toString is EVRInitError)
                Error = OpenVR.GetStringForHmdError((EVRInitError)toString);
            else if (toString is EVRCompositorError)
            {
                var e = (EVRCompositorError)toString;

                if (e == EVRCompositorError.TextureIsOnWrongDevice)
                    Error = "Texture on wrong device. Set your graphics driver to use the same video card for vvvv as the headset is plugged into.";
                else if (e == EVRCompositorError.TextureUsesUnsupportedFormat)
                    Error = "Unsupported texture format. Make sure texture uses RGBA, is not compressed and has no mipmaps.";
                else
                    Error = e.ToString();
            }
            else
                Error = toString.ToString();
        }
    }


    public abstract class OpenVRConsumerBase : OpenVRBase
    {
        protected bool _firstFrame = true;

        protected CVRSystem _system;

        public bool RefreshSerials
        {
            get;
            set;
        }


        const int CSerialBuilderSize = 64;
        protected StringBuilder _serialBuilder = new StringBuilder(CSerialBuilderSize);


        public void UpdateSystem()
        {
            var system = OpenVRManager.System;
            
            if (system != null)
            {
               _system = system;
               Update();
               _firstFrame = false;
            }
            else
            {
               SetStatus("OpenVR is not initialized, a System node needs to be present in your patch");
            }
            
        }

        protected void GetPose(TrackedDevicePose_t devicePose, out Matrix pose, out Vector3 velocity, out Vector3 angularVelocity) 
        {
            pose = devicePose.mDeviceToAbsoluteTracking.ToMatrix();
            velocity = devicePose.vVelocity.ToVector3();
            angularVelocity = devicePose.vAngularVelocity.ToVector3();
        }

        protected float GetBatteryPercentage(uint index)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var value = _system.GetFloatTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float, ref error);

            if (error == ETrackedPropertyError.TrackedProp_Success)
                return value;
            else
                return 0;
        }

        protected string GetSerial(int i)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            _serialBuilder.Clear();

            _system.GetStringTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_SerialNumber_String, _serialBuilder, CSerialBuilderSize, ref error);

            if (error == ETrackedPropertyError.TrackedProp_Success)
                return _serialBuilder.ToString();
            else
                return "";
        }
    }

}
