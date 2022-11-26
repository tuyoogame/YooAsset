
namespace YooAsset
{
	internal sealed class CompletedProvider : ProviderBase
	{
		public override float Progress
		{
			get
			{
				if (IsDone)
					return 1f;
				else
					return 0;
			}
		}

		public CompletedProvider(AssetInfo assetInfo) : base(null, string.Empty, assetInfo)
		{
		}
		public override void Update()
		{
		}
		public void SetCompleted(string error)
		{
			if (Status == EStatus.None)
			{
				Status = EStatus.Failed;
				LastError = error;
				InvokeCompletion();
			}
		}
	}
}