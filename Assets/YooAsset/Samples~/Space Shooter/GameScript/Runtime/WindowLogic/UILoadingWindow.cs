using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniFramework.Utility;

public class UILoadingWindow : MonoBehaviour
{
    private Text _info;
    private int _countdown;
    private UniTimer _timer = UniTimer.CreatePepeatTimer(0, 0.2f);

    private void Awake()
    {
        _info = this.transform.Find("info").GetComponent<Text>();
    }
    private void Start()
    {
        _info.text = "Loading";
        _timer.Reset();
        _countdown = 0;
    }
    private void Update()
    {
        if(_timer.Update(Time.deltaTime))
        {
            _countdown++;
            if (_countdown > 6)
                _countdown = 0;

            string tips = "Loading";
            for(int i=0; i<_countdown; i++)
            {
                tips += ".";
            }
            _info.text = tips;
        }
    }
}