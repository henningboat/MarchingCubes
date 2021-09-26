using Code.CubeMarching.GeometryComponents;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public class SmoothMinGeometryCombinerNode : SymmetricalGeometryCombinerNode
    {
        protected override CombinerOperation CombinerOperation => CombinerOperation.SmoothMin;
    }
}