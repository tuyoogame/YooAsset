//-------------------------------------
// 作者：Stark
//-------------------------------------

#if UNITY_ANDROID
/// <summary>
/// 为Github对开发者的友好，采用自动补充UnityPlayerActivity.java文件的通用姿势满足各个开发者
/// </summary>
internal class AndroidPost : UnityEditor.Android.IPostGenerateGradleAndroidProject
{
	public int callbackOrder => 99;
	public void OnPostGenerateGradleAndroidProject(string path)
	{
		path = path.Replace("\\", "/");
		string untityActivityFilePath = $"{path}/src/main/java/com/unity3d/player/UnityPlayerActivity.java";
		var readContent = System.IO.File.ReadAllLines(untityActivityFilePath);
		string postContent =
			"    //auto-gen-function \n" +
			"    public boolean CheckAssetExist(String filePath) \n" +
			"    { \n" +
			"        android.content.res.AssetManager assetManager = getAssets(); \n" +
			"        try \n" +
			"        { \n" +
			"            java.io.InputStream inputStream = assetManager.open(filePath); \n" +
			"            if (null != inputStream) \n" +
			"            { \n" +
			"                 inputStream.close(); \n" +
			"                 return true; \n" +
			"            } \n" +
			"        } \n" +
			"        catch(java.io.IOException e) \n" +
			"        { \n" +
			"            e.printStackTrace(); \n" +
			"        } \n" +
			"        return false; \n" +
			"    } \n" +
			"}";

		if (CheckFunctionExist(readContent) == false)
			readContent[readContent.Length - 1] = postContent;
		System.IO.File.WriteAllLines(untityActivityFilePath, readContent);
	}
	private bool CheckFunctionExist(string[] contents)
	{
		for (int i = 0; i < contents.Length; i++)
		{
			if (contents[i].Contains("CheckAssetExist"))
			{
				return true;
			}
		}
		return false;
	}
}
#endif

/*
//auto-gen-function
public boolean CheckAssetExist(String filePath)
{
	android.content.res.AssetManager assetManager = getAssets();
	try
	{
		java.io.InputStream inputStream = assetManager.open(filePath);
		if(null != inputStream)
		{
			inputStream.close();
			return true;
		}
	}
	catch(java.io.IOException e)
	{
		e.printStackTrace();
	}
	return false;
}
*/