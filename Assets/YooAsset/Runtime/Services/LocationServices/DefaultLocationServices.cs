
namespace YooAsset
{
	public class DefaultLocationServices : ILocationServices
	{
		private readonly string _resourceRoot;

		public DefaultLocationServices(string resourceRoot)
		{
			if (string.IsNullOrEmpty(resourceRoot) == false)
				_resourceRoot = PathHelper.GetRegularPath(resourceRoot);
		}

		string ILocationServices.ConvertLocationToAssetPath(AssetsPackage package, string location)
		{
			if (string.IsNullOrEmpty(_resourceRoot))
			{
				return package.MappingToAssetPath(location);
			}
			else
			{
				string tempLocation = $"{_resourceRoot}/{location}";
				return package.MappingToAssetPath(tempLocation);
			}
		}
	}
}