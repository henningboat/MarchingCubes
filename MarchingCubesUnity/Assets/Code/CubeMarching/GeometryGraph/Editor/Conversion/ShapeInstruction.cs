﻿using System;
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
    internal class ShapeInstruction : GeometryGraphResolverContext.GeometryGraphInstruction
    {
        public readonly ShapeType ShapeType;
        public readonly GeometryGraphProperty Position;
        public readonly List<GeometryGraphProperty> ShapeProperties;
        private CGeometryCombiner _combiner;

        public ShapeInstruction(ShapeType shapeType, GeometryGraphProperty position, List<GeometryGraphProperty> shapeProperties, int currentCombinerDepth,
            CGeometryCombiner combiner) : base(currentCombinerDepth)
        {
            _combiner = combiner;
            ShapeType = shapeType;
            Position = position;
            ShapeProperties = shapeProperties;
        }

        public override GeometryInstruction GetInstruction()
        {
            return new()
            {
                CombinerDepth = Depth,
                CoverageMask = BitArray512.AllBitsTrue,
                DependencyIndex = default, Combiner = _combiner,
                TerrainShape = new GeometryShapeTranslationTuple()
                {
                    Translation = new CGeometryTransformation(float3.zero),
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