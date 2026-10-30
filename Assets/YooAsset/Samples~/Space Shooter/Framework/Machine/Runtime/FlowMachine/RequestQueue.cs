namespace Framework.Machine
{
    /// <summary>
    /// 切换请求队列
    /// </summary>
    /// <remarks>
    /// <para>固定容量环形缓冲，先进先出，不扩容，元素是目标状态的索引。</para>
    /// <para>写满意味着存在切换环，由调用方拒绝并记录错误。</para>
    /// </remarks>
    internal sealed class RequestQueue
    {
        private readonly int[] _buffer;
        private int _head;
        private int _count;

        public RequestQueue(int capacity)
        {
            _buffer = new int[capacity];
        }
        public bool TryEnqueue(int target)
        {
            if (_count >= _buffer.Length)
                return false;

            int tail = _head + _count;
            if (tail >= _buffer.Length)
                tail -= _buffer.Length;

            _buffer[tail] = target;
            _count++;
            return true;
        }
        public bool TryDequeue(out int target)
        {
            if (_count == 0)
            {
                target = -1;
                return false;
            }

            target = _buffer[_head];
            _head++;
            if (_head >= _buffer.Length)
                _head = 0;

            _count--;
            return true;
        }
        public void Clear()
        {
            _head = 0;
            _count = 0;
        }
    }
}
