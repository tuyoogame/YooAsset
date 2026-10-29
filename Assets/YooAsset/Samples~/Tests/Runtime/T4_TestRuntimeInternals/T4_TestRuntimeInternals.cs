using NUnit.Framework;

/// <summary>
/// 运行时内部机制测试套件。
/// </summary>
/// <remarks>
/// 集中声明测试入口，具体场景由同目录下的测试类实现。
/// 用于验证异步操作、共享加载器、引用计数和任务调度等内部机制。
/// 用例不依赖资源构建、外部网络或全局 YooAssets 初始化，部分请求测试使用临时文件。
/// </remarks>
public class T4_TestRuntimeInternals
{
    [Test]
    public void A01_TestProviderFailureIsolation()
    {
        var tester = new TestProviderFailureIsolation();
        tester.RuntimeTester();
    }

    [Test]
    public void A02_TestInitializationProgress()
    {
        var tester = new TestInitializationProgress();
        tester.RuntimeTester();
    }

    [Test]
    public void A03_TestClearCacheProgress()
    {
        var tester = new TestClearCacheProgress();
        tester.RuntimeTester();
    }

    [Test]
    public void A04_TestDestroyPackageProgress()
    {
        var tester = new TestDestroyPackageProgress();
        tester.RuntimeTester();
    }

    [Test]
    public void A05_TestProviderLoadingProgress()
    {
        var tester = new TestProviderLoadingProgress();
        tester.RuntimeTester();
    }

    [Test]
    public void A06_TestDownloadRequestUpdates()
    {
        new TestDownloadRequestUpdates().RuntimeTester();
    }

    [Test]
    public void A07_TestLocalUnityRequestUpdates()
    {
        new TestDownloadRequestUpdates().LocalUnityRequestTester();
    }
}
