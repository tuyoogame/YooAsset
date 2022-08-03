
namespace YooAsset
{
	internal sealed class TempDownloader : DownloaderBase
	{
		public TempDownloader(BundleInfo bundleInfo) : base(bundleInfo)
		{
			_downloadProgress = 1f;
			_downloadedBytes = (ulong)bundleInfo.FileSize;
			_steps = ESteps.Succeed;
		}

		public override void Update()
		{
		}
		public override void Abort()
		{
		}
	}
}