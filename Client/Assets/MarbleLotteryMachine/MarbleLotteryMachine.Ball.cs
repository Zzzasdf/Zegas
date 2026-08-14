using UnityEngine;
using TKLua;
using UnityEngine.UI;

public partial class MarbleLotteryMachine
{
    public void DirectLaunch()
    {
        if (ballTemplate == null) { Debug.LogError("ballTemplate 未赋值！"); return; }
        if (ballContainer == null) { Debug.LogError("ballContainer 未赋值！"); return; }
        if (launcher == null || launcher.LaunchPos == null) { Debug.LogError("launcher.LaunchPos 未赋值！"); return; }

        UpdateCollisionInfos();
        Vector2 worldPos = launcher.LaunchPos.position;
        Vector2 containerLocalPos = ballContainer.InverseTransformPoint(worldPos);
        GameObject ballObj = GetOrCreateBallObject();
        ballObj.transform.SetParent(ballContainer, false);
        ballObj.SetActive(true);
        RectTransform rect = ballObj.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(containerLocalPos.x, containerLocalPos.y, 0);
        MarbleLotteryMachineBall ballComp = ballObj.GetComponent<MarbleLotteryMachineBall>();
        float radius = GetBallRadius(ballObj);
        BallInstance newBall = new BallInstance();
        ballCounter++;
        newBall.ballID = ballCounter;
        newBall.expectTargetIndex = trueEndIndex;
        newBall.path = null;
        newBall.isActive = true;
        newBall.obj = ballObj;
        newBall.rect = rect;
        newBall.ballComponent = ballComp;
        newBall.radius = radius;
        float angle = Random.Range(-80f, 80f) * Mathf.Deg2Rad;
        Vector2 vel = new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * initialSpeed;
        if (vel.y > 0) vel.y = -vel.y;
        newBall.velocity = vel;
        float rotAngle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
        launcher.transform.localEulerAngles = new Vector3(0, 0, rotAngle);
        newBall.label = CreateLabel(ballObj, newBall.ballID + "→" + trueEndIndex);
        newBall.label.color = Color.black;
        balls.Add(newBall);
        SetInfoText("🚀 直接发射小球 #" + newBall.ballID + " (物理模拟)");
        Debug.Log("直接发射小球 #" + newBall.ballID);
    }

    public void LaunchBall()
    {
        UpdateCollisionInfos();
        if (finalPath == null || finalPath.Count < 2) { GeneratePath(); if (finalPath == null || finalPath.Count < 2) return; }
        if (launcher == null || launcher.LaunchPos == null) { Debug.LogError("launcher.LaunchPos 未赋值！"); return; }
        Vector2 currentWorldPos = launcher.LaunchPos.position;
        Vector2 currentLocalPos = activityArea.ActivityArea.InverseTransformPoint(currentWorldPos);
        Vector2 pathStart = finalPath[0];
        Vector2 offset = currentLocalPos - pathStart;
        if (offset.magnitude > 0.01f)
        {
            Debug.Log("路径起点偏差 " + offset.magnitude + "，自动平移对齐");
            for (int i = 0; i < finalPath.Count; i++) finalPath[i] += offset;
            startPoint = currentLocalPos;
            DrawPath();
        }
        if (ballTemplate == null) { Debug.LogError("ballTemplate 未赋值！"); return; }
        if (ballContainer == null) { Debug.LogError("ballContainer 未赋值！"); return; }
        Vector2 containerLocalPos = ballContainer.InverseTransformPoint(currentWorldPos);
        GameObject ballObj = GetOrCreateBallObject();
        ballObj.transform.SetParent(ballContainer, false);
        ballObj.SetActive(true);
        RectTransform rect = ballObj.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(containerLocalPos.x, containerLocalPos.y, 0);
        MarbleLotteryMachineBall ballComp = ballObj.GetComponent<MarbleLotteryMachineBall>();
        float radius = GetBallRadius(ballObj);
        BallInstance newBall = new BallInstance();
        ballCounter++;
        newBall.ballID = ballCounter;
        newBall.expectTargetIndex = trueEndIndex;
        newBall.path = new CSLList<Vector2>(finalPath);
        newBall.progress = 0f;
        float totalLen = 0f;
        for (int i = 0; i < newBall.path.Count - 1; i++) totalLen += Distance(newBall.path[i], newBall.path[i + 1]);
        newBall.totalLength = totalLen;
        newBall.moveSpeed = initialSpeed * 1.5f;
        newBall.isActive = true;
        newBall.obj = ballObj;
        newBall.rect = rect;
        newBall.ballComponent = ballComp;
        newBall.radius = radius;
        newBall.label = CreateLabel(ballObj, newBall.ballID + "→" + trueEndIndex);
        newBall.label.color = Color.black;
        newBall.collisionRecords.Clear();
        foreach (var rec in lastCollisionRecords) newBall.collisionRecords.Add(rec);
        newBall.triggeredIndex = 0;
        balls.Add(newBall);
        if (finalPath.Count >= 2)
        {
            Vector2 dir = (finalPath[1] - finalPath[0]).normalized;
            float rotAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            launcher.transform.localEulerAngles = new Vector3(0, 0, rotAngle);
        }
        SetInfoText("发射路径小球 #" + newBall.ballID + " → 目标 " + trueEndIndex + "，当前小球总数 " + balls.Count);
        Debug.Log("LaunchBall: path count=" + newBall.path.Count + ", totalLength=" + totalLen + ", 碰撞记录数=" + newBall.collisionRecords.Count);
    }

