using System;
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
        Dictionary<string, Dictionary<Type, HashSet<MonitoredObjectPool.IMonitoredPool>>> poolGroupDict = MonitoredObjectPool.Pools;
        if (poolGroupDict == null) return;
        scroll = EditorGUILayout.BeginScrollView(scroll);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("ClassName");
            EditorGUILayout.LabelField("CountActive\tCountInactive\tCountAll");
        }
        foreach (var group in poolGroupDict)
        {
            using (new EditorGUILayout.VerticalScope("framebox"))
            {
                foldoutDict[group.Key] = EditorGUILayout.Foldout(foldoutDict.GetValueOrDefault(group.Key, true), group.Key);
                if (foldoutDict[group.Key])
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        foreach (var pools in group.Value)
                        {
                            foreach (var pool in pools.Value)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.LabelField(pools.Key.ToString());
                                    EditorGUILayout.LabelField($"{pool.CountActive,11}\t{pool.CountInactive,11+2+13}\t{pool.CountAll,13+2+8}");
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
