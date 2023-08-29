using Valve.VR;
using Stride.Core.Mathematics;
using System.Runtime.Intrinsics.X86;

namespace VL.IO.ValveOpenVR
{
    public class OpenVRTrackedDeviceSplit: OpenVRConsumerBase
    {
        public OpenVRController.Device Controller
        {
            get;
            set;
        }

        private Matrix _pose = Matrix.Identity;
        public Matrix Pose
        {
            get => _pose;
        }

        private Vector3 _velocity;
        public Vector3 Velocity
        {
            get => _velocity;
        }

        private Vector3 _angularVelocity;
        public Vector3 AngularVelocity
        {
            get => _angularVelocity;
        }

        public int DeviceIndex
        {
            get;
            private set;
        }

        public string DeviceSerial
        {
            get;
            private set;
        }

        public ETrackedControllerRole DeviceRole
        {
            get;
            private set;
        }

        public ETrackedDeviceClass DeviceClass
        {
            get;
            private set;
        }

        public bool TriggerTouch
        {
            get;
            private set;
        }

        public bool TriggerPress
        {
            get;
            private set;
        }

        public double TriggerAxis
        {
            get;
            private set;
        }

        public bool TouchpadTouch
        {
            get;
            private set;
        }

        public bool TouchpadPress
        {
            get;
            private set;
        }

        public Vector2 TouchpadAxis
        {
            get;
            private set;
        }

        public bool SystemPress
        {
            get;
            private set;
        }

        public bool ApplicationMenuPress
        {
            get;
            private set;
        }

        public bool GripPress
        {
            get;
            private set;
        }

        public float BatteryPercentage
        {
            get;
            private set;
        }

        public bool Valid
        {
            get;
            private set;
        }

        public bool Connected
        {
            get;
            private set;
        }

        public bool HasTracking
        {
            get;
            private set;
        }

        public bool OutOfRange
        {
            get;
            private set;
        }

        public bool Calibrating
        {
            get;
            private set;
        }

        public bool Uninitialized
        {
            get;
            private set;
        }

        public OpenVRTrackedDeviceSplit() 
        {
            DeviceSerial = "";
            BatteryPercentage = 0.0f;
        }

        public override void Update() 
        {
            
            if (Controller == null || OpenVRManager.GamePoses == null)
                return;
            
            var devicePose = OpenVRManager.GamePoses[Controller.index];

            GetPose(devicePose, out _pose, out _velocity, out _angularVelocity);
            
            DeviceIndex = (int)Controller.index;

            DeviceSerial = GetSerial(DeviceIndex);
            
            
            DeviceRole = _system.GetControllerRoleForTrackedDeviceIndex(Controller.index);
            DeviceClass = _system.GetTrackedDeviceClass(Controller.index);

            TriggerTouch = Controller.GetTouch(OpenVRController.ButtonMask.Trigger);
            TriggerPress = Controller.GetPress(OpenVRController.ButtonMask.Trigger);
            TriggerAxis = Controller.hairTriggerValue;


            TouchpadTouch = Controller.GetTouch(OpenVRController.ButtonMask.Touchpad);
            TouchpadPress = Controller.GetPress(OpenVRController.ButtonMask.Touchpad);
            TouchpadAxis = Controller.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);

            SystemPress = Controller.GetPress(OpenVRController.ButtonMask.System);
            ApplicationMenuPress = Controller.GetPress(OpenVRController.ButtonMask.ApplicationMenu);
            GripPress = Controller.GetPress(OpenVRController.ButtonMask.Grip);

            BatteryPercentage = GetBatteryPercentage(Controller.index);

            Valid = Controller.valid;
            Connected = Controller.connected;
            HasTracking = Controller.hasTracking;
            OutOfRange = Controller.outOfRange;
            Calibrating = Controller.calibrating;
            Uninitialized = Controller.uninitialized;
        }

    }
}
