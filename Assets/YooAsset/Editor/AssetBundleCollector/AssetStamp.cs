using System.Runtime.InteropServices;

namespace YooAsset.Editor
{
    [StructLayout(LayoutKind.Auto)]
    internal struct AssetStamp
    {
        private readonly string m_AssetName;
        private readonly string m_DependAssetPath;

        public AssetStamp(string assetName, string dependencyAssetName)
        {
            m_AssetName = assetName;
            m_DependAssetPath = dependencyAssetName;
        }

        public string AssetName
        {
            get
            {
                return m_AssetName;
            }
        }

        public string DependAssetPath
        {
            get
            {
                return m_DependAssetPath;
            }
        }
    }
}
