using System;
using Unity.Entities;
using UnityEngine;

namespace Code.CubeMarching.Rendering
{
    public class MeshGenerator
    {
        public static CClusterMesh GenerateClusterMesh()
        {
            var clusterMesh = new Mesh();
            clusterMesh.name = "ClusterMesh";

            return new CClusterMesh {mesh = clusterMesh};
        }
    }

    [Serializable]
    public struct CClusterMesh : ISharedComponentData,IEquatable<CClusterMesh>
    {
        public bool Equals(CClusterMesh other)
        {
            return Equals(mesh, other.mesh);
        }

        public override bool Equals(object obj)
        {
            return obj is CClusterMesh other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (mesh != null ? mesh.GetHashCode() : 0);
        }

        public Mesh mesh;
        
    }
}