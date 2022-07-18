using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatchWindow : MonoBehaviour
{
	/// <summary>
	/// 对话框封装类
	/// </summary>
	private class MessageBox
	{
		private GameObject _cloneObject;
		private Text _content;
		private Button _btnOK;
		private System.Action _clickOK;

		public bool ActiveSelf
		{
			get
			{
				return _cloneObject.activeSelf;
			}
		}

		public void Create(GameObject cloneObject)
		{
			_cloneObject = cloneObject;
			_content = cloneObject.transform.Find("txt_content").GetComponent<Text>();
			_btnOK = cloneObject.transform.Find("btn_ok").GetComponent<Button>();
			_btnOK.onClick.AddListener(OnClickYes);
		}
		public void Show(string content, System.Action clickOK)
		{
			_content.text = content;
			_clickOK = clickOK;
			_cloneObject.SetActive(true);
			_cloneObject.transform.SetAsLastSibling();
		}
		public void Hide()
		{
			_content.text = string.Empty;
			_clickOK = null;
			_cloneObject.SetActive(false);
		}
		private void OnClickYes()
		{
			_clickOK?.Invoke();
			Hide();
		}
	}


	private readonly EventGroup _eventGroup = new EventGroup();
	private readonly List<MessageBox> _msgBoxList = new List<MessageBox>();

	// UGUI相关
	private GameObject _messageBoxObj;
	private Slider _slider;
	private Text _tips;


	void Awake()
	{
		_slider = transform.Find("UIWindow/Slider").GetComponent<Slider>();
		_tips = transform.Find("UIWindow/Slider/txt_tips").GetComponent<Text>();
		_tips.text = "Initializing the game world !";
		_messageBoxObj = transform.Find("UIWindow/MessgeBox").gameObject;
		_messageBoxObj.SetActive(false);

		_eventGroup.AddListener<PatchEventMessageDefine.PatchStatesChange>(OnHandleEvent);
		_eventGroup.AddListener<PatchEventMessageDefine.FoundUpdateFiles>(OnHandleEvent);
		_eventGroup.AddListener<PatchEventMessageDefine.DownloadProgressUpdate>(OnHandleEvent);
		_eventGroup.AddListener<PatchEventMessageDefine.StaticVersionUpdateFailed>(OnHandleEvent);
		_eventGroup.AddListener<PatchEventMessageDefine.PatchManifestUpdateFailed>(OnHandleEvent);
		_eventGroup.AddListener<PatchEventMessageDefine.WebFileDownloadFailed>(OnHandleEvent);
	}
	void OnDestroy()
	{
		_eventGroup.RemoveAllListener();
	}

	/// <summary>
	/// 接收事件
	/// </summary>
	private void OnHandleEvent(IEventMessage msg)
	{
		if (msg is PatchEventMessageDefine.PatchStatesChange)
		{
			var message = msg as PatchEventMessageDefine.PatchStatesChange;
			if (message.CurrentStates == EPatchStates.UpdateStaticVersion)
				_tips.text = "Update static version.";
			else if (message.CurrentStates == EPatchStates.UpdateManifest)
				_tips.text = "Update patch manifest.";
			else if (message.CurrentStates == EPatchStates.CreateDownloader)
				_tips.text = "Check download contents.";
			else if (message.CurrentStates == EPatchStates.DownloadWebFiles)
				_tips.text = "Downloading patch files.";
			else if (message.CurrentStates == EPatchStates.PatchDone)
				_tips.text = "Welcome to game world !";
			else
				throw new NotImplementedException(message.CurrentStates.ToString());
		}
		else if (msg is PatchEventMessageDefine.FoundUpdateFiles)
		{
			var message = msg as PatchEventMessageDefine.FoundUpdateFiles;
			System.Action callback = () =>
			{
				PatchUpdater.HandleOperation(EPatchOperation.BeginDownloadWebFiles);
			};
			float sizeMB = message.TotalSizeBytes / 1048576f;
			sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
			string totalSizeMB = sizeMB.ToString("f1");
			ShowMessageBox($"Found update patch files, Total count {message.TotalCount} Total szie {totalSizeMB}MB", callback);
		}
		else if (msg is PatchEventMessageDefine.DownloadProgressUpdate)
		{
			var message = msg as PatchEventMessageDefine.DownloadProgressUpdate;
			_slider.value = (float)message.CurrentDownloadCount / message.TotalDownloadCount;
			string currentSizeMB = (message.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
			string totalSizeMB = (message.TotalDownloadSizeBytes / 1048576f).ToString("f1");
			_tips.text = $"{message.CurrentDownloadCount}/{message.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
		}
		else if (msg is PatchEventMessageDefine.StaticVersionUpdateFailed)
		{
			System.Action callback = () =>
			{
				PatchUpdater.HandleOperation(EPatchOperation.TryUpdateStaticVersion);
			};
			ShowMessageBox($"Failed to update static version, please check the network status.", callback);
		}
		else if (msg is PatchEventMessageDefine.PatchManifestUpdateFailed)
		{
			System.Action callback = () =>
			{
				PatchUpdater.HandleOperation(EPatchOperation.TryUpdatePatchManifest);
			};
			ShowMessageBox($"Failed to update patch manifest, please check the network status.", callback);
		}
		else if (msg is PatchEventMessageDefine.WebFileDownloadFailed)
		{
			var message = msg as PatchEventMessageDefine.WebFileDownloadFailed;
			System.Action callback = () =>
			{
				PatchUpdater.HandleOperation(EPatchOperation.TryDownloadWebFiles);
			};
			ShowMessageBox($"Failed to download file : {message.FileName}", callback);
		}
		else
		{
			throw new System.NotImplementedException($"{msg.GetType()}");
		}
	}

	/// <summary>
	/// 显示对话框
	/// </summary>
	private void ShowMessageBox(string content, System.Action ok)
	{
		// 尝试获取一个可用的对话框
		MessageBox msgBox = null;
		for (int i = 0; i < _msgBoxList.Count; i++)
		{
			var item = _msgBoxList[i];
			if (item.ActiveSelf == false)
			{
				msgBox = item;
				break;
			}
		}

		// 如果没有可用的对话框，则创建一个新的对话框
		if (msgBox == null)
		{
			msgBox = new MessageBox();
			var cloneObject = GameObject.Instantiate(_messageBoxObj, _messageBoxObj.transform.parent);
			msgBox.Create(cloneObject);
			_msgBoxList.Add(msgBox);
		}

		// 显示对话框
		msgBox.Show(content, ok);
	}
}