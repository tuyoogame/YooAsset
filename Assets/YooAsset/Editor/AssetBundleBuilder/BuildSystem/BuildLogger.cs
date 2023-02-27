
namespace YooAsset.Editor
{
	public static class BuildLogger
	{
		/// <summary>
		/// 是否启用LOG
		/// </summary>
		public static bool EnableLog = true;

		/// <summary>
		/// 日志输出
		/// </summary>
		public static void Log(string info)
		{
			if (EnableLog)
			{
				UnityEngine.Debug.Log(info);
			}
		}

		/// <summary>
		/// 日志输出
		/// </summary>
		public static void Info(string info)
		{
			UnityEngine.Debug.Log(info);
		}
	}
}