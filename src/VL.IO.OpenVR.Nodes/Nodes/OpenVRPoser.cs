using System.Text;
using Valve.VR;
using Stride.Core.Mathematics;
using VL.Lib.Collections;


namespace VL.IO.ValveOpenVR
{
    public class OpenVRPoser : OpenVRConsumerBase
    {
        
        private bool _refreshSerials = false;
        public bool RefreshSerials { set => _refreshSerials = value; }

        private Matrix _hmdPose;
        public Matrix HMDPose { get => _hmdPose; }
        

        private SpreadBuilder<Matrix> _lighthousePoses;
        public Spread<Matrix> LighthousePoses { get => _lighthousePoses.ToSpread(); }
        

        private SpreadBuilder<Matrix> _controllerPoses;
        public Spread<Matrix> ControllerPoses { get => _controllerPoses.ToSpread(); }


        private SpreadBuilder<Matrix> _trackerPoses;
        public Spread<Matrix> TrackerPoses { get => _trackerPoses.ToSpread(); }


        private SpreadBuilder<Matrix> _renderPoses;
        public Spread<Matrix> RenderPoses { get => _renderPoses.ToSpread(); }


        private SpreadBuilder<Matrix> _gamePoses;
        public Spread<Matrix> GamePoses { get => _gamePoses.ToSpread(); }


        private SpreadBuilder<string> _trackerSerials;
        public Spread<string> TrackerSerials { get => _trackerSerials.ToSpread(); }


        private SpreadBuilder<string> _deviceClasses;
        public Spread<string> DeviceClasses { get => _deviceClasses.ToSpread(); }


        private SpreadBuilder<string> _deviceSerials;
        public Spread<string> DeviceSerials { get => _deviceSerials.ToSpread(); }


        private float _remainingTimePre;
        public float RemainingTimePre { get => _remainingTimePre; }
        

        private float _remainingTimePost;
        public float RemainingTimePost { get => _remainingTimePost; }



        public OpenVRPoser()
        {
            _lighthousePoses = new SpreadBuilder<Matrix>();
            _controllerPoses = new SpreadBuilder<Matrix>();
            _trackerPoses = new SpreadBuilder<Matrix>();
            _renderPoses = new SpreadBuilder<Matrix>();
            _gamePoses = new SpreadBuilder<Matrix>();
            _trackerSerials = new SpreadBuilder<string>();
            _deviceClasses = new SpreadBuilder<string>();
            _deviceSerials = new SpreadBuilder<string>();
        }


        string GetSerial(int i)
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
        
        private StringBuilder _serialBuilder = new StringBuilder(CSerialBuilderSize);

        public override void Update()
        {

            if (OpenVRManager.RenderPoses == null)
                return;

            //poses
            var poseCount = (int)OpenVR.k_unMaxTrackedDeviceCount;
            var renderPoses = OpenVRManager.RenderPoses;
            var gamePoses = OpenVRManager.GamePoses;
            var refreshSerials = _refreshSerials || _firstFrame;

            _renderPoses.Clear();
            _gamePoses.Clear();
            _deviceClasses.Clear();
            _deviceSerials.Clear();
            _lighthousePoses.Clear();
            _controllerPoses.Clear();
            _trackerPoses.Clear();

            if (refreshSerials)
                _trackerSerials.Clear();

            for (int i = 0; i < poseCount; i++)
            {
                _renderPoses.Add(renderPoses[i].mDeviceToAbsoluteTracking.ToMatrix());
                _gamePoses.Add(gamePoses[i].mDeviceToAbsoluteTracking.ToMatrix());
                
                var deviceClass = _system.GetTrackedDeviceClass((uint)i);
                
                _deviceClasses.Add(deviceClass.ToString());

                if (refreshSerials)
                    _deviceSerials.Add(GetSerial(i));

                if (deviceClass == ETrackedDeviceClass.TrackingReference)
                {
                    _lighthousePoses.Add(_gamePoses[i]);
                }

                if (deviceClass == ETrackedDeviceClass.Controller)
                {
                    _controllerPoses.Add(_gamePoses[i]);
                }

                if (deviceClass == ETrackedDeviceClass.GenericTracker)
                {
                    _trackerPoses.Add(_gamePoses[i]);
                    if (refreshSerials)
                        _trackerSerials.Add(_deviceSerials[i]);
                }
            }

            _hmdPose = _renderPoses[0];
        }

    }
}
