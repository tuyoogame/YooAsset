using UnityEditor.SceneManagement;

namespace YooAsset.Editor
{
    /// <summary>
    /// 场景工具类
    /// </summary>
    public static class EditorSceneUtility
    {
        /// <summary>
        /// 检查当前打开的场景中是否存在未保存的修改
        /// </summary>
        /// <returns>存在未保存修改时为 true</returns>
        public static bool HasDirtyScenes()
        {
            var sceneCount = EditorSceneManager.sceneCount;
            for (var i = 0; i < sceneCount; ++i)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.isDirty)
                    return true;
            }
            return false;
        }
    }
}
