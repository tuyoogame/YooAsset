
public static class PatchEventDispatcher
{
	public static void SendPatchStepsChangeMsg(EPatchStates currentStates)
	{
		PatchEventDefine.PatchStatesChange msg = new PatchEventDefine.PatchStatesChange();
		msg.CurrentStates = currentStates;
		EventManager.SendMessage(msg);
	}
	public static void SendFoundUpdateFilesMsg(int totalCount, long totalSizeBytes)
	{
		PatchEventDefine.FoundUpdateFiles msg = new PatchEventDefine.FoundUpdateFiles();
		msg.TotalCount = totalCount;
		msg.TotalSizeBytes = totalSizeBytes;
		EventManager.SendMessage(msg);
	}
	public static void SendDownloadProgressUpdateMsg(int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes)
	{
		PatchEventDefine.DownloadProgressUpdate msg = new PatchEventDefine.DownloadProgressUpdate();
		msg.TotalDownloadCount = totalDownloadCount;
		msg.CurrentDownloadCount = currentDownloadCount;
		msg.TotalDownloadSizeBytes = totalDownloadSizeBytes;
		msg.CurrentDownloadSizeBytes = currentDownloadSizeBytes;
		EventManager.SendMessage(msg);
	}
	public static void SendStaticVersionUpdateFailedMsg()
	{
		PatchEventDefine.StaticVersionUpdateFailed msg = new PatchEventDefine.StaticVersionUpdateFailed();
		EventManager.SendMessage(msg);
	}
	public static void SendPatchManifestUpdateFailedMsg()
	{
		PatchEventDefine.PatchManifestUpdateFailed msg = new PatchEventDefine.PatchManifestUpdateFailed();
		EventManager.SendMessage(msg);
	}
	public static void SendWebFileDownloadFailedMsg(string fileName, string error)
	{
		PatchEventDefine.WebFileDownloadFailed msg = new PatchEventDefine.WebFileDownloadFailed();
		msg.FileName = fileName;
		msg.Error = error;
		EventManager.SendMessage(msg);
	}
}