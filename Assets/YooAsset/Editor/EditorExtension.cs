#if UNITY_2019_4
namespace YooAsset.Editor
{
	public static partial class UnityEngine_UIElements_ListView_Extension
	{
		public static void ClearSelection(this UnityEngine.UIElements.ListView o)
		{
			o.selectedIndex = -1;
		}
	}
}
#endif