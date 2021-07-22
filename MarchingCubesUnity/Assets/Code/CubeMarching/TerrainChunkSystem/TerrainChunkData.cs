using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Code.CubeMarching.TerrainChunkSystem
{
    /// <summary>
    ///     Contains 512 float values, accessible via indexer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TerrainChunkData
    {
        public PackedTerrainData this[int i]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (i < 0 || i >= PackedCapacity)
                {
                    throw new ArgumentOutOfRangeException($"SurfaceDistance was accessed with index {i}, must be between 0 and 127");
                }
#endif
                var pointer = UnsafeUtility.AddressOf(ref this);
                var arrayEntry = UnsafeUtility.ReadArrayElement<PackedTerrainData>(pointer, i);
                return arrayEntry;
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (i < 0 || i >= PackedCapacity)
                {
                    throw new ArgumentOutOfRangeException($"SurfaceDistance was accessed with index {i}, must be between 0 and 127");
                }
#endif
                var pointer = UnsafeUtility.AddressOf(ref this);
                var valueToWrite = value;
                //the plan is to clamp the distance field to a certain range, to limit the spacial influence of smaller objects
                //valueToWrite.SurfaceDistance = clamp(valueToWrite.SurfaceDistance, DefaultInsideValue, DefaultOutsideValue);
                UnsafeUtility.WriteArrayElement(pointer, i, valueToWrite);
            }
        }

        public TerrainData ReadUnpacked(int i)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (i < 0 || i >= UnPackedCapacity)
            {
                throw new ArgumentOutOfRangeException($"SurfaceDistance was accessed with index {i}, must be between 0 and 512");
            }
