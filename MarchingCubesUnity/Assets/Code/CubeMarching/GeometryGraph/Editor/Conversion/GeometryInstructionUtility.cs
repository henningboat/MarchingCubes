using System.Collections.Generic;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Runtime;
using Code.CubeMarching.TerrainChunkEntitySystem;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    internal static class GeometryInstructionUtility
    {
        public static GeometryInstruction CreateInstruction(GeometryInstructionType geometryInstructionType, int subType, int depth, CGeometryCombiner combiner, GeometryGraphProperty transformation,
            List<GeometryGraphProperty> shapeProperties)
        {
            var transformationValue = new Float4X4Value() {Index = transformation.Index};
            var propertyIndexes = new int16();
            for (int i = 0; i < shapeProperties.Count; i++)
            {
                propertyIndexes[i] = shapeProperties[i].Index;
            }

            return new GeometryInstruction()
            {
                GeometryInstructionType = geometryInstructionType,
                GeometryInstructionSubType = subType,
                Combiner = combiner,
                CombinerDepth = depth,
                TransformationValue = transformationValue,
                PropertyIndexes = propertyIndexes
            };
        }
    }
}