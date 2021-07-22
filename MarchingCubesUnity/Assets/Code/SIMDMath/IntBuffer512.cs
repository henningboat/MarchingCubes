using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Code.CubeMarching
{
    /// <summary>
    ///     Contains 512 float values, accessible via indexer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IntBuffer512
    {
        public int this[int i]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (i < 0 || i >= Capacity)
                {
                    throw new ArgumentOutOfRangeException($"SurfaceDistance was accessed with index {i}, must be between 0 and 127");
                }
#endif
                var pointer = UnsafeUtility.AddressOf(ref this);
                var arrayEntry = UnsafeUtility.ReadArrayElement<int>(pointer, i);
                return arrayEntry;
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (i < 0 || i >= Capacity)
                {
                    throw new ArgumentOutOfRangeException($"SurfaceDistance was accessed with index {i}, must be between 0 and 127");
                }
#endif
                var pointer = UnsafeUtility.AddressOf(ref this);
                var valueToWrite = value;
                //the plan is to clamp the distance field to a certain range, to limit the spacial influence of smaller objects
                UnsafeUtility.WriteArrayElement(pointer, i, valueToWrite);
            }
        }

        public int4x4 _valueBlock0;
        public int4x4 _valueBlock1;
        public int4x4 _valueBlock2;
        public int4x4 _valueBlock3;
        public int4x4 _valueBlock4;
        public int4x4 _valueBlock5;
        public int4x4 _valueBlock6;
        public int4x4 _valueBlock7;
        public int4x4 _valueBlock8;
        public int4x4 _valueBlock9;

        public int4x4 _valueBlock10;
        public int4x4 _valueBlock11;
        public int4x4 _valueBlock12;
        public int4x4 _valueBlock13;
        public int4x4 _valueBlock14;
        public int4x4 _valueBlock15;
        public int4x4 _valueBlock16;
        public int4x4 _valueBlock17;
        public int4x4 _valueBlock18;
        public int4x4 _valueBlock19;

        public int4x4 _valueBlock20;
        public int4x4 _valueBlock21;
        public int4x4 _valueBlock22;
        public int4x4 _valueBlock23;
        public int4x4 _valueBlock24;
        public int4x4 _valueBlock25;
        public int4x4 _valueBlock26;
        public int4x4 _valueBlock27;
        public int4x4 _valueBlock28;
        public int4x4 _valueBlock29;

        public int4x4 _valueBlock30;
        public int4x4 _valueBlock31;

        public const int Capacity = 512;

        public const int BufferSizeInBytes = 512 * 4;
    }
}