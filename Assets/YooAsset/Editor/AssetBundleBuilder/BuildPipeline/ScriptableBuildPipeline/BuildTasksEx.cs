using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine.U2D;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using System.Linq;

namespace UnityEditor.Build.Pipeline.Tasks
{
    /// <summary>
    /// Ref https://zhuanlan.zhihu.com/p/586918159
    /// </summary>
    public class RemoveSpriteAtlasRedundancy : IBuildTask
    {
        /// <inheritdoc />
        public int Version => 1;

        [InjectContext]
        IBundleWriteData writeDataParam;

        /// <inheritdoc />
        public ReturnCode Run()
        {
            BundleWriteData writeData = (BundleWriteData)writeDataParam;

            // 所有图集散图的 guid 集合
            HashSet<GUID> spriteGuids = new HashSet<GUID>();

            // 遍历资源包里的资源记录其中图集的散图 guid
            foreach (var pair in writeData.FileToObjects)
            {
                foreach (ObjectIdentifier objectIdentifier in pair.Value)
                {
                    string path = AssetDatabase.GUIDToAssetPath(objectIdentifier.guid);
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (asset is SpriteAtlas)
                    {
                        List<string> spritePaths = AssetDatabase.GetDependencies(path, false).ToList();
                        foreach (string spritePath in spritePaths)
                        {
                            GUID spriteGuild = AssetDatabase.GUIDFromAssetPath(spritePath);
                            spriteGuids.Add(spriteGuild);
                        }
                    }
                }
            }

            // 将 writeData.FileToObjects 包含的图集散图的 texture 删掉避免冗余
            foreach (var pair in writeData.FileToObjects)
            {
                List<ObjectIdentifier> objectIdentifiers = pair.Value;
                for (int i = objectIdentifiers.Count - 1; i >= 0; i--)
                {
                    ObjectIdentifier objectIdentifier = objectIdentifiers[i];
                    if (spriteGuids.Contains(objectIdentifier.guid))
                    {
                        if (objectIdentifier.localIdentifierInFile == 2800000)
                        {
                            // 删除图集散图的冗余 texture
                            objectIdentifiers.RemoveAt(i);
                        }
                    }
                }
            }

            return ReturnCode.Success;
        }
    }
}