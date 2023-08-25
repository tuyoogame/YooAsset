// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Standart.Hash.xxHash
{
    public static partial class xxHash128
    {
        /// <summary>
        /// Compute xxHash for the data byte array
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="length">The length of the data for hashing</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe uint128 ComputeHash(byte[] data, int length, ulong seed = 0)
        {
            Debug.Assert(data != null);
            Debug.Assert(length >= 0);
            Debug.Assert(length <= data.Length);
            
            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed);
            }
        }  
        
        /// <summary>
        /// Compute xxHash for the data byte span 
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="length">The length of the data for hashing</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe uint128 ComputeHash(Span<byte> data, int length, ulong seed = 0)
        {
            Debug.Assert(data != null);
            Debug.Assert(length >= 0);
            Debug.Assert(length <= data.Length);

            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed);
            }
        }
        
        /// <summary>
        /// Compute xxHash for the data byte span 
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="length">The length of the data for hashing</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe uint128 ComputeHash(ReadOnlySpan<byte> data, int length, ulong seed = 0)
        {
            Debug.Assert(data != null);
            Debug.Assert(length >= 0);
            Debug.Assert(length <= data.Length);

            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed);
            }
        }
        
        /// <summary>
        /// Compute xxHash for the string 
        /// </summary>
        /// <param name="str">The source of data</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe uint128 ComputeHash(string str, ulong seed = 0)
        {
            Debug.Assert(str != null);
            
            fixed (char* c = str)
            {
                byte* ptr = (byte*) c;
                int length = str.Length * 2;
                
                return UnsafeComputeHash(ptr, length, seed);
            }
        }
        
        /// <summary>
        /// Compute hash bytes for the data byte array
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="length">The length of the data for hashing</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe byte[] ComputeHashBytes(byte[] data, int length, ulong seed = 0)
        {
            Debug.Assert(data != null);
            Debug.Assert(length >= 0);
            Debug.Assert(length <= data.Length);
            
            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed).ToBytes();
            }
        }  
        
        /// <summary>
        /// Compute hash bytes for the span 
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="length">The length of the data for hashing</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe byte[] ComputeHashBytes(Span<byte> data, int length, ulong seed = 0)
        {
            Debug.Assert(data != null);
            Debug.Assert(length >= 0);
            Debug.Assert(length <= data.Length);

            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed).ToBytes();
            }
        }
        
        /// <summary>
        /// Compute hash bytes for the data byte span 
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="length">The length of the data for hashing</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe byte[] ComputeHashBytes(ReadOnlySpan<byte> data, int length, ulong seed = 0)
        {
            Debug.Assert(data != null);
            Debug.Assert(length >= 0);
            Debug.Assert(length <= data.Length);

            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed).ToBytes();
            }
        }
        
        /// <summary>
        /// Compute hash bytes for the string 
        /// </summary>
        /// <param name="str">The source of data</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe byte[] ComputeHashBytes(string str, ulong seed = 0)
        {
            Debug.Assert(str != null);
            
            fixed (char* c = str)
            {
                byte* ptr = (byte*) c;
                int length = str.Length * 2;
                
                return UnsafeComputeHash(ptr, length, seed).ToBytes();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 UnsafeComputeHash(byte* input, int len, ulong seed)
        {
            fixed (byte* secret = &XXH3_SECRET[0])
            {
                return XXH3_128bits_internal(input, len, seed, secret, XXH3_SECRET_DEFAULT_SIZE);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct uint128
    {
        public ulong low64;
        public ulong high64;
    }
}