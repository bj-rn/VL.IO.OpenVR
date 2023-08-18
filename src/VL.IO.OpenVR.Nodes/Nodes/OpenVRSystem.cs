using System.Text;
using Valve.VR;
using Stride.Core.Mathematics;
using VL.Lib.Collections;


namespace VL.IO.ValveOpenVR
{
    public class OpenVRSystem : OpenVRConsumerBase
    {
        private bool _waitForSync = false;
        public bool WaitForSync { set => _waitForSync = value; }


        private bool _getTiming = false;
        public bool GetTiming { set => _getTiming = value; }


        private float _remainingTimePre;
        public float RemainingTimePre { get => _remainingTimePre; }


        private float _remainingTimePost;
        public float RemainingTimePost { get => _remainingTimePost; }



        public OpenVRSystem()
        {

        }

        public override void Update()
        {
            //poses
            var poseCount = (int)OpenVR.k_unMaxTrackedDeviceCount;
            var renderPoses = new TrackedDevicePose_t[poseCount];
            var gamePoses = new TrackedDevicePose_t[poseCount];

            if (_getTiming)
                _remainingTimePre = OpenVR.Compositor.GetFrameTimeRemaining();
            else
                _remainingTimePre = 0;

            var error = default(EVRCompositorError);

            if (_waitForSync)
                error = OpenVR.Compositor.WaitGetPoses(renderPoses, gamePoses);
            else
                error = OpenVR.Compositor.GetLastPoses(renderPoses, gamePoses);

            SetStatus(error);
            if (error != EVRCompositorError.None) return;

            if (_getTiming)
                _remainingTimePost = OpenVR.Compositor.GetFrameTimeRemaining();
            else
                _remainingTimePost = 0;

            OpenVRManager.RenderPoses = renderPoses;
            OpenVRManager.GamePoses = gamePoses;

        }

    }
}
