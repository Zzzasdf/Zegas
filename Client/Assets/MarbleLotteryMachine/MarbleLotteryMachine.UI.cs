using UnityEngine;
using UnityEngine.UI;
using TKLua;

public partial class MarbleLotteryMachine
{
    void CreateUI()
    {
        generatePathBtn = CreateButton(transform, "GenPathBtn", new Vector2(0.02f, 0.82f), new Vector2(0.12f, 0.90f), "生成路径", new Color(0f, 0.5f, 0.5f));
        launchBtn = CreateButton(transform, "LaunchBtn", new Vector2(0.02f, 0.72f), new Vector2(0.12f, 0.80f), "发射", new Color(0.2f, 0.6f, 0.2f));
        resetBtn = CreateButton(transform, "ResetBtn", new Vector2(0.02f, 0.62f), new Vector2(0.12f, 0.70f), "重置", new Color(0.5f, 0.2f, 0.2f));
        regenerateBtn = CreateButton(transform, "RegenerateBtn", new Vector2(0.02f, 0.52f), new Vector2(0.12f, 0.60f), "重生", new Color(0.1f, 0.4f, 0.6f));
        directLaunchBtn = CreateButton(transform, "DirectLaunchBtn", new Vector2(0.02f, 0.42f), new Vector2(0.12f, 0.50f), "直接发射", new Color(0.6f, 0.2f, 0.6f));

        GameObject infoObj = new GameObject("InfoText");
        infoObj.transform.SetParent(transform, false);
        infoObj.AddComponent<RectTransform>();
        Text infoTextComp = infoObj.AddComponent<Text>();
        RectTransform infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.02f, 0.02f);
        infoRect.anchorMax = new Vector2(0.80f, 0.08f);
        infoRect.sizeDelta = Vector2.zero;
        infoRect.anchoredPosition = Vector2.zero;
        infoText = infoTextComp;
        infoText.text = "点击生成路径，然后发射";
        infoText.color = Color.white;
        infoText.fontSize = 16;
        infoText.font = GetFont();
        infoText.alignment = TextAnchor.MiddleLeft;
        infoText.raycastTarget = false;
        infoText.horizontalOverflow = HorizontalWrapMode.Wrap;

        float rightPanelLeft = 0.83f;
        float rightPanelRight = 0.98f;

