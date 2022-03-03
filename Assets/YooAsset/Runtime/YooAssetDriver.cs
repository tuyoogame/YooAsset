using UnityEngine;

namespace YooAsset
{
    internal class YooAssetDriver : MonoBehaviour
    {
        void Update()
        {
            YooAssets.InternalUpdate();
        }
    }
}