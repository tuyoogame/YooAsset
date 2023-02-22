using System.IO;

namespace YooAsset
{
	internal class VerifyElement
	{
		public string PackageName { private set; get; }
		public string CacheGUID { private set; get; }
		public string FileRootPath { private set; get; }
		public string DataFilePath { private set; get; }
		public string InfoFilePath { private set; get; }
		
		public EVerifyResult Result;
		public string DataFileCRC;
		public long DataFileSize;

		public VerifyElement(string packageName, string cacheGUID, string fileRootPath, string dataFilePath, string infoFilePath)
		{
			PackageName = packageName;
			CacheGUID = cacheGUID;
			FileRootPath = fileRootPath;
			DataFilePath = dataFilePath;
			InfoFilePath = infoFilePath;
		}

		public void DeleteFiles()
		{
			if (File.Exists(DataFilePath))
			{
				File.Delete(DataFilePath);
			}

			if (File.Exists(InfoFilePath))
			{
				File.Delete(InfoFilePath);
			}
		}
	}
}