using UnityEngine;
using UnityEngine.UI;
using TKLua;
using LuaFramework;
using System.Collections.Generic;

public partial class MarbleLotteryMachine
{
    protected CSLList<Button> slotBtns = new CSLList<Button>();

    private void CreateUI()
    {
        this.generatePathBtn = this.CreateButton(this.transform, "GenPathBtn", new Vector2(0.02f, 0.82f), new Vector2(0.12f, 0.90f), "生成路径", new Color(0f, 0.5f, 0.5f, 1f));
        this.launchBtn = this.CreateButton(this.transform, "LaunchBtn", new Vector2(0.02f, 0.72f), new Vector2(0.12f, 0.80f), "发射", new Color(0.2f, 0.6f, 0.2f, 1f));
        this.resetBtn = this.CreateButton(this.transform, "ResetBtn", new Vector2(0.02f, 0.62f), new Vector2(0.12f, 0.70f), "重置", new Color(0.5f, 0.2f, 0.2f, 1f));
        this.regenerateBtn = this.CreateButton(this.transform, "RegenerateBtn", new Vector2(0.02f, 0.52f), new Vector2(0.12f, 0.60f), "重生", new Color(0.1f, 0.4f, 0.6f, 1f));

        GameObject infoObj = new GameObject("InfoText");
        infoObj.transform.SetParent(this.transform, false);
        RectTransform infoRect = infoObj.gameObject.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.02f, 0.02f);
        infoRect.anchorMax = new Vector2(0.80f, 0.08f);
        infoRect.sizeDelta = new Vector2(0f, 0f);
        infoRect.anchoredPosition = new Vector2(0f, 0f);
        Text infoTextComp = infoObj.gameObject.AddComponent<Text>();
        this.infoText = infoTextComp;
        this.infoText.text = "点击生成路径，然后发射";
        this.infoText.color = new Color(1f, 1f, 1f, 1f);
        this.infoText.fontSize = 16;
        this.infoText.font = this.GetFont();
        this.infoText.alignment = TextAnchor.MiddleLeft;
        this.infoText.raycastTarget = false;
        this.infoText.horizontalOverflow = HorizontalWrapMode.Wrap;

        float rightPanelLeft = 0.83f;
        float rightPanelRight = 0.98f;

