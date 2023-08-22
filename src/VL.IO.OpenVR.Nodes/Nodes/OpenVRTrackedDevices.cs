using System.Runtime.InteropServices;
using Valve.VR;
using VL.Lib.Collections;

namespace VL.IO.ValveOpenVR
{
    public class OpenVRTrackedDevices : OpenVRConsumerBase
    {
        private SpreadBuilder<String> _events;
        public Spread<string> Events { get => _events.ToSpread(); }


        private SpreadBuilder<int> _deviceIndices;
        public Spread<int> DeviceIndices { get => _deviceIndices.ToSpread(); }

        private SpreadBuilder<string> _deviceSerials;
        public Spread<string> DeviceSerials { get => _deviceSerials.ToSpread(); }

        private SpreadBuilder<OpenVRController.Device> _devices;
        public Spread<OpenVRController.Device> Devices { get => _devices.ToSpread(); }


        private SpreadBuilder<ETrackedControllerRole> _deviceRoles;
        public Spread<ETrackedControllerRole> DeviceRoles { get => _deviceRoles.ToSpread(); }


        private SpreadBuilder<ETrackedDeviceClass> _deviceClasses;
        public Spread<ETrackedDeviceClass> DeviceClasses { get => _deviceClasses.ToSpread(); }


        private OpenVRController.Device _controllerLeft;
        public OpenVRController.Device ControllerLeft { get => _controllerLeft; }


        private OpenVRController.Device _controllerRight;
        public OpenVRController.Device ControllerRight { get => _controllerRight; }


        private SpreadBuilder<OpenVRController.Device> _controllerLeftRight;
        public Spread<OpenVRController.Device> ControllerLeftRight { get => _controllerLeftRight.ToSpread(); }


        private SpreadBuilder<OpenVRController.Device> _trackers;
        public Spread<OpenVRController.Device> Trackers { get => _trackers.ToSpread(); }


        private SpreadBuilder<string> _trackerSerials;
        public Spread<string> TrackerSerials { get => _trackerSerials.ToSpread(); }
       

        private uint _eventSize = (uint)Marshal.SizeOf(typeof(VREvent_t));

        private int _frame = 0;

        private bool _pollEvents = false;
        public bool PollEvents { set => _pollEvents = value; }


        public OpenVRTrackedDevices()
        {
            _events = new SpreadBuilder<string>();
            _deviceIndices = new SpreadBuilder<int>();
            _deviceSerials = new SpreadBuilder<string>();
            _devices = new SpreadBuilder<OpenVRController.Device>();
            _deviceRoles = new SpreadBuilder<ETrackedControllerRole>();
            _deviceClasses = new SpreadBuilder<ETrackedDeviceClass>();
            _controllerLeftRight = new SpreadBuilder<OpenVRController.Device>();
            _trackers = new SpreadBuilder<OpenVRController.Device>();
            _trackerSerials = new SpreadBuilder<string>(); 
        }

        public override void Update()
        {

            if (_pollEvents) { 

                VREvent_t evt = default(VREvent_t);

                _events.Clear();
                _deviceIndices.Clear();

                while (_system.PollNextEvent(ref evt, _eventSize))
                {
                    var evtType = (EVREventType)evt.eventType;

                    _events.Add(evtType.ToString());
                    _deviceIndices.Add((int)evt.trackedDeviceIndex);
                }
            }

            //controller states
            OpenVRController.Update(_frame++);

            _devices.Clear();
            _deviceRoles.Clear();
            _deviceClasses.Clear();
            _controllerLeftRight.Clear();
            _trackers.Clear();


            var refreshSerials = _refreshSerials || _firstFrame;

            if (refreshSerials)
            {
               _deviceSerials.Clear();
               _trackerSerials.Clear();
            }

            var indexLeft = (int)_system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            var indexRight = (int)_system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            if (indexLeft > 0)
            {
                var c = OpenVRController.Input(indexLeft);
                _controllerLeft = c;
                _controllerLeftRight.Add(c);
            }

            if (indexRight > 0)
            {
                var c = OpenVRController.Input(indexRight);
                _controllerRight = c;
                _controllerLeftRight.Add(c);
            }


            var devicecount = (int)OpenVR.k_unMaxTrackedDeviceCount;

            for (int i = 0; i < devicecount; i++)
            {
                var deviceclass = _system.GetTrackedDeviceClass((uint)i);

                // if (deviceclass != ETrackedDeviceClass.Controller && deviceclass != ETrackedDeviceClass.GenericTracker ) continue;

                var c = OpenVRController.Input(i);

                if (!c.connected || !c.valid) continue;

                _devices.Add(c);
                _deviceRoles.Add(_system.GetControllerRoleForTrackedDeviceIndex((uint)i));

                _deviceClasses.Add(deviceclass);

                if (refreshSerials)
                    _deviceSerials.Add(GetSerial(i));

                if (deviceclass == ETrackedDeviceClass.GenericTracker)
                {
                    _trackers.Add(c);

                    if (refreshSerials)
                        _trackerSerials.Add(_deviceSerials[i]);
                }
            }
        }






    }
}
