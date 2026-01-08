using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 异步操作调度器
    /// </summary>
    internal class OperationScheduler : IComparable<OperationScheduler>
    {
        private readonly List<AsyncOperationBase> _operations = new List<AsyncOperationBase>(100);
        private readonly List<AsyncOperationBase> _newList = new List<AsyncOperationBase>(100);

        /// <summary>
        /// 所属包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 调度器优先级（值越大越优先）
        /// </summary>
        public int Priority { private set; get; }

        /// <summary>
        /// 创建顺序（用于同优先级稳定排序）
        /// </summary>
        public int CreateIndex { private set; get; }


        public OperationScheduler(string packageName, int priority, int createIndex)
        {
            PackageName = packageName;
            Priority = priority;
            CreateIndex = createIndex;
        }

        /// <summary>
        /// 开始处理异步操作
        /// </summary>
        public void StartOperation(AsyncOperationBase operation)
        {
            _newList.Add(operation);
            operation.StartOperation();

            // 通知开始回调
            OperationSystem.InvokeStartCallback(PackageName, operation);
        }

        /// <summary>
        /// 更新调度器
        /// </summary>
        public void Update()
        {
            // 移除已经完成的异步操作
            for (int i = _operations.Count - 1; i >= 0; i--)
            {
                var operation = _operations[i];
                if (operation.IsFinish)
                {
                    _operations.RemoveAt(i);

                    // 通知完成回调
                    OperationSystem.InvokeFinishCallback(PackageName, operation);
                }
            }

            // 添加新增的异步操作
            if (_newList.Count > 0)
            {
                bool sorting = false;
                foreach (var operation in _newList)
                {
                    if (operation.Priority > 0)
                    {
                        sorting = true;
                        break;
                    }
                }

                _operations.AddRange(_newList);
                _newList.Clear();

                // 重新排序优先级
                if (sorting)
                    _operations.Sort();
            }

            // 更新进行中的异步操作
            for (int i = 0; i < _operations.Count; i++)
            {
                // 检查全局时间切片预算
                if (OperationSystem.IsBusy)
                    break;

                var operation = _operations[i];
                if (operation.IsFinish)
                    continue;

                operation.UpdateOperation();
            }
        }

        /// <summary>
        /// 清空并中止所有任务
        /// </summary>
        public void ClearAll()
        {
            // 终止临时队列里的任务
            foreach (var operation in _newList)
            {
                operation.AbortOperation();
                operation.FinishOperation(); //注意：强制收尾，确保Task能完成
            }
            _newList.Clear();

            // 终止正在进行的任务
            foreach (var operation in _operations)
            {
                operation.AbortOperation();
                operation.FinishOperation(); //注意：强制收尾，确保Task能完成
            }
            _operations.Clear();
        }

        /// <summary>
        /// 获取调试信息
        /// </summary>
        public List<DebugOperationInfo> GetDebugOperationInfos()
        {
            int totalCount = _operations.Count + _newList.Count;
            List<DebugOperationInfo> result = new List<DebugOperationInfo>(totalCount);

            // 包含正在执行的任务
            foreach (var operation in _operations)
            {
                var operationInfo = operation.GetDebugOperationInfo();
                result.Add(operationInfo);
            }

            // 包含待处理的新任务
            foreach (var operation in _newList)
            {
                var operationInfo = operation.GetDebugOperationInfo();
                result.Add(operationInfo);
            }

            return result;
        }

        #region 排序接口实现
        public int CompareTo(OperationScheduler other)
        {
            // 优先级高的排前面
            int result = other.Priority.CompareTo(this.Priority);
            if (result == 0)
            {
                // 优先级相同，按创建顺序
                result = this.CreateIndex.CompareTo(other.CreateIndex);
            }
            return result;
        }
        #endregion
    }
}
