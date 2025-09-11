using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using NUnit.Framework.Constraints;

namespace YooAsset.Editor
{
    public class AssetArtScannerSetting : ScriptableObject
    {
        /// <summary>
        /// 扫描器列表
        /// </summary>
        public List<AssetArtScanner> Scanners = new List<AssetArtScanner>();

        /// <summary>
        /// 开始扫描
        /// </summary>
        public ScannerResult BeginScan(string scannerGUID)
        {
            try
            {
                // 获取扫描器配置
                var scanner = GetScanner(scannerGUID);
                if (scanner == null)
                    throw new Exception($"Invalid scanner GUID : {scannerGUID}");

                // 检测配置合法性
                scanner.CheckConfigError();

                // 开始扫描工作
                ScanReport report = scanner.RunScanner();

                // 检测报告合法性
                report.CheckError();

                // 保存扫描结果
                string saveDirectory = scanner.SaveDirectory;
                if (string.IsNullOrEmpty(saveDirectory))
                    saveDirectory = "Assets/";
                string filePath = $"{saveDirectory}/{scanner.ScannerName}_{scanner.ScannerDesc}.json";
                ScanReportConfig.ExportJsonConfig(filePath, report);
                return new ScannerResult(filePath, report);
            }
            catch (Exception e)
            {
                return new ScannerResult(e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 获取指定的扫描器
        /// </summary>
        public AssetArtScanner GetScanner(string scannerGUID)
        {
            foreach (var scanner in Scanners)
            {
                if (scanner.ScannerGUID == scannerGUID)
                    return scanner;
            }

            Debug.LogWarning($"Not found scanner : {scannerGUID}");
            return null;
        }
    }
}