        GameObject panelBg = new GameObject("SavePanelBg");
        panelBg.transform.SetParent(transform, false);
        RectTransform bgRect = panelBg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(rightPanelLeft, 0.52f);
        bgRect.anchorMax = new Vector2(rightPanelRight, 0.96f);
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        Image bgImg = panelBg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.2f, 0.6f);
        bgImg.raycastTarget = false;

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelBg.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.93f);
        titleRect.anchorMax = new Vector2(1, 0.98f);
        titleRect.sizeDelta = Vector2.zero;
        titleRect.anchoredPosition = Vector2.zero;
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "路径槽";
        titleText.color = Color.white;
        titleText.fontSize = 16;
        titleText.font = GetFont();
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.raycastTarget = false;

        int slotCount = 8, cols = 4;
        float tabHeight = 0.04f, tabSpacingX = 0.01f, tabSpacingY = 0.008f;
        float tabWidth = (1f - (cols - 1) * tabSpacingX - 0.04f) / cols;
        float startY = 0.88f;
        for (int i = 0; i < slotCount; i++)
        {
            int row = i / cols;
            int col = i % cols;
            float x = 0.02f + col * (tabWidth + tabSpacingX);
            float y = startY - row * (tabHeight + tabSpacingY);
            Button btn = CreateButton(panelBg.transform, "SlotBtn_" + i, new Vector2(x, y - tabHeight), new Vector2(x + tabWidth, y), "目标 " + (i + 1), new Color(0.3f, 0.3f, 0.5f));
            Text btnLabel = btn.GetComponentInChildren<Text>();
            if (btnLabel != null) btnLabel.fontSize = 13;
            slotBtns[i] = btn;
            int idx = i;
            btn.onClick.AddListener(() => SelectSlot(idx));
        }

        float infoY = startY - 2 * (tabHeight + tabSpacingY) - 0.02f;
        GameObject slotInfoObj = new GameObject("SlotInfoText");
        slotInfoObj.transform.SetParent(panelBg.transform, false);
        RectTransform slotInfoRect = slotInfoObj.AddComponent<RectTransform>();
        slotInfoRect.anchorMin = new Vector2(0.05f, infoY - 0.02f);
        slotInfoRect.anchorMax = new Vector2(0.95f, infoY + 0.02f);
        slotInfoText = slotInfoObj.AddComponent<Text>();
        slotInfoText.text = "目标 1: 空";
        slotInfoText.color = Color.white;
        slotInfoText.fontSize = 14;
        slotInfoText.font = GetFont();
        slotInfoText.alignment = TextAnchor.MiddleCenter;
        slotInfoText.raycastTarget = false;

        float listY = infoY - 0.04f;
        float btnAreaHeight = 0.15f;
        float listBottom = btnAreaHeight + 0.02f;
        float listHeight = listY - listBottom;
        GameObject listScrollObj = new GameObject("PathListScroll");
        listScrollObj.transform.SetParent(panelBg.transform, false);
        RectTransform listScrollRect = listScrollObj.AddComponent<RectTransform>();
        listScrollRect.anchorMin = new Vector2(0.02f, listBottom);
        listScrollRect.anchorMax = new Vector2(0.98f, listY);
        listScrollRect.sizeDelta = Vector2.zero;
        Image listBg = listScrollObj.AddComponent<Image>();
        listBg.color = new Color(0.2f, 0.2f, 0.3f, 0.5f);
        ScrollRect scrollRect = listScrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(listScrollObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        viewportObj.AddComponent<RectMask2D>();

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        contentRect.anchoredPosition = Vector2.zero;
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        pathListContent = contentRect;

        float rowHeight = 0.06f;
        float spacing = 0.01f;
        float opWidth = (1f - 2 * 0.02f - spacing) / 2f;
        float opStartX = 0.02f;

        savePersistentBtn = CreateButton(panelBg.transform, "SavePersistentBtn",
            new Vector2(opStartX, 0f),
            new Vector2(opStartX + opWidth, rowHeight),
            "保存到持久化", new Color(0.2f, 0.5f, 0.8f));
        Button replaceBtn = CreateButton(panelBg.transform, "ReplaceBtn",
            new Vector2(opStartX + opWidth + spacing, 0f),
            new Vector2(opStartX + 2 * opWidth + spacing, rowHeight),
            "替换 TextAsset", new Color(0.4f, 0.4f, 0.8f));
        replaceBtn.onClick.AddListener(ReplaceTextAsset);

        float row2Bottom = rowHeight + spacing;
        float row2Top = row2Bottom + rowHeight;
        saveBtn = CreateButton(panelBg.transform, "SaveBtn",
            new Vector2(opStartX, row2Bottom),
            new Vector2(opStartX + opWidth, row2Top),
            "保存当前", new Color(0.2f, 0.7f, 0.2f));
        Button clearAllBtn = CreateButton(panelBg.transform, "ClearAllBtn",
            new Vector2(opStartX + opWidth + spacing, row2Bottom),
            new Vector2(opStartX + 2 * opWidth + spacing, row2Top),
            "清空所有", new Color(0.8f, 0.2f, 0.2f));
        clearAllBtn.onClick.AddListener(ClearAllPaths);

        foreach (var btn in new[] { savePersistentBtn, replaceBtn, saveBtn, clearAllBtn })
        {
            Text lbl = btn.GetComponentInChildren<Text>();
            if (lbl != null) lbl.fontSize = 14;
        }

        // RunTestPanel
        GameObject runPanel = new GameObject("RunTestPanel");
        runPanel.transform.SetParent(transform, false);
        RectTransform runRect = runPanel.AddComponent<RectTransform>();
        runRect.anchorMin = new Vector2(rightPanelLeft, 0.02f);
        runRect.anchorMax = new Vector2(rightPanelRight, 0.48f);
        runRect.sizeDelta = Vector2.zero;
        runRect.anchoredPosition = Vector2.zero;
        Image runBg = runPanel.AddComponent<Image>();
        runBg.color = new Color(0.2f, 0.3f, 0.2f, 0.6f);
        runBg.raycastTarget = false;

        GameObject runTitleObj = new GameObject("RunTitle");
        runTitleObj.transform.SetParent(runPanel.transform, false);
        RectTransform runTitleRect = runTitleObj.AddComponent<RectTransform>();
        runTitleRect.anchorMin = new Vector2(0, 0.85f);
        runTitleRect.anchorMax = new Vector2(1, 1);
        runTitleRect.sizeDelta = Vector2.zero;
        Text runTitleText = runTitleObj.AddComponent<Text>();
        runTitleText.text = "Run 测试";
        runTitleText.color = Color.white;
        runTitleText.fontSize = 14;
        runTitleText.font = GetFont();
        runTitleText.alignment = TextAnchor.MiddleCenter;
        runTitleText.raycastTarget = false;

        int[][] presets = new int[][] {
            new int[]{0,1,2,3},
            new int[]{4,5,6,7},
            new int[]{0,2,4,6},
            new int[]{1,3,5,7},
            new int[]{0,1,2,3,4,5,6,7}
        };
        string[] presetLabels = { "0-3", "4-7", "偶", "奇", "全部" };
        float btnH = 0.06f, btnW = 0.18f, spacing2 = 0.01f;
        float rowY = 0.78f;
        for (int i = 0; i < presets.Length; i++)
        {
            float x = 0.02f + i * (btnW + spacing2);
            if (x + btnW > 1f) break;
            Button btn = CreateButton(runPanel.transform, "PresetBtn_" + i,
                new Vector2(x, rowY - btnH),
                new Vector2(x + btnW, rowY),
                presetLabels[i],
                new Color(0.3f, 0.3f, 0.6f));
            int[] targets = presets[i];
            btn.onClick.AddListener(() => OnRunTest(targets));
            Text lbl = btn.GetComponentInChildren<Text>();
            if (lbl != null) lbl.fontSize = 12;
        }

        float actionHeight = 0.10f;
        Button runTestBtn = CreateButton(runPanel.transform, "RunTestBtn",
            new Vector2(0.02f, 0f),
            new Vector2(0.48f, actionHeight),
            "启动 Run",
            new Color(0.0f, 0.6f, 0.0f));
        runTestBtn.onClick.AddListener(() => OnRunTest(new int[]{0,1,2,3,4,5,6,7}));
        Text runBtnLabel = runTestBtn.GetComponentInChildren<Text>();
        if (runBtnLabel != null) runBtnLabel.fontSize = 14;

        GameObject statusObj = new GameObject("RunStatus");
        statusObj.transform.SetParent(runPanel.transform, false);
        RectTransform statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.55f, 0f);
        statusRect.anchorMax = new Vector2(0.98f, actionHeight);
        statusRect.sizeDelta = Vector2.zero;
        runStatusText = statusObj.AddComponent<Text>();
        runStatusText.text = "就绪";
        runStatusText.color = Color.white;
        runStatusText.fontSize = 14;
        runStatusText.font = GetFont();
        runStatusText.alignment = TextAnchor.MiddleCenter;
        runStatusText.raycastTarget = false;

        Canvas.ForceUpdateCanvases();
    }

    Button CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string text, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = Vector2.zero;
        Image bg = btnObj.AddComponent<Image>();
        bg.color = bgColor;
        bg.raycastTarget = true;
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = bg;
        btn.interactable = true;
        GameObject label = new GameObject("Label");
        label.transform.SetParent(btnObj.transform, false);
        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        Text txt = label.AddComponent<Text>();
        txt.text = text;
        txt.color = Color.white;
        txt.fontSize = 20;
        txt.font = GetFont();
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;
        return btn;
    }

    Font GetFont() => Font.CreateDynamicFontFromOSFont("Arial", 14);

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotBtns.Length) return;
        selectedSlot = slotIndex;
        for (int i = 0; i < slotBtns.Length; i++)
            if (slotBtns[i] != null) { Image img = slotBtns[i].GetComponent<Image>(); if (img != null) img.color = (i == slotIndex) ? new Color(0.5f, 0.8f, 1f) : new Color(0.3f, 0.3f, 0.5f); }
        UpdateSlotInfo();
        RefreshPathList();
    }

    private void UpdateSlotInfo()
    {
        if (selectedSlot < 0 || selectedSlot >= savedPaths.Length) return;
        var list = savedPaths[selectedSlot];
        slotInfoText.text = "目标 " + (selectedSlot + 1) + ": " + (list.Count == 0 ? "空" : list.Count + " 条路径");
    }

    public void SaveCurrentPath()
    {
        if (finalPath == null || finalPath.Count < 2) { SetInfoText("当前无有效路径可保存"); return; }
        int targetIdx = trueEndIndex;
        if (targetIdx < 0 || targetIdx >= savedPaths.Length) { SetInfoText("路径目标无效，无法保存"); return; }
        SavedPathData data = new SavedPathData();
        data.finalPath = new CSLList<Vector2>(finalPath);
        data.trueEndIndex = targetIdx;
        data.collisionRecords = new CSLList<CollisionRecord>();
        foreach (var rec in lastCollisionRecords) data.collisionRecords.Add(new CollisionRecord { position = rec.position, obstacleIndex = rec.obstacleIndex });
        savedPaths[targetIdx].Add(data);
        SelectSlot(targetIdx);
        SetInfoText("路径已保存到目标 " + (targetIdx + 1) + "，当前共 " + savedPaths[targetIdx].Count + " 条");
    }

    private void RefreshPathList()
    {
        foreach (var obj in pathEntryObjects) if (obj != null) GameObject.Destroy(obj);
        pathEntryObjects.Clear();
        if (selectedSlot < 0 || selectedSlot >= savedPaths.Length) return;
        var list = savedPaths[selectedSlot];
        if (list.Count == 0)
        {
            GameObject emptyLabel = new GameObject("EmptyLabel");
            emptyLabel.transform.SetParent(pathListContent, false);
            RectTransform rect = emptyLabel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            Text text = emptyLabel.AddComponent<Text>();
            text.text = "无保存路径";
            text.color = Color.gray;
            text.fontSize = 14;
            text.font = GetFont();
            text.alignment = TextAnchor.MiddleCenter;
            pathEntryObjects.Add(emptyLabel);
            return;
        }
        float entryHeight = 30f, spacing = 2f;
        float totalHeight = list.Count * (entryHeight + spacing);
        RectTransform contentRect = pathListContent.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(0, totalHeight);
        for (int i = 0; i < list.Count; i++)
        {
            GameObject entry = new GameObject("PathEntry_" + i);
            entry.transform.SetParent(pathListContent, false);
            RectTransform entryRect = entry.AddComponent<RectTransform>();
            entryRect.anchorMin = new Vector2(0, 1);
            entryRect.anchorMax = new Vector2(1, 1);
            entryRect.pivot = new Vector2(0, 1);
            entryRect.sizeDelta = new Vector2(0, entryHeight);
            entryRect.anchoredPosition = new Vector2(0, -i * (entryHeight + spacing));

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(entry.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.35f, 1);
            labelRect.sizeDelta = Vector2.zero;
            Text label = labelObj.AddComponent<Text>();
            label.text = "路径 " + (i + 1);
            label.color = Color.white;
            label.fontSize = 12;
            label.font = GetFont();
            label.alignment = TextAnchor.MiddleLeft;

            GameObject loadBtnObj = new GameObject("LoadBtn_" + i);
            loadBtnObj.transform.SetParent(entry.transform, false);
            RectTransform loadRect = loadBtnObj.AddComponent<RectTransform>();
            loadRect.anchorMin = new Vector2(0.40f, 0.1f);
            loadRect.anchorMax = new Vector2(0.62f, 0.9f);
            loadRect.sizeDelta = Vector2.zero;
            Image loadImg = loadBtnObj.AddComponent<Image>();
            loadImg.color = new Color(0.2f, 0.5f, 0.9f);
            loadImg.raycastTarget = true;
            Button loadBtn = loadBtnObj.AddComponent<Button>();
            loadBtn.targetGraphic = loadImg;
            GameObject loadLabelObj = new GameObject("Label");
            loadLabelObj.transform.SetParent(loadBtnObj.transform, false);
            RectTransform loadLabelRect = loadLabelObj.AddComponent<RectTransform>();
            loadLabelRect.anchorMin = Vector2.zero;
            loadLabelRect.anchorMax = Vector2.one;
            loadLabelRect.sizeDelta = Vector2.zero;
            Text loadLabel = loadLabelObj.AddComponent<Text>();
            loadLabel.text = "加载";
            loadLabel.color = Color.white;
            loadLabel.fontSize = 12;
            loadLabel.font = GetFont();
            loadLabel.alignment = TextAnchor.MiddleCenter;
            loadLabel.raycastTarget = false;
            int idx = i;
            loadBtn.onClick.AddListener(() => LoadPathAt(idx));

            GameObject delBtnObj = new GameObject("DelBtn_" + i);
            delBtnObj.transform.SetParent(entry.transform, false);
            RectTransform delRect = delBtnObj.AddComponent<RectTransform>();
            delRect.anchorMin = new Vector2(0.68f, 0.1f);
            delRect.anchorMax = new Vector2(0.98f, 0.9f);
            delRect.sizeDelta = Vector2.zero;
            Image delImg = delBtnObj.AddComponent<Image>();
            delImg.color = new Color(0.8f, 0.2f, 0.2f);
            delImg.raycastTarget = true;
            Button delBtn = delBtnObj.AddComponent<Button>();
            delBtn.targetGraphic = delImg;
            GameObject delLabelObj = new GameObject("Label");
            delLabelObj.transform.SetParent(delBtnObj.transform, false);
            RectTransform delLabelRect = delLabelObj.AddComponent<RectTransform>();
            delLabelRect.anchorMin = Vector2.zero;
            delLabelRect.anchorMax = Vector2.one;
            delLabelRect.sizeDelta = Vector2.zero;
            Text delLabel = delLabelObj.AddComponent<Text>();
            delLabel.text = "删除";
            delLabel.color = Color.white;
            delLabel.fontSize = 12;
            delLabel.font = GetFont();
            delLabel.alignment = TextAnchor.MiddleCenter;
            delLabel.raycastTarget = false;
            delBtn.onClick.AddListener(() => DeletePathAt(idx));
            pathEntryObjects.Add(entry);
        }
    }

    private void LoadPathAt(int index)
    {
        if (selectedSlot < 0 || selectedSlot >= savedPaths.Length) return;
        var list = savedPaths[selectedSlot];
        if (index < 0 || index >= list.Count) return;
        var data = list[index];
        finalPath = new CSLList<Vector2>(data.finalPath);
        trueEndIndex = data.trueEndIndex;
        lastCollisionRecords.Clear();
        foreach (var rec in data.collisionRecords) lastCollisionRecords.Add(new CollisionRecord { position = rec.position, obstacleIndex = rec.obstacleIndex });
        for (int i = 0; i < endBlocks.Count; i++)
        {
            GameObject block = endBlocks[i];
            Transform border = block.transform.Find("Border");
            if (border != null)
            {
                Image img = border.GetComponent<Image>();
                if (img != null) img.color = (i == trueEndIndex) ? trueEndColor : Color.white;
            }
        }
        for (int i = 0; i < targetSelectButtons.Count; i++)
        {
            Button btn = targetSelectButtons[i];
            if (btn != null)
            {
                Image img = btn.GetComponent<Image>();
                if (img != null) img.color = (i == trueEndIndex) ? new Color(0.2f, 0.8f, 0.2f) : Color.gray;
            }
        }
        DrawPath();
        DrawCollisionMarkers(lastCollisionPoints);
        SetInfoText("已加载目标 " + (selectedSlot + 1) + " 的第 " + (index + 1) + " 条路径（含 " + lastCollisionRecords.Count + " 个碰撞点）");
    }

    private void DeletePathAt(int index)
    {
        if (selectedSlot < 0 || selectedSlot >= savedPaths.Length) return;
        var list = savedPaths[selectedSlot];
        if (index < 0 || index >= list.Count) return;
        list.RemoveAt(index);
        RefreshPathList();
        UpdateSlotInfo();
        SetInfoText("已删除目标 " + (selectedSlot + 1) + " 的第 " + (index + 1) + " 条路径");
        SavePathsToJson();
    }

    private void ClearAllPaths()
    {
        if (selectedSlot < 0 || selectedSlot >= savedPaths.Length) return;
        var list = savedPaths[selectedSlot];
        if (list.Count == 0) { SetInfoText("目标 " + (selectedSlot + 1) + " 下没有路径"); return; }
        list.Clear();
        RefreshPathList();
        UpdateSlotInfo();
        SetInfoText("已清空目标 " + (selectedSlot + 1) + " 的所有路径");
        SavePathsToJson();
    }

    private void ClearCollisionMarkers()
    {
        foreach (var marker in collisionMarkers) if (marker != null) GameObject.Destroy(marker);
        collisionMarkers.Clear();
        lastCollisionPoints.Clear();
    }

    private void OnRunTest(int[] targetArray)
    {
        if (IsRunning)
        {
            SetInfoText("机器正在运行，请等待完成");
            return;
        }
        CSLList<int> targets = new CSLList<int>();
        foreach (int t in targetArray) targets.Add(t);
        Run(targets, () => {
            Debug.Log("Run 测试完成！");
            SetInfoText("Run 测试全部完成！");
            if (runStatusText != null)
            {
                runStatusText.text = "完成";
                runStatusText.color = Color.green;
            }
        });
        if (runStatusText != null)
        {
            runStatusText.text = "运行中...";
            runStatusText.color = Color.yellow;
        }
        SetInfoText($"启动 Run，目标数 {targets.Count}");
    }
}