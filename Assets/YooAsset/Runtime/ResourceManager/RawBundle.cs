
namespace YooAsset
{
    internal class RawBundle
    {
        private readonly IFileSystem _fileSystem;
        private readonly PackageBundle _packageBundle;
        private readonly string _filePath;

        internal RawBundle(IFileSystem fileSystem, PackageBundle packageBundle, string filePath)
        {
            _fileSystem = fileSystem;
            _packageBundle = packageBundle;
            _filePath = filePath;
        }

        public string GetFilePath()
        {
            return _filePath;
        }
        public byte[] ReadFileData()
        {
            if (_fileSystem != null)
                return _fileSystem.ReadFileData(_packageBundle);
            else
                return FileUtility.ReadAllBytes(_filePath);
        }
        public string ReadFileText()
        {
            if (_fileSystem != null)
                return _fileSystem.ReadFileText(_packageBundle);
            else
                return FileUtility.ReadAllText(_filePath);
        }
    }
}