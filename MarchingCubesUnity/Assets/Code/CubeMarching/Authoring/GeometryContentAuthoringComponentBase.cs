using System;
using System.Runtime.InteropServices;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Runtime;
using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Code.CubeMarching.Authoring
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGenericGeometryShape : IComponentData
    {
        #region Public Fields

        public int16 Data;
        public ShapeType ShapeType;

        #endregion

        #region Public methods

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref Data);
                switch (ShapeType)
                {
                    case ShapeType.Sphere:
                        return ((CShapeSphere*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    case ShapeType.BoundingBox:
                        return ((CShapeBoundingBox*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    case ShapeType.Torus:
                        return ((CShapeTorus*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    case ShapeType.Noise:
                        return ((CShapeNoise*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // public TerrainBounds CalculateBounds(Translation translation, NativeArray<float> valueBuffer)
        // {
        //     unsafe
        //     {
        //         var ptr = UnsafeUtility.AddressOf(ref Data);
        //         switch (ShapeType)
        //         {
        //             case ShapeType.Sphere:
        //                 return ((CShapeSphere*) ptr)->CalculateBounds(translation, valueBuffer);
        //                 break;
        //             case ShapeType.BoundingBox:
        //                 return ((CShapeBoundingBox*) ptr)->CalculateBounds(translation, valueBuffer);
        //                 break;
        //             case ShapeType.Torus:
        //                 return ((CShapeTorus*) ptr)->CalculateBounds(translation, valueBuffer);
        //                 break;
        //             case ShapeType.Noise:
        //                 return ((CShapeNoise*) ptr)->CalculateBounds(translation, valueBuffer);
        //                 break;
        //             default:
        //                 throw new ArgumentOutOfRangeException();
        //         }
        //     }
        //}

        // public unsafe uint CalculateHash()
        // {
        //     var ptr = UnsafeUtility.AddressOf(ref PropertyIndexesA);
        //     switch (ShapeType)
        //     {
        //         case ShapeType.Sphere:
        //             return ((CShapeSphere*) ptr)->CalculateHash();
        //             break;
        //         case ShapeType.BoundingBox:
        //             return ((CShapeBoundingBox*) ptr)->CalculateHash();
        //             break;
        //         case ShapeType.Torus:
        //             return ((CShapeTorus*) ptr)->CalculateHash();
        //             break;
        //         case ShapeType.Noise:
        //             return ((CShapeNoise*) ptr)->CalculateHash();
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        // }

        #endregion
    }
}