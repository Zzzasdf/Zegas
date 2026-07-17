using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MacroDefinerModule
{
    public sealed partial class MacroDefinerWindow : EditorWindow
    {
        [MenuItem("Tools/设置全局宏")]
        private static void Open()
        {
            GetWindow<MacroDefinerWindow>("设置全局宏", true);
        }
        private void OnEnable()
        {
            PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, out string[] _defines);
            oldDefines = _defines.ToList();
            InitRelationshipChain();
            EditorApplication.update += Repaint;
        }
        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }
        
        // 旧宏定义
        private List<string> oldDefines;
        private Vector2 scroll;

        public void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            using (new EditorGUILayout.VerticalScope())
            {
                // 固定 隐藏
                {
                    EditorGUILayout.LabelField("固定宏");
                    GUI.enabled = false;
                    {
                        ShowDefines(fixedDefines);
                    }
                    GUI.enabled = true;
                }
                // 可选
                {
                    EditorGUILayout.LabelField("可选宏");
                    ShowDefines(optionalDefines);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    foreach (var pair in modeDict)
                    {
                        if (GUILayout.Button(pair.Value.modeName))
                        {
                            oldDefines.Clear();
                            for (int i = 0; i < pair.Value.eMacroDefiners.Count; i++)
                            {
                                EMacroDefiner eMacroDefiner = pair.Value.eMacroDefiners[i];
                                DefineNode defineNode = NodeDict[eMacroDefiner];
                                AddAllParentNode(defineNode);
                                AllCurrentNode(defineNode);
                                RemoveAllChildren(defineNode);
                            }              
                        }
                    }
                }
                if (GUILayout.Button("保存"))
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, oldDefines.ToArray());
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void ShowDefines(in List<EMacroDefiner> eMacroDefiners)
        {
            if (eMacroDefiners == null || eMacroDefiners.Count == 0) return;
            List<DefineNode> defineNodes = new List<DefineNode>();
            for (int i = 0; i < eMacroDefiners.Count; i++)
            {
                EMacroDefiner eMacroDefiner = eMacroDefiners[i];
                DefineNode defineNode = NodeDict[eMacroDefiner];
                defineNodes.Add(defineNode);
            }
            using (new EditorGUILayout.VerticalScope())
            {
                foreach (var defineNode in defineNodes)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20);
                
                        using (new EditorGUILayout.VerticalScope())
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField($"{defineNode.Current} => {defineNode.Description}", GUILayout.Width(250));
                                GUILayout.FlexibleSpace();
                                bool toggle = EditorGUILayout.Toggle(oldDefines.Contains(defineNode.StrCurrent));
                                if (toggle && !oldDefines.Contains(defineNode.StrCurrent))
                                {
                                    // 父节点必须是 Add 的状态
                                    AddAllParentNode(defineNode);
                                    oldDefines.Add(defineNode.StrCurrent);
                                }
                                else if (!toggle && oldDefines.Contains(defineNode.StrCurrent))
                                {
                                    // 子节点必须是 Remove 的状态
                                    RemoveAllChildren(defineNode);
                                    oldDefines.Remove(defineNode.StrCurrent);
                                }
                            }
                            ShowDefines(defineNode.Children);
                        }
                    }
                }
            }
        }

        private void AddAllParentNode(DefineNode defineNode)
        {
            if (!defineNode.Parent.HasValue) return;
            DefineNode parentNode = NodeDict[defineNode.Parent.Value];
            if (!oldDefines.Contains(parentNode.StrCurrent))
            {
                AddAllParentNode(parentNode);
                oldDefines.Add(parentNode.StrCurrent);
            }
        }
        private void AllCurrentNode(DefineNode defineNode)
        {
            if (!oldDefines.Contains(defineNode.StrCurrent))
            {
                oldDefines.Add(defineNode.StrCurrent);
            }
        }
        private void RemoveAllChildren(DefineNode defineNode)
        {
            if (defineNode.Children == null || defineNode.Children.Count == 0) return;
            foreach (var child in defineNode.Children)
            {
                DefineNode childNode = NodeDict[child];
                RemoveAllChildren(childNode);
                if (oldDefines.Contains(childNode.StrCurrent))
                {
                    oldDefines.Remove(childNode.StrCurrent);
                }
            }
        }
    }
    
    public class DefineNode 
    {
        public readonly EMacroDefiner Current;
        public string StrCurrent => Current.ToString();
        public readonly string Description;
            
        public EMacroDefiner? Parent { get; private set; }
        public List<EMacroDefiner> Children { get; private set; }

        public DefineNode(EMacroDefiner current, string description)
        {
            Current = current;
            Description = description;
        }
        public void AddParent(EMacroDefiner parent)
        {
            Parent = parent;
        }
        public void AddChild(EMacroDefiner child)
        {
            Children ??= new List<EMacroDefiner>();
            Children.Add(child);
        }
    }
}