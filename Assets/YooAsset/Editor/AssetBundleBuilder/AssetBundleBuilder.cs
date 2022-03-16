using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class AssetBundleBuilder
	{
		/// <summary>
		/// 构建参数
		/// </summary>
		public class BuildParameters
		{
			/// <summary>
			/// 是否验证构建结果
			/// </summary>
			public bool IsVerifyBuildingResult = false;
			
			/// <summary>
			/// 输出的根目录
			/// </summary>
			public string OutputRoot;

			/// <summary>
			/// 构建的平台
			/// </summary>
			public BuildTarget BuildTarget;

			/// <summary>
			/// 构建的版本（资源版本号）
			/// </summary>
			public int BuildVersion;

			/// <summary>
			/// 是否允许冗余机制
			/// 说明：冗余机制可以帮助我们减少包体数量
			/// </summary>
			public bool ApplyRedundancy = false;

			/// <summary>
			/// 是否附加上文件扩展名
			/// </summary>
			public bool AppendFileExtension = false;


			/// <summary>
			/// 压缩选项
			/// </summary>
			public ECompressOption CompressOption;

			/// <summary>
			/// 是否强制重新构建整个项目，如果为FALSE则是增量打包
			/// </summary>
			public bool IsForceRebuild;

			/// <summary>
			/// 内置资源的标记列表
			/// 注意：分号为分隔符
			/// </summary>
			public string BuildinTags;

			#region 高级选项
			/// <summary>
			/// 文件名附加上哈希值
			/// </summary>
			public bool IsAppendHash = false;

			/// <summary>
			/// 禁止写入类型树结构（可以降低包体和内存并提高加载效率）
			/// </summary>
			public bool IsDisableWriteTypeTree = false;

			/// <summary>
			/// 忽略类型树变化
			/// </summary>
			public bool IsIgnoreTypeTreeChanges = true;

			/// <summary>
			/// 禁用名称查找资源（可以降内存并提高加载效率）
			/// </summary>
			public bool IsDisableLoadAssetByFileName = false;
			#endregion


			/// <summary>
			/// 获取内置标记列表
			/// </summary>
			public List<string> GetBuildinTags()
			{
				return StringUtility.StringToStringList(BuildinTags, ';');
			}
		}

		/// <summary>
		/// 构建参数环境
		/// </summary>
		public class BuildParametersContext : IContextObject
		{
			/// <summary>
			/// 构建参数
			/// </summary>
			public BuildParameters Parameters { private set; get; }

			/// <summary>
			/// 构建管线的输出目录
			/// </summary>
			public string PipelineOutputDirectory { private set; get; }


			public BuildParametersContext(BuildParameters parameters)
			{
				Parameters = parameters;
				PipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(parameters.OutputRoot, parameters.BuildTarget);
			}

			/// <summary>
			/// 获取本次构建的补丁目录
			/// </summary>
			public string GetPackageDirectory()
			{
				return $"{Parameters.OutputRoot}/{Parameters.BuildTarget}/{Parameters.BuildVersion}";
			}

			/// <summary>
			/// 获取构建选项
			/// </summary>
			public BuildAssetBundleOptions GetPipelineBuildOptions()
			{
				// For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
				// 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

				BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
				opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

				if (Parameters.CompressOption == ECompressOption.Uncompressed)
					opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
				else if (Parameters.CompressOption == ECompressOption.LZ4)
					opt |= BuildAssetBundleOptions.ChunkBasedCompression;

				if (Parameters.IsForceRebuild)
					opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
				if (Parameters.IsAppendHash)
					opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName; //Append the hash to the assetBundle name
				if (Parameters.IsDisableWriteTypeTree)
					opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
				if (Parameters.IsIgnoreTypeTreeChanges)
					opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.
				if (Parameters.IsDisableLoadAssetByFileName)
				{
					opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName; //Disables Asset Bundle LoadAsset by file name.
					opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension; //Disables Asset Bundle LoadAsset by file name with extension.
				}

				return opt;
			}
		}


		private readonly BuildContext _buildContext = new BuildContext();

		/// <summary>
		/// 开始构建
		/// </summary>
		public bool Run(BuildParameters buildParameters)
		{
			// 清空旧数据
			_buildContext.ClearAllContext();

			// 构建参数
			var buildParametersContext = new BuildParametersContext(buildParameters);
			_buildContext.SetContextObject(buildParametersContext);

			// 执行构建流程
			List<IBuildTask> pipeline = new List<IBuildTask>
			{
				new TaskPrepare(), //前期准备工作
				new TaskGetBuildMap(), //获取构建列表
				new TaskBuilding(), //开始执行构建			
				new TaskEncryption(), //加密资源文件
				new TaskCreatePatchManifest(), //创建清单文件
				new TaskCreateReadme(), //创建说明文件
				new TaskCreateReport(), //创建报告文件
				new TaskCreatePatchPackage(), //制作补丁包
				new TaskCopyBuildinFiles(), //拷贝内置文件
			};

			bool succeed = BuildRunner.Run(pipeline, _buildContext);
			if (succeed)
				Debug.Log($"构建成功！");
			else
				Debug.LogWarning($"构建失败！");
			return succeed;
		}
	}
}