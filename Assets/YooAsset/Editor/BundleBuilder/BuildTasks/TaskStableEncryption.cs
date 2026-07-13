using System;
using System.IO;

namespace YooAsset.Editor
{
    /// <summary>
    /// 稳定加密任务
    /// </summary>
    /// <remarks>
    /// 1. 每次构建仍执行加密，以确保加密器配置变化时能够生成新密文。
    /// 2. 当生成的密文与已有文件完全一致时，不覆盖文件，从而保留文件修改时间。
    /// </remarks>
    public class TaskStableEncryption : TaskEncryption, IBuildTask
    {
        private const int CompareBufferSize = 81920;
        private byte[] _compareBuffer;

        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParameters = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            EncryptingBundleFiles(buildParameters, buildMapContext);
        }

        /// <inheritdoc/>
        protected override bool WriteEncryptedFile(string encryptedFilePath, byte[] encryptedFileData)
        {
            if (IsFileContentEqual(encryptedFilePath, encryptedFileData))
                return false;

            FileUtility.WriteAllBytes(encryptedFilePath, encryptedFileData);
            return true;
        }

        /// <summary>
        /// 检查指定文件是否与给定数据完全一致
        /// </summary>
        private bool IsFileContentEqual(string filePath, byte[] content)
        {
            if (File.Exists(filePath) == false)
                return false;

            int contentOffset = 0;
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length != content.LongLength)
                    return false;

                if (_compareBuffer == null)
                    _compareBuffer = new byte[CompareBufferSize];

                while (contentOffset < content.Length)
                {
                    int readCount = stream.Read(_compareBuffer, 0, Math.Min(CompareBufferSize, content.Length - contentOffset));
                    if (readCount == 0)
                        throw new EndOfStreamException($"Unexpected end of encrypted file: '{filePath}'.");

                    for (int i = 0; i < readCount; i++)
                    {
                        if (_compareBuffer[i] != content[contentOffset + i])
                            return false;
                    }

                    contentOffset += readCount;
                }
            }

            return true;
        }
    }
}
