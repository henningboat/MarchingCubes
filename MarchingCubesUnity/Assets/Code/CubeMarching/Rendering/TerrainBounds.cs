using Unity.Mathematics;

namespace Code.CubeMarching.Rendering
{
    public struct TerrainBounds
    {
        #region Data

        public float3 min;
        public float3 max;

        #endregion

        #region Functionality

        public float3 size => max - min;

        public void ExpandTo(float3 position)
        {
            min = math.min(position, min);
            max = math.max(position, max);
        }

        public void ExpandTo(TerrainBounds other)
        {
            min = math.min(other.min, min);
            max = math.max(other.max, max);
        }

        public void LimitBy(TerrainBounds limiter)
        {
            min = math.max(limiter.min, min);
            max = math.min(limiter.max, max);
        }

        public bool ContainsPosition(float3 position)
        {
            return position.x >= min.x && position.x <= max.x &&
                   position.y >= min.y && position.y <= max.y &&
                   position.z >= min.z && position.z <= max.z;
        }

        public bool Overlaps(TerrainBounds other)
        {
            return min.x <= other.max.x && max.x >= other.min.x && min.y <= other.max.y && max.y >= other.min.y && min.z <= other.max.z && max.z >= other.min.z;
        }

        #endregion


        #region EqualityMembers

        public bool Equals(TerrainBounds other)
        {
            return min.Equals(other.min) && max.Equals(other.max);
        }

        public override bool Equals(object obj)
        {
            return obj is TerrainBounds other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (min.GetHashCode() * 397) ^ max.GetHashCode();
            }
        }

        #endregion
    }
}