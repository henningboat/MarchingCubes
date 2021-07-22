using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Code.CubeMarching.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BitArray512
    {
        #region Static Stuff

        public static BitArray512 operator |(BitArray512 a, BitArray512 b)
        {
            a.a |= b.a;
            a.b |= b.b;
            a.c |= b.c;
            a.d |= b.d;
            a.e |= b.e;
            a.f |= b.f;
            a.g |= b.g;
            a.h |= b.h;
            return a;
        }

        public static BitArray512 operator &(BitArray512 a, BitArray512 b)
        {
            a.a &= b.a;
            a.b &= b.b;
            a.c &= b.c;
            a.d &= b.d;
            a.e &= b.e;
            a.f &= b.f;
            a.g &= b.g;
            a.h &= b.h;
            return a;
        }

        public static unsafe BitArray512 BoundsToBitArray(int3 boundsGSMin, int3 boundsGSSize)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (math.any(boundsGSMin < 0) || math.any(boundsGSMin > 8))
            {
                throw new ArgumentException("boundsGSMin must be between[0...8]");
            }

            if (math.any(boundsGSSize < 0) || math.any(boundsGSMin + boundsGSSize > 8))
            {
                throw new ArgumentException("The bounds described need to be within [0...8]");
            }
#endif

            var result = new BitArray512();
            byte xSlice = 0;
            for (var x = 0; x < boundsGSSize.x; x++)
            {
                xSlice |= (byte) (1 << (x + boundsGSMin.x));
            }

            var resultAddress = &result;
            {
                for (var y = 0; y < boundsGSSize.y; y++)
                {
                    for (var z = 0; z < boundsGSSize.z; z++)
                    {
                        var byteIndex = y + boundsGSMin.y + (z + boundsGSMin.z) * 8;
                        SetByte(resultAddress, byteIndex, xSlice);
                    }
                }
            }

            return result;
        }

        private static unsafe void SetByte(BitArray512* ptr, int byteIndex, byte xSlice)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (byteIndex < 0 || byteIndex >= sizeof(BitArray512))
            {
                throw new ArgumentException("index must be between[0...64]");
            }
#endif
            UnsafeUtility.WriteArrayElement(ptr, byteIndex, xSlice);
        }

        #endregion

        #region Private Fields

        public ulong a;
        private ulong b;
        private ulong c;
        private ulong d;
        private ulong e;
        private ulong f;
        private ulong g;
        private ulong h;

        #endregion

        public unsafe bool this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 512)
                {
                    throw new ArgumentException("index must be between[0...511]");
                }
#endif
                fixed (BitArray512* array = &this)
                {
                    var valueByte = ((byte*) array)[index / 8];
                    return valueByte.GetBit(index % 8);
                }
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 512)
                {
                    throw new ArgumentException("index must be between[0...511]");
                }
#endif
                fixed (BitArray512* array = &this)
                {
                    var currentValue = ((byte*) array)[index / 8];
                    currentValue.SetBit(index % 8, value);
                    ((byte*) array)[index / 8] = currentValue;
                }
            }
        }

        #region Public methods

        /// <summary>
        ///     Set's all bits of the array to a specific value
        /// </summary>
        /// <param name="value">The value the bit's will have</param>
        public void Fill(bool value)
        {
            var bitMask = value ? ulong.MaxValue : ulong.MinValue;
            a = bitMask;
            b = bitMask;
            c = bitMask;
            d = bitMask;
            e = bitMask;
            f = bitMask;
            g = bitMask;
            h = bitMask;
        }

        #endregion

        public static readonly BitArray512 AllBitsTrue = new BitArray512
            {a = ulong.MaxValue, b = ulong.MaxValue, c = ulong.MaxValue, d = ulong.MaxValue, e = ulong.MaxValue, f = ulong.MaxValue, g = ulong.MaxValue, h = ulong.MaxValue};
    }
}