using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
	[TaskAttribute("创建清单文件")]
	public class TaskCreateManifest_SBP : TaskCreateManifest, IBuildTask
	{
		private TaskBuilding_SBP.BuildResultContext _buildResultContext = null;

		void IBuildTask.Run(BuildContext context)
		{
			CreateManifestFile(context);
		}

		protected override string[] GetBundleDepends(BuildContext context, string bundleName)
		{
			if (_buildResultContext == null)
				_buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();

			if (_buildResultContext.Results.BundleInfos.ContainsKey(bundleName) == false)
				throw new Exception($"Not found bundle in SBP build results : {bundleName}");
			return _buildResultContext.Results.BundleInfos[bundleName].Dependencies;
		}
	}
}