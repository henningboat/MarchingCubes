using System;
using System.Diagnostics.SymbolStore;
using Code.CubeMarching;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
{
    [Serializable]
    public abstract class ShapeNode<T>  : NodeModel,IShapeNode where T: struct, ITerrainModifierShape
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

        protected abstract TerrainModifierType GetShapeType();
        protected abstract T GetShape();

        public unsafe GeometryInstruction GetTerrainInstruction()
        {
            T shape = GetShape();
            
            CGenericTerrainModifier genericComponentData = default;
            genericComponentData.TerrainModifierType = GetShapeType();

            var ptr = UnsafeUtility.AddressOf(ref genericComponentData.TerrainModifierDataA);
            UnsafeUtility.CopyStructureToPtr(ref shape, ptr);

            var position = PositionIn.GetValue3();
            
            return new GeometryInstruction()
            {
                CombinerDepth = 0,
                CoverageMask = BitArray512.AllBitsTrue,
                DependencyIndex = 0,
                Combiner = default,
                TerrainShape = new GeometryShapeTranslationTuple() {Translation = new CGeometryTransformation(position), TerrainMaterial = default, TerrainModifier = genericComponentData},
                TerrainTransformation = default,
                WorldToLocal = new WorldToLocal() {Value = float4x4.identity},
                TerrainInstructionType = TerrainInstructionType.Shape
            };
        }
    }

    public interface IShapeNode
    {
        GeometryInstruction GetTerrainInstruction();
    }


    public abstract class GeometryCombinerNode : NodeModel
    {
        public IPortModel GeometryOut { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            GeometryOut = this.AddDataOutputPort<DistanceFieldValue>(null, nameof(GeometryOut));
        }
    }

    public abstract class SymmetricalGeometryCombinerNode : GeometryCombinerNode
    {
        protected abstract CombinerOperation CombinerOperation { get; }
        public IPortModel GeometryInputA { get; set; }
        public IPortModel GeometryInputB { get; set; }

        
        public override string Title
        {
            get => CombinerOperation.ToString();
            set { }
        }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            GeometryInputA = this.AddDataInputPort<DistanceFieldValue>("", nameof(GeometryInputA), PortOrientation.Horizontal, PortModelOptions.NoEmbeddedConstant);
            GeometryInputB = this.AddDataInputPort<DistanceFieldValue>("", nameof(GeometryInputB), PortOrientation.Horizontal, PortModelOptions.NoEmbeddedConstant);
        }
    }

    public class AdditionGeometryCombinerNode : SymmetricalGeometryCombinerNode
    {
        protected override CombinerOperation CombinerOperation => CombinerOperation.Add;
    }
}