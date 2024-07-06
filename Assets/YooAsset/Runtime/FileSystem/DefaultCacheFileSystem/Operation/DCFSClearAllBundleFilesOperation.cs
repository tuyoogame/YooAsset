using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal sealed class DCFSClearAllBundleFilesOperation : FSClearAllBundleFilesOperation
    {
        private enum ESteps
        {
            None,
            GetAllCacheFiles,
            ClearAllCacheFiles,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private List<string> _allBundleGUIDs;
        private int _fileTotalCount = 0;
        private ESteps _steps = ESteps.None;


        internal DCFSClearAllBundleFilesOperation(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.GetAllCacheFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.GetAllCacheFiles)
            {
                _allBundleGUIDs = _fileSystem.GetAllCachedBundleGUIDs();
                _fileTotalCount = _allBundleGUIDs.Count;
                _steps = ESteps.ClearAllCacheFiles;
                YooLogger.Log($"Found all cache files count : {_fileTotalCount}");
            }

            if (_steps == ESteps.ClearAllCacheFiles)
            {
                for (int i = _allBundleGUIDs.Count - 1; i >= 0; i--)
                {
                    string bundleGUID = _allBundleGUIDs[i];
                    _fileSystem.DeleteCacheFile(bundleGUID);
                    _allBundleGUIDs.RemoveAt(i);
                    if (OperationSystem.IsBusy)
                        break;
                }

                if (_fileTotalCount == 0)
                    Progress = 1.0f;
                else
                    Progress = 1.0f - (_allBundleGUIDs.Count / _fileTotalCount);

                if (_allBundleGUIDs.Count == 0)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
            }
        }
    }
}