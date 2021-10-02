using System;
using System.Collections.Generic;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Graphs;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    public class TransformationInstruction : GeometryGraphInstruction
    {
        public readonly TerrainTransformationType TransformationType;
        public readonly GeometryGraphProperty[] TransformationProperties;

        public TransformationInstruction(TerrainTransformationType transformationType, int currentCombinerDepth, params GeometryGraphProperty[] transformationProperties) : base(currentCombinerDepth)
        {
            TransformationType = transformationType;
            TransformationProperties = transformationProperties;
        }

        public override GeometryInstruction GetInstruction()
        {
            return new()
            {
                CombinerDepth = Depth,
                CoverageMask = BitArray512.AllBitsTrue,
                DependencyIndex = default, 
                TerrainTransformation = BuildGenericTerrainModifier(),
                TerrainInstructionType = TerrainInstructionType.Transformation
            };
        }

        private CGenericTerrainTransformation BuildGenericTerrainModifier()
        {
            unsafe
            {
                var shape = new CGenericTerrainTransformation();
                shape.TerrainTransformationType = TransformationType;
                if (TransformationProperties.Length > 16)
                {
                    throw new ArgumentOutOfRangeException("There's no support for more than 16 properties");
                }

                for (var i = 0; i < TransformationProperties.Length; i++)
                {
                    UnsafeUtility.WriteArrayElement((int*) UnsafeUtility.AddressOf(ref shape), i, TransformationProperties[i].Index);
                }

                return shape;
            }
        }
    }
}