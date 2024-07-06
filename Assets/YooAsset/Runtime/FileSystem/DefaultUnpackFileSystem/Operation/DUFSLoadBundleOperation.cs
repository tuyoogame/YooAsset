
namespace YooAsset
{
    internal class DUFSLoadAssetBundleOperation : DCFSLoadAssetBundleOperation
    {
        public DUFSLoadAssetBundleOperation(DefaultUnpackFileSystem fileSystem, PackageBundle bundle) : base(fileSystem, bundle)
        {
        }
        public override void WaitForAsyncComplete()
        {
            _isWaitForAsyncComplete = true;

            while (true)
            {
                // 文件解压
                if (_downloadFileOp != null)
                {
                    if (_downloadFileOp.IsDone == false)
                        _downloadFileOp.WaitForAsyncComplete();
                }

                // 驱动流程
                InternalOnUpdate();

                // 完成后退出
                if (IsDone)
                    break;
            }
        }
    }

    internal class DUFSLoadRawBundleOperation : DCFSLoadRawBundleOperation
    {
        public DUFSLoadRawBundleOperation(DefaultUnpackFileSystem fileSystem, PackageBundle bundle) : base(fileSystem, bundle)
        {
        }
        public override void WaitForAsyncComplete()
        {
            while (true)
            {
                // 文件解压
                if (_downloadFileOp != null)
                {
                    if (_downloadFileOp.IsDone == false)
                        _downloadFileOp.WaitForAsyncComplete();
                }

                // 驱动流程
                InternalOnUpdate();

                // 完成后退出
                if (IsDone)
                    break;
            }
        }
    }
}