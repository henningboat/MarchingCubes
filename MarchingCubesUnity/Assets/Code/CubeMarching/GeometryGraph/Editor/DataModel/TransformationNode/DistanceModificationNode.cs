using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    public abstract class DistanceModificationNode : NodeModel, IGeometryNode
    {
        private IPortModel _geometryInput;
        private IPortModel _geometryOutput;

        protected override void OnDefineNode()
        {
            _geometryInput = this.AddDataInputPort<DistanceFieldValue>("", nameof(_geometryInput));
            _geometryOutput = this.AddDataOutputPort<DistanceFieldValue>("", nameof(_geometryInput));
        }


        public void Resolve(GeometryGraphResolverContext context,GeometryGraphProperty transformation)
        {
            _geometryInput.ResolveGeometryInput(context, transformation);
            context.WriteDistanceModifier(GetDistanceModifierInstruction(context));
        }

        protected abstract DistanceModifierInstruction GetDistanceModifierInstruction(GeometryGraphResolverContext geometryGraphResolverContext);
    }

    public class OnioningNode : DistanceModificationNode
    {
        private IPortModel _thicknessInput;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            _thicknessInput = this.AddDataInputPort<float>("Thickness", nameof(_thicknessInput), defaultValue: 2);
        }

        protected override DistanceModifierInstruction GetDistanceModifierInstruction(GeometryGraphResolverContext context)
        {
            return new OnionDistanceModifierInstruction(_thicknessInput.ResolvePropertyInput(context, GeometryPropertyType.Float), context);
        }
    }

    public abstract class DistanceModifierInstruction:GeometryGraphInstruction
    {
        protected DistanceModifierInstruction(int depth) : base(depth)
        {
        }
    }
}