using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using UnityEngine;

namespace YooAsset
{
	internal class BufferReader
	{
		private readonly byte[] _buffer;
		private int _index = 0;

		public BufferReader(byte[] data)
		{
			_buffer = data;
		}

		/// <summary>
		/// 缓冲区容量
		/// </summary>
		public int Capacity
		{
			get { return _buffer.Length; }
		}

		public byte[] ReadBytes(int count)
		{
			CheckReaderIndex(count);
			var data = new byte[count];
			Buffer.BlockCopy(_buffer, _index, data, 0, count);
			_index += count;
			return data;
		}
		public byte ReadByte()
		{
			CheckReaderIndex(1);
			return _buffer[_index++];
		}
		public sbyte ReadSbyte()
		{
			return (sbyte)ReadByte();
		}

		public bool ReadBool()
		{
			CheckReaderIndex(1);
			return _buffer[_index++] == 1;
		}
		public short ReadInt16()
		{
			return (short)ReadUInt16();
		}
		public ushort ReadUInt16()
		{
			CheckReaderIndex(2);
			ushort value = 0;
			for (int i = 0; i < 2; i++)
			{
				value += (ushort)(_buffer[_index++] << (i * 8));
			}
			return value;
		}
		public int ReadInt32()
		{
			return (int)ReadUInt32();
		}
		public uint ReadUInt32()
		{
			CheckReaderIndex(4);
			uint value = 0;
			for (int i = 0; i < 4; i++)
			{
				value += (uint)(_buffer[_index++] << (i * 8));
			}
			return value;
		}
		public long ReadInt64()
		{
			return (long)ReadUInt64();
		}
		public ulong ReadUInt64()
		{
			CheckReaderIndex(8);
			ulong value = 0;
			for (int i = 0; i < 8; i++)
			{
				value += (ulong)(_buffer[_index++] << (i * 8));
			}
			return value;
		}

		public float ReadSingle()
		{
			CheckReaderIndex(4);
			FloatContent content = new FloatContent();
			content.uintValue = ReadUInt32();
			return content.floatValue;
		}
		public double ReadDouble()
		{
			CheckReaderIndex(8);
			DoubleContent content = new DoubleContent();
			content.ulongValue = ReadUInt64();
			return content.doubleValue;
		}

		public string ReadUTF8()
		{
			ushort count = ReadUInt16();
			if (count == 0)
				return string.Empty;

			CheckReaderIndex(count);
			string value = Encoding.UTF8.GetString(_buffer, _index, count);
			_index += count;
			return value;
		}
		public int[] ReadInt32Array()
		{
			ushort count = ReadUInt16();
			int[] values = new int[count];
			for (int i = 0; i < count; i++)
			{
				values[i] = ReadInt32();
			}
			return values;
		}
		public long[] ReadInt64Array()
		{
			ushort count = ReadUInt16();
			long[] values = new long[count];
			for (int i = 0; i < count; i++)
			{
				values[i] = ReadInt64();
			}
			return values;
		}
		public float[] ReadFloatArray()
		{
			ushort count = ReadUInt16();
			float[] values = new float[count];
			for (int i = 0; i < count; i++)
			{
				values[i] = ReadSingle();
			}
			return values;
		}
		public double[] ReadDoubleArray()
		{
			ushort count = ReadUInt16();
			double[] values = new double[count];
			for (int i = 0; i < count; i++)
			{
				values[i] = ReadDouble();
			}
			return values;
		}
		public string[] ReadUTF8Array()
		{
			ushort count = ReadUInt16();
			string[] values = new string[count];
			for (int i = 0; i < count; i++)
			{
				values[i] = ReadUTF8();
			}
			return values;
		}

		public Vector2 ReadVector2()
		{
			float x = ReadSingle();
			float y = ReadSingle();
			return new Vector2(x, y);
		}
		public Vector3 ReadVector3()
		{
			float x = ReadSingle();
			float y = ReadSingle();
			float z = ReadSingle();
			return new Vector3(x, y, z);
		}
		public Vector4 ReadVector4()
		{
			float x = ReadSingle();
			float y = ReadSingle();
			float z = ReadSingle();
			float w = ReadSingle();
			return new Vector4(x, y, z, w);
		}
		public Quaternion ReadQuaternion()
		{
			float x = ReadSingle();
			float y = ReadSingle();
			float z = ReadSingle();
			float w = ReadSingle();
			return new Quaternion(x, y, z, w);
		}

		[Conditional("DEBUG")]
		private void CheckReaderIndex(int length)
		{
			if (_index + length > Capacity)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}
}