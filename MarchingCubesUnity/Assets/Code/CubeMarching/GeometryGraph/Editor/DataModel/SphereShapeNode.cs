using System.Collections.Generic;
using Code.CubeMarching.GeometryComponents;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel
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

        public override List<GeometryGraphProperty> GetProperties()
        {
            return new() {RadiusIn.ResolvePropertyInput(GeometryPropertyType.Float)};
        }
    }

    public enum GeometryPropertyType
    {
        Float,
        Float3
    }
}