    Text CreateLabel(GameObject parent, string text)
    {
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(parent.transform, false);
        RectTransform rect = labelObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        Text label = labelObj.AddComponent<Text>();
        label.text = text;
        label.fontSize = 11;
        label.font = GetFont();
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.black;
        label.raycastTarget = false;
        return label;
    }

    void UpdateBalls()
    {
        foreach (var ball in balls)
        {
            if (!ball.isActive || ball.isComplete) continue;
            if (ball.path != null && ball.path.Count >= 2)
            {
                // 路径跟随
                if (ball.totalLength > 0.001f)
                {
                    float speed = ball.moveSpeed; if (speed <= 0) speed = initialSpeed;
                    float step = speed * Time.deltaTime / ball.totalLength;
                    ball.progress += step;
                    if (ball.progress > 1f) ball.progress = 1f;
                    Vector2 pathLocalPos = GetPointOnPath(ball.path, ball.progress);
                    Vector2 pathWorldPos = activityArea.ActivityArea.TransformPoint(pathLocalPos);
                    ball.rect.position = pathWorldPos;
                    ball.rect.localPosition = new Vector3(ball.rect.localPosition.x, ball.rect.localPosition.y, 0);

                    float progressPos = ball.progress * ball.totalLength;
                    float accumulated = 0f;
                    int pathIndex = 0;
                    for (int i = 0; i < ball.path.Count - 1; i++)
                    {
                        float segLen = Distance(ball.path[i], ball.path[i + 1]);
                        if (accumulated + segLen >= progressPos) { pathIndex = i; break; }
                        accumulated += segLen;
                    }

                    while (ball.triggeredIndex < ball.collisionRecords.Count)
                    {
                        var rec = ball.collisionRecords[ball.triggeredIndex];
                        int recordPathIndex = -1;
                        for (int i = 0; i < ball.path.Count - 1; i++)
                            if (Distance(ball.path[i], rec.position) < 0.1f || Distance(ball.path[i + 1], rec.position) < 0.1f)
                            { recordPathIndex = i; break; }
                        if (recordPathIndex < 0)
                        {
                            float minDist = float.MaxValue;
                            for (int i = 0; i < ball.path.Count - 1; i++)
                            {
                                float d = Distance(ball.path[i], rec.position);
                                if (d < minDist) { minDist = d; recordPathIndex = i; }
                            }
                        }
                        if (pathIndex >= recordPathIndex)
                        {
                            if (rec.obstacleIndex >= 0 && rec.obstacleIndex < obstacles.Count)
                            {
                                var obsComp = obstacles[rec.obstacleIndex].GetComponent<MarbleLotteryMachineObstacle>();
                                if (obsComp != null) { obsComp.Hit(); if (ball.ballComponent != null) ball.ballComponent.HitObstacle(); }
                            }
                            ball.triggeredIndex++;
                        }
                        else break;
                    }

                    if (ball.progress >= 1f)
                    {
                        ball.isActive = false;
                        ball.isComplete = true;
                        ball.completeTime = Time.time;
                        Vector2 finalPos = ball.path[ball.path.Count - 1];
                        ball.rect.position = activityArea.ActivityArea.TransformPoint(finalPos);
                        ball.actualHitIndex = CheckHitEnd(finalPos);
                        ball.success = (ball.actualHitIndex == ball.expectTargetIndex);
                        if (ball.success && ball.actualHitIndex >= 0 && ball.actualHitIndex < finalAreaCollisionInfos.Count)
                        {
                            var areaInfo = finalAreaCollisionInfos[ball.actualHitIndex];
                            if (areaInfo.component != null) areaInfo.component.Reach();
                            if (ball.ballComponent != null) ball.ballComponent.ReachFinalArea();
                        }
                        LogResult(ball);
                        UpdateTargetLabels();
                    }
                }
                continue;
            }

            // 物理模拟
            float dt = Time.deltaTime; if (dt > 0.02f) dt = 0.02f;
            Vector3 worldPos = ball.rect.position;
            Vector2 localPos = activityArea.ActivityArea.InverseTransformPoint(worldPos);
            ball.velocity.y -= gravity * dt;
            Vector2 newLocalPos = localPos + ball.velocity * dt;

            foreach (var info in obstacleCollisionInfos)
            {
                Vector2 toBall = newLocalPos - info.center;
                float dist = toBall.magnitude;
                if (dist < ball.radius + info.radius && dist > 0.001f)
                {
                    Vector2 normal = toBall.normalized;
                    newLocalPos = info.center + normal * (ball.radius + info.radius);
                    float velAlongNormal = Vector2.Dot(ball.velocity, normal);
                    if (velAlongNormal < 0) ball.velocity -= normal * 2 * velAlongNormal * bounceFactor;
                    if (info.component != null) info.component.Hit();
                    if (ball.ballComponent != null) ball.ballComponent.HitObstacle();
                    break;
                }
            }

            if (newLocalPos.x - ball.radius < activityRect.xMin) { newLocalPos.x = activityRect.xMin + ball.radius; if (ball.velocity.x < 0) ball.velocity.x *= -bounceFactor; }
            if (newLocalPos.x + ball.radius > activityRect.xMax) { newLocalPos.x = activityRect.xMax - ball.radius; if (ball.velocity.x > 0) ball.velocity.x *= -bounceFactor; }
            if (newLocalPos.y + ball.radius > activityRect.yMax) { newLocalPos.y = activityRect.yMax - ball.radius; if (ball.velocity.y > 0) ball.velocity.y = -Mathf.Abs(ball.velocity.y) * 0.2f; else ball.velocity.y = -1f; }
            if (newLocalPos.y - ball.radius < activityRect.yMin) { newLocalPos.y = activityRect.yMin + ball.radius; if (ball.velocity.y < 0) ball.velocity.y *= -bounceFactor; }

            ball.rect.position = activityArea.ActivityArea.TransformPoint(newLocalPos);
            int hitIdx = CheckHitEnd(newLocalPos);
            bool reached = (hitIdx >= 0);
            if (reached)
            {
                ball.actualHitIndex = hitIdx;
                ball.success = (hitIdx == ball.expectTargetIndex);
                if (hitIdx < finalAreaCollisionInfos.Count)
                {
                    var areaInfo = finalAreaCollisionInfos[hitIdx];
                    if (areaInfo.component != null) areaInfo.component.Reach();
                    if (ball.ballComponent != null) ball.ballComponent.ReachFinalArea();
                }
            }
            if (reached || newLocalPos.y - ball.radius < activityRect.yMin + 5f)
            {
                ball.isActive = false;
                ball.isComplete = true;
                ball.completeTime = Time.time;
                if (!reached) { ball.actualHitIndex = CheckHitEnd(newLocalPos); ball.success = (ball.actualHitIndex == ball.expectTargetIndex); }
                LogResult(ball);
                UpdateTargetLabels();
            }
        }
    }

