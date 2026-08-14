using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class OpenFolder : Editor
{
    [MenuItem("Tools/Open Persistent Folder")]
    static void Open()
    {
        string path = Application.persistentDataPath;
        if (!System.IO.Directory.Exists(path))
            System.IO.Directory.CreateDirectory(path);
        EditorUtility.RevealInFinder(path);
        Debug.Log("已打开: " + path);
    }
}
