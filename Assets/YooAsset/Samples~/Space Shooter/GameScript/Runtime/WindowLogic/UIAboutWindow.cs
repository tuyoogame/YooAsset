using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAboutWindow : MonoBehaviour
{
    private void Awake()
    {
        var maskBtn = this.transform.Find("mask").GetComponent<Button>();
        maskBtn.onClick.AddListener(OnClicMaskBtn);
    }

    private void OnClicMaskBtn()
    {
        GameObject.Destroy(this.gameObject);
    }
}