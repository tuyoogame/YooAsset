using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源包加密任务，在管线输出目录对资源包文件执行加密并记录加密路径。
    /// </summary>
    public class TaskEncryption
    {
        /// <summary>
        /// 加密资源包文件
        /// </summary>
        /// <param name="buildParametersContext">构建参数上下文</param>
        /// <param name="buildMapContext">构建映射上下文</param>
        public void EncryptingBundleFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
        {
            var encryptionServices = buildParametersContext.Parameters.BundleEncryptor;
            if (encryptionServices == null)
                return;

            if (encryptionServices is EncryptionNone)
                return;

            int progressValue = 0;
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                string filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                var args = new BundleEncryptArgs(bundleInfo.BundleName, filePath);
                var encryptResult = encryptionServices.Encrypt(args);
                if (encryptResult.IsEncrypted)
                {
                    string encryptedFilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}.encrypt";
                    bool isWritten = WriteEncryptedFile(encryptedFilePath, encryptResult.EncryptedFileData);
                    bundleInfo.EncryptedFilePath = encryptedFilePath;
                    bundleInfo.Encrypted = true;
                    if (isWritten)
                        BuildLogger.Log($"Bundle file encryption complete: '{filePath}'.");
                    else
                        BuildLogger.Log($"Bundle file encryption reused: '{filePath}'.");
                }
                else
                {
                    bundleInfo.Encrypted = false;
                }

                // 进度条
                EditorDialogUtility.DisplayProgressBar("Encrypting bundle", ++progressValue, buildMapContext.Collection.Count);
            }
            EditorDialogUtility.ClearProgressBar();
        }

        /// <summary>
        /// 写入加密后的资源包文件
        /// </summary>
        /// <param name="encryptedFilePath">加密文件路径</param>
        /// <param name="encryptedFileData">加密文件数据</param>
        /// <returns>是否实际写入了文件</returns>
        protected virtual bool WriteEncryptedFile(string encryptedFilePath, byte[] encryptedFileData)
        {
            FileUtility.WriteAllBytes(encryptedFilePath, encryptedFileData);
            return true;
        }
    }
}