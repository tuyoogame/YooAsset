using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	/// <summary>
	/// 构建参数
	/// </summary>
	public abstract class BuildParameters
	{
		/// <summary>
		/// 构建输出的根目录
		/// </summary>
		public string BuildOutputRoot;

		/// <summary>
		/// 内置文件的根目录
		/// </summary>
		public string BuildinFileRoot;

		/// <summary>
		/// 构建管线
		/// </summary>
		public string BuildPipeline;

		/// <summary>
		/// 构建的平台
		/// </summary>
		public BuildTarget BuildTarget;

		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode;

		/// <summary>
		/// 构建的包裹名称
		/// </summary>
		public string PackageName;

		/// <summary>
		/// 构建的包裹版本
		/// </summary>
		public string PackageVersion;


		/// <summary>
		/// 验证构建结果
		/// </summary>
		public bool VerifyBuildingResult = false;

		/// <summary>
		/// 资源包名称样式
		/// </summary>
		public EFileNameStyle FileNameStyle;

		/// <summary>
		/// 内置文件的拷贝选项
		/// </summary>
		public EBuildinFileCopyOption BuildinFileCopyOption;

		/// <summary>
		/// 内置文件的拷贝参数
		/// </summary>
		public string BuildinFileCopyParams;

		/// <summary>
		/// 资源包加密服务类
		/// </summary>
		public IEncryptionServices EncryptionServices;
		


		private string _pipelineOutputDirectory = string.Empty;
		private string _packageOutputDirectory = string.Empty;
		private string _packageRootDirectory = string.Empty;
		private string _buildinRootDirectory = string.Empty;
		
		/// <summary>
		/// 检测构建参数是否合法
		/// </summary>
		public virtual void CheckBuildParameters()
		{
			// 检测当前是否正在构建资源包
			if (UnityEditor.BuildPipeline.isBuildingPlayer)
				throw new Exception("当前正在构建资源包，请结束后再试");

			// 检测构建参数合法性
			if (BuildTarget == BuildTarget.NoTarget)
				throw new Exception("请选择目标平台！");
			if (string.IsNullOrEmpty(PackageName))
				throw new Exception("包裹名称不能为空！");
			if (string.IsNullOrEmpty(PackageVersion))
				throw new Exception("包裹版本不能为空！");
			if (string.IsNullOrEmpty(BuildOutputRoot))
				throw new Exception("构建输出的根目录为空！");
			if (string.IsNullOrEmpty(BuildinFileRoot))
				throw new Exception("内置资源根目录为空！");

			// 检测是否有未保存场景
			if (BuildMode != EBuildMode.SimulateBuild)
			{
				if (EditorTools.HasDirtyScenes())
					throw new Exception("检测到未保存的场景文件");
			}

			// 强制构建删除包裹目录
			if (BuildMode == EBuildMode.ForceRebuild)
			{
				string packageRootDirectory = GetPackageRootDirectory();
				if (EditorTools.DeleteDirectory(packageRootDirectory))
				{
					BuildLogger.Log($"删除包裹目录：{packageRootDirectory}");
				}
			}

			// 检测包裹输出目录是否存在
			if (BuildMode != EBuildMode.SimulateBuild)
			{
				string packageOutputDirectory = GetPackageOutputDirectory();
				if (Directory.Exists(packageOutputDirectory))
					throw new Exception($"本次构建的补丁目录已经存在：{packageOutputDirectory}");
			}

			// 如果输出目录不存在
			string pipelineOutputDirectory = GetPipelineOutputDirectory();
			if (EditorTools.CreateDirectory(pipelineOutputDirectory))
			{
				BuildLogger.Log($"创建输出目录：{pipelineOutputDirectory}");
			}
		}


		/// <summary>
		/// 获取构建管线的输出目录
		/// </summary>
		/// <returns></returns>
		public string GetPipelineOutputDirectory()
		{
			if (string.IsNullOrEmpty(_pipelineOutputDirectory))
			{
				_pipelineOutputDirectory = $"{BuildOutputRoot}/{BuildTarget}/{PackageName}/{YooAssetSettings.OutputFolderName}";
			}
			return _pipelineOutputDirectory;
		}

		/// <summary>
		/// 获取本次构建的补丁输出目录
		/// </summary>
		public string GetPackageOutputDirectory()
		{
			if (string.IsNullOrEmpty(_packageOutputDirectory))
			{
				_packageOutputDirectory = $"{BuildOutputRoot}/{BuildTarget}/{PackageName}/{PackageVersion}";
			}
			return _packageOutputDirectory;
		}

		/// <summary>
		/// 获取本次构建的补丁根目录
		/// </summary>
		public string GetPackageRootDirectory()
		{
			if (string.IsNullOrEmpty(_packageRootDirectory))
			{
				_packageRootDirectory = $"{BuildOutputRoot}/{BuildTarget}/{PackageName}";
			}
			return _packageRootDirectory;
		}

		/// <summary>
		/// 获取内置资源的根目录
		/// </summary>
		public string GetBuildinRootDirectory()
		{
			if (string.IsNullOrEmpty(_buildinRootDirectory))
			{
				_buildinRootDirectory = $"{BuildinFileRoot}/{PackageName}";
			}
			return _buildinRootDirectory;
		}
	}
}