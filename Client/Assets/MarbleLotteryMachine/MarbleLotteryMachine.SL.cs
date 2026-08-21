using System;
using System.Collections.Generic;
using TKLua;
using UnityEngine;

public partial class MarbleLotteryMachine
{
    [Serializable]
    [CSLStub.Out("Activity/Marble")]
    public class TClientMarblePosition
    {
        public int x = 0;
        public int y = 0;
    }
    
    [Serializable]
    [CSLStub.Out("Activity/Marble")]
    public class TClientMarbleCollisionRecord
    {
        public int iProgress = 0;
        public int iObstacleIndex = 0;
    }

    [Serializable]
    [CSLStub.Out("Activity/Marble")]
    public class TClientMarblePathData
    {
        public List<TClientMarblePosition> vecFinalPath = new List<TClientMarblePosition>();
        public int iTrueEndIndex = -1;
        public List<TClientMarbleCollisionRecord> vecCollisionRecords = new List<TClientMarbleCollisionRecord>();
    }

    [Serializable]
    [CSLStub.Out("Activity/Marble")]
    public class TClientMarbleSlotData
    {
        public List<TClientMarblePathData> vecPaths = new List<TClientMarblePathData>();
    }

    [Serializable]
    [CSLStub.Out("Activity/Marble")]
    public class TClientMarbleAllPathsData
    {
        public List<TClientMarbleSlotData> vecSlots = new List<TClientMarbleSlotData>();
    }
    
    private void LoadPathsFromBytes()
    {
        if (this.marbleLotteryPaths == null)
        {
            GLog.Debug("MarbleLotteryMachine::LoadPathsFromJson", "marbleLotteryPaths 未赋值");
            return;
        }
        string json = this.marbleLotteryPaths.text;
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        try
        {
            TClientMarbleAllPathsData data = JsonUtility.FromJson<TClientMarbleAllPathsData>(json);
            if (data == null)
            {
                return;
            }
            if (data.vecSlots == null)
            {
                return;
            }

            InitSavedPaths();

            int slotCount = Math.Min(data.vecSlots.Count, this.savedPaths.Count);
            int slot = 0;
            while (slot < slotCount)
            {
                this.savedPaths[slot].Clear();
                if (data.vecSlots[slot] == null)
                {
                    slot = slot + 1;
                    continue;
                }
                if (data.vecSlots[slot].vecPaths == null)
                {
                    slot = slot + 1;
                    continue;
                }
                int p = 0;
                while (p < data.vecSlots[slot].vecPaths.Count)
                {
                    TClientMarblePathData pathData = data.vecSlots[slot].vecPaths[p];
                    SavedPathData spd = new SavedPathData();
                    spd.trueEndIndex = pathData.iTrueEndIndex;
                    spd.finalPath = new CSLList<Vector2>();
                    int i = 0;
                    while (i < pathData.vecFinalPath.Count)
                    {
                        TClientMarblePosition pos = pathData.vecFinalPath[i];
                        spd.finalPath.Add(new Vector2(pos.x, pos.y));
                        i = i + 1;
                    }
                    spd.collisionRecords = new CSLList<CollisionRecord>();
                    if (pathData.vecCollisionRecords != null)
                    {
                        int r = 0;
                        while (r < pathData.vecCollisionRecords.Count)
                        {
                            TClientMarbleCollisionRecord rec = pathData.vecCollisionRecords[r];
                            spd.collisionRecords.Add(new CollisionRecord { progress = rec.iProgress, obstacleIndex = rec.iObstacleIndex });
                            r = r + 1;
                        }
                    }
                    this.savedPaths[slot].Add(spd);
                    p = p + 1;
                }
                slot = slot + 1;
            }
            this.RefreshPathList();
            this.UpdateSlotInfo();
            GLog.Debug("MarbleLotteryMachine::LoadPathsFromJson", "路径加载成功");
        }
        catch (Exception e)
        {
            GLog.Debug("MarbleLotteryMachine::LoadPathsFromJson", "加载路径 JSON 失败: " + e.Message);
        }
    }

    private void SavePathsToBytes()
    {
        try
        {
            TClientMarbleAllPathsData data = new TClientMarbleAllPathsData();
            data.vecSlots.Clear();

            int slotCount = this.savedPaths.Count;
            int slot = 0;
            while (slot < slotCount)
            {
                TClientMarbleSlotData slotData = new TClientMarbleSlotData();
                slotData.vecPaths.Clear();

                CSLList<SavedPathData> list = this.savedPaths[slot];
                int p = 0;
                while (p < list.Count)
                {
                    SavedPathData src = list[p];
                    TClientMarblePathData dest = new TClientMarblePathData();
                    dest.iTrueEndIndex = src.trueEndIndex;
                    dest.vecFinalPath.Clear();
                    int j = 0;
                    while (j < src.finalPath.Count)
                    {
                        Vector2 v = src.finalPath[j];
                        dest.vecFinalPath.Add(new TClientMarblePosition { x = (int)v.x, y = (int)v.y });
                        j = j + 1;
                    }
                    dest.vecCollisionRecords.Clear();
                    int r = 0;
                    while (r < src.collisionRecords.Count)
                    {
                        CollisionRecord rec = src.collisionRecords[r];
                        dest.vecCollisionRecords.Add(new TClientMarbleCollisionRecord { iProgress = rec.progress, iObstacleIndex = rec.obstacleIndex });
                        r = r + 1;
                    }
                    slotData.vecPaths.Add(dest);
                    p = p + 1;
                }
                data.vecSlots.Add(slotData);
                slot = slot + 1;
            }

            string json = JsonUtility.ToJson(data, true);
            string path = Application.persistentDataPath + "/MarbleLotteryPaths.bytes";
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            System.IO.File.WriteAllText(path, json);
            GLog.Debug("MarbleLotteryMachine::SavePathsToJson", "路径已保存到: " + path);
            this.SetInfoText("✅ 保存成功：" + path);
        }
        catch (Exception e)
        {
            GLog.Debug("MarbleLotteryMachine::SavePathsToJson", "保存路径失败: " + e.Message);
            this.SetInfoText("❌ 保存失败：" + e.Message);
        }
    }
    
    
    public void ReplaceTextAsset()
    {
#if UNITY_EDITOR
        if (this.marbleLotteryPaths == null)
        {
            this.SetInfoText("错误：marbleLotteryPaths 未赋值");
            return;
        }
        string persistentPath = Application.persistentDataPath + "/MarbleLotteryPaths.bytes";
        bool fileExists = System.IO.File.Exists(persistentPath);
        if (!fileExists)
        {
            this.SetInfoText("持久化文件不存在，请先保存路径");
            return;
        }
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this.marbleLotteryPaths);
        if (string.IsNullOrEmpty(assetPath))
        {
            this.SetInfoText("无法获取 TextAsset 的路径");
            return;
        }
        string json = System.IO.File.ReadAllText(persistentPath);
        System.IO.File.WriteAllText(assetPath, json);
        UnityEditor.AssetDatabase.Refresh();
        this.marbleLotteryPaths = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
        this.LoadPathsFromBytes();
        this.SetInfoText("已成功替换 TextAsset 并重新加载");
        GLog.Debug("MarbleLotteryMachine::ReplaceTextAsset", "Replaced TextAsset at: " + assetPath);
#else
        this.SetInfoText("此功能仅在 Unity Editor 中可用");
#endif
    }
}