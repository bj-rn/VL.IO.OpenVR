using Valve.VR;

namespace VL.IO.ValveOpenVR
{
    public class OpenVRSystem : OpenVRConsumerBase
    {
        public bool WaitForSync
        {
            get;
            set;
        }

        public bool GetTiming
        {
            get;
            set;
        }

        public float RemainingTimePre
        {
            get;
            private set;
        }

        public float RemainingTimePost
        {
            get;
            private set;
        }


        public OpenVRSystem()
        {

        }

        public override void Update()
        {
            //poses
            var poseCount = (int)OpenVR.k_unMaxTrackedDeviceCount;
            var renderPoses = new TrackedDevicePose_t[poseCount];
            var gamePoses = new TrackedDevicePose_t[poseCount];

            if (GetTiming)
                RemainingTimePre = OpenVR.Compositor.GetFrameTimeRemaining();
            else
                RemainingTimePre = 0;

            var error = default(EVRCompositorError);

            if (WaitForSync)
                error = OpenVR.Compositor.WaitGetPoses(renderPoses, gamePoses);
            else
                error = OpenVR.Compositor.GetLastPoses(renderPoses, gamePoses);

            SetStatus(error);
            if (error != EVRCompositorError.None) return;

            if (GetTiming)
                RemainingTimePost = OpenVR.Compositor.GetFrameTimeRemaining();
            else
                RemainingTimePost = 0;

            OpenVRManager.RenderPoses = renderPoses;
            OpenVRManager.GamePoses = gamePoses;
        }

        public void SetTrackingSpace(ETrackingUniverseOrigin origin) 
        { 
            if (_system != null)
                OpenVR.Compositor.SetTrackingSpace(origin);
        }

    }
}
