using UnityEngine.Networking;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 自定义下载器的请求委托
    /// </summary>
    public delegate UnityWebRequest UnityWebRequestCreator(string url, string method);
}
