using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;

namespace YooAsset.Editor
{
    /// <summary>
    /// AssetBundle 调试器窗口，用于实时查看运行时资源加载与资源包状态
    /// </summary>
    public class BundleDebuggerWindow : EditorWindow
    {
        [MenuItem("YooAsset/Bundle Debugger", false, 104)]
        public static void OpenWindow()
        {
            Type[] dockedTypes = EditorWindowDefine.GetDockedWindowTypes();
            BundleDebuggerWindow wnd = GetWindow<BundleDebuggerWindow>("Bundle Debugger", true, dockedTypes);
            wnd.minSize = new Vector2(800, 600);
        }

        /// <summary>
        /// 视图模式
        /// </summary>
        private enum EViewMode
        {
            /// <summary>
            /// 资源对象视图
            /// </summary>
            AssetView,

            /// <summary>
            /// 资源包视图
            /// </summary>
            BundleView,

            /// <summary>
            /// 异步操作视图
            /// </summary>
            OperationView,
        }


        private readonly Dictionary<int, RemotePlayerSession> _playerSessions = new Dictionary<int, RemotePlayerSession>();

        private ToolbarButton _playerName;
        private ToolbarMenu _viewModeMenu;
        private SliderInt _frameSlider;
        private DebuggerAssetListViewer _assetListViewer;
        private DebuggerBundleListViewer _bundleListViewer;
        private DebuggerOperationListViewer _operationListViewer;

        private EViewMode _viewMode;
        private string _searchKeyword;
        private DiagnosticReport _currentReport;
        private RemotePlayerSession _currentPlayerSession;

        private double _lastRepaintTime = 0;
        private int _nextRepaintIndex = -1;
        private int _lastRepaintIndex = 0;
        private int _rangeIndex = 0;


