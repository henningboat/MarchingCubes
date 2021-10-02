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
        public readonly GeometryTransformationInstruction Transformation;
        public readonly List<GeometryGraphProperty> ShapeProperties;
        private CombinerInstruction _combiner;

        public ShapeInstruction(ShapeType shapeType, GeometryTransformationInstruction transformation, List<GeometryGraphProperty> shapeProperties, int currentCombinerDepth,
            CombinerInstruction combiner) : base(currentCombinerDepth)
        {
            _combiner = combiner;
            ShapeType = shapeType;
            Transformation = transformation;
            ShapeProperties = shapeProperties;
        }

        public override GeometryInstruction GetInstruction()
        {
            return new()
            {
                CombinerDepth = Depth,
                CoverageMask = BitArray512.AllBitsTrue,
                DependencyIndex = default, Combiner = _combiner.GetCombinerSetting(),
                TerrainShape = new GeometryShapeTranslationTuple()
                {
                    //todo
                   // Translation = new CGeometryTransformation(new Float3Value() {Index = Position.Index}),
                    TerrainMaterial = default,
                    TerrainModifier = BuildGenericTerrainModifier()
                },
                TerrainInstructionType = TerrainInstructionType.Shape
            };
        }

        private CGenericTerrainModifier BuildGenericTerrainModifier()
        {
            unsafe
            {
                var shape = new CGenericTerrainModifier();
                shape.ShapeType = ShapeType;
                if (ShapeProperties.Count > 16)
                {
                    throw new ArgumentOutOfRangeException("There's no support for more than 16 properties");
                }

                for (var i = 0; i < ShapeProperties.Count; i++)
                {
                    UnsafeUtility.WriteArrayElement((int*) UnsafeUtility.AddressOf(ref shape), i, ShapeProperties[i].Index);
                }

                return shape;
            }
        }
    }
}