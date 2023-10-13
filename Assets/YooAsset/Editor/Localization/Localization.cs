using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	[InitializeOnLoad]
	internal static class Localization
	{
		private static ILocalization _localization;

		static Localization()
		{
			var types = EditorTools.GetAssignableTypes(typeof(ILocalization));
			_localization = (ILocalization)Activator.CreateInstance(types[0]);
			CheckContent();
		}

		/// <summary>
		/// 检测多语言是否配置完整
		/// </summary>
		private static void CheckContent()
		{
			foreach (ELanguageKey key in Enum.GetValues(typeof(ELanguageKey)))
			{
				_localization.GetLanguage(key);
			}
		}

		/// <summary>
		/// 字符串转换为枚举类型
		/// </summary>
		public static ELanguageKey Convert(string enumName)
		{
			return (ELanguageKey)Enum.Parse(typeof(ELanguageKey), enumName);
		}

		/// <summary>
		/// 获取多语言内容
		/// </summary>
		public static string Language(ELanguageKey key, params object[] args)
		{
			string content = _localization.GetLanguage(key);
			if (args.Length > 0)
				return string.Format(content, args);
			else
				return content;
		}
	}
}