#if UNITY_WEBGL && WEIXINMINIGAME
using System.Collections;
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

            IRemoteServices remoteServices = new RemoteServices(hostServer);
            WechatFileSystem fileSystem = new WechatFileSystem();
            fileSystem.SetParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
            fileSystem.OnCreate(packageName, packageRoot);

            FileSystemTester tester = new FileSystemTester();
            yield return tester.RunTester(fileSystem, testLocation);
        }

        private class RemoteServices : IRemoteServices
        {
            private readonly string _hostServer;

            public RemoteServices(string hostServer)
            {
                _hostServer = hostServer;
            }
            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{_hostServer}/{fileName}";
            }
            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{_hostServer}/{fileName}";
            }
        }
    }
}
#endif