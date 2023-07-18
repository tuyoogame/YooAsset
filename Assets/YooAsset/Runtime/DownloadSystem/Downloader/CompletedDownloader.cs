
namespace YooAsset
{
	internal sealed class CompletedDownloader : DownloaderBase
	{
		public CompletedDownloader(BundleInfo bundleInfo) : base(bundleInfo, 0, 0)
		{
			_downloadProgress = 1f;
			_downloadedBytes = (ulong)bundleInfo.Bundle.FileSize;
			_status = EStatus.Succeed;
		}

		public override void SendRequest(params object[] param)
		{
		}
		public override void Update()
		{
		}
		public override void Abort()
		{
		}
	}
}