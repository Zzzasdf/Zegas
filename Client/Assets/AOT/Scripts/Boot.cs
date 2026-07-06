using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HybridCLR;
using UniFramework.Event;
using UnityEngine;
using YooAsset;

public sealed partial class Boot : MonoBehaviour
{
    /// <summary>
    /// 游戏模式
    /// </summary>
    [SerializeField] private E_GAME_MODE EGameMode;
    private Configure ModeConfigure => ModeConfigures[EGameMode];
    
    /// <summary>
    /// 资源系统运行模式
    /// </summary>
    [SerializeField] private EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
    
    private void Awake()
    {
        Debug.Log($"游戏运行模式：{ EGameMode }模式资源系统运行模式：{ PlayMode }");
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
    }

    private IEnumerator Start()
    {
        // 游戏管理器
        GameManager.Instance.Behaviour = this;

        // 初始化事件系统
        UniEvent.Initalize();
        
        // 初始化资源系统
        YooAssets.Initialize();
        
        // 加载更新页面
        var go = Resources.Load<GameObject>("PatchWindow");
        GameObject.Instantiate(go);

        string yooAssetPackage = ModeConfigure.YooAssetPackage;
        
        // 开始补丁更新流程
        var operation = new PatchOperation(yooAssetPackage, PlayMode);
        YooAssets.StartOperation(operation);
        yield return operation;
        
        // 设置默认的资源包
        ResourcePackage gamePackage = YooAssets.GetPackage("DefaultPackage");
        YooAssets.SetDefaultPackage(gamePackage);

        yield return LoadDlls(gamePackage);
    }
    
    private IEnumerator LoadDlls(ResourcePackage gamePackage)
    {
        var assets = new List<string> { ModeConfigure.HotUpdateDllWithExt }.Concat(ModeConfigure.AOTMetaAssemblyFiles);
        Dictionary<string, TextAsset> assetDataDict = new Dictionary<string, TextAsset>();
        // 逐个加载程序集字节数据
        foreach (var asset in assets)
        {
            Debug.Log($"正在加载程序集：{asset}");

            var handle = gamePackage.LoadAssetAsync<TextAsset>(asset);
            yield return handle;
            if (handle.Status == EOperationStatus.Succeed)
            {
                TextAsset textAsset = handle.AssetObject as TextAsset;
                assetDataDict[asset] = textAsset;
                Debug.Log($"程序集加载成功：{asset}, 大小：{textAsset.bytes.Length} 字节");
            }
            else
            {
                Debug.LogError($"程序集加载失败：{asset}, 错误：{handle.LastError}");
            }
            // 释放句柄
            handle.Release();
        }
        LoadMetadataForAOTAssemblies(assetDataDict);
        LoadHotUpdateDlls(assetDataDict);
        
        // 切换到主页面场景
        SceneEventDefine.ChangeToHomeScene.SendEventMessage(ModeConfigure.HotUpdateFirstScene);
    }
    
    /// <summary>
    /// 补充元数据
    /// </summary>
    private void LoadMetadataForAOTAssemblies(Dictionary<string, TextAsset> assetDataDict)
    {
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyFiles)
        {
            byte[] dllBytes = assetDataDict[aotDllName].bytes;
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
        }
    }

    private void LoadHotUpdateDlls(Dictionary<string, TextAsset> assetDataDict)
    {
        if (assetDataDict.Count == 0)
        {
            Debug.Log("程序集字节数据为空，跳过加载热更dll");
            return;
        }
        
        // 加载热更dll
#if !UNITY_EDITOR
        Assembly.Load(assetDataDict[ModeConfigure.HotUpdateDllWithExt].bytes);
#else
        System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == ModeConfigure.HotUpdateDll);
#endif
    }
}
