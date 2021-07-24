using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Code.CubeMarching.TerrainChunkSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TerrainData
    {
        public float SurfaceDistance;
        public TerrainMaterial TerrainMaterial;

        public static readonly TerrainData DefaultOutside = new()
        {
            SurfaceDistance = TerrainChunkData.DefaultOutsideValue
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PackedTerrainData
    {
        public PackedFloat SurfaceDistance;
        public PackedTerrainMaterial TerrainMaterial;

        public PackedTerrainData(PackedFloat surfaceDistance, PackedTerrainMaterial packedTerrainMaterial = default) : this()
        {
            SurfaceDistance = surfaceDistance;
            TerrainMaterial = packedTerrainMaterial;
        }

        public const int UnpackedCapacity = 4;

        public TerrainData this[int i]
        {
            get
            {
                unsafe
                {

                    throw new NotImplementedException();
                    
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    if (i < 0 || i >= UnpackedCapacity)
                    {
                        throw new ArgumentOutOfRangeException($"SurfaceDistance was accessed with index {i}, must be between 0 and 127");
                    }
#endif
                    var pointer = UnsafeUtility.AddressOf(ref this);
                    var arrayEntry = UnsafeUtility.ReadArrayElement<TerrainData>(pointer, i);
                    return arrayEntry;
                }
            }
            set
            {
                unsafe
                {
                    throw new NotImplementedException();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    if (i < 0 || i >= UnpackedCapacity)
                    {
                        throw new ArgumentOutOfRangeException($"SurfaceDistance was accessed with index {i}, must be between 0 and 127");
                    }
#endif
                    var pointer = UnsafeUtility.AddressOf(ref this);
                    var valueToWrite = value;
                    UnsafeUtility.WriteArrayElement(pointer, i, valueToWrite);
                }
            }
        }
    }

    /// <summary>
    ///     Always make sure to keep the content of this struct in sync with TerrainMaterial.hlsl
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct TerrainMaterial
    {
        public byte MaterialID;
        public TerrainMaterialFlags Flags;
        public byte Padding0;
        public byte Padding1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsStatic()
        {
            return (Flags & TerrainMaterialFlags.Static) > 0;
        }

        public static TerrainMaterial GetSpecialMaterial()
        {
            return new() {MaterialID = 1};
        }

        public static TerrainMaterial GetDefaultMaterial()
        {
            return default;
        }
    }

    [Flags]
    public enum TerrainMaterialFlags : byte
    {
        Static = 1 << 0,
        DisplayExternalBordes = 1 << 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PackedTerrainMaterial
    {
        public TerrainMaterial a;
        public TerrainMaterial b;
        public TerrainMaterial c;
        public TerrainMaterial d;

        public PackedTerrainMaterial(TerrainMaterial material)
        {
            a = material;
            b = material;
            c = material;
            d = material;
        }

        public static PackedTerrainMaterial Select(PackedTerrainMaterial packedMaterialA, PackedTerrainMaterial packedMaterialB, bool4 selection)
        {
            return new()
            {
                a = selection.x ? packedMaterialB.a : packedMaterialA.a,
                b = selection.y ? packedMaterialB.b : packedMaterialA.b,
                c = selection.z ? packedMaterialB.c : packedMaterialA.c,
                d = selection.w ? packedMaterialB.d : packedMaterialA.d
            };
        }
    }
}