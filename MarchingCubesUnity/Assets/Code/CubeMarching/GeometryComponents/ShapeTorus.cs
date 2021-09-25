using System;
using System.Runtime.InteropServices;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Code.CubeMarching.GeometryComponents
{
    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeTorus : IComponentData, ITerrainModifierShape
    {
        [FieldOffset(0)] public FloatValue radius;
        [FieldOffset(4)] public FloatValue thickness;

        //SDF code from
        //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
        private PackedFloat sdTorus(PackedFloat3 p, PackedFloat radius, PackedFloat thickness)
        {
            var q = new PackedFloat2(SimdMath.length(new PackedFloat2(p.x, p.z)) - radius, p.y);
            return SimdMath.length(q) - thickness;
        }

        public TerrainBounds CalculateBounds(Translation translation, NativeArray<float> valueBuffer)
        {
            var center = translation.Value;
            var extends = radius.Resolve(valueBuffer) + thickness.Resolve(valueBuffer);
            return new TerrainBounds
            {
                min = center - extends, max = center + extends
            };
        }

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            return sdTorus(positionOS, radius.Resolve(valueBuffer), thickness.Resolve(valueBuffer));
        }

        public uint CalculateHash()
        {
            return math.hash(new float2(radius.Index, thickness.Index));
        }

        public ShapeType Type => ShapeType.Torus;
    }
}