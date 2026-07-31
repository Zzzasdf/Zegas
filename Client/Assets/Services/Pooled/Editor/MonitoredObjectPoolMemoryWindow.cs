using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 监控对象池引用计数窗口
/// </summary>
public class MonitoredObjectPoolMemoryWindow: EditorWindow
{
    [MenuItem("Tools/打开监控对象池引用计数窗口")]
    private static void Open()
    {
        GetWindow<MonitoredObjectPoolMemoryWindow>("监控对象池引用计数", true);
    }
    private void OnEnable()
    {
        EditorApplication.update += Repaint;
    }
    private void OnDisable()
    {
        EditorApplication.update -= Repaint;
    }
    
    private Vector2 scroll;
    private Dictionary<string, bool> foldoutDict = new Dictionary<string, bool>();
    
    public void OnGUI()
    {
#if !POOLED_EXCEPTION
        EditorGUILayout.LabelField("请开启 POOLED_EXCEPTION");
#else
        Dictionary<string, Dictionary<System.Type, HashSet<MonitoredObjectPool.IMonitoredPool>>> poolGroupDict = MonitoredObjectPool.Pools;
        if (poolGroupDict == null) return;
        scroll = EditorGUILayout.BeginScrollView(scroll);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("ClassName");
            EditorGUILayout.LabelField("CountActive\tCountInactive\tCountAll");
            if (GUILayout.Button("Clear"))
            {
                foreach (var groupPair in poolGroupDict)
                {
                    foreach (var poolsPair in groupPair.Value)
                    {
                        foreach (var poolPair in poolsPair.Value)
                        {
                            poolPair.Clear();
                        }
                    }
                }
            }
        }
        foreach (var groupPair in poolGroupDict)
        {
            using (new EditorGUILayout.VerticalScope("framebox"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    foldoutDict[groupPair.Key] = EditorGUILayout.Foldout(foldoutDict.GetValueOrDefault(groupPair.Key, true), groupPair.Key);
                }
                if (foldoutDict.ContainsKey(groupPair.Key))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        foreach (var poolsPair in groupPair.Value)
                        {
                            foreach (var poolPair in poolsPair.Value)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.LabelField(poolsPair.Key.ToString());
                                    EditorGUILayout.LabelField($"{poolPair.CountActive,11}\t{poolPair.CountInactive,11+2+13}\t{poolPair.CountAll,13+2+8}");
                                    if (GUILayout.Button("Clear"))
                                    {
                                        poolPair.Clear();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        EditorGUILayout.EndScrollView();
#endif
    }
}
