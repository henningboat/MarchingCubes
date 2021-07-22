using UnityEngine;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    public class DependencyVisualizer : MonoBehaviour
    {
        [SerializeField] private bool _showSubChunksOnly;

        private void OnDrawGizmos()
        {
            // foreach (var visualization in SShowCombinerLoad.GizmosVisualizations)
            // {
            //     if (_showSubChunksOnly ^ visualization.IsSubChunk)
            //     {
            //         Gizmos.color = Color.white;
            //         Gizmos.matrix = visualization.transformation;
            //         Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            //     }
            // }
        }
    }
}