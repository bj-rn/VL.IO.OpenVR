using System.Xml.Linq;
using Valve.VR;


namespace VL.IO.ValveOpenVR
{
    public abstract class OpenVRBase
    {
        
        private String FError;
        public string Error { get => FError; }

        public abstract void Update();

        protected void SetStatus(object toString)
        {
            if (toString is EVRInitError)
                FError = OpenVR.GetStringForHmdError((EVRInitError)toString);
            else if (toString is EVRCompositorError)
            {
                var error = (EVRCompositorError)toString;

                if (error == EVRCompositorError.TextureIsOnWrongDevice)
                    FError = "Texture on wrong device. Set your graphics driver to use the same video card for vvvv as the headset is plugged into.";
                else if (error == EVRCompositorError.TextureUsesUnsupportedFormat)
                    FError = "Unsupported texture format. Make sure texture uses RGBA, is not compressed and has no mipmaps.";
                else
                    FError = error.ToString();
            }
            else
                FError = toString.ToString();
        }
    }


    public abstract class OpenVRConsumerBase : OpenVRBase
    {
        protected bool FFirstFrame = true;

        protected CVRSystem FSystem;


        public void Update(CVRSystem system)
        {
            if (system != null)
            {
               FSystem = system;
               Update();
               FFirstFrame = false;

            }
            else
            {
                SetStatus("OpenVR is not initialized");
            }
            
        }
    }


}
