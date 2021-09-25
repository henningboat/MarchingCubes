using System;
using System.Runtime.InteropServices;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Code.SIMDMath.SimdMath;


namespace Code.CubeMarching.GeometryComponents
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    [Serializable]
    public struct CShapeSphere : IComponentData, ITerrainModifierShape
    {
        public bool Equals(CShapeSphere other)
        {
            return radius.Equals(other.radius);
        }

        public override bool Equals(object obj)
        {
            return obj is CShapeSphere other && Equals(other);
        }

        public override int GetHashCode()
        {
            return radius.GetHashCode();
        }

        #region ActualData

        [FieldOffset(0)] public FloatValue radius;

        #endregion

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            var radiusValue = radius.Resolve(valueBuffer);
            return length(positionOS) - radiusValue;
        }

        public TerrainBounds CalculateBounds(Translation translation, NativeArray<float> valueBuffer)
        {
            var radiusValue = radius.Resolve(valueBuffer);
            var center = translation.Value;
            return new TerrainBounds
            {
                min = center - radiusValue, max = center + radiusValue
            };
        }

        public uint CalculateHash()
        {
            return math.asuint(radius.Index);
        }

        public ShapeType Type => ShapeType.Sphere;
    }

    public interface IValueReference
    {
        int GetDimensions();
    }

    public struct FloatValue
    {
        public int Index;

        public float Resolve(NativeArray<float> valueBuffer)
        {
            return valueBuffer[Index];
        }
    }

    public struct Float3Value
    {
        public int Index;

        public float3 Resolve(NativeArray<float> valueBuffer)
        {
            //todo probably better to handle this with pointer magic
            return new(valueBuffer[Index], valueBuffer[Index + 1], valueBuffer[Index] + 2);
        }
    }
}