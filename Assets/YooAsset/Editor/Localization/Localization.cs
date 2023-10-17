using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	[InitializeOnLoad]
	public static class Localization
	{
		private static readonly string EditorPrefsLanguageKey;
		private static ILocalization _localization;

		static Localization()
		{
			EditorPrefsLanguageKey = $"{Application.productName}_yoo_language";

			// 创建本地化实例类
			var classType = GetDefaultLocaleType();
			_localization = (ILocalization)Activator.CreateInstance(classType);

			// 检测本地化配置
			CheckContent();
		}

		/// <summary>
		/// 改变本地化语言
		/// </summary>
		/// <param name="classType">适配的本地化类</param>
		public static void ChangeDefaultLocale(System.Type classType)
		{
			var localization = (ILocalization)Activator.CreateInstance(classType);
			if (localization == null)
			{
				Debug.LogWarning($"Failed to create {nameof(ILocalization)} instance : {classType.FullName}");
				return;
			}

			EditorPrefs.SetString(EditorPrefsLanguageKey, classType.FullName);
			_localization = localization;

			// 检测本地化配置
			CheckContent();
		}

		/// <summary>
		/// 获取默认的本地化类
		/// </summary>
		private static System.Type GetDefaultLocaleType()
		{
			Type englishType = typeof(English);
			string defaultClassName = EditorPrefs.GetString(EditorPrefsLanguageKey, englishType.FullName);
			var classTypes = EditorTools.GetAssignableTypes(typeof(ILocalization));
			Type defaultType = classTypes.Find(x => x.FullName.Equals(defaultClassName));
			if (defaultType == null)
			{
				Debug.LogWarning($"Invalid {nameof(ILocalization)} type : {defaultType}");
				return englishType;
			}
			return defaultType;
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
		internal static ELanguageKey Convert(string enumName)
		{
			return (ELanguageKey)Enum.Parse(typeof(ELanguageKey), enumName);
		}

		/// <summary>
		/// 获取多语言内容
		/// </summary>
		internal static string Language(ELanguageKey key, params object[] args)
		{
			string content = _localization.GetLanguage(key);
			if (args.Length > 0)
				return string.Format(content, args);
			else
				return content;
		}
	}
}