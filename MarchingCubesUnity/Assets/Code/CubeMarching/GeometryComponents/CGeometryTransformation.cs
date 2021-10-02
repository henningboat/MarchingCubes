using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static Code.SIMDMath.SimdMath;

namespace Code.CubeMarching.GeometryComponents
{
    //todo reimplement
    // [StructLayout(LayoutKind.Sequential)]
    // public struct CTerrainTransformationMirror : ITerrainTransformation
    // {
    //     public float3 PositionOffset;
    //     public bool3 Axis;
    //
    //     public uint CalculateHash()
    //     {
    //         var hash = math.hash(Axis);
    //         hash.AddToHash(math.hash(PositionOffset));
    //         return hash;
    //     }
    //
    //     public PackedFloat3 TransformPosition(PackedFloat3 positionWS)
    //     {
    //         if (Axis[0])
    //         {
    //             positionWS.x = abs(positionWS.x - PositionOffset.x) + PositionOffset.x;
    //         }
    //
    //         if (Axis[1])
    //         {
    //             positionWS.y = abs(positionWS.y - PositionOffset.y) + PositionOffset.y;
    //         }
    //
    //         if (Axis[2])
    //         {
    //             positionWS.z = abs(positionWS.z - PositionOffset.z) + PositionOffset.z;
    //         }
    //
    //         return positionWS;
    //     }
    //
    //     public TerrainTransformationType TerrainTransformationType => TerrainTransformationType.Mirror;
    // }
    //
    // [StructLayout(LayoutKind.Sequential, Size = 1)]
    // public struct CTerrainTransformationWave : ITerrainTransformation
    // {
    //     public PackedFloat3 TransformPosition(PackedFloat3 positionWS)
    //     {
    //         positionWS.y += sin(positionWS.x * 0.25f) * 4;
    //         return positionWS;
    //     }
    //
    //     public TerrainTransformationType TerrainTransformationType => TerrainTransformationType.Wave;
    // }


