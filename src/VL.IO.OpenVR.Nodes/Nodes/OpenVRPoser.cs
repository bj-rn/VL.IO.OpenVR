using Valve.VR;
using Stride.Core.Mathematics;
using VL.Lib.Collections;


namespace VL.IO.ValveOpenVR
{
    public class OpenVRPoser : OpenVRConsumerBase
    {
        
        private Matrix _hmdPose;
        public Matrix HMDPose { get => _hmdPose; }

        private SpreadBuilder<Matrix> _lighthousePoses;
        public Spread<Matrix> LighthousePoses { get => _lighthousePoses.ToSpread(); }
        


        private SpreadBuilder<Matrix> _controllerPoses;
        public Spread<Matrix> ControllerPoses { get => _controllerPoses.ToSpread(); }


        private SpreadBuilder<Vector3> _controllerVelocities;
        public Spread<Vector3> ControllerVelocities { get => _controllerVelocities.ToSpread(); }


        private SpreadBuilder<Vector3> _controllerAngularVelocities;
        public Spread<Vector3> ControllerAngularVelocities { get => _controllerAngularVelocities.ToSpread(); }



        private SpreadBuilder<Matrix> _trackerPoses;
        public Spread<Matrix> TrackerPoses { get => _trackerPoses.ToSpread(); }


        private SpreadBuilder<Vector3> _trackerVelocities;
        public Spread<Vector3> TrackerVelocities { get => _trackerVelocities.ToSpread(); }


        private SpreadBuilder<Vector3> _trackerAngularVelocities;
        public Spread<Vector3> TrackerAngularVelocities { get => _trackerAngularVelocities.ToSpread(); }



        private SpreadBuilder<Matrix> _renderPoses;
        public Spread<Matrix> RenderPoses { get => _renderPoses.ToSpread(); }


        private SpreadBuilder<Matrix> _gamePoses;
        public Spread<Matrix> GamePoses { get => _gamePoses.ToSpread(); }

        private SpreadBuilder<Vector3> _gameVelocities;
        private SpreadBuilder<Vector3> _gameAngularVelocities;

        private SpreadBuilder<string> _trackerSerials;
        public Spread<string> TrackerSerials { get => _trackerSerials.ToSpread(); }


        private SpreadBuilder<string> _deviceClasses;
        public Spread<string> DeviceClasses { get => _deviceClasses.ToSpread(); }


        private SpreadBuilder<string> _deviceSerials;
        public Spread<string> DeviceSerials { get => _deviceSerials.ToSpread(); }
        

        public OpenVRPoser()
        {
            _lighthousePoses = new SpreadBuilder<Matrix>();

            _controllerPoses = new SpreadBuilder<Matrix>();
            _controllerVelocities = new SpreadBuilder<Vector3>();
            _controllerAngularVelocities = new SpreadBuilder<Vector3>();

            _trackerPoses = new SpreadBuilder<Matrix>();
            _trackerVelocities = new SpreadBuilder<Vector3>();
            _trackerAngularVelocities = new SpreadBuilder<Vector3>();

            _renderPoses = new SpreadBuilder<Matrix>();

            _gamePoses = new SpreadBuilder<Matrix>();
            _gameVelocities = new SpreadBuilder<Vector3>();
            _gameAngularVelocities = new SpreadBuilder<Vector3>();

            _trackerSerials = new SpreadBuilder<string>();
            _deviceClasses = new SpreadBuilder<string>();
            _deviceSerials = new SpreadBuilder<string>();
        }

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
            _gameVelocities.Clear();
            _gameAngularVelocities.Clear();
    

            _deviceClasses.Clear();
            _deviceSerials.Clear();
            _lighthousePoses.Clear();

            _controllerPoses.Clear();
            _controllerVelocities.Clear();
            _controllerAngularVelocities.Clear();

            _trackerPoses.Clear();
            _trackerVelocities.Clear();
            _controllerAngularVelocities.Clear();

            if (refreshSerials)
            {
                _deviceSerials.Clear();
                _trackerSerials.Clear();
            }

            for (int i = 0; i < poseCount; i++)
            {
                _renderPoses.Add(renderPoses[i].mDeviceToAbsoluteTracking.ToMatrix());

                var gamePose = gamePoses[i];
                _gamePoses.Add(gamePose.mDeviceToAbsoluteTracking.ToMatrix());
                _gameVelocities.Add(gamePose.vVelocity.ToVector3());
                _gameAngularVelocities.Add(gamePose.vAngularVelocity.ToVector3());
                
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
                    _controllerVelocities.Add(_gameVelocities[i]);
                    _controllerAngularVelocities.Add(_gameAngularVelocities[i]);
                }

                if (deviceClass == ETrackedDeviceClass.GenericTracker)
                {
                    _trackerPoses.Add(_gamePoses[i]);
                    _trackerVelocities.Add(_gameVelocities[i]);
                    _trackerAngularVelocities.Add(_gameAngularVelocities[i]);

                    if (refreshSerials)
                        _trackerSerials.Add(_deviceSerials[i]);
                }
            }

            _hmdPose = _renderPoses[0];
        }

    }
}
