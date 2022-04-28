
namespace YooAsset
{
	public class DefaultLocationServices : ILocationServices
	{
		private readonly string _resourceRoot;

		public DefaultLocationServices(string resourceRoot)
		{
			if (string.IsNullOrEmpty(resourceRoot) == false)
				_resourceRoot = PathHelper.GetRegularPath(resourceRoot);

#if UNITY_EDITOR
			LocationServicesHelper.InitEditorPlayMode(false);
#endif
		}
		
		public string ConvertLocationToAssetPath(YooAssets.EPlayMode playMode, string location)
		{
			location = CombineAssetPath(_resourceRoot, location);
			if (playMode == YooAssets.EPlayMode.EditorPlayMode)
			{
#if UNITY_EDITOR
				return LocationServicesHelper.ConvertLocationToAssetPath(location);
#else
				throw new System.NotImplementedException();
#endif
			}
			else
			{
				return location;
			}
		}
		private string CombineAssetPath(string root, string location)
		{
			if (string.IsNullOrEmpty(root))
				return location;
			else
				return $"{root}/{location}";
		}
	}
}