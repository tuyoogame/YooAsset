using NUnit.Framework;

/// <summary>
/// 运行时内部机制测试套件。
/// </summary>
/// <remarks>
/// 集中声明测试入口，具体场景由同目录下的测试类实现。
/// 用于验证异步操作、共享加载器、引用计数和任务调度等内部机制。
/// 当前用例通过内存场景控制执行顺序，不依赖资源构建、网络或全局 YooAssets 初始化。
/// </remarks>
public class T4_TestRuntimeInternals
{
    [Test]
    public void A01_TestProviderFailureIsolation()
    {
        var tester = new TestProviderFailureIsolation();
        tester.RuntimeTester();
    }
}
