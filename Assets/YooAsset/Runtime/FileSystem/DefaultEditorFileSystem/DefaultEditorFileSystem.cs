using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 内置文件系统
    /// </summary>
    internal class DefaultEditorFileSystem : IFileSystem
    {
        protected string _packageRoot;

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 文件访问权限
        /// </summary>
        public EFileAccess FileSystemAccess
        {
            get
            {
                return EFileAccess.Read;
            }
        }

        /// <summary>
        /// 文件根目录
        /// </summary>
        public string FileRoot
        {
            get
            {
                return _packageRoot;
            }
        }

        /// <summary>
        /// 文件数量
        /// </summary>
        public int FileCount
        {
            get
            {
                return 0;
            }
        }

        #region 自定义参数
        /// <summary>
        /// 自定义参数：模拟构建结果
        /// </summary>
        public SimulateBuildResult BuildResult { private set; get; } = null;
        #endregion


        public DefaultEditorFileSystem()
        {
        }
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DEFSInitializeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(params object[] args)
        {
            var operation = new DEFSLoadPackageManifestOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(params object[] args)
        {
            var operation = new DEFSRequestPackageVersionOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSClearAllBundleFilesOperation ClearAllBundleFilesAsync(params object[] args)
        {
            var operation = new FSClearAllBundleFilesCompleteOperation();
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(params object[] args)
        {
            var operation = new FSClearUnusedBundleFilesCompleteOperation();
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSDownloadFileOperation DownloadFileAsync(params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SetParameter(string name, object value)
        {
            if (name == "SIMULATE_BUILD_RESULT")
            {
                BuildResult = (SimulateBuildResult)value;
            }
            else
            {
                YooLogger.Warning($"Invalid parameter : {name}");
            }
        }
        public virtual void OnCreate(string packageName, string rootDirectory)
        {
            PackageName = packageName;

            if (string.IsNullOrEmpty(rootDirectory))
                rootDirectory = GetDefaultRoot();

            _packageRoot = PathUtility.Combine(rootDirectory, packageName);
        }
        public virtual void OnUpdate()
        {
        }

        public virtual bool Belong(PackageBundle bundle)
        {
            return true;
        }
        public virtual bool Belong(string bundleGUID)
        {
            return true;
        }
        public virtual bool Exists(PackageBundle bundle)
        {
            return true;
        }
        public virtual bool Exists(string bundleGUID)
        {
            return true;
        }

        public virtual bool CheckNeedDownload(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool CheckNeedUnpack(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool CheckNeedImport(PackageBundle bundle)
        {
            return false;
        }

        public virtual bool WriteFile(PackageBundle bundle, string copyPath)
        {
            return true;
        }
        public virtual bool DeleteFile(PackageBundle bundle)
        {
            return true;
        }
        public virtual bool DeleteFile(string bundleGUID)
        {
            return true;
        }
        public virtual EFileVerifyResult VerifyFile(PackageBundle bundle)
        {
            return EFileVerifyResult.Succeed;
        }

        public virtual byte[] ReadFileBytes(PackageBundle bundle)
        {
            throw new System.NotImplementedException();
        }
        public virtual string ReadFileText(PackageBundle bundle)
        {
            throw new System.NotImplementedException();
        }

        public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
        {
            var operation = new DEFSLoadBundleOperation(this, bundle);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual void UnloadBundleFile(PackageBundle bundle, object result)
        {
        }

        #region 内部方法
        protected string GetDefaultRoot()
        {
            return "Assets/";
        }

        /// <summary>
        /// 记录缓存信息
        /// </summary>
        public bool Record(string bundleGUID, object value)
        {
            return true;
        }
        #endregion
    }
}