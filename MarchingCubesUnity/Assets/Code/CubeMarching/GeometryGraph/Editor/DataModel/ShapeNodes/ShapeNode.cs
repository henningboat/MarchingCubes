using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    [Serializable]
    public abstract class ShapeNode<T> : NodeModel, IGeometryNode where T : struct, ITerrainModifierShape
    {
        public IPortModel GeometryOut { get; set; }
        public IPortModel PositionIn { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            GeometryOut = this.AddDataOutputPort<DistanceFieldValue>(nameof(GeometryOut));
            PositionIn = this.AddDataInputPort<Vector3>(nameof(PositionIn));
        }

        public void WriteGeometryInstruction(ref GeometryInstruction instruction)
        {
        }

        protected abstract ShapeType GetShapeType();

        public unsafe GeometryInstruction GetTerrainInstruction()
        {
            //
            // CGenericTerrainModifier genericComponentData = default;
            // genericComponentData.ShapeType = GetShapeType();
            //
            // var ptr = UnsafeUtility.AddressOf(ref genericComponentData.TerrainModifierDataA);
            // UnsafeUtility.CopyStructureToPtr(ref shape, ptr);
            //
            // var position = PositionIn.GetValue3();
            //
            // return new GeometryInstruction()
            // {
            //     CombinerDepth = 0,
            //     CoverageMask = BitArray512.AllBitsTrue,
            //     DependencyIndex = 0,
            //     Combiner = default,
            //     TerrainShape = new GeometryShapeTranslationTuple() {Translation = new CGeometryTransformation(position), TerrainMaterial = default, TerrainModifier = genericComponentData},
            //     TerrainTransformation = default,
            //     WorldToLocal = new WorldToLocal() {Value = float4x4.identity},
            //     TerrainInstructionType = TerrainInstructionType.Shape
            // };
            throw new Exception();
        }

        public abstract List<GeometryGraphProperty> GetProperties(GeometryGraphResolverContext context);

        public void Resolve(GeometryGraphResolverContext context)
        {
            context.WriteShape(GetShapeType(), PositionIn.ResolvePropertyInput(context, GeometryPropertyType.Float3), GetProperties(context));
        }
    }
}