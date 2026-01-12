using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// URL 选择策略
    /// </summary>
    public interface IDownloadUrlPolicy
    {
        /// <summary>
        /// 选择本次请求应使用的 URL
        /// </summary>
        /// <param name="candidateUrls">候选 URL 列表（至少包含一个）</param>
        /// <returns>选中的 URL</returns>
        string SelectUrl(IReadOnlyList<string> candidateUrls);

        /// <summary>
        /// 反馈请求成功，策略可据此更新内部状态。
        /// </summary>
        /// <param name="url">实际使用的 URL</param>
        void OnRequestSucceeded(string url);

        /// <summary>
        /// 反馈请求失败，策略可据此更新内部状态。
        /// </summary>
        /// <param name="url">实际使用的 URL</param>
        /// <param name="httpCode">HTTP 状态码（0 表示网络中断或非 HTTP 错误）</param>
        /// <param name="httpError">服务器返回的错误描述文本</param>
        void OnRequestFailed(string url, long httpCode, string httpError);
    }
}
