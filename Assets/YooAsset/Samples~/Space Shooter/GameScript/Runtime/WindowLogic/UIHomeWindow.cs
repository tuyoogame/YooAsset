using UnityEngine;
using UnityEngine.UI;

public class UIHomeWindow : MonoBehaviour
{
    private Text _version;
    private GameObject _aboutView;

    private void Awake()
    {
        _version = this.transform.Find("version").GetComponent<Text>();
        _aboutView = this.transform.Find("AboutView").gameObject;

        var loginBtn = this.transform.Find("PlayGameButton").GetComponent<Button>();
        loginBtn.onClick.AddListener(OnClickPlayGameBtn);

        var aboutBtn = this.transform.Find("AboutButton").GetComponent<Button>();
        aboutBtn.onClick.AddListener(OnClickAboutBtn);

        var maskBtn = this.transform.Find("AboutView/mask").GetComponent<Button>();
        maskBtn.onClick.AddListener(OnClickMaskBtn);
    }
    private void Start()
    {
        var package = YooAsset.YooAssets.GetPackage("DefaultPackage");
        _version.text = "Version: " + package.GetPackageVersion();
    }

    private void OnClickPlayGameBtn()
    {
        SceneChangeToBattleEvent.SendEventMessage();
    }
    private void OnClickAboutBtn()
    {
        _aboutView.SetActive(true);
    }
    private void OnClickMaskBtn()
    {
        _aboutView.SetActive(false);
    }
}