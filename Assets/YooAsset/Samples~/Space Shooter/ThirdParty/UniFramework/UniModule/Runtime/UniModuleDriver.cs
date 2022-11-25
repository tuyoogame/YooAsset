using UnityEngine;

namespace UniFramework.Module
{
	internal class UniModuleDriver : MonoBehaviour
	{
		void Update()
		{
			UniModule.Update();
		}

		void OnDestroy()
		{
			UniModule.Destroy();
		}
	}
}