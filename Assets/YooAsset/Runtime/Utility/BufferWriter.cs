using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace YooAsset
{
	internal class BufferWriter
	{
		private readonly byte[] _buffer;
		private int _index = 0;

		public BufferWriter(int capacity)
		{
			_buffer = new byte[capacity];
		}

		/// <summary>
		/// 缓冲区容量
		/// </summary>
		public int Capacity
		{
			get { return _buffer.Length; }
		}

		/// <summary>
		/// 将有效数据写入文件流
		/// </summary>
		public void WriteToStream(FileStream fileStream)
		{
			fileStream.Write(_buffer, 0, _index);
		}

		public void WriteBytes(byte[] data)
		{
			WriteBytes(data, 0, data.Length);
		}
		public void WriteBytes(byte[] data, int offset, int count)
		{
			CheckWriterIndex(count);
			Buffer.BlockCopy(data, offset, _buffer, _index, count);
			_index += count;
		}
		public void WriteByte(byte value)
		{
			CheckWriterIndex(1);
			_buffer[_index++] = value;
		}
		public void WriteSbyte(sbyte value)
		{
			WriteByte((byte)value);
		}

		public void WriteBool(bool value)
		{
			WriteByte((byte)(value ? 1 : 0));
		}
		public void WriteInt16(short value)
		{
			WriteUInt16((ushort)value);
		}
		public void WriteUInt16(ushort value)
		{
			CheckWriterIndex(2);
			for (int i = 0; i < 2; i++)
			{
				_buffer[_index++] = (byte)(value >> (i * 8));
			}
		}
		public void WriteInt32(int value)
		{
			WriteUInt32((uint)value);
		}
		public void WriteUInt32(uint value)
		{
			CheckWriterIndex(4);
			for (int i = 0; i < 4; i++)
			{
				_buffer[_index++] = (byte)(value >> (i * 8));
			}
		}
		public void WriteInt64(long value)
		{
			WriteUInt64((ulong)value);
		}
		public void WriteUInt64(ulong value)
		{
			CheckWriterIndex(8);
			for (int i = 0; i < 8; i++)
			{
				_buffer[_index++] = (byte)(value >> (i * 8));
			}
		}

		public void WriteSingle(float value)
		{
			FloatContent content = new FloatContent();
			content.floatValue = value;
			WriteUInt32(content.uintValue);
		}
		public void WriteDouble(double value)
		{
			DoubleContent content = new DoubleContent();
			content.doubleValue = value;
			WriteUInt64(content.ulongValue);
		}

		public void WriteUTF8(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				WriteUInt16(0);
			}
			else
			{
				byte[] bytes = Encoding.UTF8.GetBytes(value);
				int count = bytes.Length;
				if (count > ushort.MaxValue)
					throw new FormatException($"Write string length cannot be greater than {ushort.MaxValue} !");

				WriteUInt16(Convert.ToUInt16(count));
				WriteBytes(bytes);
			}
		}
		public void WriteInt32Array(int[] values)
		{
			if (values == null)
			{
				WriteUInt16(0);
			}
			else
			{
				int count = values.Length;
				if (count > ushort.MaxValue)
					throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

				WriteUInt16(Convert.ToUInt16(count));
				for (int i = 0; i < count; i++)
				{
					WriteInt32(values[i]);
				}
			}
		}
		public void WriteInt64Array(long[] values)
		{
			if (values == null)
			{
				WriteUInt16(0);
			}
			else
			{
				int count = values.Length;
				if (count > ushort.MaxValue)
					throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

				WriteUInt16(Convert.ToUInt16(count));
				for (int i = 0; i < count; i++)
				{
					WriteInt64(values[i]);
				}
			}
		}
		public void WriteSingleArray(float[] values)
		{
			if (values == null)
			{
				WriteUInt16(0);
			}
			else
			{
				int count = values.Length;
				if (count > ushort.MaxValue)
					throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

				WriteUInt16(Convert.ToUInt16(count));
				for (int i = 0; i < count; i++)
				{
					WriteSingle(values[i]);
				}
			}
		}
		public void WriteDoubleArray(double[] values)
		{
			if (values == null)
			{
				WriteUInt16(0);
			}
			else
			{
				int count = values.Length;
				if (count > ushort.MaxValue)
					throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

				WriteUInt16(Convert.ToUInt16(count));
				for (int i = 0; i < count; i++)
				{
					WriteDouble(values[i]);
				}
			}
		}
		public void WriteUTF8Array(string[] values)
		{
			if (values == null)
			{
				WriteUInt16(0);
			}
			else
			{
				int count = values.Length;
				if (count > ushort.MaxValue)
					throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

				WriteUInt16(Convert.ToUInt16(count));
				for (int i = 0; i < count; i++)
				{
					WriteUTF8(values[i]);
				}
			}
		}

		public void WriteVector2(Vector2 value)
		{
			WriteSingle(value.x);
			WriteSingle(value.y);
		}
		public void WriteVector3(Vector3 value)
		{
			WriteSingle(value.x);
			WriteSingle(value.y);
			WriteSingle(value.z);
		}
		public void WriteVector4(Vector4 value)
		{
			WriteSingle(value.x);
			WriteSingle(value.y);
			WriteSingle(value.z);
			WriteSingle(value.w);
		}
		public void WriteQuaternion(Quaternion value)
		{
			WriteSingle(value.x);
			WriteSingle(value.y);
			WriteSingle(value.z);
			WriteSingle(value.w);
		}

		[Conditional("DEBUG")]
		private void CheckWriterIndex(int length)
		{
			if (_index + length > Capacity)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}
}