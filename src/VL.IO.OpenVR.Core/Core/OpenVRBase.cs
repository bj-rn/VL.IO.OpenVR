using System.Xml.Linq;
using Valve.VR;


namespace VL.IO.ValveOpenVR
{
    public abstract class OpenVRBase
    {
        
        private String FErrorOut;

        public string ErrorOut { get => FErrorOut;  }

        public abstract void Update(CVRSystem system);

        protected void SetStatus(object toString)
        {
            if (toString is EVRInitError)
                FErrorOut = OpenVR.GetStringForHmdError((EVRInitError)toString);
            else if (toString is EVRCompositorError)
            {
                var error = (EVRCompositorError)toString;

                if (error == EVRCompositorError.TextureIsOnWrongDevice)
                    FErrorOut = "Texture on wrong device. Set your graphics driver to use the same video card for vvvv as the headset is plugged into.";
                else if (error == EVRCompositorError.TextureUsesUnsupportedFormat)
                    FErrorOut = "Unsupported texture format. Make sure texture uses RGBA, is not compressed and has no mipmaps.";
                else
                    FErrorOut = error.ToString();
            }
            else
                FErrorOut = toString.ToString();
        }
    }

    public abstract class OpenVRProducer : OpenVRBase
    {
        private bool FInitIn;

        public bool InitIn { set => FInitIn = value; }


        protected bool FFirstFrame = true;

        //the vr system
        private CVRSystem FOpenVRSystem;
        public CVRSystem SystemOut { get => FOpenVRSystem; }



        public void Update()
        {
            if (FInitIn || FFirstFrame)
            {
                FOpenVRSystem = OpenVRManager.InitOpenVR();
                SetStatus(OpenVRManager.ErrorMessage);
            }

            if (FOpenVRSystem != null)
            {
                Update(FOpenVRSystem);
            }

            FFirstFrame = false;
        }
    }

    public abstract class OpenVRConsumerBaseNode : OpenVRBase
    {

        //the vr system
        private CVRSystem FOpenVRSystemIn;

        public CVRSystem OpenVRSystemIn { get => FOpenVRSystemIn; set => FOpenVRSystemIn = value; }


        public void Update()
        {
           

            if (FOpenVRSystemIn != null)
            {
                Update(FOpenVRSystemIn);
            }
            else
            {
                SetStatus("OpenVR is not initialized, at least one Poser (OpenVR) or Camera (OpenVR) must exist");
            }
        }
    }


}
