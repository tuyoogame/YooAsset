using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset.Editor
{
    [CreateAssetMenu(fileName = "ExtraCollectPath", menuName = "YooAsset/Create ExtraCollectPath")]
    public class ExtraCollectPath : ScriptableObject
    {
        public string path;
    }
}