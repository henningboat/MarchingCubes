using System;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    [Serializable]
    public struct CGeometryPropertyOverwrite : IBufferElementData
    {
        public Entity OverwritePropertyProvider;
        public GeometryPropertyType PropertyType;
        public int TargetIndex;
    }

    public struct CFloat4x4PropertyFromTransformation : IComponentData
    {
    }

    [ExecuteAlways]
    public  class SUpdateGraphProperties : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref CGeometryGraphPropertyValueProvider propertyValue, in LocalToWorld localToWorld, in CFloat4x4PropertyFromTransformation _) =>
            {
                var worldToLocal = math.inverse(localToWorld.Value);
                propertyValue.CopyFromFloat4x4(ref worldToLocal);

            }).ScheduleParallel();
        }
    }
}