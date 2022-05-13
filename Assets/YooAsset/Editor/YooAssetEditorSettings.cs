using UnityEngine;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
	public class YooAssetEditorSettings : ScriptableObject
	{
		// 资源包收集
		public VisualTreeAsset AssetBundleCollectorUXML;

		// 资源包构建
		public VisualTreeAsset AssetBundleBuilderUXML;

		// 资源包调试
		public VisualTreeAsset AssetBundleDebuggerUXML;
		public VisualTreeAsset DebuggerAssetListViewerUXML;
		public VisualTreeAsset DebuggerBundleListViewerUXML;

		// 构建报告
		public VisualTreeAsset AssetBundleReporterUXML;
		public VisualTreeAsset ReporterSummaryViewerUXML;
		public VisualTreeAsset ReporterAssetListViewerUXML;
		public VisualTreeAsset ReporterBundleListViewerUXML;
	}
}