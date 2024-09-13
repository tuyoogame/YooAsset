using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 清理未使用的缓存文件
/// </summary>
internal class FsmClearPackageCache : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        PatchEventDefine.PatchStatesChange.SendEventMessage("清理未使用的缓存文件！");
        var packageName = (string)_machine.GetBlackboardValue("PackageName");
        var package = YooAssets.GetPackage(packageName);
        var operation = package.ClearUnusedBundleFilesAsync();
        operation.Completed += Operation_Completed;
    }
    void IStateNode.OnUpdate()
    {
    }
    void IStateNode.OnExit()
    {
    }

    private void Operation_Completed(YooAsset.AsyncOperationBase obj)
    {
#if UNITY_WEBGL && WEIXINMINIGAME

		//删除旧的资源清单文件和哈希文件
		if (WX.StorageHasKeySync(YooAssets.DefaultPackageVersion_Key))
		{
            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var packageVersion = (string)_machine.GetBlackboardValue("PackageVersion");
            var lastPackageVersion = WX.StorageGetStringSync(YooAssets.DefaultPackageVersion_Key, "");
			if (lastPackageVersion != packageVersion)
			{
                WX.StorageSetStringSync(YooAssets.DefaultPackageVersion_Key, packageVersion);
            }
		}
#endif
        _machine.ChangeState<FsmUpdaterDone>();
    }
}