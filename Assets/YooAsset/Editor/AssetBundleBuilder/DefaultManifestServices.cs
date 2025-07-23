
namespace YooAsset.Editor
{
    public class ManifestProcessNone : IManifestProcessServices
    {
        byte[] IManifestProcessServices.ProcessManifest(byte[] fileData)
        {
            return fileData;
        }
    }
    
    public class ManifestRestoreNone : IManifestRestoreServices
    {
        byte[] IManifestRestoreServices.RestoreManifest(byte[] fileData)
        {
            return fileData;
        }
    }
}