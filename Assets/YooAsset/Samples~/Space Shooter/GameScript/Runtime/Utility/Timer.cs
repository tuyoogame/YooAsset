using System;

/// <summary>
/// 手动驱动的单次或循环计时器
/// </summary>
/// <remarks>仅支持单线程，时间单位为秒。</remarks>
public sealed class Timer
{
    private readonly double _firstDelay;
    private readonly double _interval;
    private double _remaining;

    /// <summary>
    /// 是否已暂停
    /// </summary>
    public bool IsPaused { get; private set; }

    /// <summary>
    /// 是否已到期结束或主动停止
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// 距离下次触发的剩余秒数，结束后为 0。
    /// </summary>
    public double Remaining => _remaining;

    private Timer(double firstDelay, double interval)
    {
        _firstDelay = firstDelay;
        _interval = interval;
        _remaining = firstDelay;
    }

    /// <summary>
    /// 推进计时并检测触发
    /// </summary>
    /// <param name="deltaTime">本次时间增量秒数</param>
    /// <returns>本次是否触发</returns>
    /// <remarks>暂停或结束时不计时；循环保留周期余量，每次更新最多触发一次。</remarks>
    public bool Update(double deltaTime)
    {
        if (IsPaused || IsCompleted)
            return false;

        if (deltaTime < _remaining)
        {
            _remaining -= deltaTime;
            return false;
        }

        if (_interval == 0d)
        {
            Stop();
        }
        else
        {
            // 跳过错过的周期，保留余量。
            double overdue = deltaTime - _remaining;
            _remaining = _interval - overdue % _interval;
        }

        return true;
    }

    /// <summary>
    /// 暂停计时
    /// </summary>
    public void Pause()
    {
        if (!IsCompleted)
            IsPaused = true;
    }

    /// <summary>
    /// 恢复计时，不重启已结束的计时器。
    /// </summary>
    public void Resume()
    {
        IsPaused = false;
    }

    /// <summary>
    /// 重置计时，清除暂停和结束状态。
    /// </summary>
    public void Reset()
    {
        _remaining = _firstDelay;
        IsPaused = false;
        IsCompleted = false;
    }

    /// <summary>
    /// 停止计时，可通过 Reset 重新开始。
    /// </summary>
    public void Stop()
    {
        _remaining = 0d;
        IsPaused = false;
        IsCompleted = true;
    }


    /// <summary>
    /// 创建单次计时器
    /// </summary>
    /// <param name="delay">延迟秒数</param>
    public static Timer Once(double delay)
    {
        if (double.IsNaN(delay) || double.IsInfinity(delay) || delay < 0d)
            throw new ArgumentOutOfRangeException(nameof(delay), "Time must be finite and non-negative.");
        return new Timer(delay, 0d);
    }

    /// <summary>
    /// 创建循环计时器，首次等待一个完整间隔。
    /// </summary>
    /// <param name="interval">触发间隔秒数</param>
    public static Timer Repeat(double interval)
    {
        return Repeat(interval, interval);
    }

    /// <summary>
    /// 创建指定首次延迟的循环计时器
    /// </summary>
    /// <param name="interval">触发间隔秒数</param>
    /// <param name="firstDelay">首次延迟秒数</param>
    public static Timer Repeat(double interval, double firstDelay)
    {
        if (double.IsNaN(interval) || double.IsInfinity(interval) || interval < 0d)
            throw new ArgumentOutOfRangeException(nameof(interval), "Time must be finite and non-negative.");
        if (double.IsNaN(firstDelay) || double.IsInfinity(firstDelay) || firstDelay < 0d)
            throw new ArgumentOutOfRangeException(nameof(firstDelay), "Time must be finite and non-negative.");
        if (interval == 0d)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than zero.");
        return new Timer(firstDelay, interval);
    }
}
