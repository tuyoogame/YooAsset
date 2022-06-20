using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YooAsset
{
    public interface IRemoteHostServices
    {
        /// <summary>
        /// 默认的资源服务器下载地址
        /// </summary>
        /// <returns></returns>
        string GetDefaultHost();

        /// <summary>
        /// 备用的资源服务器下载地址
        /// </summary>
        /// <returns></returns>
        string GetFallbackHost();
    }
}
