using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace YooAsset
{
    /// <summary>
    /// 二进制缓冲区读取器，数据以小端字节序存储
    /// </summary>
    internal class BufferReader
    {
        private readonly byte[] _buffer;
        private int _position = 0;

        /// <summary>
        /// 使用指定的字节数组创建缓冲区读取器
        /// </summary>
        public BufferReader(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            _buffer = data;
        }

        /// <summary>
        /// 缓冲区是否包含有效数据
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (_buffer == null || _buffer.Length == 0)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// 缓冲区的总字节数
        /// </summary>
        public int Capacity
        {
            get { return _buffer.Length; }
        }

        /// <summary>
        /// 读取指定数量的字节
        /// </summary>
        public byte[] ReadBytes(int count)
        {
            EnsureCapacity(count);
            var data = new byte[count];
            Buffer.BlockCopy(_buffer, _position, data, 0, count);
            _position += count;
            return data;
        }

        /// <summary>
        /// 读取单个字节
        /// </summary>
        public byte ReadByte()
        {
            EnsureCapacity(1);
            return _buffer[_position++];
        }

        /// <summary>
        /// 读取布尔值
        /// </summary>
        public bool ReadBoolean()
        {
            EnsureCapacity(1);
            return _buffer[_position++] == 1;
        }

        /// <summary>
        /// 读取16位有符号整数
        /// </summary>
        public short ReadInt16()
        {
            EnsureCapacity(2);
            short value = (short)((_buffer[_position]) | (_buffer[_position + 1] << 8));
            _position += 2;
            return value;
        }
        /// <summary>
        /// 读取16位无符号整数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            return (ushort)ReadInt16();
        }

        /// <summary>
        /// 读取32位有符号整数
        /// </summary>
        public int ReadInt32()
        {
            EnsureCapacity(4);
            int value = (_buffer[_position]) | (_buffer[_position + 1] << 8) | (_buffer[_position + 2] << 16) | (_buffer[_position + 3] << 24);
            _position += 4;
            return value;
        }

        /// <summary>
        /// 读取32位无符号整数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            return (uint)ReadInt32();
        }

        /// <summary>
        /// 读取64位有符号整数
        /// </summary>
        public long ReadInt64()
        {
            EnsureCapacity(8);
            int i1 = (_buffer[_position]) | (_buffer[_position + 1] << 8) | (_buffer[_position + 2] << 16) | (_buffer[_position + 3] << 24);
            int i2 = (_buffer[_position + 4]) | (_buffer[_position + 5] << 8) | (_buffer[_position + 6] << 16) | (_buffer[_position + 7] << 24);
            _position += 8;
            return (uint)i1 | ((long)i2 << 32);
        }
        /// <summary>
        /// 读取64位无符号整数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            return (ulong)ReadInt64();
        }

        /// <summary>
        /// 跳过一个UTF8编码的字符串
        /// </summary>
        public void SkipString()
        {
            ushort count = ReadUInt16();
            if (count == 0)
                return;

            EnsureCapacity(count);
            _position += count;
        }

        /// <summary>
        /// 读取UTF8编码的字符串
        /// </summary>
        public string ReadString()
        {
            ushort count = ReadUInt16();
            if (count == 0)
                return string.Empty;

            EnsureCapacity(count);
            string value = Encoding.UTF8.GetString(_buffer, _position, count);
            _position += count;
            return value;
        }

        /// <summary>
        /// 读取32位有符号整数数组
        /// </summary>
        public int[] ReadInt32Array()
        {
            ushort count = ReadUInt16();
            if (count == 0)
                return Array.Empty<int>();

            int[] values = new int[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadInt32();
            }
            return values;
        }

        /// <summary>
        /// 读取64位有符号整数数组
        /// </summary>
        public long[] ReadInt64Array()
        {
            ushort count = ReadUInt16();
            if (count == 0)
                return Array.Empty<long>();

            long[] values = new long[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadInt64();
            }
            return values;
        }

        /// <summary>
        /// 读取UTF8编码的字符串数组
        /// </summary>
        public string[] ReadStringArray()
        {
            ushort count = ReadUInt16();
            if (count == 0)
                return Array.Empty<string>();

            string[] values = new string[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadString();
            }
            return values;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int length)
        {
            if (_position + length > Capacity)
            {
                throw new InvalidOperationException("Insufficient buffer capacity.");
            }
        }
    }
}