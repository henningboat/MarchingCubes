using System.Collections.Generic;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    public class SphereShapeNode : ShapeNode<CShapeSphere>
    {
        public override string Title
        {
            get => "Sphere";
            set { }
        }

        public IPortModel RadiusIn { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            RadiusIn = this.AddDataInputPort<float>(nameof(RadiusIn), defaultValue: 8);
        }

        protected override ShapeType GetShapeType()
        {
            return ShapeType.Sphere;
        }

        public override List<GeometryGraphProperty> GetProperties(GeometryGraphResolverContext context)
        {
            return new() {RadiusIn.ResolvePropertyInput(context, GeometryPropertyType.Float)};
        }
    }
}