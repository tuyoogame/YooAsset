using System;

namespace YooAsset
{
    /// <summary>
    /// 标签集合
    /// </summary>
    public class PackageTags
    {
        /// <summary>
        /// 空标签集合
        /// </summary>
        public static readonly PackageTags Empty = new PackageTags(Array.Empty<string>());

        private readonly string[] _tags;

        /// <summary>
        /// 解析后的完整标签字符串数组
        /// </summary>
        internal string[] RawTags
        {
            get { return _tags; }
        }

        /// <summary>
        /// 标签数量
        /// </summary>
        public int TagCount
        {
            get { return _tags.Length; }
        }

        /// <summary>
        /// 是否包含分类标签
        /// </summary>
        public bool IsTagged
        {
            get { return _tags.Length > 0; }
        }


        /// <summary>
        /// 创建标签集合实例
        /// </summary>
        /// <param name="tags">解析后的标签字符串数组</param>
        public PackageTags(string[] tags)
        {
            if (tags == null)
                _tags = Array.Empty<string>();
            else
                _tags = tags;
        }

        /// <summary>
        /// 获取指定索引的标签
        /// </summary>
        public string GetTag(int index)
        {
            return _tags[index];
        }

        /// <summary>
        /// 是否包含指定的单个标签
        /// </summary>
        public bool HasTag(string tag)
        {
            for (int i = 0; i < _tags.Length; i++)
            {
                if (_tags[i] == tag)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否包含指定标签数组中的任意一个
        /// </summary>
        public bool HasAnyTag(string[] tags)
        {
            if (tags == null || tags.Length == 0 || _tags.Length == 0)
                return false;

            for (int i = 0; i < tags.Length; i++)
            {
                if (HasTag(tags[i]))
                    return true;
            }
            return false;
        }
    }
}
