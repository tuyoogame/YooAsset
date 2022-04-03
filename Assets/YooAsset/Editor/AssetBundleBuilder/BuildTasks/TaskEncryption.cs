using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	public class TaskEncryption : IBuildTask
	{
		public class EncryptionContext : IContextObject
		{
			public List<string> EncryptList;

			/// <summary>
			/// 检测是否为加密文件
			/// </summary>
			public bool IsEncryptFile(string bundleName)
			{
				return EncryptList.Contains(bundleName);
			}
		}

		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			EncryptionContext encryptionContext = new EncryptionContext();
			encryptionContext.EncryptList = EncryptFiles(buildParameters, buildMapContext);
			context.SetContextObject(encryptionContext);
		}

		/// <summary>
		/// 加密文件
		/// </summary>
		private List<string> EncryptFiles(AssetBundleBuilder.BuildParametersContext buildParameters, BuildMapContext buildMapContext)
		{
			var encryptionServices = buildParameters.Parameters.EncryptionServices;

			// 加密资源列表
			List<string> encryptList = new List<string>();

			// 如果没有设置加密类
			if (encryptionServices == null)
				return encryptList;

			UnityEngine.Debug.Log($"开始加密资源文件");
			int progressValue = 0;
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				if (encryptionServices.Check(bundleInfo.BundleName))
				{
					if (bundleInfo.IsRawFile)
					{
						UnityEngine.Debug.LogWarning($"Encryption not support raw file : {bundleInfo.BundleName}");
						continue;
					}

					encryptList.Add(bundleInfo.BundleName);

					// 注意：通过判断文件合法性，规避重复加密一个文件
					string filePath = $"{buildParameters.PipelineOutputDirectory}/{bundleInfo.BundleName}";
					byte[] fileData = File.ReadAllBytes(filePath);
					if (EditorTools.CheckBundleFileValid(fileData))
					{
						byte[] bytes = encryptionServices.Encrypt(fileData);
						File.WriteAllBytes(filePath, bytes);
						UnityEngine.Debug.Log($"文件加密完成：{filePath}");
					}
				}

				// 进度条
				EditorTools.DisplayProgressBar("加密资源包", ++progressValue, buildMapContext.BundleInfos.Count);
			}
			EditorTools.ClearProgressBar();

			return encryptList;
		}
	}
}