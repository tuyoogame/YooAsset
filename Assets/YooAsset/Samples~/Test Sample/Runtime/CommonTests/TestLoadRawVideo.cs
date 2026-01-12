using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;
using UnityEngine.Video;

/// <summary>
/// 测试视频文件加载与播放
/// </summary>
/// <remarks>
/// 覆盖 API: LoadRawFileAsync / RawFileHandle.GetRawFilePath
/// 测试内容:
/// 1. 异步加载视频原生文件，验证加载状态和文件存在
/// 2. 拷贝缓存文件到临时路径（补充 .mp4 扩展名以适配 VideoPlayer）
/// 3. 创建 VideoPlayer 组件播放视频，等待 1 秒后验证正在播放
/// 4. 销毁 VideoPlayer 并清理临时文件
/// </remarks>
public class TestLoadRawVideo
{
    public IEnumerator RuntimeTester()
    {
        ResourcePackage package = YooAssets.GetPackage(TestConsts.RawBundlePackageName);
        Assert.IsNotNull(package);

        var rawFileHandle = package.LoadRawFileAsync("video_logo");
        yield return rawFileHandle;
        Assert.AreEqual(EOperationStatus.Succeeded, rawFileHandle.Status);

        // 获取视频文件地址
        string videoFilePath = rawFileHandle.GetRawFilePath();
        Assert.IsTrue(File.Exists(videoFilePath));

        // VideoPlayer 需要文件带正确扩展名才能识别格式，缓存文件名为 __data 无扩展名
        string tempPath = Path.Combine(Application.temporaryCachePath, "test_video.mp4");
        File.Copy(videoFilePath, tempPath, true);

        // 创建预制体播放视频
        GameObject go = new GameObject("video player");
        var videoPlayer = go.AddComponent<VideoPlayer>(); 
        videoPlayer.source = VideoSource.Url;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.url = tempPath;
        videoPlayer.Play();

        yield return new WaitForSeconds(1f);
        Assert.IsTrue(videoPlayer.isPlaying);

        // 清理 VideoPlayer 和临时文件
        videoPlayer.Stop();
        GameObject.Destroy(go);
        if (File.Exists(tempPath))
            File.Delete(tempPath);
        rawFileHandle.Release();
    }
}
