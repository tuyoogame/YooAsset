# 代码贡献

代码贡献请遵循下面的规范。

### 提交规范

每个PR只针对一项内容的改进或修复，请勿合并提交。

PR标题尽量选择英文，备注内容可选中文。

### 对齐规范

Tab键对齐（可以在VS里设置）

### 命名规范

[规则1-1] 英文单词命名。禁止使用拼音或无意义的字母命名。

[规则1-2] 直观易懂。使用能够描述其功能或有意义的英文单词或词组。

[规则1-3] 不要采用下划线命名法。

```C#
int car_type //错误：下划线命名。 
```

[规则1-4] 常量、静态字段、类、结构体、非私有字段、方法等名称采用**大驼峰式命名法**

```C#
public const float MaxSpeed = 100f; //常量
public static float MaxSpeed = 100f; //静态字段
public class GameClass; //类
public struct GameStruct; //结构体
public string FirstName; //public字段
protected string FirstName; //protected字段
public void SendMessage(string message) {} //方法
```

[规则1-5] 私有字段、方法形参、局部变量采用 **小驼峰式命名法** 

注意：私有字段以下划线开头

```C#
private string _firstName; //私有字段
public void FindByFirstName(string firstName) {} //方法参数
string firstName; //局部变量
```

[规则1-6] 接口命名

注意：接口以大写字母I开头

```C#
public interface IState; //接口
```

[规则1-7] 枚举命名

注意：枚举以大写字母E开头

```C#
public enum EGameType {Simple, Hard}//枚举及枚举值
```

### 编码规范

[规则2-1] 声明变量时，一行只声明一个变量。

```C#
private string _firstName;
private string _lastName;
```

[规则2-2] 类的字段声明统一放置于类的最前端。

```C#
public class Student 
{
	private string _firstName;
	private string _lastName;

    public string GetFirstName() 
    {
        return _firstFiled;
    }
}
```

[规则2-3] 一行代码长度不要超过屏幕宽度。如果超过了，将超过部分换行。

### 注释规范

[规则3-1] 公共方法注释，采用 /// 形式自动产生XML标签格式的注释。包括方法介绍，参数含义，返回内容。

注意：私有方法可以不用注释。

```C#
/// <summary>
/// 设置场景名称
/// </summary>
/// <param name="sceneName">场景名</param>
/// <returns>如果设置成功返回True</returns>
public bool SetSceneName(string sceneName)
{
}
```

[规则3-2] 公共字段注释，采用 /// 形式自动产生XML标签格式的注释。

注意：私有字段可以不用注释。

```C#
public class SceneManager
{
    /// <summary>
    /// 场景的名字
    /// </summary>
    public string SceneName;
}
```

[规则3-3] 私有字段注释，注释位于代码后面，中间Space键隔开。

```C#
public class Student
{
   	private string _firstName; //姓氏
	private string _lastName; //姓名
}
```

[规则3-4] 方法内的代码块注释。

```C#
public void UpdateHost
{
    // 和服务器通信
    ...
        
    // 检测通信结果
    ...
        
    // 分析数据
    ...
}
```

