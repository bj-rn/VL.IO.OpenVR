using Valve.VR;
using Stride.Core.Mathematics;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace VL.IO.ValveOpenVR
{
    //the convention horror
    public static class OpenVRExtensions
    {
       

        // from beta: left handed

        //  public static Matrix ToMatrix(this HmdMatrix34_t m)
        //{
        //    return new Matrix()
        //    {
        //        M11 = m.m0,     M12 = m.m4,     M13 = -m.m8,    M14 = 0,

        //        M21 = m.m1,     M22 = m.m5,     M23 = -m.m9,    M24 = 0,

        //        M31 = -m.m2,    M32 = -m.m6,    M33 = m.m10,    M34 = 0,

        //        M41 = m.m3,     M42 = m.m7,     M43 = -m.m11,   M44 = 1
        //    };
        //}


        // hope  that's correct 
        public static Matrix ToMatrix(this HmdMatrix34_t m)
        {
            return new Matrix()
            {
                M11 = m.m0,     M12 = m.m4,     M13 = m.m8,    M14 = 0,

                M21 = m.m1,     M22 = m.m5,     M23 = m.m9,    M24 = 0,

                M31 = m.m2,     M32 = m.m6,     M33 = m.m10,   M34 = 0,

                M41 = -m.m3,    M42 = m.m7,     M43 = -m.m11,  M44 = 1
            };
        }


        private static Vector3 UnsafeToVector3(this HmdVector3_t vector)
        {
            Debug.Assert(Unsafe.SizeOf<Vector3>() == Unsafe.SizeOf<HmdVector3_t>());

            var v = new Vector3();

            Unsafe.As<Vector3, HmdVector3_t>(ref v) = vector;

            return v;
        }

        public static Vector3 ToVector3(this HmdVector3_t vector)
        { return UnsafeToVector3(vector); }



        public static Vector3 Pos(this Matrix m)
        {
            return new Vector3(m.M41, m.M42, m.M43);
        }
    }
}
