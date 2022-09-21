using UnityEngine;

namespace YooAsset
{
	internal class YooAssetDriver : MonoBehaviour
	{
		void Update()
		{
			YooAssets.InternalUpdate();
		}

		void OnDestroy()
		{
			YooAssets.InternalDestroy();
		}

		void OnApplicationQuit()
		{
			YooAssets.InternalDestroy();
		}
	}
}