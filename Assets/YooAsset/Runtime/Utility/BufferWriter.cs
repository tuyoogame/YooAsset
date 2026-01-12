using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 二进制缓冲区写入器，数据以小端字节序存储
    /// </summary>
    internal class BufferWriter
    {
        private readonly byte[] _buffer;
        private int _position = 0;

        /// <summary>
        /// 创建指定容量的缓冲区写入器
        /// </summary>
        public BufferWriter(int capacity)
        {
            _buffer = new byte[capacity];
        }

        /// <summary>
        /// 缓冲区的总字节数
        /// </summary>
        public int Capacity
        {
            get { return _buffer.Length; }
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            _position = 0;
        }

        /// <summary>
        /// 将已写入的数据复制到新的字节数组
        /// </summary>
        public byte[] ToArray()
        {
            byte[] newArray = new byte[_position];
            Buffer.BlockCopy(_buffer, 0, newArray, 0, _position);
            return newArray;
        }

        /// <summary>
        /// 将已写入的数据写入文件流
        /// </summary>
        public void WriteToStream(FileStream fileStream)
        {
            fileStream.Write(_buffer, 0, _position);
        }

        /// <summary>
        /// 写入字节数组
        /// </summary>
        public void WriteBytes(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            int count = data.Length;
            EnsureCapacity(count);
            Buffer.BlockCopy(data, 0, _buffer, _position, count);
            _position += count;
        }

        /// <summary>
        /// 写入单个字节
        /// </summary>
        public void WriteByte(byte value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = value;
        }

        /// <summary>
        /// 写入布尔值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            WriteByte((byte)(value ? 1 : 0));
        }

        /// <summary>
        /// 写入16位有符号整数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            WriteUInt16((ushort)value);
        }

        /// <summary>
        /// 写入16位无符号整数
        /// </summary>
        public void WriteUInt16(ushort value)
        {
            EnsureCapacity(2);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
        }

        /// <summary>
        /// 写入32位有符号整数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            WriteUInt32((uint)value);
        }

        /// <summary>
        /// 写入32位无符号整数
        /// </summary>
        public void WriteUInt32(uint value)
        {
            EnsureCapacity(4);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }

        /// <summary>
        /// 写入64位有符号整数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            WriteUInt64((ulong)value);
        }

        /// <summary>
        /// 写入64位无符号整数
        /// </summary>
        public void WriteUInt64(ulong value)
        {
            EnsureCapacity(8);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 32);
            _buffer[_position++] = (byte)(value >> 40);
            _buffer[_position++] = (byte)(value >> 48);
            _buffer[_position++] = (byte)(value >> 56);
        }

        /// <summary>
        /// 写入UTF8编码的字符串
        /// </summary>
        public void WriteString(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
            {
                WriteUInt16(0);
            }
            else
            {
                int count = Encoding.UTF8.GetByteCount(value);
                if (count > ushort.MaxValue)
                    throw new OverflowException($"String length exceeds the maximum value of {ushort.MaxValue}.");

                WriteUInt16(Convert.ToUInt16(count));
                EnsureCapacity(count);
                Encoding.UTF8.GetBytes(value, 0, value.Length, _buffer, _position);
                _position += count;
            }
        }

        /// <summary>
        /// 写入32位有符号整数数组
        /// </summary>
        public void WriteInt32Array(int[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            int count = values.Length;
            if (count > ushort.MaxValue)
                throw new OverflowException($"Array length exceeds the maximum value of {ushort.MaxValue}.");

            WriteUInt16(Convert.ToUInt16(count));
            for (int i = 0; i < count; i++)
            {
                WriteInt32(values[i]);
            }
        }

        /// <summary>
        /// 写入64位有符号整数数组
        /// </summary>
        public void WriteInt64Array(long[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            int count = values.Length;
            if (count > ushort.MaxValue)
                throw new OverflowException($"Array length exceeds the maximum value of {ushort.MaxValue}.");

            WriteUInt16(Convert.ToUInt16(count));
            for (int i = 0; i < count; i++)
            {
                WriteInt64(values[i]);
            }
        }

        /// <summary>
        /// 写入UTF8编码的字符串数组
        /// </summary>
        public void WriteStringArray(string[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            int count = values.Length;
            if (count > ushort.MaxValue)
                throw new OverflowException($"Array length exceeds the maximum value of {ushort.MaxValue}.");

            WriteUInt16(Convert.ToUInt16(count));
            for (int i = 0; i < count; i++)
            {
                WriteString(values[i]);
            }
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