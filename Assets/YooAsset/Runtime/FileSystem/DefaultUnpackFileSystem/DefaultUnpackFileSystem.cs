
namespace YooAsset
{
    /// <summary>
    /// 解压文件系统
    /// </summary>
    internal class DefaultUnpackFileSystem : DefaultCacheFileSystem
    {
        public DefaultUnpackFileSystem()
        {
        }
        public override void OnCreate(string packageName, string rootDirectory)
        {
            base.OnCreate(packageName, rootDirectory);

            // 注意：重写保存根目录和临时目录
            _saveFileRoot = PathUtility.Combine(_packageRoot, DefaultUnpackFileSystemDefine.SaveFilesFolderName);
            _tempFileRoot = PathUtility.Combine(_packageRoot, DefaultUnpackFileSystemDefine.TempFilesFolderName);
        }
    }
}