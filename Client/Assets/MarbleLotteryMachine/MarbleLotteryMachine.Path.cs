using System.Diagnostics;
using UnityEngine;
using TKLua;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public partial class MarbleLotteryMachine
{
    void UpdateCollisionInfos()
    {
        if (activityArea != null && activityArea.ActivityArea != null)
        {
            RectTransform rect = activityArea.ActivityArea;
            Vector2 size = rect.rect.size;
            Vector2 pivot = rect.pivot;
            activityRect = new Rect(-size.x * pivot.x, -size.y * pivot.y, size.x, size.y);
        }
        else
        {
            float logicWidth = 1334f, logicHeight = 750f;
            float marginX = logicWidth * horizontalMargin, marginY = logicHeight * verticalMargin;
            activityRect = new Rect(marginX, marginY, logicWidth - 2 * marginX, logicHeight - 2 * marginY);
        }

        obstacleCollisionInfos.Clear();
        foreach (var obs in obstacles)
        {
            if (obs == null) continue;
            MarbleLotteryMachineObstacle comp = obs.GetComponent<MarbleLotteryMachineObstacle>();
            RectTransform rect = obs.GetComponent<RectTransform>();
            Vector2 worldPos = rect.position;
            Vector2 localPos = activityArea.ActivityArea.InverseTransformPoint(worldPos);
            float radius = obstacleRadius;
            if (comp != null && comp.CollisionArea != null)
            {
                RectTransform areaRect = comp.CollisionArea;
                radius = areaRect.rect.width * 0.5f;
                worldPos = areaRect.position;
                localPos = activityArea.ActivityArea.InverseTransformPoint(worldPos);
            }
            else radius = rect.rect.width * 0.5f;

            obstacleCollisionInfos.Add(new ObstacleCollisionInfo
            {
                center = localPos,
                radius = radius,
                obj = obs,
                component = comp
            });
        }

        finalAreaCollisionInfos.Clear();
        foreach (var area in endBlocks)
        {
            if (area == null) continue;
            MarbleLotteryMachineFinalArea comp = area.GetComponent<MarbleLotteryMachineFinalArea>();
            RectTransform rect = area.GetComponent<RectTransform>();
            Vector2 worldPos = rect.position;
            Vector2 size = rect.rect.size;
            Vector2 localPos = activityArea.ActivityArea.InverseTransformPoint(worldPos);
            Rect localRect = new Rect(localPos.x - size.x * 0.5f, localPos.y - size.y * 0.5f, size.x, size.y);
            if (comp != null && comp.CollisionArea != null)
            {
                RectTransform areaRect = comp.CollisionArea;
                worldPos = areaRect.position;
                localPos = activityArea.ActivityArea.InverseTransformPoint(worldPos);
                Vector2 areaSize = areaRect.rect.size;
                localRect = new Rect(localPos.x - areaSize.x * 0.5f, localPos.y - areaSize.y * 0.5f, areaSize.x, areaSize.y);
            }
            finalAreaCollisionInfos.Add(new FinalAreaCollisionInfo
            {
                rect = localRect,
                obj = area,
                component = comp
            });
        }
    }

    public void GeneratePath()
    {
        if (launcher != null && launcher.LaunchPos != null)
        {
            Vector2 worldPos = launcher.LaunchPos.position;
            startPoint = activityArea.ActivityArea.InverseTransformPoint(worldPos);
        }
        if (pathGenStopwatch == null) pathGenStopwatch = new Stopwatch();
        pathGenStopwatch.Reset();
        pathGenStopwatch.Start();
        isPathGenRecording = true;
        ClearPath();

        PathResult result = GenerateValidPath(trueEndIndex, ballRadius);
        pathGenStopwatch.Stop();
        isPathGenRecording = false;

        if (result.Success)
        {
            pathPoints.Clear();
            pathPoints.AddRange(result.Path);
            lastCollisionPoints.Clear();
            foreach (var rec in result.Records) lastCollisionPoints.Add(rec.position);
            lastCollisionRecords = result.Records;
            GenerateBezierPath();
            DrawPath();
            DrawCollisionMarkers(lastCollisionPoints);
            float seconds = (float)pathGenStopwatch.Elapsed.TotalSeconds;
            SetInfoText("✅ 成功生成路径！尝试次数: " + pathGenAttempts + "，耗时: " + seconds.ToString() + " 秒");
            Debug.Log("✅ 成功生成路径！尝试次数: " + pathGenAttempts + "，耗时: " + seconds.ToString() + " 秒");
        }
        else
        {
            SetInfoText("❌ 生成失败：尝试 " + pathGenAttempts + " 次");
            Debug.LogWarning("❌ 生成失败：尝试 " + pathGenAttempts + " 次");
        }
    }

    public PathResult GenerateValidPath(int targetIndex, float ballRadius)
    {
        int maxAttempts = 10000;
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            attempts++;
            PathResult physResult = GeneratePhysicalPath(targetIndex);
            CSLList<Vector2> path = physResult.Path;
            if (path == null || path.Count < 2) continue;
            Vector2 finalPos = path[path.Count - 1];
            if (CheckHitEnd(finalPos) == targetIndex)
            {
                pathGenAttempts = attempts;
                return new PathResult { Success = true, Path = path, Records = physResult.Records };
            }
        }
        return new PathResult { Success = false, Path = null, Records = null };
    }

    private PathResult GeneratePhysicalPath(int targetIndex)
    {
        CSLList<CollisionRecord> collisionRecords = new CSLList<CollisionRecord>();
        if (targetIndex < 0 || targetIndex >= finalAreaCollisionInfos.Count)
            return new PathResult { Success = false, Path = new CSLList<Vector2>(), Records = collisionRecords };

        Rect targetRect = finalAreaCollisionInfos[targetIndex].rect;
        float targetTop = targetRect.yMax;
        float targetLeft = targetRect.xMin;
        float targetRight = targetRect.xMax;
        Vector2 pos = startPoint;
        float dt = 0.02f;
        int steps = 3000;
        float minDist = 12f;

        Vector2 targetCenter = targetRect.center;
        Vector2 dirToTarget = (targetCenter - startPoint).normalized;
        float angleOffset = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(
            dirToTarget.x * Mathf.Cos(angleOffset) - dirToTarget.y * Mathf.Sin(angleOffset),
            dirToTarget.x * Mathf.Sin(angleOffset) + dirToTarget.y * Mathf.Cos(angleOffset)
        );
        if (dir.y >= 0) dir.y = -0.01f;
        dir.Normalize();
        Vector2 vel = dir * initialSpeed;
        CSLList<Vector2> path = new CSLList<Vector2>();
        path.Add(pos);

        for (int i = 0; i < steps; i++)
        {
            vel.y -= gravity * dt;
            Vector2 newPos = pos + vel * dt;

            bool hitObs = false;
            for (int obsIdx = 0; obsIdx < obstacleCollisionInfos.Count; obsIdx++)
            {
                var info = obstacleCollisionInfos[obsIdx];
                Vector2 toBall = newPos - info.center;
                float dist = toBall.magnitude;
                if (dist < ballRadius + info.radius && dist > 0.001f)
                {
                    hitObs = true;
                    Vector2 normal = toBall.normalized;
                    Vector2 contactPoint = info.center + normal * info.radius;
                    collisionRecords.Add(new CollisionRecord { position = contactPoint, obstacleIndex = obsIdx });
                    newPos = info.center + normal * (ballRadius + info.radius);
                    float velAlongNormal = Vector2.Dot(vel, normal);
                    if (velAlongNormal < 0)
                    {
                        Vector2 reflectVel = vel - normal * 2 * velAlongNormal;
                        vel = reflectVel * bounceFactor;
                    }
                    break;
                }
            }

            if (newPos.x - ballRadius < activityRect.xMin)
            {
                collisionRecords.Add(new CollisionRecord { position = new Vector2(activityRect.xMin, newPos.y), obstacleIndex = -1 });
                newPos.x = activityRect.xMin + ballRadius;
                if (vel.x < 0) vel.x *= -bounceFactor;
            }
            if (newPos.x + ballRadius > activityRect.xMax)
            {
                collisionRecords.Add(new CollisionRecord { position = new Vector2(activityRect.xMax, newPos.y), obstacleIndex = -1 });
                newPos.x = activityRect.xMax - ballRadius;
                if (vel.x > 0) vel.x *= -bounceFactor;
            }
            if (newPos.y + ballRadius > activityRect.yMax)
            {
                collisionRecords.Add(new CollisionRecord { position = new Vector2(newPos.x, activityRect.yMax), obstacleIndex = -1 });
                newPos.y = activityRect.yMax - ballRadius;
                if (vel.y > 0) vel.y = -Mathf.Abs(vel.y) * 0.2f;
                else vel.y = -1f;
            }
            if (newPos.y - ballRadius < activityRect.yMin)
            {
                collisionRecords.Add(new CollisionRecord { position = new Vector2(newPos.x, activityRect.yMin), obstacleIndex = -1 });
                newPos.y = activityRect.yMin + ballRadius;
                if (vel.y < 0) vel.y *= -bounceFactor;
                path.Add(newPos);
                break;
            }

            if (newPos.x >= targetLeft && newPos.x <= targetRight &&
                newPos.y >= targetTop - ballRadius && newPos.y <= targetTop + ballRadius)
            {
                path.Add(newPos);
                break;
            }

            pos = newPos;
            if (Distance(pos, path[path.Count - 1]) >= minDist) path.Add(pos);
            if (vel.magnitude < 1f && pos.y - ballRadius < activityRect.yMin + 30f) break;
            if (pos.y > activityRect.yMax + 100 || pos.y < activityRect.yMin - 100) break;
        }
        if (path.Count < 2) path.Add(new Vector2(startPoint.x, activityRect.yMin + ballRadius));
        return new PathResult { Success = true, Path = path, Records = collisionRecords };
    }

    private int CheckHitEnd(Vector2 localPos)
    {
        for (int i = 0; i < finalAreaCollisionInfos.Count; i++)
        {
            Rect rect = finalAreaCollisionInfos[i].rect;
            float targetTop = rect.yMax;
            float targetLeft = rect.xMin;
            float targetRight = rect.xMax;
            if (localPos.x >= targetLeft && localPos.x <= targetRight &&
                localPos.y >= targetTop - ballRadius && localPos.y <= targetTop + ballRadius)
                return i;
        }
        return -1;
    }

    void GenerateBezierPath()
    {
        if (pathPoints.Count < 2) return;
        bezierPoints.Clear();
        int segments = 20;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Vector2 p0 = pathPoints[i];
            Vector2 p1 = pathPoints[i + 1];
            Vector2 dir = (p1 - p0).normalized;
            float dist = Distance(p0, p1);
            Vector2 cp1 = p0 + dir * 0.3f * dist;
            Vector2 cp2 = p1 - dir * 0.3f * dist;
            for (int j = 0; j <= segments; j++)
            {
                float t = j / (float)segments;
                bezierPoints.Add(BezierCurve(p0, cp1, cp2, p1, t));
            }
        }
        finalPath = new CSLList<Vector2>(bezierPoints);
        if (finalPath.Count > 0) finalPath[0] = startPoint;
    }

    Vector2 BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1 - t;
        return p0 * u * u * u + p1 * 3 * u * u * t + p2 * 3 * u * t * t + p3 * t * t * t;
    }

    void DrawPath()
    {
        if (pathLine == null)
        {
            GameObject lineObj = new GameObject("PathLine");
            lineObj.transform.SetParent(gameContainer.transform, false);
            pathLine = lineObj.AddComponent<LineRenderer>();
            pathLine.material = new Material(Shader.Find("Sprites/Default"));
            pathLine.startColor = Color.cyan;
            pathLine.endColor = Color.cyan;
            pathLine.useWorldSpace = true;
            pathLine.positionCount = 0;
            pathLine.sortingOrder = 3000;
            pathLine.startWidth = pathLine.endWidth = 0.03f;
        }
        if (finalPath == null || finalPath.Count < 2) { pathLine.positionCount = 0; return; }
        pathLine.positionCount = finalPath.Count;
        for (int i = 0; i < finalPath.Count; i++)
        {
            Vector2 worldPos = activityArea.ActivityArea.TransformPoint(finalPath[i]);
            pathLine.SetPosition(i, new Vector3(worldPos.x, worldPos.y, 0));
        }
    }

    void DrawCollisionMarkers(CSLList<Vector2> points)
    {
        foreach (var marker in collisionMarkers) if (marker != null) GameObject.Destroy(marker);
        collisionMarkers.Clear();
        if (points == null) return;
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 localPos = points[i];
            Vector2 worldPos = activityArea.ActivityArea.TransformPoint(localPos);
            GameObject marker = CreateCircle("CollisionMarker", Color.red, 6f);
            RectTransform markerRect = marker.GetComponent<RectTransform>();
            markerRect.position = worldPos;
            markerRect.localPosition = new Vector3(markerRect.localPosition.x, markerRect.localPosition.y, 0);
            collisionMarkers.Add(marker);
            GameObject labelObj = new GameObject("MarkerLabel");
            labelObj.transform.SetParent(marker.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            Text label = labelObj.AddComponent<Text>();
            label.text = (i + 1).ToString();
            label.fontSize = 12;
            label.font = GetFont();
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
        }
    }

    public void ClearPath()
    {
        if (pathLine != null) pathLine.positionCount = 0;
        finalPath.Clear();
        foreach (var marker in collisionMarkers) if (marker != null) GameObject.Destroy(marker);
        collisionMarkers.Clear();
        lastCollisionPoints.Clear();
        lastCollisionRecords.Clear();
    }
}