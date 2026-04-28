using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

/// <summary>
/// 提供着色器变种收集工具窗口
/// </summary>
public class ShaderVariantCollectorWindow : EditorWindow
{
    [MenuItem("Tools/Shader Variant Collector", false, 100)]
    public static void OpenWindow()
    {
        ShaderVariantCollectorWindow window = GetWindow<ShaderVariantCollectorWindow>("Shader Variant Collector", true);
        window.minSize = new Vector2(800, 600);
    }

    private List<string> _packageNames;
    private int _packageIndex;
    private string _currentPackageName;
    private string _collectOutputPath;
    private int _processCapacity;

    private void OnEnable()
    {
        _packageNames = GetBuildPackageNames();
        if (_packageNames.Count > 0)
        {
            _packageIndex = 0;
            _currentPackageName = _packageNames[_packageIndex];
            RefreshPackageSettings();
        }
    }
    private void OnGUI()
    {
        EditorGUILayout.Space(4);

        bool hasPackages = _packageNames.Count > 0;

        // Package
        EditorGUI.BeginDisabledGroup(!hasPackages);
        {
            int newIndex = EditorGUILayout.Popup("Package", _packageIndex, _packageNames.ToArray());
            if (newIndex != _packageIndex && hasPackages)
            {
                _packageIndex = newIndex;
                _currentPackageName = _packageNames[_packageIndex];
                RefreshPackageSettings();
            }
        }
        EditorGUI.EndDisabledGroup();

        // Save path
        string newPath = EditorGUILayout.TextField("Save Path", _collectOutputPath);
        if (newPath != _collectOutputPath)
        {
            _collectOutputPath = newPath;
            if (!string.IsNullOrEmpty(_currentPackageName))
                ShaderVariantCollectorSetting.SetFileSavePath(_currentPackageName, _collectOutputPath);
        }

        // Shader / variant counts
        int shaderCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount();
        int variantCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount();
        EditorGUILayout.LabelField("Current Shader Count", shaderCount.ToString());
        EditorGUILayout.LabelField("Current Variant Count", variantCount.ToString());

        // Process capacity slider
        int newCapacity = EditorGUILayout.IntSlider("Capacity", _processCapacity, 10, 1000);
        if (newCapacity != _processCapacity)
        {
            _processCapacity = newCapacity;
            if (!string.IsNullOrEmpty(_currentPackageName))
                ShaderVariantCollectorSetting.SetProcessCapacity(_currentPackageName, _processCapacity);
        }

        GUILayout.FlexibleSpace();

        // Collect button
        EditorGUI.BeginDisabledGroup(!hasPackages);
        {
            Color defaultColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.16f, 0.42f, 0.16f, 1f);
            if (GUILayout.Button("Collect", GUILayout.Height(50)))
            {
                CollectButtonClicked();
            }
            GUI.backgroundColor = defaultColor;
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(4);
    }

    private void CollectButtonClicked()
    {
        if (string.IsNullOrEmpty(_currentPackageName))
        {
            Debug.LogError("Package name is empty.");
            return;
        }

        string savePath = ShaderVariantCollectorSetting.GetFileSavePath(_currentPackageName);
        ShaderVariantCollector.Run(savePath, _currentPackageName, _processCapacity, null);
    }
    private void RefreshPackageSettings()
    {
        if (string.IsNullOrEmpty(_currentPackageName))
            return;

        _collectOutputPath = ShaderVariantCollectorSetting.GetFileSavePath(_currentPackageName);
        _processCapacity = ShaderVariantCollectorSetting.GetProcessCapacity(_currentPackageName);
    }
    private List<string> GetBuildPackageNames()
    {
        List<string> result = new List<string>();
        foreach (var package in BundleCollectorSettingData.Setting.Packages)
        {
            result.Add(package.PackageName);
        }
        return result;
    }
}
