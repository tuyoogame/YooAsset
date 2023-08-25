// ReSharper disable InconsistentNaming

using System.Runtime.CompilerServices;

namespace Standart.Hash.xxHash
{
    public static partial class xxHash32
    {
        private static readonly uint XXH_PRIME32_1 = 2654435761U;
        private static readonly uint XXH_PRIME32_2 = 2246822519U;
        private static readonly uint XXH_PRIME32_3 = 3266489917U;
        private static readonly uint XXH_PRIME32_4 = 668265263U;
        private static readonly uint XXH_PRIME32_5 = 374761393U;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint XXH_rotl32(uint x, int r)
        {
            return (x << r) | (x >> (32 - r));
        }
    }    
}
