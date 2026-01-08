using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal sealed class ClearUnusedCacheBundleFilesOperation : FSClearCacheFilesOperation
    {
        private enum ESteps
        {
            None,
            CheckManifest,
            GetUnusedCacheFiles,
            ClearUnusedCacheFiles,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly PackageManifest _manifest;
        private List<string> _unusedBundleGUIDs;
        private int _unusedFileTotalCount = 0;
        private ESteps _steps = ESteps.None;


        internal ClearUnusedCacheBundleFilesOperation(DefaultCacheFileSystem fileSystem, PackageManifest manifest)
        {
            _fileSystem = fileSystem;
            _manifest = manifest;
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
                    _steps = ESteps.GetUnusedCacheFiles;
                }
            }

            if (_steps == ESteps.GetUnusedCacheFiles)
            {
                _unusedBundleGUIDs = GetUnusedBundleGUIDs();
                _unusedFileTotalCount = _unusedBundleGUIDs.Count;
                _steps = ESteps.ClearUnusedCacheFiles;
                YooLogger.Log($"Found unused cache files count : {_unusedFileTotalCount}");
            }

            if (_steps == ESteps.ClearUnusedCacheFiles)
            {
                for (int i = _unusedBundleGUIDs.Count - 1; i >= 0; i--)
                {
                    string bundleGUID = _unusedBundleGUIDs[i];
                    _fileSystem.DeleteCacheBundleFile(bundleGUID);
                    _unusedBundleGUIDs.RemoveAt(i);
                    if (IsBusy)
                        break;
                }

                if (_unusedFileTotalCount == 0)
                    Progress = 1.0f;
                else
                    Progress = 1.0f - (_unusedBundleGUIDs.Count / _unusedFileTotalCount);

                if (_unusedBundleGUIDs.Count == 0)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
            }
        }

        private List<string> GetUnusedBundleGUIDs()
        {
            var allBundleGUIDs = _fileSystem.GetAllCachedBundleGUIDs();
            List<string> result = new List<string>(allBundleGUIDs.Count);
            foreach (var bundleGUID in allBundleGUIDs)
            {
                if (_manifest.IsIncludeBundleFile(bundleGUID) == false)
                {
                    result.Add(bundleGUID);
                }
            }
            return result;
        }
    }
}