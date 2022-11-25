
namespace UniFramework.Module
{
	public abstract class ModuleSingleton<T> where T : class, IModule
	{
		private static T _instance;
		public static T Instance
		{
			get
			{
				if (_instance == null)
					UniLogger.Error($"{typeof(T)} is not create. Use {nameof(UniModule)}.{nameof(UniModule.CreateModule)} create.");
				return _instance;
			}
		}

		protected ModuleSingleton()
		{
			if (_instance != null)
				throw new System.Exception($"{typeof(T)} instance already created.");
			_instance = this as T;
		}
		protected void DestroySingleton()
		{
			_instance = null;
		}
	}
}