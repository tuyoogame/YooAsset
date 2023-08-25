using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Standart.Hash.xxHash
{
    public static class Utils
    {
        public static Guid ToGuid(this uint128 value)
        {
            var a = (Int32) (value.low64);
            var b = (Int16) (value.low64 >> 32);
            var c = (Int16) (value.low64 >> 48);
            
            var d = (Byte) (value.high64);
            var e = (Byte) (value.high64 >> 8);
            var f = (Byte) (value.high64 >> 16);
            var g = (Byte) (value.high64 >> 24);
            var h = (Byte) (value.high64 >> 32);
            var i = (Byte) (value.high64 >> 40);
            var j = (Byte) (value.high64 >> 48);
            var k = (Byte) (value.high64 >> 56);
            
            return new Guid(a, b, c, d, e, f,g, h, i, j, k);
        }

        public static byte[] ToBytes(this uint128 value)
        {
            // allocation
            byte[] bytes = new byte[sizeof(ulong) * 2];
            Unsafe.As<byte, ulong>(ref bytes[0]) = value.low64;
            Unsafe.As<byte, ulong>(ref bytes[8]) = value.high64;
            return bytes;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void BlockCopy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            Debug.Assert(src != null);
            Debug.Assert(dst != null);
            Debug.Assert(srcOffset >= 0 && srcOffset < src.Length);
            Debug.Assert(dstOffset >= 0 && dstOffset < dst.Length);
            Debug.Assert(count >= 0);
            Debug.Assert(count + srcOffset <= src.Length);
            Debug.Assert(count + dstOffset <= dst.Length);
                      
            fixed (byte* pSrc = &src[srcOffset])
            fixed (byte* pDst = &dst[dstOffset])
            {
                byte* ptrSrc = pSrc;
                byte* ptrDst = pDst;
                
                SMALLTABLE:
                switch (count)
                {
                    case 0:
                        return;
                    case 1:
                        *ptrDst = *ptrSrc;
                        return;
                    case 2:
                        *(short*)ptrDst = *(short*)ptrSrc;
                        return;
                    case 3:
                        *(short*)(ptrDst + 0) = *(short*)(ptrSrc + 0);
                        *(ptrDst + 2) = *(ptrSrc + 2);
                        return;
                    case 4:
                        *(int*)ptrDst = *(int*)ptrSrc;
                        return;
                    case 5:
                        *(int*)(ptrDst + 0) = *(int*)(ptrSrc + 0);
                        *(ptrDst + 4) = *(ptrSrc + 4);
                        return;
                    case 6:
                        *(int*)(ptrDst + 0) = *(int*)(ptrSrc + 0);
                        *(short*)(ptrDst + 4) = *(short*)(ptrSrc + 4);
                        return;
                    case 7:
                        *(int*)(ptrDst + 0) = *(int*)(ptrSrc + 0);
                        *(short*)(ptrDst + 4) = *(short*)(ptrSrc + 4);
                        *(ptrDst + 6) = *(ptrSrc + 6);
                        return;
                    case 8:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        return;
                    case 9:
                        *(long*)(ptrDst + 0) = *(long*)(ptrSrc + 0);
                        *(ptrDst + 8) = *(ptrSrc + 8);
                        return;
                    case 10:
                        *(long*)(ptrDst + 0) = *(long*)(ptrSrc + 0);
                        *(short*)(ptrDst + 8) = *(short*)(ptrSrc + 8);
                        return;
                    case 11:
                        *(long*)(ptrDst + 0) = *(long*)(ptrSrc + 0);
                        *(short*)(ptrDst + 8) = *(short*)(ptrSrc + 8);
                        *(ptrDst + 10) = *(ptrSrc + 10);
                        return;
                    case 12:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(int*)(ptrDst + 8) = *(int*)(ptrSrc + 8);
                        return;
                    case 13:
                        *(long*)(ptrDst + 0) = *(long*)(ptrSrc + 0);
                        *(int*)(ptrDst + 8) = *(int*)(ptrSrc + 8);
                        *(ptrDst + 12) = *(ptrSrc + 12);
                        return;
                    case 14:
                        *(long*)(ptrDst + 0) = *(long*)(ptrSrc + 0);
                        *(int*)(ptrDst + 8) = *(int*)(ptrSrc + 8);
                        *(short*)(ptrDst + 12) = *(short*)(ptrSrc + 12);
                        return;
                    case 15:
                        *(long*)(ptrDst + 0) = *(long*)(ptrSrc + 0);
                        *(int*)(ptrDst + 8) = *(int*)(ptrSrc + 8);
                        *(short*)(ptrDst + 12) = *(short*)(ptrSrc + 12);
                        *(ptrDst + 14) = *(ptrSrc + 14);
                        return;
                    case 16:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        return;
                    case 17:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(ptrDst + 16) = *(ptrSrc + 16);
                        return;
                    case 18:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(short*)(ptrDst + 16) = *(short*)(ptrSrc + 16);
                        return;
                    case 19:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(short*)(ptrDst + 16) = *(short*)(ptrSrc + 16);
                        *(ptrDst + 18) = *(ptrSrc + 18);
                        return;
                    case 20:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(int*)(ptrDst + 16) = *(int*)(ptrSrc + 16);
                        return;
    
                    case 21:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(int*)(ptrDst + 16) = *(int*)(ptrSrc + 16);
                        *(ptrDst + 20) = *(ptrSrc + 20);
                        return;
                    case 22:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(int*)(ptrDst + 16) = *(int*)(ptrSrc + 16);
                        *(short*)(ptrDst + 20) = *(short*)(ptrSrc + 20);
                        return;
                    case 23:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(int*)(ptrDst + 16) = *(int*)(ptrSrc + 16);
                        *(short*)(ptrDst + 20) = *(short*)(ptrSrc + 20);
                        *(ptrDst + 22) = *(ptrSrc + 22);
                        return;
                    case 24:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(long*)(ptrDst + 16) = *(long*)(ptrSrc + 16);
                        return;
                    case 25:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(long*)(ptrDst + 16) = *(long*)(ptrSrc + 16);
                        *(ptrDst + 24) = *(ptrSrc + 24);
                        return;
                    case 26:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(long*)(ptrDst + 16) = *(long*)(ptrSrc + 16);
                        *(short*)(ptrDst + 24) = *(short*)(ptrSrc + 24);
                        return;
                    case 27:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(long*)(ptrDst + 16) = *(long*)(ptrSrc + 16);
                        *(short*)(ptrDst + 24) = *(short*)(ptrSrc + 24);
                        *(ptrDst + 26) = *(ptrSrc + 26);
                        return;
                    case 28:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(long*)(ptrDst + 16) = *(long*)(ptrSrc + 16);
                        *(int*)(ptrDst + 24) = *(int*)(ptrSrc + 24);
                        return;
                    case 29:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(long*)(ptrDst + 16) = *(long*)(ptrSrc + 16);
                        *(int*)(ptrDst + 24) = *(int*)(ptrSrc + 24);
                        *(ptrDst + 28) = *(ptrSrc + 28);
                        return;
                    case 30:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(long*)(ptrDst + 16) = *(long*)(ptrSrc + 16);
                        *(int*)(ptrDst + 24) = *(int*)(ptrSrc + 24);
                        *(short*)(ptrDst + 28) = *(short*)(ptrSrc + 28);
                        return;
                    case 31:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(long*)(ptrDst + 16) = *(long*)(ptrSrc + 16);
                        *(int*)(ptrDst + 24) = *(int*)(ptrSrc + 24);
                        *(short*)(ptrDst + 28) = *(short*)(ptrSrc + 28);
                        *(ptrDst + 30) = *(ptrSrc + 30);
                        return;
                    case 32:
                        *(long*)ptrDst = *(long*)ptrSrc;
                        *(long*)(ptrDst + 8) = *(long*)(ptrSrc + 8);
                        *(long*)(ptrDst + 16) = *(long*)(ptrSrc + 16);
                        *(long*)(ptrDst + 24) = *(long*)(ptrSrc + 24);
                        return;
                }
    
                long* lpSrc = (long*)ptrSrc;
                long* ldSrc = (long*)ptrDst;
                while (count >= 64)
                {
                    *(ldSrc + 0) = *(lpSrc + 0);
                    *(ldSrc + 1) = *(lpSrc + 1);
                    *(ldSrc + 2) = *(lpSrc + 2);
                    *(ldSrc + 3) = *(lpSrc + 3);
                    *(ldSrc + 4) = *(lpSrc + 4);
                    *(ldSrc + 5) = *(lpSrc + 5);
                    *(ldSrc + 6) = *(lpSrc + 6);
                    *(ldSrc + 7) = *(lpSrc + 7);
                    if (count == 64)
                        return;
                    count -= 64;
                    lpSrc += 8;
                    ldSrc += 8;
                }
                if (count > 32)
                {
                    *(ldSrc + 0) = *(lpSrc + 0);
                    *(ldSrc + 1) = *(lpSrc + 1);
                    *(ldSrc + 2) = *(lpSrc + 2);
                    *(ldSrc + 3) = *(lpSrc + 3);
                    count -= 32;
                    lpSrc += 4;
                    ldSrc += 4;
                }
                
                ptrSrc = (byte*)lpSrc;
                ptrDst = (byte*)ldSrc;
                goto SMALLTABLE;
            }
        }
    }
}