    public interface ITerrainTransformation
    {
        public TerrainTransformationType TerrainTransformationType { get; }
        public PackedFloat3 TransformPosition(PackedFloat3 positionWS, NativeArray<float> valueBuffer);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CGenericTerrainTransformation : IComponentData
    {
        public Bytes16 TerrainModifierDataA;
        public Bytes16 TerrainModifierDataB;
        public Bytes16 TerrainModifierDataC;
        public Bytes16 TerrainModifierDataD;
        public TerrainTransformationType TerrainTransformationType;

        public PackedFloat3 TransformPosition(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref TerrainModifierDataA);
                switch (TerrainTransformationType)
                {
                    //todo reimplement
                    // case TerrainTransformationType.Mirror:
                    //     return ((CTerrainTransformationMirror*) ptr)->TransformPosition(positionOS);
                    //     break;
                    case TerrainTransformationType.Repetition:
                        return ((CTerrainTransformationRepetition*) ptr)->TransformPosition(positionOS, valueBuffer);
                    case TerrainTransformationType.Transform:
                        return ((CGeometryTransformation*) ptr)->TransformPosition(positionOS, valueBuffer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public uint CalculateHash()
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref TerrainModifierDataA);
                switch (TerrainTransformationType)
                {
                    //todo reimplement
                    // case TerrainTransformationType.Mirror:
                    //     return ((CTerrainTransformationMirror*) ptr)->CalculateHash();
                    case TerrainTransformationType.Transform:
                        return ((CGeometryTransformation*) ptr)->CalculateHash();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public enum TerrainTransformationType
    {
        Mirror = 0,
        Wave = 1,
        Transform = 2,
        Repetition = 3
    }

    public struct CGeometryTransformation : IComponentData, ITerrainTransformation
    {
        public TerrainModifierTransformationType Type;
        public Float3Value objectOrigin;
        public float3x3 inverseRotationScaleMatrix;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat3 TransformPosition(PackedFloat3 positionWS, NativeArray<float> valueBuffer)
        {
            var objectOriginResolved = objectOrigin.Resolve(valueBuffer);

            switch (Type)
            {
                case TerrainModifierTransformationType.None:
                    return positionWS;

                case TerrainModifierTransformationType.TransformationOnly:
                    return positionWS - objectOriginResolved;

                case TerrainModifierTransformationType.TransformationAndUniformScale:
                    return (positionWS - objectOriginResolved) * inverseRotationScaleMatrix.c0.x;

                case TerrainModifierTransformationType.TransformationRotationAndScale:
                    var positionOS = positionWS - objectOriginResolved;

                    positionOS = mul(inverseRotationScaleMatrix.c0, inverseRotationScaleMatrix.c1, inverseRotationScaleMatrix.c2, positionOS);
                    return positionOS;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public TerrainTransformationType TerrainTransformationType => TerrainTransformationType.Transform;

        public static PackedFloat3 mul(PackedFloat3 c0, PackedFloat3 c1, PackedFloat3 c2, PackedFloat3 b)
        {
            return c0 * b.x + c1 * b.y + c2 * b.z;
        }

        public CGeometryTransformation(Float3Value positionWS)
        {
            Type = TerrainModifierTransformationType.TransformationOnly;
            objectOrigin = positionWS;
            inverseRotationScaleMatrix = default;
        }

        //todo reimplement
        // public CGeometryTransformation(float3 positionWS, float uniformScale)
        // {
        //     Type = TerrainModifierTransformationType.TransformationAndUniformScale;
        //     objectOrigin = positionWS;
        //     inverseRotationScaleMatrix = 1f / uniformScale;
        // }
        //
        // public CGeometryTransformation(float3 positionWS, quaternion rotation)
        // {
        //     Type = TerrainModifierTransformationType.TransformationRotationAndScale;
        //     objectOrigin = positionWS;
        //
        //     var inverseRotation = math.inverse(rotation);
        //     inverseRotationScaleMatrix = new float3x3(inverseRotation);
        // }
        //
        // public CGeometryTransformation(float3 positionWS, quaternion rotation, float3 scale)
        // {
        //     Type = TerrainModifierTransformationType.TransformationAndUniformScale;
        //     objectOrigin = positionWS;
        //
        //     var inverseRotation = math.inverse(rotation);
        //     var rotationMatrix = new float3x3(inverseRotation);
        //     var inverseScale = 1f / scale;
        //     inverseRotationScaleMatrix = math.mul(rotationMatrix, float3x3.Scale(inverseScale));
        // }

        // public static CGeometryTransformation GetFromTransform(Transform transform)
        // {
        //     //todo check for floating point precision issues
        //     var hasTransformation = transform.position != Vector3.zero;
        //     var hasRotation = transform.rotation == Quaternion.identity;
        //     var hasScale = transform.lossyScale == Vector3.one;
        //     var hasNonUniformScale = hasScale && Math.Abs(transform.lossyScale.x - transform.lossyScale.y) < math.EPSILON && Math.Abs(transform.lossyScale.x - transform.lossyScale.z) < math.EPSILON;
        //
        //     return new CGeometryTransformation(transform.position, transform.rotation);
        // }

        public uint CalculateHash()
        {
            var hash = math.hash(inverseRotationScaleMatrix);
            hash.AddToHash((uint) objectOrigin.Index);
            hash.AddToHash((uint) Type);
            return hash;
        }
    }

    public enum TerrainModifierTransformationType : byte
    {
        None,
        TransformationOnly,
        TransformationAndUniformScale,
        TransformationRotationAndScale
    }


    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct CTerrainTransformationRepetition : ITerrainTransformation
    {
        [SerializeField] public Float3Value Period;
        [SerializeField] public Float3Value Offset;

        public TerrainTransformationType TerrainTransformationType { get; }

        public PackedFloat3 TransformPosition(PackedFloat3 positionWS, NativeArray<float> valueBuffer)
        {
            return positionWS % new PackedFloat3(Period.Resolve(valueBuffer));
        }
    }
}