#endif
            var pointer = UnsafeUtility.AddressOf(ref this);
            var arrayEntry = UnsafeUtility.ReadArrayElement<TerrainData>(pointer, i);
            return arrayEntry;
        }


        public void WriteUnpacked(int i, TerrainData value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (i < 0 || i >= UnPackedCapacity)
            {
                throw new ArgumentOutOfRangeException($"SurfaceDistance was accessed with index {i}, must be between 0 and 512");
            }
#endif
            var pointer = UnsafeUtility.AddressOf(ref this);
            UnsafeUtility.WriteArrayElement(pointer, i, value);
        }

        public const int DefaultOutsideValue = 10;
        public const int DefaultInsideValue = -10;

        public TerrainData4x4 _valueBlock0;
        public TerrainData4x4 _valueBlock1;
        public TerrainData4x4 _valueBlock2;
        public TerrainData4x4 _valueBlock3;
        public TerrainData4x4 _valueBlock4;
        public TerrainData4x4 _valueBlock5;
        public TerrainData4x4 _valueBlock6;
        public TerrainData4x4 _valueBlock7;
        public TerrainData4x4 _valueBlock8;
        public TerrainData4x4 _valueBlock9;

        public TerrainData4x4 _valueBlock10;
        public TerrainData4x4 _valueBlock11;
        public TerrainData4x4 _valueBlock12;
        public TerrainData4x4 _valueBlock13;
        public TerrainData4x4 _valueBlock14;
        public TerrainData4x4 _valueBlock15;
        public TerrainData4x4 _valueBlock16;
        public TerrainData4x4 _valueBlock17;
        public TerrainData4x4 _valueBlock18;
        public TerrainData4x4 _valueBlock19;

        public TerrainData4x4 _valueBlock20;
        public TerrainData4x4 _valueBlock21;
        public TerrainData4x4 _valueBlock22;
        public TerrainData4x4 _valueBlock23;
        public TerrainData4x4 _valueBlock24;
        public TerrainData4x4 _valueBlock25;
        public TerrainData4x4 _valueBlock26;
        public TerrainData4x4 _valueBlock27;
        public TerrainData4x4 _valueBlock28;
        public TerrainData4x4 _valueBlock29;

        public TerrainData4x4 _valueBlock30;
        public TerrainData4x4 _valueBlock31;


        [StructLayout(LayoutKind.Sequential)]
        public struct TerrainData4x4
        {
            public PackedTerrainData _value0;
            public PackedTerrainData _value1;
            public PackedTerrainData _value2;
            public PackedTerrainData _value3;

            public static implicit operator TerrainData4x4(float value)
            {
                return new()
                {
                    _value0 = new PackedTerrainData(value),
                    _value1 = new PackedTerrainData(value),
                    _value2 = new PackedTerrainData(value),
                    _value3 = new PackedTerrainData(value)
                };
            }
        }

        public static TerrainChunkData Outside => new()
        {
            _valueBlock0 = DefaultOutsideValue,
            _valueBlock1 = DefaultOutsideValue,
            _valueBlock2 = DefaultOutsideValue,
            _valueBlock3 = DefaultOutsideValue,
            _valueBlock4 = DefaultOutsideValue,
            _valueBlock5 = DefaultOutsideValue,
            _valueBlock6 = DefaultOutsideValue,
            _valueBlock7 = DefaultOutsideValue,
            _valueBlock8 = DefaultOutsideValue,
            _valueBlock9 = DefaultOutsideValue,

            _valueBlock10 = DefaultOutsideValue,
            _valueBlock11 = DefaultOutsideValue,
            _valueBlock12 = DefaultOutsideValue,
            _valueBlock13 = DefaultOutsideValue,
            _valueBlock14 = DefaultOutsideValue,
            _valueBlock15 = DefaultOutsideValue,
            _valueBlock16 = DefaultOutsideValue,
            _valueBlock17 = DefaultOutsideValue,
            _valueBlock18 = DefaultOutsideValue,
            _valueBlock19 = DefaultOutsideValue,

            _valueBlock20 = DefaultOutsideValue,
            _valueBlock21 = DefaultOutsideValue,
            _valueBlock22 = DefaultOutsideValue,
            _valueBlock23 = DefaultOutsideValue,
            _valueBlock24 = DefaultOutsideValue,
            _valueBlock25 = DefaultOutsideValue,
            _valueBlock26 = DefaultOutsideValue,
            _valueBlock27 = DefaultOutsideValue,
            _valueBlock28 = DefaultOutsideValue,
            _valueBlock29 = DefaultOutsideValue,

            _valueBlock30 = DefaultOutsideValue,
            _valueBlock31 = DefaultOutsideValue
        };

        public static TerrainChunkData Inside => new()
        {
            _valueBlock0 = DefaultInsideValue,
            _valueBlock1 = DefaultInsideValue,
            _valueBlock2 = DefaultInsideValue,
            _valueBlock3 = DefaultInsideValue,
            _valueBlock4 = DefaultInsideValue,
            _valueBlock5 = DefaultInsideValue,
            _valueBlock6 = DefaultInsideValue,
            _valueBlock7 = DefaultInsideValue,
            _valueBlock8 = DefaultInsideValue,
            _valueBlock9 = DefaultInsideValue,

            _valueBlock10 = DefaultInsideValue,
            _valueBlock11 = DefaultInsideValue,
            _valueBlock12 = DefaultInsideValue,
            _valueBlock13 = DefaultInsideValue,
            _valueBlock14 = DefaultInsideValue,
            _valueBlock15 = DefaultInsideValue,
            _valueBlock16 = DefaultInsideValue,
            _valueBlock17 = DefaultInsideValue,
            _valueBlock18 = DefaultInsideValue,
            _valueBlock19 = DefaultInsideValue,

            _valueBlock20 = DefaultInsideValue,
            _valueBlock21 = DefaultInsideValue,
            _valueBlock22 = DefaultInsideValue,
            _valueBlock23 = DefaultInsideValue,
            _valueBlock24 = DefaultInsideValue,
            _valueBlock25 = DefaultInsideValue,
            _valueBlock26 = DefaultInsideValue,
            _valueBlock27 = DefaultInsideValue,
            _valueBlock28 = DefaultInsideValue,
            _valueBlock29 = DefaultInsideValue,

            _valueBlock30 = DefaultInsideValue,
            _valueBlock31 = DefaultInsideValue
        };

        public const int UnpackedElementSize = 8;

        public const int UnPackedCapacity = 512;

        public const int PackedCapacity = 128;

        public const int BufferSizeInBytes = UnPackedCapacity * UnpackedElementSize;
    }
}