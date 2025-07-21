using System.Runtime.CompilerServices;

// 内部友元
[assembly: InternalsVisibleTo("YooAsset.Editor")]
[assembly: InternalsVisibleTo("YooAsset.Test")]
[assembly: InternalsVisibleTo("YooAsset.Test.Editor")]

// 外部友元
[assembly: InternalsVisibleTo("YooAsset.MiniGame")]
[assembly: InternalsVisibleTo("YooAsset.RuntimeExtension")]
[assembly: InternalsVisibleTo("YooAsset.EditorExtension")]
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]