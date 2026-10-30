using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Framework.Event;

public class PatchWindow : MonoBehaviour
{
    /// <summary>
    /// Message box wrapper.
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

    private EventSubscriptionScope _eventScope;
    private readonly List<MessageBox> _msgBoxList = new List<MessageBox>();

    // UGUI references.
    private GameObject _messageBoxObj;
    private Slider _slider;
    private Text _tips;

    void Awake()
    {
        _slider = transform.Find("UIWindow/Slider").GetComponent<Slider>();
        _tips = transform.Find("UIWindow/Slider/txt_tips").GetComponent<Text>();
        _tips.text = "Initializing the game world.";
        _messageBoxObj = transform.Find("UIWindow/MessgeBox").gameObject;
        _messageBoxObj.SetActive(false);

        _eventScope = EventManager.Instance.CreateScope();
        _eventScope.AddListener<PatchInitializeFailedEvent>(OnHandleEventMessage);
        _eventScope.AddListener<PatchStepChangedEvent>(OnHandleEventMessage);
        _eventScope.AddListener<PatchFoundUpdateFilesEvent>(OnHandleEventMessage);
        _eventScope.AddListener<PatchDownloadUpdatedEvent>(OnHandleEventMessage);
        _eventScope.AddListener<PatchPackageVersionRequestFailedEvent>(OnHandleEventMessage);
        _eventScope.AddListener<PatchPackageManifestUpdateFailedEvent>(OnHandleEventMessage);
        _eventScope.AddListener<PatchWebFileDownloadFailedEvent>(OnHandleEventMessage);
    }
    void OnDestroy()
    {
        _eventScope?.Dispose();
    }

    /// <summary>
    /// Handles event messages.
    /// </summary>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is PatchInitializeFailedEvent)
        {
            System.Action callback = () =>
            {
                UserTryInitializePackageEvent.SendEventMessage();
            };
            ShowMessageBox("Failed to initialize package.", callback);
        }
        else if (message is PatchStepChangedEvent patchStepChanged)
        {
            _tips.text = patchStepChanged.Tips;
            UnityEngine.Debug.Log(patchStepChanged.Tips);
        }
        else if (message is PatchFoundUpdateFilesEvent patchFoundUpdateFiles)
        {
            System.Action callback = () =>
            {
                UserBeginDownloadWebFilesEvent.SendEventMessage();
            };
            float sizeMB = patchFoundUpdateFiles.TotalSizeBytes / 1048576f;
            sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
            string totalSizeMB = sizeMB.ToString("f1");
            ShowMessageBox($"Update files were found. Total count: {patchFoundUpdateFiles.TotalCount}. Total size: {totalSizeMB} MB.", callback);
        }
        else if (message is PatchDownloadUpdatedEvent patchDownloadUpdated)
        {
            _slider.value = (float)patchDownloadUpdated.CurrentDownloadCount / patchDownloadUpdated.TotalDownloadCount;
            string currentSizeMB = (patchDownloadUpdated.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
            string totalSizeMB = (patchDownloadUpdated.TotalDownloadSizeBytes / 1048576f).ToString("f1");
            _tips.text = $"{patchDownloadUpdated.CurrentDownloadCount}/{patchDownloadUpdated.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
        }
        else if (message is PatchPackageVersionRequestFailedEvent)
        {
            System.Action callback = () =>
            {
                UserTryRequestPackageVersionEvent.SendEventMessage();
            };
            ShowMessageBox("Failed to request package version. Check the network status.", callback);
        }
        else if (message is PatchPackageManifestUpdateFailedEvent)
        {
            System.Action callback = () =>
            {
                UserTryUpdatePackageManifestEvent.SendEventMessage();
            };
            ShowMessageBox("Failed to update package manifest. Check the network status.", callback);
        }
        else if (message is PatchWebFileDownloadFailedEvent patchWebFileDownloadFailed)
        {
            System.Action callback = () =>
            {
                UserTryDownloadWebFilesEvent.SendEventMessage();
            };
            ShowMessageBox($"Failed to download file: '{patchWebFileDownloadFailed.FileName}'.", callback);
        }
    }

    /// <summary>
    /// Shows a message box.
    /// </summary>
    private void ShowMessageBox(string content, System.Action ok)
    {
        // Try to reuse an inactive message box.
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

        // Create a new message box if none are available.
        if (msgBox == null)
        {
            msgBox = new MessageBox();
            var cloneObject = GameObject.Instantiate(_messageBoxObj, _messageBoxObj.transform.parent);
            msgBox.Create(cloneObject);
            _msgBoxList.Add(msgBox);
        }

        // Show the message box.
        msgBox.Show(content, ok);
    }
}
