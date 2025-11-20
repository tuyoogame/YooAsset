using System.Collections.Generic;

namespace YooAsset
{
    internal class ClearCacheBundleFilesByLocationsOperaiton : FSClearCacheFilesOperation
    {
        private enum ESteps
        {
            None,
            CheckManifest,
            CheckArgs,
            GetClearCacheFiles,
            ClearFilterCacheFiles,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly PackageManifest _manifest;
        private readonly object _clearParam;
        private string[] _locations;
        private List<string> _clearBundleGUIDs;
        private int _clearFileTotalCount = 0;
        private ESteps _steps = ESteps.None;

        internal ClearCacheBundleFilesByLocationsOperaiton(DefaultCacheFileSystem fileSystem, PackageManifest manifest, object clearParam)
        {
            _fileSystem = fileSystem;
            _manifest = manifest;
            _clearParam = clearParam;
        }

        internal override void InternalStart()
        {
            _steps = ESteps.CheckManifest;
        }

        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckManifest)
            {
                if (_manifest == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Can not found active package manifest !";
                }
                else
                {
                    _steps = ESteps.CheckArgs;
                }
            }

            if (_steps == ESteps.CheckArgs)
            {
                if (_clearParam == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Clear param is null !";
                    return;
                }

                if (_clearParam is string)
                {
                    _locations = new string[] { _clearParam as string };
                }
                else if (_clearParam is List<string>)
                {
                    var tempList = _clearParam as List<string>;
                    _locations = tempList.ToArray();
                }
                else if (_clearParam is string[])
                {
                    _locations = _clearParam as string[];
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Invalid clear param : {_clearParam.GetType().FullName}";
                    return;
                }

                _steps = ESteps.GetClearCacheFiles;
            }

            if (_steps == ESteps.GetClearCacheFiles)
            {
                _clearBundleGUIDs = GetBundleGUIDsByLocation();
                _clearFileTotalCount = _clearBundleGUIDs.Count;
                _steps = ESteps.ClearFilterCacheFiles;
            }

            if (_steps == ESteps.ClearFilterCacheFiles)
            {
                for (int i = _clearBundleGUIDs.Count - 1; i >= 0; i--)
                {
                    string bundleGUID = _clearBundleGUIDs[i];
                    _fileSystem.DeleteCacheBundleFile(bundleGUID);
                    _clearBundleGUIDs.RemoveAt(i);
                    if (OperationSystem.IsBusy)
                        break;
                }

                if (_clearFileTotalCount == 0)
                    Progress = 1.0f;
                else
                    Progress = 1.0f - (_clearBundleGUIDs.Count / _clearFileTotalCount);

                if (_clearBundleGUIDs.Count == 0)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
            }
        }

        private List<string> GetBundleGUIDsByLocation()
        {
            List<string> result = new List<string>(_locations.Length);

            foreach (var location in _locations)
            {
                string assetPath = _manifest.TryMappingToAssetPath(location);
                if (_manifest.TryGetPackageAsset(assetPath, out PackageAsset packageAsset))
                {
                    PackageBundle bundle = _manifest.GetMainPackageBundle(packageAsset.BundleID);

                    if (bundle != null)
                    {
                        result.Add(bundle.BundleGUID);
                    }
                }
            }

            return result;
        }
    }
}