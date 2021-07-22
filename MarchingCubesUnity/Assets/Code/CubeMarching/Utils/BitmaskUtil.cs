using System.Runtime.CompilerServices;

namespace Code.CubeMarching.Utils
{
    public static class BitmaskUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(this byte data, int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || index >= 8)
                throw new System.ArgumentException("index must be between[0..7]");
#endif
            byte indexBitmask = IndexToBitmask(index);
            return (data & indexBitmask) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(this ref byte data, int index, bool value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || index >= 8)
                throw new System.ArgumentException("index must be between[0..7]");
#endif
            byte indexBitmask = IndexToBitmask(index);
            data = (byte) (value ? (data | indexBitmask) : data & ~indexBitmask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte IndexToBitmask(int index)
        {
            var indexBitmask = (byte)(1 << index);
            return indexBitmask;
        }
    }
}