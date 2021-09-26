using System;
using Code.CubeMarching.GeometryComponents;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Mathematics;
using static Code.SIMDMath.SimdMath;

namespace Code.CubeMarching.TerrainChunkSystem
{
    public static class TerrainChunkOperations
    {
        public static PackedTerrainData CombinePackedTerrainData(CGeometryCombiner combiner, PackedTerrainData valuesA, PackedTerrainData valuesB, NativeArray<float> propertyBuffer)
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
                    packedTerrainData = CombineTerrainSmoothMin(valuesA, valuesB, combiner.BlendFactor.Resolve(propertyBuffer));
                    break;
                case CombinerOperation.SmoothSubtract:
                    packedTerrainData = CombineTerrainSmoothSubtract(valuesA, valuesB, combiner.BlendFactor.Resolve(propertyBuffer));
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
            var replaceTerrainMaterial = a.SurfaceDistance.PackedValues > 0;
            return new PackedTerrainData {SurfaceDistance = b.SurfaceDistance, TerrainMaterial = PackedTerrainMaterial.Select(b.TerrainMaterial, a.TerrainMaterial, replaceTerrainMaterial)};
        }

        public static PackedTerrainData CombineTerrainMin(PackedTerrainData a, PackedTerrainData b)
        {
            var bIsSmaller = a.SurfaceDistance.PackedValues > b.SurfaceDistance.PackedValues;
            var surfaceDistance = math.min(a.SurfaceDistance.PackedValues, b.SurfaceDistance.PackedValues);
            var combinedMaterial = PackedTerrainMaterial.Select(a.TerrainMaterial, b.TerrainMaterial, bIsSmaller);

            return new PackedTerrainData(new PackedFloat(surfaceDistance), combinedMaterial);
        }

        public static PackedTerrainData CombineTerrainMax(PackedTerrainData a, PackedTerrainData b)
        {
            var bIsBigger = a.SurfaceDistance.PackedValues < b.SurfaceDistance.PackedValues;
            var surfaceDistance = math.select(a.SurfaceDistance.PackedValues, b.SurfaceDistance.PackedValues, bIsBigger);
            var combinedMaterial = PackedTerrainMaterial.Select(a.TerrainMaterial, b.TerrainMaterial, !bIsBigger);

            return new PackedTerrainData(new PackedFloat(surfaceDistance), combinedMaterial);
        }

        public static PackedTerrainData CombineTerrainAdd(PackedTerrainData a, PackedTerrainData b)
        {
            return new(a.SurfaceDistance + b.SurfaceDistance, a.TerrainMaterial);
        }

        //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
        public static PackedTerrainData CombineTerrainSmoothMin(PackedTerrainData terrainDataA, PackedTerrainData terrainDataB, PackedFloat blendFactor)
        {
            var a = terrainDataA.SurfaceDistance;
            var b = terrainDataB.SurfaceDistance;
            var h = clamp(0.5f + 0.5f * (a - b) / blendFactor, 0.0f, 1.0f);
            var blendedSurfaceDistance = lerp(a, b, h) - blendFactor * h * (1.0f - h);

            var bIsSmaller = terrainDataA.SurfaceDistance.PackedValues > terrainDataB.SurfaceDistance.PackedValues;
            var combinedMaterial = PackedTerrainMaterial.Select(terrainDataA.TerrainMaterial, terrainDataB.TerrainMaterial, bIsSmaller);

            return new PackedTerrainData(blendedSurfaceDistance, combinedMaterial);
        }

        private static PackedTerrainData CombineTerrainSmoothSubtract(PackedTerrainData terrainDataA, PackedTerrainData terrainDataB, PackedFloat blendFactor)
        {
            var a = terrainDataA.SurfaceDistance;
            var b = terrainDataB.SurfaceDistance;
            var h = clamp(0.5f - 0.5f * (b + a) / blendFactor, 0.0f, 1.0f);
            var blendedSurfaceDistance = lerp(b, -a, h) + blendFactor * h * (1.0f - h);

            var bIsSmaller = terrainDataA.SurfaceDistance.PackedValues > terrainDataB.SurfaceDistance.PackedValues;
            var combinedMaterial = PackedTerrainMaterial.Select(terrainDataA.TerrainMaterial, terrainDataB.TerrainMaterial, bIsSmaller);

            return new PackedTerrainData(blendedSurfaceDistance, combinedMaterial);
        }


        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TerrainChunkData FilterStatic(this TerrainChunkData data)
        {
            for (var i = 0; i < TerrainChunkData.PackedCapacity; i++)
            {
                var packedTerrainData = data[i];
                data[i] = packedTerrainData.FilterStatic();
            }

            return data;
        }

        public static PackedTerrainData FilterStatic(this PackedTerrainData data)
        {
            for (var i = 0; i < PackedTerrainData.UnpackedCapacity; i++)
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