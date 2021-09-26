using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    [Serializable]
    public class GeometryGraphPropertyOverwrite
    {
        [SerializeField] private SerializableGUID _propertyGUID;
        [SerializeField] private float[] _value;
        [SerializeField] private MonoBehaviour _providerObject;

        public GeometryGraphPropertyOverwrite(SerializableGUID guid)
        {
            _propertyGUID = guid;
        }

        public SerializableGUID PropertyGUID => _propertyGUID;

        public MonoBehaviour ProviderObject => _providerObject;

        public float[] Value => _value;

        public void SetValueCapacity(int length)
        {
            if (_value == null || _value.Length != length)
            {
                _value = new float[length];
            }
        }
    }
}