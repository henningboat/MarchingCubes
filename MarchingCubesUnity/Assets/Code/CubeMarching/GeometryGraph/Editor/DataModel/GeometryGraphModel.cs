using System;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel
{
    [Serializable]
    public class GeometryGraphModel : GraphModel
    {
        public GeometryGraphModel()
        {
            StencilType = null;
        }
        public override Type DefaultStencilType => typeof(GeometryGraphStencil);
    }
}
