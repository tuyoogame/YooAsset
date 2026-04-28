using System.Runtime.CompilerServices;

// 内部友元
[assembly: InternalsVisibleTo("YooAsset.Editor")]
[assembly: InternalsVisibleTo("YooAsset.Tests")]
[assembly: InternalsVisibleTo("YooAsset.Tests.Editor")]

// 外部友元
[assembly: InternalsVisibleTo("YooAsset.MiniGame")]
[assembly: InternalsVisibleTo("YooAsset.Extension")]
[assembly: InternalsVisibleTo("YooAsset.Extension.Editor")]
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]