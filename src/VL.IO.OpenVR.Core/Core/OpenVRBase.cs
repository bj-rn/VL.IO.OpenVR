using Stride.Core.Mathematics;
using System.Text;
using System.Xml.Linq;
using Valve.VR;
using static Valve.VR.IVRIOBuffer;


namespace VL.IO.ValveOpenVR
{
    public abstract class OpenVRBase
    {
        
        private String error;
        public string Error { get => error; }

        public abstract void Update();

        protected void SetStatus(object toString)
        {
            if (toString is EVRInitError)
                error = OpenVR.GetStringForHmdError((EVRInitError)toString);
            else if (toString is EVRCompositorError)
            {
                var e = (EVRCompositorError)toString;

                if (e == EVRCompositorError.TextureIsOnWrongDevice)
                    error = "Texture on wrong device. Set your graphics driver to use the same video card for vvvv as the headset is plugged into.";
                else if (e == EVRCompositorError.TextureUsesUnsupportedFormat)
                    error = "Unsupported texture format. Make sure texture uses RGBA, is not compressed and has no mipmaps.";
                else
                    error = e.ToString();
            }
            else
                error = toString.ToString();
        }
    }


    public abstract class OpenVRConsumerBase : OpenVRBase
    {
        protected bool _firstFrame = true;

        protected CVRSystem _system;


        protected bool _refreshSerials = false;
        public bool RefreshSerials { set => _refreshSerials = value; }


        public void Update(CVRSystem system)
        {
            if (system != null)
            {
               _system = system;
               Update();
               _firstFrame = false;
            }
            else
            {
                SetStatus("OpenVR is not initialized");
            }
            
        }

        protected void GetPose(TrackedDevicePose_t devicePose, out Matrix pose, out Vector3 velocity, out Vector3 angularVelocity) 
        {
            pose = devicePose.mDeviceToAbsoluteTracking.ToMatrix();
            velocity = devicePose.vVelocity.ToVector3();
            angularVelocity = devicePose.vAngularVelocity.ToVector3();
        }

        protected float BatteryPercentage(uint index)
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

        const int CSerialBuilderSize = 64;
        protected StringBuilder _serialBuilder = new StringBuilder(CSerialBuilderSize);
    }

}
