using System.Text;
using Valve.VR;
using Stride.Core.Mathematics;
using VL.Lib.Collections;


namespace VL.IO.ValveOpenVR
{
    public class OpenVRInput : OpenVRProducer, IDisposable
    {

        private bool FSyncAfterFrame;
        public bool SyncAfterFrame{ set => FSyncAfterFrame = value; }
        

        private bool FWaitForSync;
        public bool WaitForSync { set => FWaitForSync = value; }
        

        private bool FRefreshSerials;
        public bool RefreshSerials { set => FRefreshSerials = value; }
    
        
        private bool FGetTiming;
        public bool GetTiming { set => FGetTiming = value; }
        


        private Matrix FHMDPoseOut;
        public Matrix HMDPoseOut { get => FHMDPoseOut; }
        

        private SpreadBuilder<Matrix> FLighthousePosesOut;
        public Spread<Matrix> LighthousePosesOut { get => FLighthousePosesOut.ToSpread(); }
        

        private SpreadBuilder<Matrix> FControllerPosesOut;
        public Spread<Matrix> ControllerPosesOut { get => FControllerPosesOut.ToSpread(); }


        private SpreadBuilder<Matrix> FTrackerPosesOut;
        public Spread<Matrix> TrackerPosesOut { get => FTrackerPosesOut.ToSpread(); }


        private SpreadBuilder<Matrix> FRenderPosesOut;
        public Spread<Matrix> RenderPosesOut { get => FRenderPosesOut.ToSpread(); }


        private SpreadBuilder<Matrix> FGamePosesOut;
        public Spread<Matrix> GamePosesOut { get => FGamePosesOut.ToSpread(); }


        private SpreadBuilder<string> FTrackerSerialOut;
        public Spread<string> TrackerSerialOut { get => FTrackerSerialOut.ToSpread(); }


        private SpreadBuilder<string> FDeviceClassOut;
        public Spread<string> DeviceClassOut { get => FDeviceClassOut.ToSpread(); }


        private SpreadBuilder<string> FDeviceSerialOut;
        public Spread<string> DeviceSerialOut { get => FDeviceSerialOut.ToSpread(); }


        private float FRemainingTimePre;
        public float RemainingTimePre { get => FRemainingTimePre; }
        

        private float FRemainingTimePost;
        public float RemainingTimePost { get => FRemainingTimePost; }



        public OpenVRInput()
        {
            FLighthousePosesOut = new SpreadBuilder<Matrix>();
            FControllerPosesOut = new SpreadBuilder<Matrix>();
            FTrackerPosesOut = new SpreadBuilder<Matrix>();
            FRenderPosesOut = new SpreadBuilder<Matrix>();
            FGamePosesOut = new SpreadBuilder<Matrix>();
            FTrackerSerialOut = new SpreadBuilder<string>();
            FDeviceClassOut = new SpreadBuilder<string>();
            FDeviceSerialOut = new SpreadBuilder<string>();
        }

            void GetPoses()
        {
            //poses
            var poseCount = (int)OpenVR.k_unMaxTrackedDeviceCount;
            var renderPoses = new TrackedDevicePose_t[poseCount];
            var gamePoses = new TrackedDevicePose_t[poseCount];

            if (FGetTiming)
                FRemainingTimePre = OpenVR.Compositor.GetFrameTimeRemaining();
            else
                FRemainingTimePre = 0;

            var error = default(EVRCompositorError);

            if (FWaitForSync)
                error = OpenVR.Compositor.WaitGetPoses(renderPoses, gamePoses);
            else
                error = OpenVR.Compositor.GetLastPoses(renderPoses, gamePoses);

            SetStatus(error);
            if (error != EVRCompositorError.None) return;

            if (FGetTiming)
                FRemainingTimePost = OpenVR.Compositor.GetFrameTimeRemaining();
            else
                FRemainingTimePost = 0;

            OpenVRManager.RenderPoses = renderPoses;
            OpenVRManager.GamePoses = gamePoses;
        }

        string GetSerial(int i)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            FSerialBuilder.Clear();
            FSystem.GetStringTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_SerialNumber_String, FSerialBuilder, CSerialBuilderSize, ref error);
            if (error == ETrackedPropertyError.TrackedProp_Success)
                return FSerialBuilder.ToString();
            else
                return "";
        }


        const int CSerialBuilderSize = 64;
        
        private StringBuilder FSerialBuilder = new StringBuilder(CSerialBuilderSize);

        private CVRSystem FSystem;

        public override void Update(CVRSystem system)
        {
            FSystem = system;

            GetPoses();

            if (OpenVRManager.RenderPoses == null)
                return;

            //poses
            var poseCount = (int)OpenVR.k_unMaxTrackedDeviceCount;
            var renderPoses = OpenVRManager.RenderPoses;
            var gamePoses = OpenVRManager.GamePoses;
            var refreshSerials = FRefreshSerials || FFirstFrame;

            FRenderPosesOut.Clear();
            FGamePosesOut.Clear();
            FDeviceClassOut.Clear();
            FDeviceSerialOut.Clear();
            FLighthousePosesOut.Clear();
            FControllerPosesOut.Clear();
            FTrackerPosesOut.Clear();

            if (refreshSerials)
                FTrackerSerialOut.Clear();

            for (int i = 0; i < poseCount; i++)
            {
                FRenderPosesOut.Add(renderPoses[i].mDeviceToAbsoluteTracking.ToMatrix());
                FGamePosesOut.Add(gamePoses[i].mDeviceToAbsoluteTracking.ToMatrix());
                var deviceClass = system.GetTrackedDeviceClass((uint)i);
                FDeviceClassOut.Add(deviceClass.ToString());

                if (refreshSerials)
                    FDeviceSerialOut.Add(GetSerial(i));

                if (deviceClass == ETrackedDeviceClass.TrackingReference)
                {
                    FLighthousePosesOut.Add(FGamePosesOut[i]);
                }

                if (deviceClass == ETrackedDeviceClass.Controller)
                {
                    FControllerPosesOut.Add(FGamePosesOut[i]);
                }

                if (deviceClass == ETrackedDeviceClass.GenericTracker)
                {
                    FTrackerPosesOut.Add(FGamePosesOut[i]);
                    if (refreshSerials)
                        FTrackerSerialOut.Add(FDeviceSerialOut[i]);
                }
            }

            FHMDPoseOut = FRenderPosesOut[0];
        }


        public void Dispose()
        {
            OpenVRManager.ShutDownOpenVR();
        }
    }
}
