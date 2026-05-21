using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 游戏对象实例化的操作选项
    /// </summary>
    public readonly struct InstantiateOptions
    {
        /// <summary>
        /// 是否激活实例化对象
        /// </summary>
        public bool IsActive { get; }

        /// <summary>
        /// 将指定给新对象的父对象
        /// </summary>
        public Transform Parent { get; }

        /// <summary>
        /// 是否在世界空间中定位新对象
        /// </summary>
        /// <value>
        /// 为 true 时保持世界空间位置，为 false 时相对于父对象定位。
        /// </value>
        public bool InWorldSpace { get; }

        /// <summary>
        /// 新对象的位置
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// 新对象的旋转
        /// </summary>
        public Quaternion Rotation { get; }

        internal bool SetPositionAndRotation { get; }

        /// <summary>
        /// 创建实例化选项（仅指定激活状态）
        /// </summary>
        /// <param name="isActive">是否激活实例化对象</param>
        public InstantiateOptions(bool isActive)
        {
            IsActive = isActive;
            Parent = null;
            InWorldSpace = false;

            SetPositionAndRotation = false;
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
        }

        /// <summary>
        /// 创建实例化选项（指定父对象）
        /// </summary>
        /// <param name="isActive">是否激活实例化对象</param>
        /// <param name="parent">父对象</param>
        /// <param name="inWorldSpace">是否在世界空间中定位</param>
        public InstantiateOptions(bool isActive, Transform parent, bool inWorldSpace)
        {
            IsActive = isActive;
            Parent = parent;
            InWorldSpace = inWorldSpace;

            SetPositionAndRotation = false;
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
        }

        /// <summary>
        /// 创建实例化选项（指定父对象和位置旋转）
        /// </summary>
        /// <param name="isActive">是否激活实例化对象</param>
        /// <param name="parent">父对象</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        public InstantiateOptions(bool isActive, Transform parent, Vector3 position, Quaternion rotation)
        {
            IsActive = isActive;
            Parent = parent;
            InWorldSpace = false;

            SetPositionAndRotation = true;
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        /// 创建实例化选项（指定位置旋转）
        /// </summary>
        /// <param name="isActive">是否激活实例化对象</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        public InstantiateOptions(bool isActive, Vector3 position, Quaternion rotation)
        {
            IsActive = isActive;
            Parent = null;
            InWorldSpace = false;

            SetPositionAndRotation = true;
            Position = position;
            Rotation = rotation;
        }
    }
}