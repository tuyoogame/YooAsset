
namespace YooAsset.Editor
{
    public class ManifestNone : IManifestServices
    {
        public byte[] ProcessManifest(byte[] fileData)
        {
            return fileData;
        }
        public byte[] RestoreManifest(byte[] fileData)
        {
            return fileData;
        }
    }
}