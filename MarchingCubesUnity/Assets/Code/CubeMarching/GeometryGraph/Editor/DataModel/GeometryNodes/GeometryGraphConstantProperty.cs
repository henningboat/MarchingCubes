using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public class GeometryGraphConstantProperty : GeometryGraphProperty
    {
        public GeometryGraphConstantProperty(object objectValue, GeometryGraphResolverContext context, GeometryPropertyType geometryPropertyType, string debugInformation) : base(geometryPropertyType,
            debugInformation)
        {
            Value = objectValue;
        }
    }

    public class GeometryGraphMathOperatorProperty : GeometryGraphProperty
    {
        public MathOperatorType OperatorType { get; }
        public GeometryGraphProperty A { get; }
        public GeometryGraphProperty B { get; }

        public GeometryGraphMathOperatorProperty(GeometryGraphResolverContext context, GeometryPropertyType type, MathOperatorType operatorType, GeometryGraphProperty a, GeometryGraphProperty b,
            string debugInformation) : base(type, debugInformation)
        {
            OperatorType = operatorType;
            A = a;
            B = b;
        }

        public MathInstruction GetMathInstruction()
        {
            return new()
            {
                MathOperationType = OperatorType,
                InputAIndex = A.Index,
                InputAType = A.Type,
                InputBIndex = B?.Index ?? 0,
                InputBType = B?.Type ?? default,
                ResultIndex = Index,
                ResultType = Type,
            };
        }
    }
}