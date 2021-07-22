using Code.CubeMarching.GeometryComponents;
using Unity.Mathematics;
using UnityEngine;

namespace Code.CubeMarching.Authoring
{
    public class TerrainTransformationMirror : TerrainTransformationBase<CTerrainTransformationMirror>
    {
        #region Serialize Fields

        [SerializeField] private bool3 _axis;

        #endregion

        #region Unity methods

        private void OnDrawGizmosSelected()
        {
            Color[] colors = {new(1, 0, 0, 0.2f), new(0, 1, 0, 0.2f), new(0, 0, 1, 0.2f)};
            float3[] axis = {math.right(), math.up(), math.forward()};

            for (var i = 0; i < axis.Length; i++)
            {
                if (_axis[i])
                {
                    Gizmos.color = colors[i];
                    Gizmos.matrix = float4x4.TRS(transform.position, quaternion.LookRotation(axis[i], math.right()), 1);
                    Gizmos.DrawCube(Vector3.zero, new float3(1000000, 1000000, 0.001f));
                }
            }
        }

        #endregion

        #region Protected methods

        protected override CTerrainTransformationMirror GetComponentData()
        {
            return new() {Axis = _axis, PositionOffset = transform.position};
        }

        #endregion
    }
}