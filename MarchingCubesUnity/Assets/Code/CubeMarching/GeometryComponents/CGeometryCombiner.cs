using Unity.Entities;

namespace Code.CubeMarching.GeometryComponents
{
    public struct CGeometryCombiner : IComponentData
    {
        #region Public Fields

        public float BlendFactor;
        public CombinerOperation Operation;

        #endregion
    }
}