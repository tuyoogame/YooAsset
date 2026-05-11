using System;

namespace YooAsset
{
    /// <summary>
    /// 封装原生文件字节数据的资源包对象
    /// </summary>
    internal class RawBundle
    {
        private byte[] _bytes;
        private RawFileObject _cachedObject;
        private bool _isUnloaded;

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="bytes">原生文件的字节数据</param>
        public RawBundle(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            _bytes = bytes;
            _isUnloaded = false;
        }

        /// <summary>
        /// 创建基于当前字节数据的原生文件对象
        /// </summary>
        /// <returns>创建得到的原生文件对象</returns>
        public RawFileObject CreateRawFileObject()
        {
            if (_isUnloaded)
                throw new InvalidOperationException($"{nameof(RawBundle)} has been unloaded.");

            if (_cachedObject == null)
                _cachedObject = RawFileObject.CreateFromBytes(_bytes);
            return _cachedObject;
        }

        /// <summary>
        /// 卸载原生资源包
        /// </summary>
        public void Unload()
        {
            _isUnloaded = true;
            if (_cachedObject != null)
            {
                _cachedObject.Release();
                UnityEngine.Object.Destroy(_cachedObject);
                _cachedObject = null;
            }
            _bytes = null;
        }
    }
}