#if UNITY_WEBGL && WEIXINMINIGAME
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal class WechatFileSystemTest : MonoBehaviour
    {
        private void Awake()
        {
            YooAssets.Initialize();
        }

        private IEnumerator Start()
        {
            string packageName = "DefaultPackage";
            string testLocation = "asteroid01";
            string hostServer = "http://127.0.0.1/CDN/WebGL/yoo";
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE";

            IRemoteService remoteService = new RemoteService(hostServer);
            WechatFileSystem fileSystem = new WechatFileSystem();
            fileSystem.SetParameter(EFileSystemParameter.RemoteService, remoteService);
            fileSystem.OnCreate(packageName, packageRoot);

            FileSystemTester tester = new FileSystemTester();
            yield return tester.RunTester(fileSystem, testLocation);
        }

        private class RemoteService : IRemoteService
        {
            private readonly string _hostServer;

            public RemoteService(string hostServer)
            {
                _hostServer = hostServer;
            }
            IReadOnlyList<string> IRemoteService.GetRemoteUrls(string fileName)
            {
                return new List<string> { $"{_hostServer}/{fileName}" };
            }
        }
    }
}
#endif
