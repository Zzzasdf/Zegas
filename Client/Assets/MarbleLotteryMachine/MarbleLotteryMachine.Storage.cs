using TKLua;
using UnityEngine;
using UnityEngine.UI;

public partial class MarbleLotteryMachine
{
    public int selectedSlot = -1;
    protected Text slotInfoText = null;
    protected Transform pathListContent = null;
    protected CSLList<GameObject> pathEntryObjects = new CSLList<GameObject>();

    private void RefreshPathList()
    {
        int i = 0;
        while (i < this.pathEntryObjects.Count)
        {
            GameObject obj = this.pathEntryObjects[i];
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
            i = i + 1;
        }
        this.pathEntryObjects.Clear();
        bool slotValid = (this.selectedSlot >= 0);
        if (slotValid)
        {
            slotValid = (this.selectedSlot < this.savedPaths.Count);
        }
        if (!slotValid)
        {
            GameObject emptyLabel = new GameObject("EmptyLabel");
            emptyLabel.transform.SetParent(this.pathListContent, false);
            RectTransform rect = emptyLabel.gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(0f, 0f);
            Text text = emptyLabel.gameObject.AddComponent<Text>();
            text.text = "无效槽位";
            text.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            text.fontSize = 14;
            text.font = this.GetFont();
            text.alignment = TextAnchor.MiddleCenter;
            this.pathEntryObjects.Add(emptyLabel);
            return;
        }

        CSLList<SavedPathData> list = this.savedPaths[this.selectedSlot];
        if (list.Count == 0)
        {
            GameObject emptyLabel = new GameObject("EmptyLabel");
            emptyLabel.transform.SetParent(this.pathListContent, false);
            RectTransform rect = emptyLabel.gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(0f, 0f);
            Text text = emptyLabel.gameObject.AddComponent<Text>();
            text.text = "无保存路径";
            text.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            text.fontSize = 14;
            text.font = this.GetFont();
            text.alignment = TextAnchor.MiddleCenter;
            this.pathEntryObjects.Add(emptyLabel);
            return;
        }

        float entryHeight = 30f;
        float spacing = 2f;
        float totalHeight = list.Count * (entryHeight + spacing);
        RectTransform contentRect = this.pathListContent.gameObject.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(0f, totalHeight);

        int idxEntry = 0;
        while (idxEntry < list.Count)
        {
            GameObject entry = new GameObject("PathEntry_" + idxEntry);
            entry.transform.SetParent(this.pathListContent, false);
            RectTransform entryRect = entry.gameObject.AddComponent<RectTransform>();
            entryRect.anchorMin = new Vector2(0f, 1f);
            entryRect.anchorMax = new Vector2(1f, 1f);
            entryRect.pivot = new Vector2(0f, 1f);
            entryRect.sizeDelta = new Vector2(0f, entryHeight);
            entryRect.anchoredPosition = new Vector2(0f, -idxEntry * (entryHeight + spacing));

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(entry.transform, false);
            RectTransform labelRect = labelObj.gameObject.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.35f, 1f);
            labelRect.sizeDelta = new Vector2(0f, 0f);
            Text label = labelObj.gameObject.AddComponent<Text>();
            label.text = "路径 " + (idxEntry + 1);
            label.color = new Color(1f, 1f, 1f, 1f);
            label.fontSize = 12;
            label.font = this.GetFont();
            label.alignment = TextAnchor.MiddleLeft;

            GameObject loadBtnObj = new GameObject("LoadBtn_" + idxEntry);
            loadBtnObj.transform.SetParent(entry.transform, false);
            RectTransform loadRect = loadBtnObj.gameObject.AddComponent<RectTransform>();
            loadRect.anchorMin = new Vector2(0.40f, 0.1f);
            loadRect.anchorMax = new Vector2(0.62f, 0.9f);
            loadRect.sizeDelta = new Vector2(0f, 0f);
            Image loadImg = loadBtnObj.gameObject.AddComponent<Image>();
            loadImg.color = new Color(0.2f, 0.5f, 0.9f, 1f);
            loadImg.raycastTarget = true;
            Button loadBtn = loadBtnObj.gameObject.AddComponent<Button>();
            loadBtn.targetGraphic = loadImg;
            GameObject loadLabelObj = new GameObject("Label");
            loadLabelObj.transform.SetParent(loadBtnObj.transform, false);
            RectTransform loadLabelRect = loadLabelObj.gameObject.AddComponent<RectTransform>();
            loadLabelRect.anchorMin = new Vector2(0f, 0f);
            loadLabelRect.anchorMax = new Vector2(1f, 1f);
            loadLabelRect.sizeDelta = new Vector2(0f, 0f);
            Text loadLabel = loadLabelObj.gameObject.AddComponent<Text>();
            loadLabel.text = "加载";
            loadLabel.color = new Color(1f, 1f, 1f, 1f);
            loadLabel.fontSize = 12;
            loadLabel.font = this.GetFont();
            loadLabel.alignment = TextAnchor.MiddleCenter;
            loadLabel.raycastTarget = false;
            int idx = idxEntry;
            loadBtn.onClick.AddListener(delegate { this.LoadPathAt(idx); });

            GameObject delBtnObj = new GameObject("DelBtn_" + idxEntry);
            delBtnObj.transform.SetParent(entry.transform, false);
            RectTransform delRect = delBtnObj.gameObject.AddComponent<RectTransform>();
            delRect.anchorMin = new Vector2(0.68f, 0.1f);
            delRect.anchorMax = new Vector2(0.98f, 0.9f);
            delRect.sizeDelta = new Vector2(0f, 0f);
            Image delImg = delBtnObj.gameObject.AddComponent<Image>();
            delImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            delImg.raycastTarget = true;
            Button delBtn = delBtnObj.gameObject.AddComponent<Button>();
            delBtn.targetGraphic = delImg;
            GameObject delLabelObj = new GameObject("Label");
            delLabelObj.transform.SetParent(delBtnObj.transform, false);
            RectTransform delLabelRect = delLabelObj.gameObject.AddComponent<RectTransform>();
            delLabelRect.anchorMin = new Vector2(0f, 0f);
            delLabelRect.anchorMax = new Vector2(1f, 1f);
            delLabelRect.sizeDelta = new Vector2(0f, 0f);
            Text delLabel = delLabelObj.gameObject.AddComponent<Text>();
            delLabel.text = "删除";
            delLabel.color = new Color(1f, 1f, 1f, 1f);
            delLabel.fontSize = 12;
            delLabel.font = this.GetFont();
            delLabel.alignment = TextAnchor.MiddleCenter;
            delLabel.raycastTarget = false;
            delBtn.onClick.AddListener(delegate { this.DeletePathAt(idx); });
            this.pathEntryObjects.Add(entry);
            idxEntry = idxEntry + 1;
        }
    }

    private void LoadPathAt(int index)
    {
        bool slotValid = (this.selectedSlot >= 0);
        if (slotValid)
        {
            slotValid = (this.selectedSlot < this.savedPaths.Count);
        }
        if (!slotValid)
        {
            return;
        }
        CSLList<SavedPathData> list = this.savedPaths[this.selectedSlot];
        if (index < 0)
        {
            return;
        }
        if (index >= list.Count)
        {
            return;
        }
        SavedPathData data = list[index];
        this.finalPath = new CSLList<Vector2>(data.finalPath);
        this.trueEndIndex = data.trueEndIndex;
        this.lastCollisionRecords.Clear();
        int i = 0;
        while (i < data.collisionRecords.Count)
        {
            CollisionRecord rec = data.collisionRecords[i];
            this.lastCollisionRecords.Add(new CollisionRecord { progress = rec.progress, obstacleIndex = rec.obstacleIndex });
            i = i + 1;
        }
        int j = 0;
        while (j < this.endBlocks.Count)
        {
            GameObject block = this.endBlocks[j];
            if (block != null)
            {
                Transform border = block.transform.Find("Border");
                if (border != null)
                {
                    Image img = border.gameObject.GetComponent<Image>();
                    if (img != null)
                    {
                        if (j == this.trueEndIndex)
                        {
                            img.color = new Color(this.trueEndColor.r, this.trueEndColor.g, this.trueEndColor.b, this.trueEndColor.a);
                        }
                        else
                        {
                            img.color = new Color(1f, 1f, 1f, 1f);
                        }
                    }
                }
            }
            j = j + 1;
        }
        j = 0;
        while (j < this.targetSelectButtons.Count)
        {
            Button btn = this.targetSelectButtons[j];
            if (btn != null)
            {
                Image img = btn.gameObject.GetComponent<Image>();
                if (img != null)
                {
                    if (j == this.trueEndIndex)
                    {
                        img.color = new Color(0.2f, 0.8f, 0.2f, 1f);
                    }
                    else
                    {
                        img.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                }
            }
            j = j + 1;
        }
        this.DrawPath();
        this.SetInfoText("已加载目标 " + (this.selectedSlot + 1) + " 的第 " + (index + 1) + " 条路径（含 " + this.lastCollisionRecords.Count + " 个碰撞点）");
    }

    private void DeletePathAt(int index)
    {
        bool slotValid = (this.selectedSlot >= 0);
        if (slotValid)
        {
            slotValid = (this.selectedSlot < this.savedPaths.Count);
        }
        if (!slotValid)
        {
            return;
        }
        CSLList<SavedPathData> list = this.savedPaths[this.selectedSlot];
        if (index < 0)
        {
            return;
        }
        if (index >= list.Count)
        {
            return;
        }
        list.RemoveAt(index);
        this.RefreshPathList();
        this.UpdateSlotInfo();
        this.SetInfoText("已删除目标 " + (this.selectedSlot + 1) + " 的第 " + (index + 1) + " 条路径");
        this.SavePathsToBytes();
    }

    private void ClearAllPaths()
    {
        bool slotValid = (this.selectedSlot >= 0);
        if (slotValid)
        {
            slotValid = (this.selectedSlot < this.savedPaths.Count);
        }
        if (!slotValid)
        {
            return;
        }
        CSLList<SavedPathData> list = this.savedPaths[this.selectedSlot];
        if (list.Count == 0)
        {
            this.SetInfoText("目标 " + (this.selectedSlot + 1) + " 下没有路径");
            return;
        }
        list.Clear();
        this.RefreshPathList();
        this.UpdateSlotInfo();
        this.SetInfoText("已清空目标 " + (this.selectedSlot + 1) + " 的所有路径");
        this.SavePathsToBytes();
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0)
        {
            return;
        }
        if (slotIndex >= this.savedPaths.Count)
        {
            return;
        }
        this.selectedSlot = slotIndex;
        int i = 0;
        while (i < this.slotBtns.Count)
        {
            Button btn = this.slotBtns[i];
            if (btn != null)
            {
                Image img = btn.gameObject.GetComponent<Image>();
                if (img != null)
                {
                    if (i == slotIndex)
                    {
                        img.color = new Color(0.5f, 0.8f, 1f, 1f);
                    }
                    else
                    {
                        img.color = new Color(0.3f, 0.3f, 0.5f, 1f);
                    }
                }
            }
            i = i + 1;
        }
        this.UpdateSlotInfo();
        this.RefreshPathList();
    }

    private void UpdateSlotInfo()
    {
        bool slotValid = (this.selectedSlot >= 0);
        if (slotValid)
        {
            slotValid = (this.selectedSlot < this.savedPaths.Count);
        }
        if (!slotValid)
        {
            return;
        }
        CSLList<SavedPathData> list = this.savedPaths[this.selectedSlot];
        if (list.Count == 0)
        {
            this.slotInfoText.text = "目标 " + (this.selectedSlot + 1) + ": 空";
        }
        else
        {
            this.slotInfoText.text = "目标 " + (this.selectedSlot + 1) + ": " + list.Count + " 条路径";
        }
    }

    public void SaveCurrentPath()
    {
        if (this.finalPath == null)
        {
            this.SetInfoText("当前无有效路径可保存");
            return;
        }
        if (this.finalPath.Count < 2)
        {
            this.SetInfoText("当前无有效路径可保存");
            return;
        }
        int targetIdx = this.trueEndIndex;
        bool targetValid = (targetIdx >= 0);
        if (targetValid)
        {
            targetValid = (targetIdx < this.savedPaths.Count);
        }
        if (!targetValid)
        {
            targetIdx = 0;
            this.trueEndIndex = targetIdx;
            this.SelectTrueEnd(targetIdx);
            this.SetInfoText("自动选择目标 " + (targetIdx + 1) + " 进行保存");
        }

        SavedPathData data = new SavedPathData();
        data.finalPath = new CSLList<Vector2>(this.finalPath);
        data.trueEndIndex = targetIdx;
        data.collisionRecords = new CSLList<CollisionRecord>();
        int i = 0;
        while (i < this.lastCollisionRecords.Count)
        {
            CollisionRecord rec = this.lastCollisionRecords[i];
            data.collisionRecords.Add(new CollisionRecord { progress = rec.progress, obstacleIndex = rec.obstacleIndex });
            i = i + 1;
        }
        this.savedPaths[targetIdx].Add(data);
        this.SelectSlot(targetIdx);
        this.SetInfoText("路径已保存到目标 " + (targetIdx + 1) + "，当前共 " + this.savedPaths[targetIdx].Count + " 条");
        this.SavePathsToBytes();
    }
}