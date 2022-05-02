#if UNITY_EDITOR
using System.Reflection;

namespace YooAsset
{
	internal static class LocationServicesHelper
	{
		private static System.Type _classType;

		public static void InitEditorPlayMode(bool enableAddressable)
		{
			_classType = Assembly.Load("YooAsset.Editor").GetType("YooAsset.Editor.AssetBundleGrouperRuntimeSupport");
			InvokePublicStaticMethod(_classType, "InitEditorPlayMode", enableAddressable);
		}
		public static string ConvertLocationToAssetPath(string location)
		{
			return (string)InvokePublicStaticMethod(_classType, "ConvertLocationToAssetPath", location);
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
#endif