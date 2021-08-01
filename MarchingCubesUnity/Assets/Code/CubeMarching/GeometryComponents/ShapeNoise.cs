using System.Runtime.InteropServices;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Code.CubeMarching.GeometryComponents
{
    public class ShapeNoise : GeometryContentAuthoringComponentBase<CShapeNoise>
    {
        [SerializeField] private float _strength;
        [SerializeField] private float _valueOffse;

        protected override CShapeNoise GetShape()
        {
            return new()
            {
                offset = transform.position,
                scale = new float3(1f) / transform.lossyScale,
                strength = _strength,
                valueOffset = _valueOffse
            };
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeNoise : IComponentData, ITerrainModifierShape
    {
        [FieldOffset(0)] public float strength;
        [FieldOffset(4)] public float valueOffset;
        [FieldOffset(8)] public float3 offset;
        [FieldOffset(20)] public float3 scale;

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionWS)
        {
            var positionOS = scale * (positionWS - offset);
            return (cnoise4(positionOS) + valueOffset) * strength;
        }

        public TerrainBounds CalculateBounds(Translation translation)
        {
            return new() {min = int.MinValue, max = int.MaxValue};
        }

        public uint CalculateHash()
        {
            return math.hash(new float4x2(new float4(strength, offset), new float4(valueOffset, scale)));
        }

        public TerrainModifierType Type => TerrainModifierType.Noise;

        private PackedFloat cnoise4(PackedFloat3 input)
        {
            PackedFloat result = default;
            result.PackedValues[0] = NoiseSlice(input, 0);
            result.PackedValues[1] = NoiseSlice(input, 1);
            result.PackedValues[2] = NoiseSlice(input, 2);
            result.PackedValues[3] = NoiseSlice(input, 3);
            return result;
        }

        private static float NoiseSlice(PackedFloat3 input, int slice)
        {
            return noise.cnoise(-new float3(input.x.PackedValues[slice], input.y.PackedValues[slice], input.z.PackedValues[slice]));
        }
    }
}