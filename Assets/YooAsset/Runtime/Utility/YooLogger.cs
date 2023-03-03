using System.Diagnostics;

namespace YooAsset
{

	public abstract class IYooLogger
	{
		public abstract void Info(string msg);
		public abstract void Warning(string msg);
		
		public abstract void Error(string msg);
		public abstract void Exception(System.Exception exception);
	}

	internal static class YooLogger
	{

		/// <summary>
		/// 自定义日志处理
		/// </summary>
		public static IYooLogger Logger = null;
		
		/// <summary>
		/// 日志
		/// </summary>
		[Conditional("DEBUG")]
		public static void Log(string info)
		{
			if (Logger != null)
			{
				Logger.Info(info);
			}
			else
			{
				UnityEngine.Debug.Log(info);
			}
		}

		/// <summary>
		/// 警告
		/// </summary>
		public static void Warning(string info)
		{
			if (Logger != null)
			{
				Logger.Warning(info);
			}
			else
			{
				UnityEngine.Debug.LogWarning(info);
			}
		}

		/// <summary>
		/// 错误
		/// </summary>
		public static void Error(string info)
		{
			if (Logger != null)
			{
				Logger.Error(info);
			}
			else
			{
				UnityEngine.Debug.LogError(info);
			}
		}

		/// <summary>
		/// 异常
		/// </summary>
		public static void Exception(System.Exception exception)
		{
			if (Logger != null)
			{
				Logger.Exception(exception);
			}
			else
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
	}
}