
/// <summary>
/// 测试日志工具，输出格式为 [类名] 消息
/// </summary>
public static class TestLogger
{
    public static void Log(System.Object instance, string message)
    {
        if (instance == null)
            UnityEngine.Debug.Log($"[Null] {message}");
        else
            UnityEngine.Debug.Log($"[{instance.GetType().Name}] {message}");
    }
}