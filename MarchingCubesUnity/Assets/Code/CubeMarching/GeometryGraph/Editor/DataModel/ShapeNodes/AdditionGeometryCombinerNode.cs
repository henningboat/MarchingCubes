using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    public class AdditionGeometryCombinerNode : SymmetricalGeometryCombinerNode
    {
        protected override CombinerOperation CombinerOperation => CombinerOperation.Add;
    }
}