using System.Text;
using Valve.VR;
using Stride.Core.Mathematics;
using VL.Lib.Collections;


namespace VL.IO.ValveOpenVR
{
    public class OpenVRPoser : OpenVRConsumerBase
    {
        private bool FWaitForSync = false;
        public bool WaitForSync { set => FWaitForSync = value; }
        

        private bool FRefreshSerials = false;
        public bool RefreshSerials { set => FRefreshSerials = value; }
    
        
        private bool FGetTiming = false;
        public bool GetTiming { set => FGetTiming = value; }
        

        private Matrix FHMDPose;
        public Matrix HMDPose { get => FHMDPose; }
        

        private SpreadBuilder<Matrix> FLighthousePoses;
        public Spread<Matrix> LighthousePoses { get => FLighthousePoses.ToSpread(); }
        

        private SpreadBuilder<Matrix> FControllerPoses;
        public Spread<Matrix> ControllerPoses { get => FControllerPoses.ToSpread(); }


        private SpreadBuilder<Matrix> FTrackerPoses;
        public Spread<Matrix> TrackerPoses { get => FTrackerPoses.ToSpread(); }


        private SpreadBuilder<Matrix> FRenderPoses;
        public Spread<Matrix> RenderPoses { get => FRenderPoses.ToSpread(); }


        private SpreadBuilder<Matrix> FGamePoses;
        public Spread<Matrix> GamePoses { get => FGamePoses.ToSpread(); }


        private SpreadBuilder<string> FTrackerSerials;
        public Spread<string> TrackerSerials { get => FTrackerSerials.ToSpread(); }


        private SpreadBuilder<string> FDeviceClasses;
        public Spread<string> DeviceClasses { get => FDeviceClasses.ToSpread(); }


        private SpreadBuilder<string> FDeviceSerials;
        public Spread<string> DeviceSerials { get => FDeviceSerials.ToSpread(); }


        private float FRemainingTimePre;
        public float RemainingTimePre { get => FRemainingTimePre; }
        

        private float FRemainingTimePost;
        public float RemainingTimePost { get => FRemainingTimePost; }



        public OpenVRPoser()
        {
            FLighthousePoses = new SpreadBuilder<Matrix>();
            FControllerPoses = new SpreadBuilder<Matrix>();
            FTrackerPoses = new SpreadBuilder<Matrix>();
            FRenderPoses = new SpreadBuilder<Matrix>();
            FGamePoses = new SpreadBuilder<Matrix>();
            FTrackerSerials = new SpreadBuilder<string>();
            FDeviceClasses = new SpreadBuilder<string>();
            FDeviceSerials = new SpreadBuilder<string>();
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

        public override void Update()
        {

            GetPoses();

            if (OpenVRManager.RenderPoses == null)
                return;

            //poses
            var poseCount = (int)OpenVR.k_unMaxTrackedDeviceCount;
            var renderPoses = OpenVRManager.RenderPoses;
            var gamePoses = OpenVRManager.GamePoses;
            var refreshSerials = FRefreshSerials || FFirstFrame;

            FRenderPoses.Clear();
            FGamePoses.Clear();
            FDeviceClasses.Clear();
            FDeviceSerials.Clear();
            FLighthousePoses.Clear();
            FControllerPoses.Clear();
            FTrackerPoses.Clear();

            if (refreshSerials)
                FTrackerSerials.Clear();

            for (int i = 0; i < poseCount; i++)
            {
                FRenderPoses.Add(renderPoses[i].mDeviceToAbsoluteTracking.ToMatrix());
                FGamePoses.Add(gamePoses[i].mDeviceToAbsoluteTracking.ToMatrix());
                var deviceClass = FSystem.GetTrackedDeviceClass((uint)i);
                FDeviceClasses.Add(deviceClass.ToString());

                if (refreshSerials)
                    FDeviceSerials.Add(GetSerial(i));

                if (deviceClass == ETrackedDeviceClass.TrackingReference)
                {
                    FLighthousePoses.Add(FGamePoses[i]);
                }

                if (deviceClass == ETrackedDeviceClass.Controller)
                {
                    FControllerPoses.Add(FGamePoses[i]);
                }

                if (deviceClass == ETrackedDeviceClass.GenericTracker)
                {
                    FTrackerPoses.Add(FGamePoses[i]);
                    if (refreshSerials)
                        FTrackerSerials.Add(FDeviceSerials[i]);
                }
            }

            FHMDPose = FRenderPoses[0];
        }

    }
}
