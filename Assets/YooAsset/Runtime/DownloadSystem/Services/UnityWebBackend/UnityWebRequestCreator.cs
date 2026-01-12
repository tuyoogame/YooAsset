using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// 自定义 UnityWebRequest 创建委托
    /// </summary>
    /// <remarks>
    /// <para>用于自定义 UnityWebRequest 的创建方式，例如添加证书验证、代理设置等。</para>
    /// <para>通过 UnityWebRequestBackend 构造函数传入</para>
    /// </remarks>
    /// <param name="url">请求地址</param>
    /// <param name="method">HTTP 方法（如 GET、HEAD）</param>
    /// <returns>自定义配置的 UnityWebRequest 实例</returns>
    public delegate UnityWebRequest UnityWebRequestCreator(string url, string method);
}