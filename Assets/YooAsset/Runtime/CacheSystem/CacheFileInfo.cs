using System;
using System.IO;

namespace YooAsset
{
	[Serializable]
	internal class CacheFileInfo
	{
		public string FileCRC;
		public long FileSize;
	}
}