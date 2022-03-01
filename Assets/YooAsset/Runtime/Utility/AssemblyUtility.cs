using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace YooAsset.Utility
{
	public static class AssemblyUtility
	{
		public const string YooAssetAssemblyName = "YooAsset";
		public const string YooAssetAssemblyEditorName = "YooAsset.Editor";
		public const string UnityDefaultAssemblyName = "Assembly-CSharp";
		public const string UnityDefaultAssemblyEditorName = "Assembly-CSharp-Editor";
		

		private static readonly Dictionary<string, List<Type>> _cache = new Dictionary<string, List<Type>>();

		static AssemblyUtility()
		{
			_cache.Clear();
		}

		/// <summary>
		/// 获取程序集
		/// </summary>
		public static Assembly GetAssembly(string assemblyName)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.GetName().Name == assemblyName)
					return assembly;
			}
			return null;
		}

		/// <summary>
		/// 获取程序集里的所有类型
		/// </summary>
		private static List<Type> GetTypes(string assemblyName)
		{
			if (_cache.ContainsKey(assemblyName))
				return _cache[assemblyName];

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.GetName().Name == assemblyName)
				{
					List<Type> types = assembly.GetTypes().ToList();
					_cache.Add(assemblyName, types);
					return types;
				}
			}

			// 注意：如果没有找到程序集返回空列表
			UnityEngine.Debug.LogWarning($"Not found assembly : {assemblyName}");
			return new List<Type>();
		}

		/// <summary>
		/// 获取带继承关系的所有类的类型
		/// <param name="parentType">父类类型</param> 
		/// </summary>
		public static List<Type> GetAssignableTypes(string assemblyName, System.Type parentType)
		{
			List<Type> result = new List<Type>();
			List<Type> cacheTypes =	GetTypes(assemblyName);
			for (int i = 0; i < cacheTypes.Count; i++)
			{
				Type type = cacheTypes[i];

				// 判断继承关系
				if (parentType.IsAssignableFrom(type))
				{
					if (type.Name == parentType.Name)
						continue;
					result.Add(type);
				}
			}
			return result;
		}

		/// <summary>
		/// 获取带属性标签的所有类的类型
		/// <param name="attributeType">属性类型</param>
		/// </summary>
		public static List<Type> GetAttributeTypes(string assemblyName, System.Type attributeType)
		{
			List<Type> result = new List<Type>();
			List<Type> cacheTypes = GetTypes(assemblyName);
			for (int i = 0; i < cacheTypes.Count; i++)
			{
				System.Type type = cacheTypes[i];

				// 判断属性标签
				if (Attribute.IsDefined(type, attributeType))
				{
					result.Add(type);	
				}
			}
			return result;
		}

		/// <summary>
		/// 获取带继承关系和属性标签的所有类的类型
		/// </summary>
		/// <param name="parentType">父类类型</param>
		/// <param name="attributeType">属性类型</param>
		public static List<Type> GetAssignableAttributeTypes(string assemblyName, System.Type parentType, System.Type attributeType, bool checkError = true)
		{
			List<Type> result = new List<Type>();
			List<Type> cacheTypes = GetTypes(assemblyName);
			for (int i = 0; i < cacheTypes.Count; i++)
			{
				Type type = cacheTypes[i];

				// 判断属性标签
				if (Attribute.IsDefined(type, attributeType))
				{
					// 判断继承关系
					if (parentType.IsAssignableFrom(type))
					{
						if (type.Name == parentType.Name)
							continue;
						result.Add(type);
					}
					else
					{
						if(checkError)
							throw new Exception($"class {type} must inherit from {parentType}.");
					}
				}
			}
			return result;
		}
	}
}