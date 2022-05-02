
namespace YooAsset
{
	public class AddressLocationServices : ILocationServices
	{
		public string ConvertLocationToAssetPath(string location)
		{
			return YooAssets.MappingToAssetPath(location);
		}
	}
}