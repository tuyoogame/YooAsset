#if UNITY_ANDROID && GOOGLE_PLAY
using YooAsset;
using Google.Play.AssetDelivery;

/// <summary>
/// Loads an AssetBundle through Google Play Asset Delivery API.
/// </summary>
internal sealed class GPFSLoadPackageBundleOperation : FSLoadPackageBundleOperation
{
    private enum ESteps
    {
        None,
        LoadAssetBundle,
        CheckResult,
        Done,
    }

    private readonly GooglePlayFileSystem _fileSystem;
    private readonly FSLoadPackageBundleOptions _options;
    private PlayAssetBundleRequest _bundleRequest;
    private ESteps _steps = ESteps.None;

    internal GPFSLoadPackageBundleOperation(GooglePlayFileSystem fileSystem, FSLoadPackageBundleOptions options)
    {
        _fileSystem = fileSystem;
        _options = options;
    }
    protected override void InternalStart()
    {
        _steps = ESteps.LoadAssetBundle;
    }
    protected override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.LoadAssetBundle)
        {
            if (_options.Bundle.IsEncrypted)
            {
                _steps = ESteps.Done;
                SetError($"{nameof(GooglePlayFileSystem)} does not support encrypted bundles.");
                YooLogger.LogError(Error);
                return;
            }

            _bundleRequest = PlayAssetDelivery.RetrieveAssetBundleAsync(_options.Bundle.GetFileName());
            _steps = ESteps.CheckResult;
        }

        if (_steps == ESteps.CheckResult)
        {
            if (_bundleRequest.IsDone == false)
                return;

            if (_bundleRequest.Error != AssetDeliveryErrorCode.NoError)
            {
                _steps = ESteps.Done;
                SetError($"Failed to load asset bundle via Play Asset Delivery: {_options.Bundle.BundleName} Error: {_bundleRequest.Error}");
                YooLogger.LogError(Error);
            }
            else
            {
                _steps = ESteps.Done;
                SetResult();
                BundleHandle = new AssetBundleHandle(string.Empty, _options.Bundle, _bundleRequest.AssetBundle, null);
            }
        }
    }
    protected override void InternalWaitForCompletion()
    {
        if (_steps != ESteps.Done)
        {
            _steps = ESteps.Done;
            SetError($"{nameof(GooglePlayFileSystem)} does not support synchronous loading.");
            YooLogger.LogError(Error);
        }
    }
}
#endif
