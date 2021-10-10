using System;
using System.Collections.Generic;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    internal class ShapeInstruction : GeometryGraphInstruction
    {
        public readonly ShapeType ShapeType;
        public readonly GeometryGraphProperty Transformation;
        public readonly List<GeometryGraphProperty> ShapeProperties;
        private readonly CombinerInstruction _combiner;

        public ShapeInstruction(ShapeType shapeType, GeometryGraphProperty transformation, List<GeometryGraphProperty> shapeProperties, int currentCombinerDepth,
            CombinerInstruction combiner) : base(currentCombinerDepth)
        {
            ShapeType = shapeType;
            Transformation = transformation;
            ShapeProperties = shapeProperties;
            _combiner = combiner;
        }

        public override GeometryInstruction GetInstruction()
        {
            return GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape, (int) ShapeType, Depth, _combiner.GetCombinerSetting(), Transformation, ShapeProperties);
        }
    }
}