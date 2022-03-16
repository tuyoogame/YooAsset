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
			var buildMapContext = context.GetContextObject<TaskGetBuildMap.BuildMapContext>();

			var encrypter = CreateAssetEncrypter();
			List<string> encryptList = EncryptFiles(encrypter, buildParameters, buildMapContext);

			EncryptionContext encryptionContext = new EncryptionContext();
			encryptionContext.EncryptList = encryptList;
			context.SetContextObject(encryptionContext);
		}

		/// <summary>
		/// 创建加密类
		/// </summary>
		/// <returns>如果没有定义类型，则返回NULL</returns>
		private IAssetEncrypter CreateAssetEncrypter()
		{
			var types = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IAssetEncrypter));
			if (types.Count == 0)
				return null;
			if (types.Count != 1)
				throw new Exception($"Found more {nameof(IAssetEncrypter)} types. We only support one.");

			UnityEngine.Debug.Log($"创建实例类 : {types[0].FullName}");
			return (IAssetEncrypter)Activator.CreateInstance(types[0]);
		}

		/// <summary>
		/// 加密文件
		/// </summary>
		private List<string> EncryptFiles(IAssetEncrypter encrypter, AssetBundleBuilder.BuildParametersContext buildParameters, TaskGetBuildMap.BuildMapContext buildMapContext)
		{
			// 加密资源列表
			List<string> encryptList = new List<string>();

			// 如果没有设置加密类
			if (encrypter == null)
				return encryptList;

			UnityEngine.Debug.Log($"开始加密资源文件");
			int progressValue = 0;
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				var bundleName = bundleInfo.BundleName;
				string filePath = $"{buildParameters.PipelineOutputDirectory}/{bundleName}";
				if (encrypter.Check(filePath))
				{
					encryptList.Add(bundleName);

					// 注意：通过判断文件合法性，规避重复加密一个文件
					byte[] fileData = File.ReadAllBytes(filePath);
					if (EditorTools.CheckBundleFileValid(fileData))
					{
						byte[] bytes = encrypter.Encrypt(fileData);
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