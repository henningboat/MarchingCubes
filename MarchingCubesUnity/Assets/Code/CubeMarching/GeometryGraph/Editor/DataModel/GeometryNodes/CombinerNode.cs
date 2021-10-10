using System;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using Port = UnityEditor.Experimental.GraphView.Port;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    [Serializable]
    public class CombinerNode : GeometryCombinerNode,IMultipleExecutionInputs
    {
        public IPortModel BlendModeInput { get; set; }
        public IPortModel GeometryInputA { get; set; }
        public IPortModel GeometryInputB { get; set; }
        public IPortModel BlendFactor { get; set; }

        public override void Resolve(GeometryGraphResolverContext context, GeometryGraphProperty transformation)
        {
            var blendFactorProperty = BlendFactor.ResolvePropertyInput(context, GeometryPropertyType.Float);
            context.BeginWriteCombiner(new CombinerInstruction(EvaluateCombinerOperation(), blendFactorProperty, context.CurrentCombinerDepth));
            GeometryInputA.ResolveGeometryInput(context, transformation);
            GeometryInputB.ResolveGeometryInput(context, transformation);

            context.FinishWritingCombiner();
        }

        private CombinerOperation EvaluateCombinerOperation()
        {
            var combinerOperation = (CombinerOperation) ((EnumValueReference) BlendModeInput.EmbeddedValue.ObjectValue).Value;
            return combinerOperation;
        }

        public override string Title
        {
            get => Enum.GetName(typeof(CombinerOperation), EvaluateCombinerOperation());
            set { }
        }
        
        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            BlendModeInput = this.AddDataInputPort<CombinerOperation>("Blend Mode", nameof(BlendModeInput));

            GeometryInputA = this.AddExecutionInputPort("", nameof(GeometryInputA));
            GeometryInputB = this.AddExecutionInputPort("", nameof(GeometryInputB));


            BlendFactor = this.AddDataInputPort<float>("BlendFactor", nameof(BlendFactor));
        }
    }

    public interface IMultipleExecutionInputs
    {
    }
}