using System;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Unity.Mathematics;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    public class GeometryTransformationInstruction
    {
        public GeometryTransformationInstruction Parent { get; private set; }
        public GeometryTransformationType Type { get; private set; }
        public GeometryGraphProperty Value { get; private set; }
        public int Index { get; set; }

        public GeometryTransformationInstruction(GeometryTransformationType type, GeometryGraphProperty property, GeometryTransformationInstruction parent)
        {
            Type = type;
            Value = property;
            Parent = parent;
        }

        public static GeometryTransformationInstruction Origin(GeometryGraphProperty geometryGraphProperty)
        {
            return new(GeometryTransformationType.Origin, geometryGraphProperty, null);
        }
    }
}