
namespace YooAsset
{
	public class AddressLocationServices : ILocationServices
	{
		string ILocationServices.ConvertLocationToAssetPath(AssetsPackage package, string location)
		{
			return package.MappingToAssetPath(location);
		}
	}
}