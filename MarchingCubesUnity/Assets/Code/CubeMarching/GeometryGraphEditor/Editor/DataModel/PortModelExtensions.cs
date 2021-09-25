using System;
using System.Linq;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
{
    public static class PortModelExtensions
    {
        public static void ResolveGeometryInput(this IPortModel port, GeometryGraphResolverContext context)
        {
            var connectedPort = port.GetConnectedPorts().FirstOrDefault(model => model != null && model.PortDataType == typeof(DistanceFieldValue) && model.NodeModel != null);
            if (connectedPort != null && connectedPort.NodeModel is IGeometryNode geometryNode)
            {
                geometryNode.Resolve(context);
            }
        }

        public static GeometryGraphProperty ResolvePropertyInput(this IPortModel portModel, GeometryPropertyType geometryPropertyType)
        {
            if (portModel.EmbeddedValue != null)
            {
                return CreateConstant(portModel.EmbeddedValue, geometryPropertyType);
            }
            else
            {
                throw new Exception();
            }
        }

        private static GeometryGraphProperty CreateConstant(IConstant portModelEmbeddedValue, GeometryPropertyType geometryPropertyType)
        {
            return new GeometryGraphConstant(portModelEmbeddedValue.ObjectValue, geometryPropertyType);
        }
    }
}