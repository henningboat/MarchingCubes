using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
{
    public abstract class ShapeNode<T> : NodeModel where T:struct
    {
        public override string Title
        {
            get => nameof(T); set{} 
        }
    }
}