
namespace YooAsset
{
	public class AddressLocationServices : ILocationServices
	{
		public AddressLocationServices()
		{
#if UNITY_EDITOR
			LocationServicesHelper.InitEditorPlayMode(true);
#endif
		}

		public string ConvertLocationToAssetPath(YooAssets.EPlayMode playMode, string location)
		{
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
				return YooAssets.ConvertAddress(location);
			}
		}
	}
}