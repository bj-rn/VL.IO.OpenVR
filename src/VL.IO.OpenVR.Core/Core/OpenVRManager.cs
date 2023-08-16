﻿
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Valve.VR;
using Stride.Core.Mathematics;

namespace VL.IO.ValveOpenVR
{
    public static class OpenVRManager
    {
        public static CVRSystem System
        {
            get;
            private set;
        }

        public static string ErrorMessage
        {
            get;
            private set;
        }

        public static Int2 RecommendedRenderTargetSize
        {
            get;
            private set;
        }

        static OpenVRManager()
        {
            //Load openvr_api.dll
            LoadDllFile(CoreAssemblyNativeDir, "openvr_api.dll");
        }

        /// <summary>
        /// Initializes the open vr system and sets the static System field.
        /// </summary>
        /// <returns>The created system class or null</returns>
        public static CVRSystem InitOpenVR()
        {
            if (System == null)
            {

                var initError = EVRInitError.Unknown;
                var system = OpenVR.Init(ref initError);
                SetStatus(initError);

                if (initError != EVRInitError.None)
                    system = null;

                System = system;

                if (System != null)
                {
                    //texture size
                    uint sizeX = 0;
                    uint sizeY = 0;
                    System.GetRecommendedRenderTargetSize(ref sizeX, ref sizeY);
                    RecommendedRenderTargetSize = new Int2((int)sizeX, (int)sizeY);
                } 
            }

            return System;
        }

        public static TrackedDevicePose_t[] RenderPoses
        {
            get;
            set;
        }

        public static TrackedDevicePose_t[] GamePoses
        {
            get;
            set;
        }

        static void SetStatus(object toString)
        {
            if (toString is EVRInitError)
                ErrorMessage = OpenVR.GetStringForHmdError((EVRInitError)toString);
            else
                ErrorMessage = toString.ToString();
        }

        public static void ShutDownOpenVR()
        {
            OpenVR.Shutdown();
        }

        private class UnsafeNativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetDllDirectory(string lpPathName);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int GetDllDirectory(int bufsize, StringBuilder buf);

            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr LoadLibrary(string librayName);
        }

        public static string CoreAssemblyNativeDir
        {
            get
            {
                //get the full location of the assembly with DaoTests in it
                string fullPath = Assembly.GetAssembly(typeof(OpenVRManager)).Location;
                var subfolder = Environment.Is64BitProcess ? "x64" : "x86";

                //get the folder that's in
                return Path.Combine(Path.GetDirectoryName(fullPath), subfolder);
            }
        }

        public static void LoadDllFile(string dllfolder, string libname)
        {
            var currentpath = new StringBuilder(255);
            var length = UnsafeNativeMethods.GetDllDirectory(currentpath.Length, currentpath);

            // use new path
            var success = UnsafeNativeMethods.SetDllDirectory(dllfolder);

            if (success)
            {
                var handle = UnsafeNativeMethods.LoadLibrary(libname);
                success = handle.ToInt64() > 0;
            }

            // restore old path
            UnsafeNativeMethods.SetDllDirectory(currentpath.ToString());
        }
    }
}
