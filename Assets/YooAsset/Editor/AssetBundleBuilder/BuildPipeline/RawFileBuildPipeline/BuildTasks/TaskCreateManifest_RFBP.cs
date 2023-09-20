using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[TaskAttribute("创建清单文件")]
	public class TaskCreateManifest_RFBP : TaskCreateManifest, IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			CreateManifestFile(context);
		}

		protected override string[] GetBundleDepends(BuildContext context, string bundleName)
		{
			return new string[] { };
		}
	}
}