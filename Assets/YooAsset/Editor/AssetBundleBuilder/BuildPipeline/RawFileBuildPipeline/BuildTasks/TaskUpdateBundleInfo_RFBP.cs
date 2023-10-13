using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public class TaskUpdateBundleInfo_RFBP : TaskUpdateBundleInfo, IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			UpdateBundleInfo(context);
		}

		protected override string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var parameters = buildParametersContext.Parameters;
			var buildMode = parameters.BuildMode;
			if (buildMode == EBuildMode.SimulateBuild)
			{
				return "00000000000000000000000000000000"; //32位
			}
			else
			{
				string filePath = bundleInfo.PackageSourceFilePath;
				return HashUtility.FileMD5(filePath);
			}
		}
		protected override uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
		{
			return 0;
		}
		protected override string GetBundleFileHash(string filePath, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.SimulateBuild)
				return GetFilePathTempHash(filePath);
			else
				return HashUtility.FileMD5(filePath);
		}
		protected override string GetBundleFileCRC(string filePath, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.SimulateBuild)
				return "00000000"; //8位
			else
				return HashUtility.FileCRC32(filePath);
		}
		protected override long GetBundleFileSize(string filePath, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.SimulateBuild)
				return 0;
			else
				return FileUtility.GetFileSize(filePath);
		}
	}
}