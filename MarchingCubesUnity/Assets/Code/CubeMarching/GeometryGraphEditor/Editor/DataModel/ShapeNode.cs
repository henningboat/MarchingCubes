using System;
using System.Collections.Generic;
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

        public abstract List<GeometryGraphProperty> GetProperties();

        public void Resolve(GeometryGraphResolverContext context)
        {
            context.WriteShape(GetShapeType(), PositionIn.ResolvePropertyInput(GeometryPropertyType.Float3), GetProperties());
        }
    }

    public abstract class GeometryGraphProperty
    {
        public int Index;

        public readonly GeometryPropertyType Type;

        public int GetSizeInBuffer()
        {
            return Type switch
            {
                GeometryPropertyType.Float => 1,
                GeometryPropertyType.Float3 => 3,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected GeometryGraphProperty(GeometryPropertyType type)
        {
            Type = type;
        }
    }

    public class GeometryGraphConstant : GeometryGraphProperty
    {
        public readonly object ConstantValue;

        public GeometryGraphConstant(object objectValue, GeometryPropertyType geometryPropertyType) : base(geometryPropertyType)
        {
            ConstantValue = objectValue;
        }
    }

    public abstract class GeometryCombinerNode : NodeModel, IGeometryNode
    {
        public IPortModel GeometryOut { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            GeometryOut = this.AddDataOutputPort<DistanceFieldValue>(null, nameof(GeometryOut));
        }

        public abstract void Resolve(GeometryGraphResolverContext context);
    }

    public interface IGeometryNode
    {
        void Resolve(GeometryGraphResolverContext context);
    }

    public abstract class SymmetricalGeometryCombinerNode : GeometryCombinerNode
    {
        protected abstract CombinerOperation CombinerOperation { get; }
        public IPortModel GeometryInputA { get; set; }
        public IPortModel GeometryInputB { get; set; }

        public override void Resolve(GeometryGraphResolverContext context)
        {
            context.BeginWriteCombiner(new CGeometryCombiner() {Operation = CombinerOperation});
            GeometryInputA.ResolveGeometryInput(context);
            GeometryInputB.ResolveGeometryInput(context);
            context.FinishWritingCombiner(CombinerOperation, new GeometryGraphConstant(0f, GeometryPropertyType.Float));
        }

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
    public class MinGeometryCombinerNode : SymmetricalGeometryCombinerNode
    {
        protected override CombinerOperation CombinerOperation => CombinerOperation.Min;
    } 
    public class SubtractGeometryCombinerNode : SymmetricalGeometryCombinerNode
    {
        protected override CombinerOperation CombinerOperation => CombinerOperation.SmoothSubtract;
    } 
}