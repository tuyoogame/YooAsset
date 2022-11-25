using UnityEngine;

namespace UniFramework.Window
{
	internal class UniWindowDriver : MonoBehaviour
	{
		void Update()
		{
			UniWindow.Update();
		}

		void OnDestroy()
		{
			UniWindow.Destroy();
		}
	}
}