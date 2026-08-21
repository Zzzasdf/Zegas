using System;
using UnityEngine;
using UnityEngine.UI;
using TKLua;
using Random = UnityEngine.Random;
using LuaFramework;

public partial class MarbleLotteryMachine
{
    private void CreateSceneElements()
    {
        float logicWidth = 1334f;
        float logicHeight = 750f;
        bool activityDestroyed = GameObjectUtils.IsDestroyed(this.activityArea.gameObject);
        bool areaDestroyed = GameObjectUtils.IsDestroyed(this.activityArea.ActivityArea);
        if (activityDestroyed || areaDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::CreateSceneElements", "activityArea 未赋值或已销毁");
            float marginX = logicWidth * this.horizontalMargin;
            float marginY = logicHeight * this.verticalMargin;
            this.activityRect = new Rect(marginX, marginY, logicWidth - 2f * marginX, logicHeight - 2f * marginY);
        }
        else
        {
            RectTransform rect = this.activityArea.ActivityArea;
            Vector2 pos = rect.localPosition;
            Vector2 size = rect.rect.size;
            Vector2 pivot = rect.pivot;
            this.activityRect = new Rect(pos.x - size.x * pivot.x, pos.y - size.y * pivot.y, size.x, size.y);
        }
        this.blockWidth = this.activityRect.width / this.blockCount;
        this.blockStartY = this.activityRect.yMin + 40f;

        this.gameContainer = new GameObject("GameContainer");
        this.gameContainer.transform.SetParent(this.transform, false);
        RectTransform containerRect = this.gameContainer.gameObject.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0f, 0f);
        containerRect.anchorMax = new Vector2(1f, 1f);
        containerRect.sizeDelta = new Vector2(0f, 0f);
        containerRect.pivot = new Vector2(0f, 0f);
        containerRect.anchoredPosition = new Vector2(0f, 0f);
        containerRect.localPosition = new Vector3(0f, 0f, 0f);

        this.GenerateEndBlocks();
        this.GenerateObstacles();

