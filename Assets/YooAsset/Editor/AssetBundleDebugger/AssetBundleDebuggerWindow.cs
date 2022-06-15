#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;

namespace YooAsset.Editor
{
	public class AssetBundleDebuggerWindow : EditorWindow
	{
		[MenuItem("YooAsset/AssetBundle Debugger", false, 104)]
		public static void ShowExample()
		{
			AssetBundleDebuggerWindow wnd = GetWindow<AssetBundleDebuggerWindow>("资源包调试工具", true, EditorDefine.DockedWindowTypes);
			wnd.minSize = new Vector2(800, 600);
		}

		/// <summary>
		/// 视图模式
		/// </summary>
		private enum EViewMode
		{
			/// <summary>
			/// 内存视图
			/// </summary>
			MemoryView,

			/// <summary>
			/// 资源对象视图
			/// </summary>
			AssetView,

			/// <summary>
			/// 资源包视图
			/// </summary>
			BundleView,
		}


		private readonly Dictionary<int, RemotePlayerSession> _playerSessions = new Dictionary<int, RemotePlayerSession>();

		private ToolbarMenu _viewModeMenu;
		private DebuggerAssetListViewer _assetListViewer;
		private DebuggerBundleListViewer _bundleListViewer;

		private EViewMode _viewMode;
		private DebugReport _debugReport;
		private string _searchKeyWord;


		public void CreateGUI()
		{
			try
			{
				VisualElement root = rootVisualElement;

				// 加载布局文件
				var visualAsset = EditorHelper.LoadWindowUXML<AssetBundleDebuggerWindow>();
				if (visualAsset == null)
					return;

				visualAsset.CloneTree(root);

				// 采样按钮
				var sampleBtn = root.Q<Button>("SampleButton");
				sampleBtn.clicked += SampleBtn_onClick;

				// 视口模式菜单
				_viewModeMenu = root.Q<ToolbarMenu>("ViewModeMenu");
				//_viewModeMenu.menu.AppendAction(EViewMode.MemoryView.ToString(), ViewModeMenuAction0, ViewModeMenuFun0);
				_viewModeMenu.menu.AppendAction(EViewMode.AssetView.ToString(), ViewModeMenuAction1, ViewModeMenuFun1);
				_viewModeMenu.menu.AppendAction(EViewMode.BundleView.ToString(), ViewModeMenuAction2, ViewModeMenuFun2);

				// 搜索栏
				var searchField = root.Q<ToolbarSearchField>("SearchField");
				searchField.RegisterValueChangedCallback(OnSearchKeyWordChange);

				// 加载视图
				_assetListViewer = new DebuggerAssetListViewer();
				_assetListViewer.InitViewer();

				// 加载视图
				_bundleListViewer = new DebuggerBundleListViewer();
				_bundleListViewer.InitViewer();

				// 显示视图
				_viewMode = EViewMode.AssetView;
				_viewModeMenu.text = EViewMode.AssetView.ToString();
				_assetListViewer.AttachParent(root);
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		public void OnDestroy()
		{
		}

		private void OnEnable()
		{
			EditorConnection.instance.Initialize();
			EditorConnection.instance.RegisterConnection(OnHandleConnectionEvent);
			EditorConnection.instance.RegisterDisconnection(OnHandleDisconnectionEvent);
			EditorConnection.instance.Register(RemoteDebuggerDefine.kMsgSendPlayerToEditor, OnHandlePlayerMessage);
			RemoteDebuggerInRuntime.EditorHandleDebugReportCallback = OnHandleDebugReport;
		}
		private void OnDisable()
		{
			EditorConnection.instance.Unregister(RemoteDebuggerDefine.kMsgSendPlayerToEditor, OnHandlePlayerMessage);
			_playerSessions.Clear();
		}
		private void OnHandleConnectionEvent(int playerId)
		{
			Debug.Log($"Game player connection : {playerId}");
		}
		private void OnHandleDisconnectionEvent(int playerId)
		{
			Debug.Log($"Game player disconnection : {playerId}");
			RemovePlayerSession(playerId);
		}
		private void OnHandlePlayerMessage(MessageEventArgs args)
		{
			var debugReport = DebugReport.Deserialize(args.data);
			OnHandleDebugReport(args.playerId, debugReport);
		}
		private void OnHandleDebugReport(int playerId, DebugReport debugReport)
		{
			var playerSession = GetOrCreatePlayerSession(playerId);
			playerSession.AddDebugReport(debugReport);

			_debugReport = debugReport;
			_assetListViewer.FillViewData(debugReport, _searchKeyWord);
			_bundleListViewer.FillViewData(debugReport, _searchKeyWord);
		}
		private RemotePlayerSession GetOrCreatePlayerSession(int playerId)
		{
			if (_playerSessions.TryGetValue(playerId, out RemotePlayerSession session))
			{
				return session;
			}
			else
			{
				RemotePlayerSession newSession = new RemotePlayerSession(playerId);
				_playerSessions.Add(playerId, newSession);
				return newSession;
			}
		}
		private void RemovePlayerSession(int playerId)
		{
			if (_playerSessions.ContainsKey(playerId))
				_playerSessions.Remove(playerId);
		}

		private void SampleBtn_onClick()
		{
			// 发送采集数据的命令
			RemoteCommand command = new RemoteCommand();
			command.CommandType = (int)ERemoteCommand.SampleOnce;
			command.CommandParam = string.Empty;
			byte[] data = RemoteCommand.Serialize(command);
			EditorConnection.instance.Send(RemoteDebuggerDefine.kMsgSendEditorToPlayer, data);
			RemoteDebuggerInRuntime.EditorRequestDebugReport();
		}
		private void OnSearchKeyWordChange(ChangeEvent<string> e)
		{
			_searchKeyWord = e.newValue;
			if (_debugReport != null)
			{
				_assetListViewer.FillViewData(_debugReport, _searchKeyWord);
				_bundleListViewer.FillViewData(_debugReport, _searchKeyWord);
			}
		}
		private void ViewModeMenuAction1(DropdownMenuAction action)
		{
			if (_viewMode != EViewMode.AssetView)
			{
				_viewMode = EViewMode.AssetView;
				VisualElement root = this.rootVisualElement;
				_viewModeMenu.text = EViewMode.AssetView.ToString();
				_assetListViewer.AttachParent(root);
				_bundleListViewer.DetachParent();
			}
		}
		private void ViewModeMenuAction2(DropdownMenuAction action)
		{
			if (_viewMode != EViewMode.BundleView)
			{
				_viewMode = EViewMode.BundleView;
				VisualElement root = this.rootVisualElement;
				_viewModeMenu.text = EViewMode.BundleView.ToString();
				_assetListViewer.DetachParent();
				_bundleListViewer.AttachParent(root);
			}
		}
		private DropdownMenuAction.Status ViewModeMenuFun1(DropdownMenuAction action)
		{
			if (_viewMode == EViewMode.AssetView)
				return DropdownMenuAction.Status.Checked;
			else
				return DropdownMenuAction.Status.Normal;
		}
		private DropdownMenuAction.Status ViewModeMenuFun2(DropdownMenuAction action)
		{
			if (_viewMode == EViewMode.BundleView)
				return DropdownMenuAction.Status.Checked;
			else
				return DropdownMenuAction.Status.Normal;
		}
	}
}
#endif