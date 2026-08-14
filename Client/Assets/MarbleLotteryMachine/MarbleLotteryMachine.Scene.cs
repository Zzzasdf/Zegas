using System;
using UnityEngine;
using UnityEngine.UI;
using TKLua;
using Random = UnityEngine.Random;

public partial class MarbleLotteryMachine
{
    void CreateSceneElements()
    {
        float logicWidth = 1334f, logicHeight = 750f;
        if (activityArea != null && activityArea.ActivityArea != null)
        {
            RectTransform rect = activityArea.ActivityArea;
            Vector2 pos = rect.localPosition;
            Vector2 size = rect.rect.size;
            Vector2 pivot = rect.pivot;
            activityRect = new Rect(pos.x - size.x * pivot.x, pos.y - size.y * pivot.y, size.x, size.y);
        }
        else
        {
            float marginX = logicWidth * horizontalMargin, marginY = logicHeight * verticalMargin;
            activityRect = new Rect(marginX, marginY, logicWidth - 2 * marginX, logicHeight - 2 * marginY);
        }
        blockWidth = activityRect.width / blockCount;
        blockStartY = activityRect.yMin + 40f;

        gameContainer = new GameObject("GameContainer");
        gameContainer.transform.SetParent(transform, false);
        RectTransform containerRect = gameContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero;
        containerRect.pivot = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.localPosition = Vector3.zero;

        GenerateEndBlocks();
        GenerateObstacles();

        if (launcher != null && launcher.LaunchPos != null)
        {
            Vector2 worldPos = launcher.LaunchPos.position;
            startPoint = activityArea.ActivityArea.InverseTransformPoint(worldPos);
        }
        else
            startPoint = new Vector2((activityRect.xMin + activityRect.xMax) / 2, activityRect.yMax - 40f);

        UpdateActivityRect();
    }

    private void UpdateActivityRect()
    {
        if (activityArea != null && activityArea.ActivityArea != null)
        {
            RectTransform rect = activityArea.ActivityArea;
            Vector2 pos = rect.localPosition;
            Vector2 size = rect.rect.size;
            Vector2 pivot = rect.pivot;
            activityRect = new Rect(pos.x - size.x * pivot.x, pos.y - size.y * pivot.y, size.x, size.y);
        }
    }

