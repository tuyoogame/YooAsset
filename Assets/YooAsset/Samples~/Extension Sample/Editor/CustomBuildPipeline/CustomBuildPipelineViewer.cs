using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using YooAsset.Editor;

[BuildPipelineAttribute("CustomBuildPipeline")]
internal class CustomBuildPipelineViewer : BuiltinBuildPipelineViewer
{
    protected override string GetDefaultPackageVersion()
    {
        return "v1.0.0";
    }
}