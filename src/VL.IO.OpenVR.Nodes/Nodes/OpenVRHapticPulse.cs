using Valve.VR;
using Stride.Core.Mathematics;
using System;
using VL.Lib.Animation;

namespace VL.IO.ValveOpenVR
{
    public class OpenVRHapticPulse : OpenVRConsumerBase
    {
        private OpenVRController.Device _controller;
        public OpenVRController.Device Controller { set => _controller = value; }

        private bool _pulse = false;
        public bool Pulse { set => _pulse = value; }

        private float _duration = 0.25f;
        public float Duration { set => _duration = value; }

        public override void Update()
        {
            if (_controller == null || !_pulse)
                return;

            // see: https://github.com/ValveSoftware/openvr/wiki/IVRSystem::TriggerHapticPulse

                            

            /*
                The parameter [Duration] you pass into TriggerHapticPulse is measured in microseconds, with the max value(in my tests) being 3999, which is 4ms - each frame at 90Hz is 11ms long.
                Because of this, if you're pulsing each frame, the effect of pulse length is felt subjectively as vibration strength.

                If you want longer vibrations, try something like this:

                length is how long the vibration should go for
                strength is vibration strength from 0-1
               
                IEnumerator LongVibration(float length, float strength)
                {
                    for (float i = 0; i < length; i += Time.deltaTime)
                    {
                        SteamVR_Controller.Input([index]).TriggerHapticPulse((ushort)Mathf.Lerp(0, 3999, strength));
                        yield return null;
                    }
                }

                And for pulsed vibrations
                
                vibrationCount is how many vibrations
                vibrationLength is how long each vibration should go for
                gapLength is how long to wait between vibrations
                strength is vibration strength from 0-1
                
                IEnumerator LongVibration(int vibrationCount, float vibrationLength, float gapLength, float strength)
                {
                    strength = Mathf.Clamp01(strength);
                    
                    for (int i = 0; i < vibrationCount; i++)
                    {
                        if (i != 0) yield return new WaitForSeconds(gapLength);
                       yield return StartCoroutine(LongVibration(vibrationLength, strength));
                    }
                }

                // https://github.com/ValveSoftware/openvr/issues/129
                // https://steamcommunity.com/app/358720/discussions/0/405693392914144440/#c357284767229628161

            */


            var duration = (int) MathUtil.Lerp(1.0, 3999, MathUtil.Clamp(_duration, 0.0, 1.0));

            // see: https://steamcommunity.com/app/358720/discussions/0/517141624283630663/
            // for now only axis with id 0 is working/implemented in OpenVR... and probably this will never change

            _system.TriggerHapticPulse(_controller.index, 0, (char)duration);

        }

    }


 
}
