using UnityEngine;
using UnityEngine.UI;

public class UIAboutWindow : MonoBehaviour
{
    private void Awake()
    {
        var maskBtn = this.transform.Find("mask").GetComponent<Button>();
        maskBtn.onClick.AddListener(OnClickMaskBtn);
    }

    private void OnClickMaskBtn()
    {
        GameObject.Destroy(this.gameObject);
    }
}