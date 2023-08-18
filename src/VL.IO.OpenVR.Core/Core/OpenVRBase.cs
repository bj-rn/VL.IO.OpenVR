using System.Xml.Linq;
using Valve.VR;


namespace VL.IO.ValveOpenVR
{
    public abstract class OpenVRBase
    {
        
        private String error;
        public string Error { get => error; }

        public abstract void Update();

        protected void SetStatus(object toString)
        {
            if (toString is EVRInitError)
                error = OpenVR.GetStringForHmdError((EVRInitError)toString);
            else if (toString is EVRCompositorError)
            {
                var e = (EVRCompositorError)toString;

                if (e == EVRCompositorError.TextureIsOnWrongDevice)
                    error = "Texture on wrong device. Set your graphics driver to use the same video card for vvvv as the headset is plugged into.";
                else if (e == EVRCompositorError.TextureUsesUnsupportedFormat)
                    error = "Unsupported texture format. Make sure texture uses RGBA, is not compressed and has no mipmaps.";
                else
                    error = e.ToString();
            }
            else
                error = toString.ToString();
        }
    }


    public abstract class OpenVRConsumerBase : OpenVRBase
    {
        protected bool _firstFrame = true;

        protected CVRSystem _system;


        public void Update(CVRSystem system)
        {
            if (system != null)
            {
               _system = system;
               Update();
               _firstFrame = false;
            }
            else
            {
                SetStatus("OpenVR is not initialized");
            }
            
        }
    }

}
