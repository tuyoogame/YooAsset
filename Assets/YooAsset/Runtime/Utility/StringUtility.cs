using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace YooAsset.Utility
{
	public static class StringUtility
	{
		[ThreadStatic]
		private static StringBuilder _cacheBuilder = new StringBuilder(1024);

		public static string Format(string format, object arg0)
		{
			if (string.IsNullOrEmpty(format))
				throw new ArgumentNullException();

			_cacheBuilder.Length = 0;
			_cacheBuilder.AppendFormat(format, arg0);
			return _cacheBuilder.ToString();
		}
		public static string Format(string format, object arg0, object arg1)
		{
			if (string.IsNullOrEmpty(format))
				throw new ArgumentNullException();

			_cacheBuilder.Length = 0;
			_cacheBuilder.AppendFormat(format, arg0, arg1);
			return _cacheBuilder.ToString();
		}
		public static string Format(string format, object arg0, object arg1, object arg2)
		{
			if (string.IsNullOrEmpty(format))
				throw new ArgumentNullException();

			_cacheBuilder.Length = 0;
			_cacheBuilder.AppendFormat(format, arg0, arg1, arg2);
			return _cacheBuilder.ToString();
		}
		public static string Format(string format, params object[] args)
		{
			if (string.IsNullOrEmpty(format))
				throw new ArgumentNullException();

			if (args == null)
				throw new ArgumentNullException();

			_cacheBuilder.Length = 0;
			_cacheBuilder.AppendFormat(format, args);
			return _cacheBuilder.ToString();
		}

		public static List<string> StringToStringList(string str, char separator)
		{
			List<string> result = new List<string>();
			if (!String.IsNullOrEmpty(str))
			{
				string[] splits = str.Split(separator);
				foreach (string split in splits)
				{
					if (!String.IsNullOrEmpty(split))
					{
						result.Add(split);
					}
				}
			}
			return result;
		}
		public static bool StringToBool(string str)
		{
			int value = (int)Convert.ChangeType(str, typeof(int));
			return value > 0;
		}
		public static T NameToEnum<T>(string name)
		{
			if (Enum.IsDefined(typeof(T), name) == false)
			{
				throw new ArgumentException($"Enum {typeof(T)} is not defined name {name}");
			}
			return (T)Enum.Parse(typeof(T), name);
		}

		public static string RemoveFirstChar(string str)
		{
			if (string.IsNullOrEmpty(str))
				return str;
			return str.Substring(1);
		}
		public static string RemoveLastChar(string str)
		{
			if (string.IsNullOrEmpty(str))
				return str;
			return str.Substring(0, str.Length - 1);
		}
		public static string RemoveExtension(string str)
		{
			if (string.IsNullOrEmpty(str))
				return str;

			int index = str.LastIndexOf(".");
			if (index == -1)
				return str;
			else
				return str.Remove(index); //"assets/config/test.unity3d" --> "assets/config/test"
		}
	}
}