    Vector2 GetPointOnPath(CSLList<Vector2> path, float t)
    {
        if (path == null || path.Count == 0) return Vector2.zero;
        if (t <= 0) return path[0];
        if (t >= 1) return path[path.Count - 1];
        float totalLen = 0f;
        for (int i = 0; i < path.Count - 1; i++) totalLen += Distance(path[i], path[i + 1]);
        float targetLen = t * totalLen;
        float accumulated = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            float segLen = Distance(path[i], path[i + 1]);
            if (accumulated + segLen >= targetLen)
            {
                float localT = (targetLen - accumulated) / segLen;
                return Lerp(path[i], path[i + 1], localT);
            }
            accumulated += segLen;
        }
        return path[path.Count - 1];
    }

    void LogResult(BallInstance ball)
    {
        string log = "小球 #" + ball.ballID + " 预期 " + ball.expectTargetIndex + " 实际 " + ball.actualHitIndex + " 成功: " + ball.success;
        Debug.Log(log);
        SetInfoText(log);
    }

    void UpdateTargetLabels()
    {
        foreach (GameObject block in endBlocks)
        {
            Transform old = block.transform.Find("TargetLabel");
            if (old != null) GameObject.Destroy(old.gameObject);
        }
        CSLDictionary<int, CSLList<int>> targetMap = new CSLDictionary<int, CSLList<int>>();
        foreach (var ball in balls)
            if (!ball.isComplete)
            {
                int idx = ball.expectTargetIndex;
                if (!targetMap.ContainsKey(idx)) targetMap[idx] = new CSLList<int>();
                targetMap[idx].Add(ball.ballID);
            }
        foreach (var k in targetMap.Keys)
        {
            int blockIdx = k;
            if (blockIdx < 0 || blockIdx >= endBlocks.Count) continue;
            GameObject block = endBlocks[blockIdx];
            RectTransform blockRect = block.GetComponent<RectTransform>();
            float blockW = blockRect.sizeDelta.x, blockH = blockRect.sizeDelta.y;
            string ids = "";
            CSLList<int> values = targetMap[blockIdx];
            for (int i = 0; i < values.Count; i++) { if (i > 0) ids += ","; ids += values[i]; }
            GameObject labelObj = new GameObject("TargetLabel");
            labelObj.transform.SetParent(block.transform, false);
            RectTransform rect = labelObj.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(blockW * 0.8f, blockH * 0.8f);
            rect.anchoredPosition = Vector2.zero;
            Text label = labelObj.AddComponent<Text>();
            label.text = ids;
            label.fontSize = (int)(blockH * 0.35f);
            label.font = GetFont();
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
        }
    }
}