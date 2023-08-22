using Valve.VR;
using Stride.Core.Mathematics;

namespace VL.IO.ValveOpenVR
{
    public class OpenVRTrackedDeviceSplit: OpenVRConsumerBase
    {

        private OpenVRController.Device _controller;
        public OpenVRController.Device Controller { set => _controller = value; }
        

        private Matrix _pose = Matrix.Identity;
        public Matrix Pose { get => _pose; }

        private Vector3 _velocity;
        public Vector3 Velocity { get => _velocity; }

        private Vector3 _angularVelocity;
        public Vector3 AngularVelocity { get => _angularVelocity; }

        private int _deviceIndex = -1;
        public int DeviceIndex { get => _deviceIndex; }


        private string _deviceSerial = "";
        public string DeviceSerial { get => _deviceSerial; }

        private ETrackedControllerRole _deviceRole;
        public ETrackedControllerRole DeviceRole { get => _deviceRole; }


        private ETrackedDeviceClass _deviceClass;
        public ETrackedDeviceClass DeviceClass { get => _deviceClass; }

        private bool _triggerTouch;
        public bool TriggerTouch { get => _triggerTouch; }
        
        
        private bool _triggerPress;
        public bool TriggerPress { get => _triggerPress; }

        private double _triggerAxis;
        public double TriggerAxis { get => _triggerAxis; }


        private bool _touchpadTouch;
        public bool TouchpadTouch { get => _touchpadTouch; }

        private bool _touchpadPress;
        public bool TouchpadPress { get => _touchpadPress; }

        private Vector2 _touchpadAxis;
        public Vector2 TouchpadAxis { get => _touchpadAxis; }


        private bool _systemPress;
        public bool SystemPress { get => _systemPress; }

        private bool _applicationMenuPress;
        public bool ApplicationMenuPress { get => _applicationMenuPress; }

        private bool _gripPress;
        public bool GripPress { get => _gripPress; }


        private float _batteryPercentage = 0.0f;
        public float BatteryPercentage { get => _batteryPercentage; }



        private bool _valid;
        public bool Valid { get => _valid; }

        private bool _connected;
        public bool Connected { get => _connected; }

        private bool _hasTracking;
        public bool HasTracking { get => _hasTracking; }

        private bool  _outOfRange;
        public bool OutOfRange { get => _outOfRange; }

        private bool _calibrating;
        public bool Calibrating { get => _calibrating; }

        private bool _uninitialized;
        public bool Uninitialized { get => _uninitialized; }


        public override void Update() 
        {
            
            if (_controller == null || OpenVRManager.GamePoses == null)
                return;
            
            var devicePose = OpenVRManager.GamePoses[_controller.index];

            GetPose(devicePose, out _pose, out _velocity, out _angularVelocity);
            
            _deviceIndex = (int)_controller.index;

            _deviceSerial = GetSerial(_deviceIndex);
            
            
            _deviceRole = _system.GetControllerRoleForTrackedDeviceIndex(_controller.index);
            _deviceClass = _system.GetTrackedDeviceClass(_controller.index);

            _triggerTouch = _controller.GetTouch(OpenVRController.ButtonMask.Trigger);
            _triggerPress = _controller.GetPress(OpenVRController.ButtonMask.Trigger);
            _triggerAxis = _controller.hairTriggerValue;


            _touchpadTouch = _controller.GetTouch(OpenVRController.ButtonMask.Touchpad);
            _touchpadPress = _controller.GetPress(OpenVRController.ButtonMask.Touchpad);
            _touchpadAxis = _controller.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);

            _systemPress = _controller.GetPress(OpenVRController.ButtonMask.System);
            _applicationMenuPress = _controller.GetPress(OpenVRController.ButtonMask.ApplicationMenu);
            _gripPress = _controller.GetPress(OpenVRController.ButtonMask.Grip);

            _batteryPercentage = GetBatteryPercentage(_controller.index);

            _valid = _controller.valid;
            _connected = _controller.connected;
            _hasTracking = _controller.hasTracking;
            _outOfRange = _controller.outOfRange;
            _calibrating = _controller.calibrating;
            _uninitialized = _controller.uninitialized;
        }

    }
}