        bool launcherDestroyed = GameObjectUtils.IsDestroyed(this.launcher.gameObject);
        bool launchPosDestroyed = GameObjectUtils.IsDestroyed(this.launcher.LaunchPos);
        if (!launcherDestroyed && !launchPosDestroyed)
        {
            Vector2 worldPos = this.launcher.LaunchPos.position;
            this.startPoint = this.activityArea.ActivityArea.InverseTransformPoint(worldPos);
        }
        else
        {
            GLog.Debug("MarbleLotteryMachine::CreateSceneElements", "launcher 未赋值，使用默认起点");
            this.startPoint = new Vector2((this.activityRect.xMin + this.activityRect.xMax) / 2f, this.activityRect.yMax - 40f);
        }
    }

    private GameObject CreateCircle(string name, Color color, float radius)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(this.gameContainer.transform, false);
        RectTransform rect = obj.gameObject.AddComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(radius * 2f, radius * 2f);
        Image img = obj.gameObject.AddComponent<Image>();
        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        Vector2 center = new Vector2(32f, 32f);
        int y = 0;
        while (y < 64)
        {
            int x = 0;
            while (x < 64)
            {
                float dist = MarbleLotteryMachine.Distance(new Vector2(x, y), center);
                if (dist < 30f)
                {
                    colors[y * 64 + x] = new Color(color.r, color.g, color.b, color.a);
                }
                else
                {
                    colors[y * 64 + x] = new Color(0f, 0f, 0f, 0f);
                }
                x = x + 1;
            }
            y = y + 1;
        }
        tex.SetPixels(colors);
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f));
        img.sprite = sprite;
        img.color = new Color(1f, 1f, 1f, 1f);
        img.raycastTarget = false;
        return obj;
    }

    private void GenerateObstacles()
    {
        int i = 0;
        while (i < this.obstacles.Count)
        {
            GameObject obs = this.obstacles[i];
            if (obs != null)
            {
                GameObject.Destroy(obs);
            }
            i = i + 1;
        }
        this.obstacles.Clear();
        this.layerObstacles.Clear();

        bool containerDestroyed = GameObjectUtils.IsDestroyed(this.obstacleContainer);
        if (containerDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::GenerateObstacles", "obstacleContainer 未赋值或已销毁");
            return;
        }

        Rect containerRect = this.obstacleContainer.rect;
        Vector2 pivot = this.obstacleContainer.pivot;
        float containerLeft = -containerRect.width * pivot.x;
        float containerRight = containerRect.width * (1f - pivot.x);
        float containerTop = containerRect.height * (1f - pivot.y);
        float containerBottom = -containerRect.height * pivot.y;
        float marginX = this.obstacleRadius + 10f;
        float marginY = this.obstacleRadius + 10f;
        float usableLeft = containerLeft + marginX;
        float usableRight = containerRight - marginX;
        float usableTop = containerTop - marginY;
        float usableBottom = containerBottom + marginY;
        if (usableRight <= usableLeft || usableTop <= usableBottom)
        {
            GLog.Debug("MarbleLotteryMachine::GenerateObstacles", "obstacleContainer 可用空间不足");
            return;
        }

        float usableHeight = usableTop - usableBottom;
        int totalLayers = 4;
        float layerHeight = usableHeight / totalLayers;
        if (this.targetXInObstacle.Count != this.blockCount)
        {
            GLog.Debug("MarbleLotteryMachine::GenerateObstacles", "目标 X 坐标数量不符！");
            return;
        }

        int id = 1;
        int layer = 0;
        while (layer < totalLayers)
        {
            bool isEven = (layer % 2 == 0);
            int count;
            if (isEven)
            {
                count = this.blockCount;
            }
            else
            {
                count = Math.Max(1, this.blockCount - 1);
            }
            CSLList<float> layerX = new CSLList<float>();
            int j = 0;
            if (isEven)
            {
                while (j < count)
                {
                    layerX.Add(this.targetXInObstacle[j]);
                    j = j + 1;
                }
            }
            else
            {
                if (count == 1)
                {
                    float minX = float.MaxValue;
                    float maxX = float.MinValue;
                    int k = 0;
                    while (k < this.targetXInObstacle.Count)
                    {
                        float val = this.targetXInObstacle[k];
                        if (val < minX)
                        {
                            minX = val;
                        }
                        if (val > maxX)
                        {
                            maxX = val;
                        }
                        k = k + 1;
                    }
                    float midX = (minX + maxX) / 2f;
                    midX = Mathf.Clamp(midX, usableLeft + this.obstacleRadius, usableRight - this.obstacleRadius);
                    layerX.Add(midX);
                }
                else
                {
                    while (j < count)
                    {
                        float x = (this.targetXInObstacle[j] + this.targetXInObstacle[j + 1]) / 2f;
                        x = Mathf.Clamp(x, usableLeft + this.obstacleRadius, usableRight - this.obstacleRadius);
                        layerX.Add(x);
                        j = j + 1;
                    }
                }
            }
            float layerY = usableTop - (layer + 0.5f) * layerHeight;
            if (layerY < usableBottom)
            {
                break;
            }
            CSLList<GameObject> layerList = new CSLList<GameObject>();
            j = 0;
            while (j < count)
            {
                Vector2 localPos = new Vector2(layerX[j], layerY);
                GameObject obsObj = GameObject.Instantiate(this.obstacleTemplate.gameObject);
                obsObj.transform.SetParent(this.obstacleContainer, false);
                obsObj.SetActive(true);
                obsObj.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(localPos.x, localPos.y, 0f);
#if MARBLE_DEV
                this.AddIDLabel(obsObj, id);
#endif
                this.obstacles.Add(obsObj);
                id = id + 1;
                layerList.Add(obsObj);
                j = j + 1;
            }
            this.layerObstacles.Add(layerList);
            layer = layer + 1;
        }
    }

#if MARBLE_DEV
    private void AddIDLabel(GameObject parent, int id)
    {
        GameObject textObj = new GameObject("IDLabel");
        textObj.transform.SetParent(parent.transform, false);
        RectTransform rect = textObj.gameObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(0f, 0f);
        Text label = textObj.gameObject.AddComponent<Text>();
        label.text = id.ToString();
        label.fontSize = 14;
        label.font = this.GetFont();
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0f, 0f, 0f, 1f);
        label.raycastTarget = false;
    }
