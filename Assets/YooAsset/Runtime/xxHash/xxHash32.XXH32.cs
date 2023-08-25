// ReSharper disable InconsistentNaming

namespace Standart.Hash.xxHash
{
    using System.Runtime.CompilerServices;

    public static partial class xxHash32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint XXH32(byte* input, int len, uint seed)
        {
            uint h32;

            if (len >= 16)
            {
                byte* end = input + len;
                byte* limit = end - 15;

                uint v1 = seed + XXH_PRIME32_1 + XXH_PRIME32_2;
                uint v2 = seed + XXH_PRIME32_2;
                uint v3 = seed + 0;
                uint v4 = seed - XXH_PRIME32_1;

                do
                {
                    v1 = XXH32_round(v1, *(uint*) input); input += 4;
                    v2 = XXH32_round(v2, *(uint*) input); input += 4;
                    v3 = XXH32_round(v3, *(uint*) input); input += 4;
                    v4 = XXH32_round(v4, *(uint*) input); input += 4;
                } while (input < limit);

                h32 = XXH_rotl32(v1, 1) + 
                      XXH_rotl32(v2, 7) +
                      XXH_rotl32(v3, 12) +
                      XXH_rotl32(v4, 18);
            }
            else
            {
                h32 = seed + XXH_PRIME32_5;
            }

            h32 += (uint)len;

            return XXH32_finalize(h32, input, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint XXH32_round(uint acc, uint input)
        {
            acc += input * XXH_PRIME32_2;
            acc = XXH_rotl32(acc, 13);
            acc *= XXH_PRIME32_1;
            return acc;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint XXH32_avalanche(uint hash)
        {
            hash ^= hash >> 15;
            hash *= XXH_PRIME32_2;
            hash ^= hash >> 13;
            hash *= XXH_PRIME32_3;
            hash ^= hash >> 16;
            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint XXH32_finalize(uint hash, byte* ptr, int len)
        {
            len &= 15;
            while (len >= 4)
            {
                hash += *((uint*)ptr) * XXH_PRIME32_3;
                ptr += 4;
                hash = XXH_rotl32(hash, 17) * XXH_PRIME32_4;
                len -= 4;
            }

            while (len > 0)
            {
                hash += *((byte*)ptr) * XXH_PRIME32_5;
                ptr++;
                hash = XXH_rotl32(hash, 11) * XXH_PRIME32_1;
                len--;
            }
            
            return XXH32_avalanche(hash);
        }
    }
}