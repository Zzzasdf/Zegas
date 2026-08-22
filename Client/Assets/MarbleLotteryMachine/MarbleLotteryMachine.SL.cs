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
    
    protected string textAssetRelativePath = "MarbleLotteryMachine/MarbleLotteryPaths.bytes";
    
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
        // 检查路径字段是否为空
        if (string.IsNullOrEmpty(this.textAssetRelativePath))
        {
            this.SetInfoText("错误：请先在 Inspector 中设置 textAssetRelativePath 路径");
            GLog.Debug("MarbleLotteryMachine::ReplaceTextAsset", "textAssetRelativePath 为空");
            return;
        }

        // 构建完整路径
        string assetPath = Application.dataPath + "/" + this.textAssetRelativePath;
        // 注意：Application.dataPath 返回的是项目根目录下的 Assets 文件夹路径（例如 C:/MyProject/Assets）
        // 因此拼接后得到例如 C:/MyProject/Assets/MarbleLotteryPaths.bytes

        // 检查持久化文件是否存在
        string persistentPath = Application.persistentDataPath + "/MarbleLotteryPaths.bytes";
        if (!System.IO.File.Exists(persistentPath))
        {
            this.SetInfoText("错误：持久化文件不存在，请先保存路径");
            GLog.Debug("MarbleLotteryMachine::ReplaceTextAsset", "持久化文件不存在: " + persistentPath);
            return;
        }

        // 删除旧文件（如果存在）
        if (System.IO.File.Exists(assetPath))
        {
            System.IO.File.Delete(assetPath);
            GLog.Debug("MarbleLotteryMachine::ReplaceTextAsset", "已删除旧文件: " + assetPath);
        }

        // 复制新文件
        System.IO.File.Copy(persistentPath, assetPath);
        GLog.Debug("MarbleLotteryMachine::ReplaceTextAsset", "已复制文件: " + persistentPath + " -> " + assetPath);

        // 由于没有 AssetDatabase.Refresh，需要提示用户手动刷新
        this.SetInfoText("文件已替换，但需要手动刷新资源（重启 Unity 或点击 Assets > Refresh）才能生效");
        GLog.Debug("MarbleLotteryMachine::ReplaceTextAsset", "操作完成，请手动刷新资源");

        // 注意：当前引用的 marbleLotteryPaths 可能仍然指向旧文件，需要重新赋值或重新加载场景
        // 建议重新加载场景或重启 Unity 来让 TextAsset 引用生效
    }
}