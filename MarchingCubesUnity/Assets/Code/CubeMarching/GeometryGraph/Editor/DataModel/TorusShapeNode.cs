using System.Collections.Generic;
using Code.CubeMarching.GeometryComponents;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel
{
    public class TorusShapeNode : ShapeNode<CShapeTorus>
    {
        public override string Title
        {
            get => "Torus";
            set { }
        }

        public IPortModel RadiusIn { get; set; }
        public IPortModel Thickness { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            RadiusIn = this.AddDataInputPort<float>(nameof(RadiusIn), defaultValue: 8);
            Thickness = this.AddDataInputPort<float>(nameof(Thickness), defaultValue: 3);
        }

        protected override ShapeType GetShapeType()
        {
            return ShapeType.Torus;
        }

        public override List<GeometryGraphProperty> GetProperties()
        {
            return new() {RadiusIn.ResolvePropertyInput(GeometryPropertyType.Float), Thickness.ResolvePropertyInput(GeometryPropertyType.Float)};
        }
    }
}