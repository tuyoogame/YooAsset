//-------------------------------------
// 作者：Stark
//-------------------------------------

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
		string postContent = "  //auto-gen-function\n" +
			"   public boolean CheckAssetExist(String filePath){\n" +
			"       android.content.res.AssetManager assetManager = getAssets();\n" +
			"       java.io.InputStream inputStream = null;\n" +
			"       try {\n" +
			"           inputStream = assetManager.open(filePath);\n" +
			"           if(null != inputStream)return true;\n" +
			"       }catch(java.io.IOException e) {\n" +
			"           e.printStackTrace();\n" +
			"        }finally{\n" +
			"            try {\n" +
			"               inputStream.close();\n" +
			"           } catch (java.io.IOException e) {\n" +
			"               e.printStackTrace();\n" +
			"           }\n" +
			"       }\n" +
			"       return false;\n" +
			"   }\n" +
			"}";
		if (!readContent[readContent.Length - 18].Contains("CheckAssetExist"))
			readContent[readContent.Length - 1] = postContent;
		System.IO.File.WriteAllLines(untityActivityFilePath, readContent);
	}
}