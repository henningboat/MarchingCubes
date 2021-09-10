using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Runtime;
using Unity.Entities;
using UnityEngine;

namespace Code.CubeMarching.GeometryComponents
{
    public class GeometryGraphHolder : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] private GeometryTree _tree;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddSharedComponentData(entity, new CGeometryGraphReference() {tree = _tree});
        }
    }

    public struct CGeometryGraphReference : ISharedComponentData, IEquatable<CGeometryGraphReference>
    {
        public GeometryTree tree;

        public bool Equals(CGeometryGraphReference other)
        {
            return tree == other.tree;
        }

        public override bool Equals(object obj)
        {
            return obj is CGeometryGraphReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return tree != null ? tree.GetHashCode() : 0;
        }
    }
}