        GameObject panelBg = new GameObject("SavePanelBg");
        panelBg.transform.SetParent(this.transform, false);
        RectTransform bgRect = panelBg.gameObject.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(rightPanelLeft, 0.52f);
        bgRect.anchorMax = new Vector2(rightPanelRight, 0.96f);
        bgRect.sizeDelta = new Vector2(0f, 0f);
        bgRect.anchoredPosition = new Vector2(0f, 0f);
        Image bgImg = panelBg.gameObject.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.2f, 0.6f);
        bgImg.raycastTarget = false;

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelBg.transform, false);
        RectTransform titleRect = titleObj.gameObject.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.93f);
        titleRect.anchorMax = new Vector2(1f, 0.98f);
        titleRect.sizeDelta = new Vector2(0f, 0f);
        titleRect.anchoredPosition = new Vector2(0f, 0f);
        Text titleText = titleObj.gameObject.AddComponent<Text>();
        titleText.text = "路径槽";
        titleText.color = new Color(1f, 1f, 1f, 1f);
        titleText.fontSize = 16;
        titleText.font = this.GetFont();
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.raycastTarget = false;

        int slotCount = Mathf.Max(1, this.blockCount);
        int cols = 4;
        float tabHeight = 0.04f;
        float tabSpacingX = 0.01f;
        float tabSpacingY = 0.008f;
        float tabWidth = (1f - (cols - 1) * tabSpacingX - 0.04f) / cols;
        float startY = 0.88f;

        this.slotBtns.Clear();
        int i = 0;
        while (i < slotCount)
        {
            int row = i / cols;
            int col = i % cols;
            float x = 0.02f + col * (tabWidth + tabSpacingX);
            float y = startY - row * (tabHeight + tabSpacingY);
            Button btn = this.CreateButton(panelBg.transform, "SlotBtn_" + i, new Vector2(x, y - tabHeight), new Vector2(x + tabWidth, y), "目标 " + (i + 1), new Color(0.3f, 0.3f, 0.5f, 1f));
            Text btnLabel = btn.gameObject.GetComponentInChildren<Text>();
            if (btnLabel != null)
            {
                btnLabel.fontSize = 13;
            }
            this.slotBtns.Add(btn);
            int idx = i;
            btn.onClick.AddListener(delegate { this.SelectSlot(idx); });
            i = i + 1;
        }

        float infoY = startY - 2f * (tabHeight + tabSpacingY) - 0.02f;
        GameObject slotInfoObj = new GameObject("SlotInfoText");
        slotInfoObj.transform.SetParent(panelBg.transform, false);
        RectTransform slotInfoRect = slotInfoObj.gameObject.AddComponent<RectTransform>();
        slotInfoRect.anchorMin = new Vector2(0.05f, infoY - 0.02f);
        slotInfoRect.anchorMax = new Vector2(0.95f, infoY + 0.02f);
        this.slotInfoText = slotInfoObj.gameObject.AddComponent<Text>();
        this.slotInfoText.text = "目标 1: 空";
        this.slotInfoText.color = new Color(1f, 1f, 1f, 1f);
        this.slotInfoText.fontSize = 14;
        this.slotInfoText.font = this.GetFont();
        this.slotInfoText.alignment = TextAnchor.MiddleCenter;
        this.slotInfoText.raycastTarget = false;

        float listY = infoY - 0.04f;
        float btnAreaHeight = 0.15f;
        float listBottom = btnAreaHeight + 0.02f;
        GameObject listScrollObj = new GameObject("PathListScroll");
        listScrollObj.transform.SetParent(panelBg.transform, false);
        RectTransform listScrollRect = listScrollObj.gameObject.AddComponent<RectTransform>();
        listScrollRect.anchorMin = new Vector2(0.02f, listBottom);
        listScrollRect.anchorMax = new Vector2(0.98f, listY);
        listScrollRect.sizeDelta = new Vector2(0f, 0f);
        Image listBg = listScrollObj.gameObject.AddComponent<Image>();
        listBg.color = new Color(0.2f, 0.2f, 0.3f, 0.5f);
        ScrollRect scrollRect = listScrollObj.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(listScrollObj.transform, false);
        RectTransform viewportRect = viewportObj.gameObject.AddComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0f, 0f);
        viewportRect.anchorMax = new Vector2(1f, 1f);
        viewportRect.sizeDelta = new Vector2(0f, 0f);
        viewportRect.anchoredPosition = new Vector2(0f, 0f);
        viewportObj.gameObject.AddComponent<RectMask2D>();

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.gameObject.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 0f);
        contentRect.anchoredPosition = new Vector2(0f, 0f);
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        this.pathListContent = contentRect;

        float rowHeight = 0.06f;
        float spacing = 0.01f;
        float opWidth = (1f - 2f * 0.02f - spacing) / 2f;
        float opStartX = 0.02f;

        this.savePersistentBtn = this.CreateButton(panelBg.transform, "SavePersistentBtn",
            new Vector2(opStartX, 0f),
            new Vector2(opStartX + opWidth, rowHeight),
            "保存到持久化", new Color(0.2f, 0.5f, 0.8f, 1f));
        Button replaceBtn = this.CreateButton(panelBg.transform, "ReplaceBtn",
            new Vector2(opStartX + opWidth + spacing, 0f),
            new Vector2(opStartX + 2f * opWidth + spacing, rowHeight),
            "替换 TextAsset", new Color(0.4f, 0.4f, 0.8f, 1f));
        replaceBtn.onClick.AddListener(this.ReplaceTextAsset);

        float row2Bottom = rowHeight + spacing;
        float row2Top = row2Bottom + rowHeight;
        this.saveBtn = this.CreateButton(panelBg.transform, "SaveBtn",
            new Vector2(opStartX, row2Bottom),
            new Vector2(opStartX + opWidth, row2Top),
            "保存当前", new Color(0.2f, 0.7f, 0.2f, 1f));
        Button clearAllBtn = this.CreateButton(panelBg.transform, "ClearAllBtn",
            new Vector2(opStartX + opWidth + spacing, row2Bottom),
            new Vector2(opStartX + 2f * opWidth + spacing, row2Top),
            "清空所有", new Color(0.8f, 0.2f, 0.2f, 1f));
        clearAllBtn.onClick.AddListener(this.ClearAllPaths);

        Button[] btnArray = new Button[] { this.savePersistentBtn, replaceBtn, this.saveBtn, clearAllBtn };
        int k = 0;
        while (k < btnArray.Length)
        {
            Button btn = btnArray[k];
            Text lbl = btn.gameObject.GetComponentInChildren<Text>();
            if (lbl != null)
            {
                lbl.fontSize = 14;
            }
            k = k + 1;
        }

        GameObject runPanel = new GameObject("RunTestPanel");
        runPanel.transform.SetParent(this.transform, false);
        RectTransform runRect = runPanel.gameObject.AddComponent<RectTransform>();
        runRect.anchorMin = new Vector2(rightPanelLeft, 0.02f);
        runRect.anchorMax = new Vector2(rightPanelRight, 0.48f);
        runRect.sizeDelta = new Vector2(0f, 0f);
        runRect.anchoredPosition = new Vector2(0f, 0f);
        Image runBg = runPanel.gameObject.AddComponent<Image>();
        runBg.color = new Color(0.2f, 0.3f, 0.2f, 0.6f);
        runBg.raycastTarget = false;

        GameObject runTitleObj = new GameObject("RunTitle");
        runTitleObj.transform.SetParent(runPanel.transform, false);
        RectTransform runTitleRect = runTitleObj.gameObject.AddComponent<RectTransform>();
        runTitleRect.anchorMin = new Vector2(0f, 0.85f);
        runTitleRect.anchorMax = new Vector2(1f, 1f);
        runTitleRect.sizeDelta = new Vector2(0f, 0f);
        Text runTitleText = runTitleObj.gameObject.AddComponent<Text>();
        runTitleText.text = "Run 测试";
        runTitleText.color = new Color(1f, 1f, 1f, 1f);
        runTitleText.fontSize = 14;
        runTitleText.font = this.GetFont();
        runTitleText.alignment = TextAnchor.MiddleCenter;
        runTitleText.raycastTarget = false;

        int[][] presets = new int[][]
        {
            new int[]{0,1,2,3},
            new int[]{4,5,6,7},
            new int[]{0,2,4,6},
            new int[]{1,3,5,7},
            new int[]{0,1,2,3,4,5,6,7}
        };
        string[] presetLabels = new string[] { "0-3", "4-7", "偶", "奇", "全部" };
        float btnH = 0.06f;
        float btnW = 0.18f;
        float spacing2 = 0.01f;
        float rowY = 0.78f;
        int p = 0;
        while (p < presets.Length)
        {
            float x = 0.02f + p * (btnW + spacing2);
            if (x + btnW > 1f)
            {
                break;
            }
            Button btn = this.CreateButton(runPanel.transform, "PresetBtn_" + p,
                new Vector2(x, rowY - btnH),
                new Vector2(x + btnW, rowY),
                presetLabels[p],
                new Color(0.3f, 0.3f, 0.6f, 1f));
            int[] targets = presets[p];
            btn.onClick.AddListener(delegate { this.OnRunTest(targets); });
            Text lbl = btn.gameObject.GetComponentInChildren<Text>();
            if (lbl != null)
            {
                lbl.fontSize = 12;
            }
            p = p + 1;
        }

        float actionHeight = 0.10f;
        Button runTestBtn = this.CreateButton(runPanel.transform, "RunTestBtn",
            new Vector2(0.02f, 0f),
            new Vector2(0.48f, actionHeight),
            "启动 Run",
            new Color(0f, 0.6f, 0f, 1f));
        runTestBtn.onClick.AddListener(delegate { this.OnRunTest(new int[]{0,1,2,3,4,5,6,7}); });
        Text runBtnLabel = runTestBtn.gameObject.GetComponentInChildren<Text>();
        if (runBtnLabel != null)
        {
            runBtnLabel.fontSize = 14;
        }

        GameObject statusObj = new GameObject("RunStatus");
        statusObj.transform.SetParent(runPanel.transform, false);
        RectTransform statusRect = statusObj.gameObject.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.55f, 0f);
        statusRect.anchorMax = new Vector2(0.98f, actionHeight);
        statusRect.sizeDelta = new Vector2(0f, 0f);
        this.runStatusText = statusObj.gameObject.AddComponent<Text>();
        this.runStatusText.text = "就绪";
        this.runStatusText.color = new Color(1f, 1f, 1f, 1f);
        this.runStatusText.fontSize = 14;
        this.runStatusText.font = this.GetFont();
        this.runStatusText.alignment = TextAnchor.MiddleCenter;
        this.runStatusText.raycastTarget = false;

        Canvas.ForceUpdateCanvases();
    }

    private Button CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string text, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rect = btnObj.gameObject.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = new Vector2(0f, 0f);
        Image bg = btnObj.gameObject.AddComponent<Image>();
        bg.color = bgColor;
        bg.raycastTarget = true;
        Button btn = btnObj.gameObject.AddComponent<Button>();
        btn.targetGraphic = bg;
        btn.interactable = true;
        GameObject label = new GameObject("Label");
        label.transform.SetParent(btnObj.transform, false);
        RectTransform labelRect = label.gameObject.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.sizeDelta = new Vector2(0f, 0f);
        Text txt = label.gameObject.AddComponent<Text>();
        txt.text = text;
        txt.color = new Color(1f, 1f, 1f, 1f);
        txt.fontSize = 20;
        txt.font = this.GetFont();
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;
        return btn;
    }

    private void OnRunTest(int[] targetArray)
    {
        if (this.IsRunning)
        {
            this.SetInfoText("机器正在运行，请等待完成");
            return;
        }
        CSLList<int> targets = new CSLList<int>();
        int i = 0;
        while (i < targetArray.Length)
        {
            targets.Add(targetArray[i]);
            i = i + 1;
        }
        this.Run(targets, delegate {
            GLog.Debug("MarbleLotteryMachine::OnRunTest", "Run 测试完成！");
            this.SetInfoText("Run 测试全部完成！");
            if (this.runStatusText != null)
            {
                this.runStatusText.text = "完成";
                this.runStatusText.color = new Color(0f, 1f, 0f, 1f);
            }
        });
        if (this.runStatusText != null)
        {
            this.runStatusText.text = "运行中...";
            this.runStatusText.color = new Color(1f, 1f, 0f, 1f);
        }
        this.SetInfoText("启动 Run，目标数 " + targets.Count);
    }

    private void UpdateRunStatus()
    {
        if (!this.IsRunning)
        {
            if (this.runStatusText != null)
            {
                this.runStatusText.text = "就绪";
                this.runStatusText.color = new Color(1f, 1f, 1f, 1f);
            }
            return;
        }

        if (!this.isRunFinished)
        {
            int total = 0;
            if (this.runTargets != null)
            {
                total = this.runTargets.Count;
            }
            int launched = this.runTargetIndex;
            if (this.runStatusText != null)
            {
                this.runStatusText.text = "发射中: " + launched + "/" + total;
                this.runStatusText.color = new Color(1f, 1f, 0f, 1f);
            }
        }
        else
        {
            if (this.runStatusText != null)
            {
                this.runStatusText.text = "回收中: " + this.balls.Count + " 个球";
                this.runStatusText.color = new Color(0f, 1f, 1f, 1f);
            }
        }
    }
}