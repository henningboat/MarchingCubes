using Code.CubeMarching.GeometryComponents;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
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

            RadiusIn = this.AddDataInputPort<float>(nameof(RadiusIn), defaultValue:  8);
            Thickness = this.AddDataInputPort<float>(nameof(Thickness), defaultValue:  3);
        }

        protected override TerrainModifierType GetShapeType()
        {
            return TerrainModifierType.Torus;
        }

        protected override CShapeTorus GetShape()
        {
            return new() {radius = RadiusIn.GetValue(), thickness = Thickness.GetValue()};
        }
    }
}