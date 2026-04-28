using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using YooAsset.Editor;

/// <summary>
/// 提供自定义构建管线的编辑器视图
/// </summary>
[BuildPipelineAttribute("CustomBuildPipeline")]
internal class CustomBuildPipelineViewer : LegacyBuildPipelineViewer
{
    protected override string GetDefaultPackageVersion()
    {
        return "v1.0.0";
    }
}