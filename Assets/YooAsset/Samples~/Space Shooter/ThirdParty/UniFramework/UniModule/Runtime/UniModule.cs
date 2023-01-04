using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Module
{
	public static class UniModule
	{
		private class Wrapper
		{
			public int Priority { private set; get; }
			public IModule Module { private set; get; }

			public Wrapper(IModule module, int priority)
			{
				Module = module;
				Priority = priority;
			}
		}

		private static bool _isInitialize = false;
		private static GameObject _driver = null;
		private static readonly List<Wrapper> _wrappers = new List<Wrapper>(100);
		private static MonoBehaviour _behaviour;
		private static bool _isDirty = false;

		/// <summary>
		/// 初始化模块系统
		/// </summary>
		public static void Initialize()
		{
			if (_isInitialize)
				throw new Exception($"{nameof(UniModule)} is initialized !");

			if (_isInitialize == false)
			{
				// 创建驱动器
				_isInitialize = true;
				_driver = new UnityEngine.GameObject($"[{nameof(UniModule)}]");
				_behaviour = _driver.AddComponent<UniModuleDriver>();
				UnityEngine.Object.DontDestroyOnLoad(_driver);
				UniLogger.Log($"{nameof(UniModule)} initalize !");
			}
		}

		/// <summary>
		/// 销毁模块系统
		/// </summary>
		public static void Destroy()
		{
			if (_isInitialize)
			{
				DestroyAll();

				_isInitialize = false;
				if (_driver != null)
					GameObject.Destroy(_driver);
				UniLogger.Log($"{nameof(UniModule)} destroy all !");
			}
		}

		/// <summary>
		/// 更新模块系统
		/// </summary>
		internal static void Update()
		{
			// 如果需要重新排序
			if (_isDirty)
			{
				_isDirty = false;
				_wrappers.Sort((left, right) =>
				{
					if (left.Priority > right.Priority)
						return -1;
					else if (left.Priority == right.Priority)
						return 0;
					else
						return 1;
				});
			}

			// 轮询所有模块
			for (int i = 0; i < _wrappers.Count; i++)
			{
				_wrappers[i].Module.OnUpdate();
			}
		}

		/// <summary>
		/// 获取模块
		/// </summary>
		public static T GetModule<T>() where T : class, IModule
		{
			System.Type type = typeof(T);
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Module.GetType() == type)
					return _wrappers[i].Module as T;
			}

			UniLogger.Error($"Not found manager : {type}");
			return null;
		}

		/// <summary>
		/// 查询模块是否存在
		/// </summary>
		public static bool Contains<T>() where T : class, IModule
		{
			System.Type type = typeof(T);
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Module.GetType() == type)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 创建模块
		/// </summary>
		/// <param name="priority">运行时的优先级，优先级越大越早执行。如果没有设置优先级，那么会按照添加顺序执行</param>
		public static T CreateModule<T>(int priority = 0) where T : class, IModule
		{
			return CreateModule<T>(null, priority);
		}

		/// <summary>
		/// 创建模块
		/// </summary>
		/// <param name="createParam">附加参数</param>
		/// <param name="priority">运行时的优先级，优先级越大越早执行。如果没有设置优先级，那么会按照添加顺序执行</param>
		public static T CreateModule<T>(System.Object createParam, int priority = 0) where T : class, IModule
		{
			if (priority < 0)
				throw new Exception("The priority can not be negative");

			if (Contains<T>())
				throw new Exception($"Module is already existed : {typeof(T)}");

			// 如果没有设置优先级
			if (priority == 0)
			{
				int minPriority = GetMinPriority();
				priority = --minPriority;
			}

			T module = Activator.CreateInstance<T>();
			Wrapper wrapper = new Wrapper(module, priority);
			wrapper.Module.OnCreate(createParam);
			_wrappers.Add(wrapper);
			_isDirty = true;
			return module;
		}

		/// <summary>
		/// 销毁模块
		/// </summary>
		public static bool DestroyModule<T>() where T : class, IModule
		{
			var type = typeof(T);
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Module.GetType() == type)
				{
					_wrappers[i].Module.OnDestroy();
					_wrappers.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 开启一个协程
		/// </summary>
		public static Coroutine StartCoroutine(IEnumerator coroutine)
		{
			return _behaviour.StartCoroutine(coroutine);
		}
		public static Coroutine StartCoroutine(string methodName)
		{
			return _behaviour.StartCoroutine(methodName);
		}

		/// <summary>
		/// 停止一个协程
		/// </summary>
		public static void StopCoroutine(Coroutine coroutine)
		{
			_behaviour.StopCoroutine(coroutine);
		}
		public static void StopCoroutine(string methodName)
		{
			_behaviour.StopCoroutine(methodName);
		}

		/// <summary>
		/// 停止所有协程
		/// </summary>
		public static void StopAllCoroutines()
		{
			_behaviour.StopAllCoroutines();
		}

		private static int GetMinPriority()
		{
			int minPriority = 0;
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_wrappers[i].Priority < minPriority)
					minPriority = _wrappers[i].Priority;
			}
			return minPriority; //小于等于零
		}
		private static void DestroyAll()
		{
			for (int i = 0; i < _wrappers.Count; i++)
			{
				_wrappers[i].Module.OnDestroy();
			}
			_wrappers.Clear();
		}
	}
}