
namespace YooAsset
{
    /// <summary>
    /// 资源清单文件处理服务接口
    /// </summary>
    public interface IManifestRestoreServices
    {
        /// <summary>
        /// 还原资源清单（解压或解密）
        /// </summary>
        byte[] RestoreManifest(byte[] fileData);
    }
}