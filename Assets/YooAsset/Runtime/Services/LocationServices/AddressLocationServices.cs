
namespace YooAsset
{
	public class AddressLocationServices : ILocationServices
	{
		string ILocationServices.ConvertLocationToAssetPath(YooAssetPackage package, string location)
		{
			return package.MappingToAssetPath(location);
		}
	}
}