        /// <summary>
        /// 创建窗口界面并初始化所有子视图与远程调试连接
        /// </summary>
        public void CreateGUI()
        {
            try
            {
                VisualElement root = rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUxml<BundleDebuggerWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 采样按钮
                var sampleBtn = root.Q<Button>("SampleButton");
                sampleBtn.clicked += OnSampleButtonClick;

                // 导出按钮
                var exportBtn = root.Q<Button>("ExportButton");
                exportBtn.clicked += OnExportButtonClick;

                // 用户列表菜单
                _playerName = root.Q<ToolbarButton>("PlayerName");
                _playerName.text = "Editor player";

                // 视口模式菜单
                _viewModeMenu = root.Q<ToolbarMenu>("ViewModeMenu");
                _viewModeMenu.menu.AppendAction(EViewMode.AssetView.ToString(), OnViewModeMenuChange, OnViewModeMenuStatusUpdate, EViewMode.AssetView);
                _viewModeMenu.menu.AppendAction(EViewMode.BundleView.ToString(), OnViewModeMenuChange, OnViewModeMenuStatusUpdate, EViewMode.BundleView);
                _viewModeMenu.menu.AppendAction(EViewMode.OperationView.ToString(), OnViewModeMenuChange, OnViewModeMenuStatusUpdate, EViewMode.OperationView);
                _viewModeMenu.text = EViewMode.AssetView.ToString();

                // 搜索栏
                var searchField = root.Q<ToolbarSearchField>("SearchField");
                searchField.RegisterValueChangedCallback(OnSearchKeywordChange);

                // 帧数相关
                {
                    _frameSlider = root.Q<SliderInt>("FrameSlider");
                    _frameSlider.label = "Frame:";
                    _frameSlider.highValue = 0;
                    _frameSlider.lowValue = 0;
                    _frameSlider.value = 0;
                    _frameSlider.RegisterValueChangedCallback(evt =>
                    {
                        OnFrameSliderChange(evt.newValue);
                    });

                    var frameLast = root.Q<ToolbarButton>("FrameLast");
                    frameLast.clicked += OnFrameLastClick;

                    var frameNext = root.Q<ToolbarButton>("FrameNext");
                    frameNext.clicked += OnFrameNextClick;

                    var frameClear = root.Q<ToolbarButton>("FrameClear");
                    frameClear.clicked += OnFrameClearClick;

                    var recorderToggle = root.Q<ToggleRecord>("FrameRecord");
                    recorderToggle.RegisterValueChangedCallback(OnRecordToggleValueChange);
                }

                // 加载视图
                _assetListViewer = new DebuggerAssetListViewer();
                _assetListViewer.InitViewer();

                // 加载视图
                _bundleListViewer = new DebuggerBundleListViewer();
                _bundleListViewer.InitViewer();

                // 加载视图
                _operationListViewer = new DebuggerOperationListViewer();
                _operationListViewer.InitViewer();

                // 显示视图
                _viewMode = EViewMode.AssetView;
                _assetListViewer.AttachParent(root);

                // 远程调试
                EditorConnection.instance.Initialize();
                EditorConnection.instance.RegisterConnection(OnHandleConnectionEvent);
                EditorConnection.instance.RegisterDisconnection(OnHandleDisconnectionEvent);
                EditorConnection.instance.Register(DiagnosticSystemConsts.PlayerToEditorMessageId, OnHandlePlayerMessage);
                MockEditorConnection.Instance.Initialize();
                MockEditorConnection.Instance.Register(DiagnosticSystemConsts.PlayerToEditorMessageId, OnHandlePlayerMessage);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 释放远程调试连接并清理会话数据
        /// </summary>
        public void OnDestroy()
        {
            EditorConnection.instance.UnregisterConnection(OnHandleConnectionEvent);
            EditorConnection.instance.UnregisterDisconnection(OnHandleDisconnectionEvent);
            EditorConnection.instance.Unregister(DiagnosticSystemConsts.PlayerToEditorMessageId, OnHandlePlayerMessage);
            MockEditorConnection.Instance.Unregister(DiagnosticSystemConsts.PlayerToEditorMessageId, OnHandlePlayerMessage);
            _playerSessions.Clear();
        }

        /// <summary>
        /// 定时刷新调试视图，每秒重绘一次最新帧数据
        /// </summary>
        public void Update()
        {
            if (EditorApplication.timeSinceStartup - _lastRepaintTime > 1f)
            {
                _lastRepaintTime = EditorApplication.timeSinceStartup;
                if (_nextRepaintIndex >= 0)
                {
                    RepaintFrame(_nextRepaintIndex);
                    _nextRepaintIndex = -1;
                }
            }
        }

        private void OnHandleConnectionEvent(int playerID)
        {
            Debug.Log($"Game player connected: {playerID}.");
            _playerName.text = $"Connected player : {playerID}";
        }
        private void OnHandleDisconnectionEvent(int playerID)
        {
            Debug.Log($"Game player disconnected: {playerID}.");
            _playerName.text = $"Disconnected player : {playerID}";
        }
        private void OnHandlePlayerMessage(MessageEventArgs args)
        {
            int playerID = args.playerId;

            DiagnosticReport debugReport;
            try
            {
                debugReport = DiagnosticReport.Deserialize(args.data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to deserialize diagnostic report: {e.Message}.");
                return;
            }

            if (debugReport.ProtocolVersion != DiagnosticSystemConsts.ProtocolVersion)
            {
                Debug.LogWarning($"Debugger versions are inconsistent: {debugReport.ProtocolVersion} != {DiagnosticSystemConsts.ProtocolVersion}.");
                return;
            }

            _currentPlayerSession = GetOrCreatePlayerSession(playerID);
            _currentPlayerSession.AddDebugReport(debugReport);
            _nextRepaintIndex = _currentPlayerSession.MaxRangeValue;
        }

        private void OnFrameSliderChange(int sliderValue)
        {
            if (_currentPlayerSession != null)
            {
                _rangeIndex = _currentPlayerSession.ClampRangeIndex(sliderValue);
                RepaintFrame(_rangeIndex);
            }
        }
        private void OnFrameLastClick()
        {
            if (_currentPlayerSession != null)
            {
                _rangeIndex = _currentPlayerSession.ClampRangeIndex(_rangeIndex - 1);
                _frameSlider.value = _rangeIndex;
                RepaintFrame(_rangeIndex);
            }
        }
        private void OnFrameNextClick()
        {
            if (_currentPlayerSession != null)
            {
                _rangeIndex = _currentPlayerSession.ClampRangeIndex(_rangeIndex + 1);
                _frameSlider.value = _rangeIndex;
                RepaintFrame(_rangeIndex);
            }
        }
        private void OnFrameClearClick()
        {
            _nextRepaintIndex = -1;
            _lastRepaintIndex = 0;
            _rangeIndex = 0;

            _frameSlider.label = $"Frame:";
            _frameSlider.value = 0;
            _frameSlider.lowValue = 0;
            _frameSlider.highValue = 0;
            _assetListViewer.ClearView();
            _bundleListViewer.ClearView();
            _operationListViewer.ClearView();

            if (_currentPlayerSession != null)
            {
                _currentPlayerSession.ClearDebugReport();
            }
        }
        private void OnRecordToggleValueChange(ChangeEvent<bool> evt)
        {
            DiagnosticCommand command = new DiagnosticCommand();
            command.CommandType = EDiagnosticCommandType.AutoSampling;
            command.Parameter = evt.newValue ? "open" : "close";
            byte[] data = DiagnosticCommand.Serialize(command);
            EditorConnection.instance.Send(DiagnosticSystemConsts.EditorToPlayerMessageId, data);
            MockEditorConnection.Instance.Send(DiagnosticSystemConsts.EditorToPlayerMessageId, data);
        }

        private void OnSampleButtonClick()
        {
            DiagnosticCommand command = new DiagnosticCommand();
            command.CommandType = EDiagnosticCommandType.SampleOnce;
            command.Parameter = string.Empty;
            byte[] data = DiagnosticCommand.Serialize(command);
            EditorConnection.instance.Send(DiagnosticSystemConsts.EditorToPlayerMessageId, data);
            MockEditorConnection.Instance.Send(DiagnosticSystemConsts.EditorToPlayerMessageId, data);
        }
        private void OnExportButtonClick()
        {
            if (_currentReport == null)
            {
                Debug.LogWarning("Debug report is null.");
                return;
            }

            string resultPath = EditorDialogUtility.OpenFolderPanel("Export JSON", "Assets/");
            if (resultPath != null)
            {
                // 注意：排序保证生成配置的稳定性
                foreach (var packageData in _currentReport.PackageDataList)
                {
                    packageData.ProviderInfos.Sort();
                    foreach (var providerInfo in packageData.ProviderInfos)
                    {
                        providerInfo.Dependencies.Sort();
                    }

                    packageData.BundleInfos.Sort();
                    foreach (var bundleInfo in packageData.BundleInfos)
                    {
                        bundleInfo.Referencers.Sort();
                    }

                    packageData.OperationInfos.Sort();
                }

                string filePath = $"{resultPath}/{nameof(DiagnosticReport)}_{_currentReport.FrameCount}.json";
                string exportJson = JsonUtility.ToJson(_currentReport, true);
                FileUtility.WriteAllText(filePath, exportJson);
                Debug.Log($"Debug report saved: '{filePath}'.");
            }
        }
        private void OnSearchKeywordChange(ChangeEvent<string> e)
        {
            _searchKeyword = e.newValue;
            if (_currentReport != null)
            {
                _assetListViewer.RebuildView(_searchKeyword);
                _bundleListViewer.RebuildView(_searchKeyword);
                _operationListViewer.RebuildView(_searchKeyword);
            }
        }
        private void OnViewModeMenuChange(DropdownMenuAction action)
        {
            var viewMode = (EViewMode)action.userData;
            if (_viewMode != viewMode)
            {
                _viewMode = viewMode;
                VisualElement root = this.rootVisualElement;
                _viewModeMenu.text = viewMode.ToString();

                if (viewMode == EViewMode.AssetView)
                {
                    _assetListViewer.AttachParent(root);
                    _bundleListViewer.DetachParent();
                    _operationListViewer.DetachParent();
                }
                else if (viewMode == EViewMode.BundleView)
                {
                    _assetListViewer.DetachParent();
                    _bundleListViewer.AttachParent(root);
                    _operationListViewer.DetachParent();
                }
                else if (viewMode == EViewMode.OperationView)
                {
                    _assetListViewer.DetachParent();
                    _bundleListViewer.DetachParent();
                    _operationListViewer.AttachParent(root);
                }
                else
                {
                    throw new YooInternalException(viewMode.ToString());
                }

                // 重新绘制该帧数据
                RepaintFrame(_lastRepaintIndex);
            }
        }
        private DropdownMenuAction.Status OnViewModeMenuStatusUpdate(DropdownMenuAction action)
        {
            var viewMode = (EViewMode)action.userData;
            if (_viewMode == viewMode)
                return DropdownMenuAction.Status.Checked;
            else
                return DropdownMenuAction.Status.Normal;
        }

        private RemotePlayerSession GetOrCreatePlayerSession(int playerID)
        {
            if (_playerSessions.TryGetValue(playerID, out RemotePlayerSession session))
            {
                return session;
            }
            else
            {
                RemotePlayerSession newSession = new RemotePlayerSession(playerID);
                _playerSessions.Add(playerID, newSession);
                return newSession;
            }
        }
        private void RepaintFrame(int repaintIndex)
        {
            if (_currentPlayerSession == null)
            {
                _assetListViewer.ClearView();
                _bundleListViewer.ClearView();
                _operationListViewer.ClearView();
                return;
            }

            var debugReport = _currentPlayerSession.GetDebugReport(repaintIndex);
            if (debugReport != null)
            {
                _lastRepaintIndex = repaintIndex;
                _currentReport = debugReport;
                _frameSlider.label = $"Frame: {debugReport.FrameCount}";
                _frameSlider.highValue = Math.Max(0, _currentPlayerSession.MaxRangeValue);
                _frameSlider.value = repaintIndex;

                if (_viewMode == EViewMode.AssetView)
                {
                    _assetListViewer.FillViewData(debugReport);
                    _bundleListViewer.ClearView();
                    _operationListViewer.ClearView();
                }
                else if (_viewMode == EViewMode.BundleView)
                {
                    _assetListViewer.ClearView();
                    _bundleListViewer.FillViewData(debugReport);
                    _operationListViewer.ClearView();
                }
                else if (_viewMode == EViewMode.OperationView)
                {
                    _assetListViewer.ClearView();
                    _bundleListViewer.ClearView();
                    _operationListViewer.FillViewData(debugReport);
                }
                else
                {
                    throw new YooInternalException(_viewMode.ToString());
                }
            }
        }
    }
}