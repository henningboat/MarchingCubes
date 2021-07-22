using System;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.GeometryComponents;
using Code.SIMDMath;
using Unity.Mathematics;
using static Code.SIMDMath.SimdMath;

namespace Code.CubeMarching.TerrainChunkSystem
{
    public static class TerrainChunkOperations
    {
        public static void CombineWithChunk(this ref TerrainChunkData target, ref TerrainChunkData source, CGeometryCombiner combiner,
            byte conerageMask)
        {
            for (int i = 0; i < 8; i++)
            {
                if ((conerageMask & 1 << i) != 0)
                {
                    int baseOffset = i * 16;
                    for (var j = 0; j < 16; j++)
                    {
                        var offset = j + baseOffset;
                        var valuesA = source[offset];
                        var valuesB = target[offset];

                        var packedTerrainData = CombinePackedTerrainData(combiner, valuesA, valuesB);

                        target[offset] = packedTerrainData;
                    }
                }
            }
        }
        
        public static PackedTerrainData CombinePackedTerrainData(CGeometryCombiner combiner, PackedTerrainData valuesA, PackedTerrainData valuesB)
        {
            PackedTerrainData packedTerrainData;
            switch (combiner.Operation)
            {
                case CombinerOperation.Min:
                    packedTerrainData = CombineTerrainMin(valuesA, valuesB);
                    break;
                case CombinerOperation.Max:
                    packedTerrainData = CombineTerrainMax(valuesA, valuesB);
                    break;
                case CombinerOperation.SmoothMin:
                    packedTerrainData = CombineTerrainSmoothMin(valuesA, valuesB, combiner.BlendFactor);
                    break;
                case CombinerOperation.SmoothSubtract:
                    packedTerrainData = CombineTerrainSmoothSubtract(valuesA, valuesB, combiner.BlendFactor);
                    break;
                case CombinerOperation.Add:
                    packedTerrainData = CombineTerrainAdd(valuesA, valuesB);
                    break;
                //todo this feels like a workaround
                case CombinerOperation.Replace:
                    packedTerrainData = valuesA;
                    break;
                case CombinerOperation.ReplaceMaterial:
                    packedTerrainData = ReplaceTerrainColor(valuesA, valuesB);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return packedTerrainData;
        }

        private static PackedTerrainData ReplaceTerrainColor(PackedTerrainData a, PackedTerrainData b)
        {
            bool4 replaceTerrainMaterial = a.SurfaceDistance.PackedValues > 0;
            return new PackedTerrainData {SurfaceDistance = b.SurfaceDistance, TerrainMaterial = PackedTerrainMaterial.Select(b.TerrainMaterial, a.TerrainMaterial, replaceTerrainMaterial)};
        }

        public static PackedTerrainData CombineTerrainMin(PackedTerrainData a, PackedTerrainData b)
        {
            bool4 bIsSmaller = a.SurfaceDistance.PackedValues > b.SurfaceDistance.PackedValues;
            float4 surfaceDistance = math.min(a.SurfaceDistance.PackedValues, b.SurfaceDistance.PackedValues);
            var combinedMaterial = PackedTerrainMaterial.Select(a.TerrainMaterial, b.TerrainMaterial, bIsSmaller);

            return new PackedTerrainData(new PackedFloat(surfaceDistance), combinedMaterial);
        }

        public static PackedTerrainData CombineTerrainMax(PackedTerrainData a, PackedTerrainData b)
        {
            bool4 bIsBigger = a.SurfaceDistance.PackedValues < b.SurfaceDistance.PackedValues;
            float4 surfaceDistance = math.@select(a.SurfaceDistance.PackedValues, b.SurfaceDistance.PackedValues, bIsBigger);
            var combinedMaterial = PackedTerrainMaterial.Select(a.TerrainMaterial, b.TerrainMaterial, !bIsBigger);

            return new PackedTerrainData(new PackedFloat(surfaceDistance), combinedMaterial);
        }

        public static PackedTerrainData CombineTerrainAdd(PackedTerrainData a, PackedTerrainData b)
        {
            return new PackedTerrainData(a.SurfaceDistance + b.SurfaceDistance, a.TerrainMaterial);
        }

        //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
        public static PackedTerrainData CombineTerrainSmoothMin(PackedTerrainData terrainDataA, PackedTerrainData terrainDataB, PackedFloat blendFactor)
        {
            var a = terrainDataA.SurfaceDistance;
            var b = terrainDataB.SurfaceDistance;
            var h = clamp(0.5f + 0.5f * (a - b) / blendFactor, 0.0f, 1.0f);
            var blendedSurfaceDistance = lerp(a, b, h) - blendFactor * h * (1.0f - h);
            
            bool4 bIsSmaller = terrainDataA.SurfaceDistance.PackedValues > terrainDataB.SurfaceDistance.PackedValues;
            var combinedMaterial = PackedTerrainMaterial.Select(terrainDataA.TerrainMaterial, terrainDataB.TerrainMaterial, bIsSmaller);
            
            return new PackedTerrainData(blendedSurfaceDistance, combinedMaterial);
        }

        private static PackedTerrainData CombineTerrainSmoothSubtract(PackedTerrainData terrainDataA, PackedTerrainData terrainDataB, PackedFloat blendFactor)
        {
            var a = terrainDataA.SurfaceDistance;
            var b = terrainDataB.SurfaceDistance;
            var h = clamp(0.5f - 0.5f * (b + a) / blendFactor, 0.0f, 1.0f);
            var blendedSurfaceDistance = lerp(b, -a, h) + blendFactor * h * (1.0f - h);
            
            bool4 bIsSmaller = terrainDataA.SurfaceDistance.PackedValues > terrainDataB.SurfaceDistance.PackedValues;
            var combinedMaterial = PackedTerrainMaterial.Select(terrainDataA.TerrainMaterial, terrainDataB.TerrainMaterial, bIsSmaller);

            return new PackedTerrainData(blendedSurfaceDistance, combinedMaterial);
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TerrainChunkData FilterStatic(this TerrainChunkData data)
        {
            for (int i = 0; i < TerrainChunkData.PackedCapacity; i++)
            {
                var packedTerrainData = data[i];
                data[i] = packedTerrainData.FilterStatic();
            }

            return data;
        }

        public static PackedTerrainData FilterStatic(this PackedTerrainData data)
        {
            for (int i = 0; i < PackedTerrainData.UnpackedCapacity; i++)
            {
                if (!data[i].TerrainMaterial.IsStatic())
                {
                    data[i] = TerrainData.DefaultOutside;
                }
            }

            return data;
        }
    }
}