    GameObject CreateCircle(string name, Color color, float radius)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(gameContainer.transform, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.sizeDelta = new Vector2(radius * 2, radius * 2);
        Image img = obj.AddComponent<Image>();
        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        Vector2 center = new Vector2(32, 32);
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float dist = Distance(new Vector2(x, y), center);
                colors[y * 64 + x] = (dist < 30) ? color : Color.clear;
            }
        tex.SetPixels(colors);
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        img.sprite = sprite;
        img.color = Color.white;
        img.raycastTarget = false;
        return obj;
    }

    GameObject CreateRect(string name, Color fillColor, Color borderColor, float width, float height)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(gameContainer.transform, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.sizeDelta = new Vector2(width, height);
        Image img = obj.AddComponent<Image>();
        img.color = fillColor;
        img.raycastTarget = false;
        GameObject border = new GameObject("Border");
        border.transform.SetParent(obj.transform, false);
        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        borderRect.localPosition = Vector3.zero;
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = borderColor;
        borderImg.raycastTarget = false;
        return obj;
    }

    void GenerateObstacles()
    {
        foreach (var obs in obstacles) GameObject.Destroy(obs);
        obstacles.Clear();
        layerObstacles.Clear();
        if (obstacleTemplate == null) { Debug.LogError("obstacleTemplate 未赋值！"); return; }
        if (obstacleContainer == null) { Debug.LogError("obstacleContainer 未赋值！"); return; }

        Rect containerRect = obstacleContainer.rect;
        Vector2 pivot = obstacleContainer.pivot;
        float containerLeft = -containerRect.width * pivot.x;
        float containerRight = containerRect.width * (1 - pivot.x);
        float containerTop = containerRect.height * (1 - pivot.y);
        float containerBottom = -containerRect.height * pivot.y;
        float marginX = obstacleRadius + 10f, marginY = obstacleRadius + 10f;
        float usableLeft = containerLeft + marginX, usableRight = containerRight - marginX;
        float usableTop = containerTop - marginY, usableBottom = containerBottom + marginY;
        if (usableRight <= usableLeft || usableTop <= usableBottom) { Debug.LogError("obstacleContainer 可用空间不足"); return; }

        float usableHeight = usableTop - usableBottom;
        int totalLayers = 4;
        float layerHeight = usableHeight / totalLayers;
        if (targetXInObstacle.Count != blockCount) { Debug.LogError("目标 X 坐标数量不符！"); return; }

        int id = 1;
        for (int layer = 0; layer < totalLayers; layer++)
        {
            bool isEven = (layer % 2 == 0);
            int count = isEven ? blockCount : Math.Max(1, blockCount - 1);
            CSLList<float> layerX = new CSLList<float>();
            if (isEven)
            {
                for (int i = 0; i < count; i++) layerX.Add(targetXInObstacle[i]);
            }
            else
            {
                if (count == 1)
                {
                    float minX = float.MaxValue, maxX = float.MinValue;
                    foreach (float x in targetXInObstacle) { if (x < minX) minX = x; if (x > maxX) maxX = x; }
                    float midX = Mathf.Clamp((minX + maxX) / 2f, usableLeft + obstacleRadius, usableRight - obstacleRadius);
                    layerX.Add(midX);
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        float x = (targetXInObstacle[i] + targetXInObstacle[i + 1]) / 2f;
                        x = Mathf.Clamp(x, usableLeft + obstacleRadius, usableRight - obstacleRadius);
                        layerX.Add(x);
                    }
                }
            }
            float layerY = usableTop - (layer + 0.5f) * layerHeight;
            if (layerY < usableBottom) break;
            CSLList<GameObject> layerList = new CSLList<GameObject>();
            for (int i = 0; i < count; i++)
            {
                Vector2 localPos = new Vector2(layerX[i], layerY);
                GameObject obsObj = GameObject.Instantiate(obstacleTemplate.gameObject);
                obsObj.transform.SetParent(obstacleContainer, false);
                obsObj.SetActive(true);
                obsObj.GetComponent<RectTransform>().localPosition = new Vector3(localPos.x, localPos.y, 0);
                AddIDLabel(obsObj, id);
                obstacles.Add(obsObj);
                id++;
                layerList.Add(obsObj);
            }
            layerObstacles.Add(layerList);
        }
    }

    void AddIDLabel(GameObject parent, int id)
    {
        GameObject textObj = new GameObject("IDLabel");
        textObj.transform.SetParent(parent.transform, false);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        Text label = textObj.AddComponent<Text>();
        label.text = id.ToString();
        label.fontSize = 14;
        label.font = GetFont();
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.black;
        label.raycastTarget = false;
    }

    void GenerateEndBlocks()
    {
        foreach (var block in endBlocks) GameObject.Destroy(block);
        endBlocks.Clear();
        targetXInObstacle.Clear();
        targetSelectButtons.Clear();
        if (finalAreaTemplate == null) { Debug.LogError("finalAreaTemplate 未赋值！"); return; }
        if (finalAreaContainer == null) { Debug.LogError("finalAreaContainer 未赋值！"); return; }
        if (obstacleContainer == null) { Debug.LogError("obstacleContainer 未赋值！"); return; }

        Rect containerRect = finalAreaContainer.rect;
        Vector2 pivot = finalAreaContainer.pivot;
        float containerLeft = -containerRect.width * pivot.x;
        float containerRight = containerRect.width * (1 - pivot.x);
        float usableWidth = containerRight - containerLeft;
        float spacing = usableWidth / blockCount;
        float startX = containerLeft + spacing * 0.5f;
        float yPos = 0f;
        float blockHeight = finalAreaTemplate.gameObject.GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < blockCount; i++)
        {
            float x = Mathf.Clamp(startX + i * spacing, containerLeft + 10f, containerRight - 10f);
            Vector2 localPos = new Vector2(x, yPos);
            GameObject block = GameObject.Instantiate(finalAreaTemplate.gameObject);
            block.transform.SetParent(finalAreaContainer, false);
            block.SetActive(true);
            RectTransform rect = block.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(localPos.x, localPos.y, 0);

            Vector3 worldCenter;
            MarbleLotteryMachineFinalArea finalComp = block.GetComponent<MarbleLotteryMachineFinalArea>();
            if (finalComp != null && finalComp.CollisionArea != null)
                worldCenter = finalComp.CollisionArea.position;
            else
                worldCenter = rect.TransformPoint(Vector3.zero);
            Vector2 obstacleLocal = obstacleContainer.InverseTransformPoint(worldCenter);
            targetXInObstacle.Add(obstacleLocal.x);

            GameObject btnObj = new GameObject("SelectBtn");
            btnObj.transform.SetParent(block.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            float btnWidth = rect.rect.width;
            float btnHeight = 30f;
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 1f);
            btnRect.sizeDelta = new Vector2(btnWidth, btnHeight);
            btnRect.anchoredPosition = new Vector2(0, -5f);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = Color.gray;
            btnImg.raycastTarget = true;
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            Text label = textObj.AddComponent<Text>();
            label.text = "目标 " + (i + 1);
            label.fontSize = 14;
            label.font = GetFont();
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            int index = i;
            btn.onClick.AddListener(() => SelectTrueEnd(index));
            targetSelectButtons.Add(btn);
            endBlocks.Add(block);
        }
        trueEndIndex = Random.Range(0, blockCount);
        SelectTrueEnd(trueEndIndex);
        blockStartY = yPos - blockHeight * 0.5f;
        blockWidth = spacing;
    }

    void SelectTrueEnd(int index)
    {
        trueEndIndex = index;
        for (int i = 0; i < endBlocks.Count; i++)
        {
            GameObject block = endBlocks[i];
            Transform border = block.transform.Find("Border");
            if (border != null)
            {
                Image img = border.GetComponent<Image>();
                if (img != null) img.color = (i == index) ? trueEndColor : Color.white;
            }
        }
        for (int i = 0; i < targetSelectButtons.Count; i++)
        {
            Button btn = targetSelectButtons[i];
            if (btn != null)
            {
                Image img = btn.GetComponent<Image>();
                if (img != null) img.color = (i == index) ? new Color(0.2f, 0.8f, 0.2f) : Color.gray;
            }
        }
        endPoint = new Vector2(activityRect.xMin + index * blockWidth + blockWidth / 2, blockStartY + blockHeight / 2);
        ClearPath();
        SetInfoText("已切换到目标 " + (index + 1) + "，请重新生成路径");
    }
}