using System.Runtime.InteropServices;

namespace YooAsset
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct DoubleContent
	{
		[FieldOffset(0)]
		public double doubleValue;
		[FieldOffset(0)]
		public ulong ulongValue;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct FloatContent
	{
		[FieldOffset(0)]
		public float floatValue;
		[FieldOffset(0)]
		public uint uintValue;
	}
}