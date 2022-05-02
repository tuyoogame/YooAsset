#if UNITY_EDITOR
using System.Reflection;

namespace YooAsset
{
	internal static class EditorPlayModeHelper
	{
		private static System.Type _classType;

		public static string DryRunBuild()
		{
			_classType = Assembly.Load("YooAsset.Editor").GetType("YooAsset.Editor.AssetBundleRuntimeBuilder");
			InvokePublicStaticMethod(_classType, "FastBuild");
			return GetPatchManifestFilePath();
		}
		private static string GetPatchManifestFilePath()
		{
			return (string)InvokePublicStaticMethod(_classType, "GetPatchManifestFilePath");
		}
		private static object InvokePublicStaticMethod(System.Type type, string method, params object[] parameters)
		{
			var methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
			if (methodInfo == null)
			{
				UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
				return null;
			}
			return methodInfo.Invoke(null, parameters);
		}
	}
}
#else
	internal static class EditorPlayModeHelper
	{
		public static string DryRunBuild() { throw new System.Exception("Only support in unity editor !"); }
	}
#endif