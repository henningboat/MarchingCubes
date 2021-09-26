using Code.CubeMarching.GeometryGraph.Runtime;
using Unity.Entities;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class GeometryGraphHolderConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((GeometryGraphHolder holder) =>
            {
                if (holder.Graph != null)
                {
                    DeclareAssetDependency(holder.gameObject, holder.Graph);
                }
            });
        }
    }
}