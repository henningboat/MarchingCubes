using System.Runtime.InteropServices;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Code.CubeMarching.GeometryComponents
{
    public class ShapeTorus : GeometryContentAuthoringComponentBase<CShapeTorus>
    {
        [SerializeField] private float _thickness = 4;

        protected override CShapeTorus GetShape()
        {
            return new()
            {
                radius = transform.lossyScale.x,
                thickness = _thickness
            };
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    public struct CShapeTorus : IComponentData, ITerrainModifierShape
    {
        [FieldOffset(0)] public float radius;
        [FieldOffset(4)] public float thickness;

        //SDF code from
        //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
        private PackedFloat sdTorus(PackedFloat3 p, PackedFloat radius, PackedFloat thickness)
        {
            var q = new PackedFloat2(SimdMath.length(new PackedFloat2(p.x, p.z)) - radius, p.y);
            return SimdMath.length(q) - thickness;
        }

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            return sdTorus(positionOS, radius, thickness);
        }

        public TerrainBounds CalculateBounds(Translation translation)
        {
            var center = translation.Value;
            var extends = radius + thickness;
            return new TerrainBounds
            {
                min = center - extends, max = center + extends
            };
        }

        public TerrainModifierType Type => TerrainModifierType.Torus;
    }
}