
namespace YooAsset.Editor
{
    /// <summary>
    /// 同类搜索命令的组内组合逻辑
    /// </summary>
    /// <remarks>
    /// 该枚举只影响同类命令（关键字组或数值组）内部的匹配方式。
    /// </remarks>
    public enum ESearchLogic
    {
        /// <summary>
        /// 组内所有条件同时满足
        /// </summary>
        AND,

        /// <summary>
        /// 组内任意条件满足即可
        /// </summary>
        OR,
    }
}
