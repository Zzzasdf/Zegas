using System.Collections.Generic;

public partial class Boot
{
    private static readonly Dictionary<E_GAME_MODE, Configure> ModeConfigures = new Dictionary<E_GAME_MODE, Configure>
    {
        [E_GAME_MODE.Default] = new Configure("DefaultPackage", "HotUpdate", "Launcher", AOTMetaAssemblyFiles), 
    };
    
    private static List<string> AOTMetaAssemblyFiles { get; } = new List<string>
    {
        // "mscorlib.dll",
        // "System.dll",
        // "System.Core.dll",
        // "YooAsset.dll",
        // "UniFramework.Event.dll",
        // "UniFramework.Machine.dll",
        // "UniFramework.Utility.dll",
        // "UniTask.dll",
    };
    
    private enum E_GAME_MODE
    {
        Default = 0,
    }
    private class Configure
    {
        public string YooAssetPackage { get; }
        public string HotUpdateDll { get; }
        public string HotUpdateDllWithExt { get; }
        public string HotUpdateFirstScene { get; }
        public List<string> AOTMetaAssemblyFiles { get; }
        public Configure(string yooAssetPackage, string hotUpdateDll, string hotUpdateFirstScene, List<string> aotMetaAssemblyFiles)
        {
            YooAssetPackage = yooAssetPackage;
            HotUpdateDll = hotUpdateDll;
            HotUpdateDllWithExt = $"{hotUpdateDll}.dll";
            HotUpdateFirstScene = hotUpdateFirstScene;
            AOTMetaAssemblyFiles = aotMetaAssemblyFiles;
        }
    }
}
