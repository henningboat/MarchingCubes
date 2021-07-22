using UnityEngine;

namespace Code.CubeMarching
{
    [CreateAssetMenu]
    public class DynamicCubeMarchingSettingsHolder : ScriptableObject
    {
        private static DynamicCubeMarchingSettingsHolder _instance; 
        public ComputeShader Compute;
        public Material[] Materials;

        public static DynamicCubeMarchingSettingsHolder Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<DynamicCubeMarchingSettingsHolder>("DynamicCubeMarchingSettingsHolder");
                }

                return _instance;
            }
        }
    }
}