
namespace YooAsset
{
    /// <summary>
    /// 资源清单文件处理服务接口
    /// </summary>
    public interface IManifestProcessServices
    {
        /// <summary>
        /// 处理资源清单（压缩或加密）
        /// </summary>
        byte[] ProcessManifest(byte[] fileData);
    }
}