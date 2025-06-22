
namespace YooAsset
{
    /// <summary>
    /// 资源清单文件处理服务接口
    /// </summary>
    public interface IManifestServices
    {
        /// <summary>
        /// 处理资源清单（压缩和加密）
        /// </summary>
        byte[] ProcessManifest(byte[] fileData);
        
        /// <summary>
        /// 还原资源清单（解压和解密）
        /// </summary>
        byte[] RestoreManifest(byte[] fileData);
    }
}