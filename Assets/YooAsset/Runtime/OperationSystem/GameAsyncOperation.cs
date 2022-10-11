
namespace YooAsset
{
	public abstract class GameAsyncOperation : AsyncOperationBase
	{
		internal override void Start()
		{
			OnStart();
		}
		internal override void Update()
		{
			OnUpdate();
		}

		protected abstract void OnStart();
		protected abstract void OnUpdate();
	}
}