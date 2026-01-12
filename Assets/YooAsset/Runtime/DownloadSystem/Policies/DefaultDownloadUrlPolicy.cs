using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 默认的 URL 选择策略
    /// </summary>
    /// <remarks>
    /// <para>失败时递增计数并轮转到下一个候选 URL，成功时保持粘滞在当前可用的 URL 上。</para>
    /// <para>策略实例随 FileSystem 重建时自动重置</para>
    /// </remarks>
    public class DefaultDownloadUrlPolicy : IDownloadUrlPolicy
    {
        private int _failureCount = 0;

        /// <summary>
        /// 基于内部失败计数轮转选择 URL
        /// </summary>
        /// <param name="candidateUrls">候选 URL 列表</param>
        /// <returns>选中的 URL</returns>
        public string SelectUrl(IReadOnlyList<string> candidateUrls)
        {
            if (candidateUrls == null || candidateUrls.Count == 0)
                throw new YooInternalException("Candidate URL list is null or empty.");

            int index = (_failureCount & 0x7FFFFFFF) % candidateUrls.Count;
            return candidateUrls[index];
        }

        /// <summary>
        /// 请求成功反馈，保持当前 URL 不变。
        /// </summary>
        /// <param name="url">成功请求的 URL</param>
        public void OnRequestSucceeded(string url)
        {
        }

        /// <summary>
        /// 请求失败反馈，递增失败计数以切换 URL。
        /// </summary>
        /// <param name="url">失败请求的 URL</param>
        /// <param name="httpCode">HTTP 响应状态码</param>
        /// <param name="httpError">HTTP 错误信息</param>
        public void OnRequestFailed(string url, long httpCode, string httpError)
        {
            _failureCount++;
        }
    }
}