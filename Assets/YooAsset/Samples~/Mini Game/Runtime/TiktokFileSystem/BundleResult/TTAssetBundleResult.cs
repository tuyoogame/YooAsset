#if UNITY_WEBGL && DOUYINMINIGAME
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TTSDK;

namespace YooAsset
{
    internal class TTAssetBundleResult : BundleResult
    {
        private readonly IFileSystem _fileSystem;
        private readonly PackageBundle _packageBundle;
        private readonly AssetBundle _assetBundle;
        
        public TTAssetBundleResult(IFileSystem fileSystem, PackageBundle packageBundle, AssetBundle assetBundle)
        {
            _fileSystem = fileSystem;
            _packageBundle = packageBundle;
            _assetBundle = assetBundle;
        }

        public override void UnloadBundleFile()
        {
            if (_assetBundle != null)
            {
                if (_packageBundle.Encrypted)
                    _assetBundle.Unload(true);
                else
                    _assetBundle.TTUnload(true);
            }
        }
        public override string GetBundleFilePath()
        {
            return _fileSystem.GetBundleFilePath(_packageBundle);
        }
        public override byte[] ReadBundleFileData()
        {
            return _fileSystem.ReadBundleFileData(_packageBundle);
        }
        public override string ReadBundleFileText()
        {
            return _fileSystem.ReadBundleFileText(_packageBundle);
        }

        public override FSLoadAssetOperation LoadAssetAsync(AssetInfo assetInfo)
        {
            var operation = new AssetBundleLoadAssetOperation(_packageBundle, _assetBundle, assetInfo);
            return operation;
        }
        public override FSLoadAllAssetsOperation LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new AssetBundleLoadAllAssetsOperation(_packageBundle, _assetBundle, assetInfo);
            return operation;
        }
        public override FSLoadSubAssetsOperation LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var operation = new AssetBundleLoadSubAssetsOperation(_packageBundle, _assetBundle, assetInfo);
            return operation;
        }
        public override FSLoadSceneOperation LoadSceneOperation(AssetInfo assetInfo, LoadSceneParameters loadParams, bool suspendLoad)
        {
            var operation = new AssetBundleLoadSceneOperation(assetInfo, loadParams, suspendLoad);
            return operation;
        }
    }
}
#endif