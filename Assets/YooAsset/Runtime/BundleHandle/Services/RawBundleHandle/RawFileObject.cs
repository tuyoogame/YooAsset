using System;
using System.Text;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 原生文件对象
    /// </summary>
    public class RawFileObject : ScriptableObject
    {
        private byte[] _fileBytes;
        private string _fileText;

        /// <summary>
        /// 获取原生文件的字节数据
        /// </summary>
        /// <returns>原生文件字节数据的副本</returns>
        public byte[] GetBytes()
        {
            if (_fileBytes == null)
                return null;
            var copy = new byte[_fileBytes.Length];
            System.Buffer.BlockCopy(_fileBytes, 0, copy, 0, _fileBytes.Length);
            return copy;
        }

        /// <summary>
        /// 获取以 UTF-8 编码解析的文本内容
        /// </summary>
        /// <returns>解析后的文本字符串</returns>
        public string GetText()
        {
            if (_fileBytes == null || _fileBytes.Length == 0)
                return null;

            if (string.IsNullOrEmpty(_fileText))
                _fileText = Encoding.UTF8.GetString(_fileBytes);
            return _fileText;
        }

#if UNITY_2021_2_OR_NEWER
        /// <summary>
        /// 获取原生文件的只读字节数据
        /// </summary>
        /// <remarks>
        /// 返回的数据直接引用内部数组，调用方不应在资源包卸载后继续使用。
        /// </remarks>
        /// <returns>原生文件的只读字节数据</returns>
        public ReadOnlySpan<byte> GetBytesAsReadOnlySpan()
        {
            if (_fileBytes == null)
                return ReadOnlySpan<byte>.Empty;
            return new ReadOnlySpan<byte>(_fileBytes);
        }
#endif

        /// <summary>
        /// 释放内部持有的字节数据和文本缓存
        /// </summary>
        internal void Release()
        {
            _fileBytes = null;
            _fileText = null;
        }

        /// <summary>
        /// 创建基于指定字节数据的原生文件对象实例
        /// </summary>
        /// <param name="bytes">原生文件的字节数据</param>
        /// <returns>创建得到的原生文件对象</returns>
        internal static RawFileObject CreateFromBytes(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            var obj = CreateInstance<RawFileObject>();
            obj._fileBytes = bytes;
            return obj;
        }
    }
}