#endif

    private void GenerateEndBlocks()
    {
        int i = 0;
        while (i < this.endBlocks.Count)
        {
            GameObject block = this.endBlocks[i];
            if (block != null)
            {
                GameObject.Destroy(block);
            }
            i = i + 1;
        }
        this.endBlocks.Clear();
        this.targetXInObstacle.Clear();
        this.targetSelectButtons.Clear();

        bool templateDestroyed = GameObjectUtils.IsDestroyed(this.finalAreaTemplate.gameObject);
        bool containerDestroyed = GameObjectUtils.IsDestroyed(this.finalAreaContainer);
        if (templateDestroyed || containerDestroyed)
        {
            GLog.Debug("MarbleLotteryMachine::GenerateEndBlocks", "finalAreaTemplate 或 finalAreaContainer 未赋值或已销毁");
            return;
        }

        Rect containerRect = this.finalAreaContainer.rect;
        Vector2 pivot = this.finalAreaContainer.pivot;
        float containerLeft = -containerRect.width * pivot.x;
        float containerRight = containerRect.width * (1f - pivot.x);
        float usableWidth = containerRight - containerLeft;
        float spacing = usableWidth / this.blockCount;
        float startX = containerLeft + spacing * 0.5f;
        float yPos = 0f;
        float blockHeight = this.finalAreaTemplate.gameObject.gameObject.GetComponent<RectTransform>().rect.height;

        i = 0;
        while (i < this.blockCount)
        {
            float x = Mathf.Clamp(startX + i * spacing, containerLeft + 10f, containerRight - 10f);
            Vector2 localPos = new Vector2(x, yPos);
            GameObject block = GameObject.Instantiate(this.finalAreaTemplate.gameObject);
            block.transform.SetParent(this.finalAreaContainer, false);
            block.SetActive(true);
            RectTransform rect = block.gameObject.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(localPos.x, localPos.y, 0f);

            Vector3 worldCenter;
            MarbleLotteryMachineFinalArea finalComp = LuaBehaviour.getLuaBehaviour<MarbleLotteryMachineFinalArea>(block);
            bool compDestroyed = GameObjectUtils.IsDestroyed(finalComp.gameObject);
            bool areaDestroyed = GameObjectUtils.IsDestroyed(finalComp.CollisionArea);
            if (!compDestroyed && !areaDestroyed)
            {
                worldCenter = finalComp.CollisionArea.position;
            }
            else
            {
                worldCenter = rect.TransformPoint(new Vector3(0f, 0f, 0f));
            }
            Vector2 obstacleLocal = this.obstacleContainer.InverseTransformPoint(worldCenter);
            this.targetXInObstacle.Add(obstacleLocal.x);

#if MARBLE_DEV
            GameObject btnObj = new GameObject("SelectBtn");
            btnObj.transform.SetParent(block.transform, false);
            RectTransform btnRect = btnObj.gameObject.AddComponent<RectTransform>();
            float btnWidth = rect.rect.width;
            float btnHeight = 30f;
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 1f);
            btnRect.sizeDelta = new Vector2(btnWidth, btnHeight);
            btnRect.anchoredPosition = new Vector2(0f, -5f);
            Image btnImg = btnObj.gameObject.AddComponent<Image>();
            btnImg.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            btnImg.raycastTarget = true;
            Button btn = btnObj.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.gameObject.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.sizeDelta = new Vector2(0f, 0f);
            Text label = textObj.gameObject.AddComponent<Text>();
            label.text = "目标 " + (i + 1);
            label.fontSize = 14;
            label.font = this.GetFont();
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 1f, 1f, 1f);
            label.raycastTarget = false;
            int index = i;
            btn.onClick.AddListener(delegate { this.SelectTrueEnd(index); });
            this.targetSelectButtons.Add(btn);
#endif
            this.endBlocks.Add(block);
            i = i + 1;
        }
        this.trueEndIndex = Random.Range(0, this.blockCount);
        this.SelectTrueEnd(this.trueEndIndex);
        this.blockStartY = yPos - blockHeight * 0.5f;
        this.blockWidth = spacing;
    }

    private void SelectTrueEnd(int index)
    {
        this.trueEndIndex = index;
        int i = 0;
        while (i < this.endBlocks.Count)
        {
            GameObject block = this.endBlocks[i];
            if (block != null)
            {
                Transform border = block.transform.Find("Border");
                if (border != null)
                {
                    Image img = border.gameObject.GetComponent<Image>();
                    if (img != null)
                    {
                        if (i == index)
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
            i = i + 1;
        }
        i = 0;
        while (i < this.targetSelectButtons.Count)
        {
            Button btn = this.targetSelectButtons[i];
            if (btn != null)
            {
                Image img = btn.gameObject.GetComponent<Image>();
                if (img != null)
                {
                    if (i == index)
                    {
                        img.color = new Color(0.2f, 0.8f, 0.2f, 1f);
                    }
                    else
                    {
                        img.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                }
            }
            i = i + 1;
        }
        this.endPoint = new Vector2(this.activityRect.xMin + index * this.blockWidth + this.blockWidth / 2f, this.blockStartY + this.blockHeight / 2f);
        this.ClearPath();
        this.SetInfoText("已切换到目标 " + (index + 1) + "，请重新生成路径");
    }
}