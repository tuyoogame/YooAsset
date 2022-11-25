using UnityEngine;

namespace UniFramework.Pooling
{
	internal class UniPoolingDriver : MonoBehaviour
	{
		void Update()
		{
			UniPooling.Update();
		}

		void OnDestroy()
		{
			UniPooling.Destroy();
		}
	}
}