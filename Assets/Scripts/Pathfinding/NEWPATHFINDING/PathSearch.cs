using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PathSearch
{
    public static bool GetPathToTarget(Vector3 startPoint, Vector3 endPoint, World worldData, SPathFinderType pfType, out List<PathPoint> pathPoints, int maxNodes = 1000)
    {
        Vector3Int intStartPoint = Vector3Int.RoundToInt(startPoint);
        Vector3Int intEndPoint = Vector3Int.RoundToInt(endPoint);
        List<PathPoint> path = new List<PathPoint>();
        List<PathPoint> openPoints = new List<PathPoint>();
        List<PathPoint> closedPoints = new List<PathPoint>();
        openPoints.Add(NewPathPoint(intStartPoint, 0, Vector3Int.Distance(intStartPoint, intEndPoint), EMoveAction.walk));
        closedPoints.Add(openPoints[0]);
        openPoints = ClosePoint(0, openPoints, closedPoints, worldData, pfType, intEndPoint);
        bool pathFound = false;
        float maxEstimatePath = 1500;

        while (true)
        {
            int minIndex = GetMinEstimate(openPoints);

            if (openPoints.Count > 0)
                if (openPoints[minIndex].estimateFullPathLenght < maxEstimatePath)
                {
                    closedPoints.Add(openPoints[minIndex]);
                    openPoints = ClosePoint(minIndex, openPoints, closedPoints, worldData, pfType, intEndPoint);
                }
                else
                {
                    closedPoints.Add(openPoints[minIndex]);
                    openPoints.RemoveAt(minIndex);
                }

            if (EndFound(closedPoints))
            {
                //Debug.Log("Finished!");
                path = GetPathToTarget(closedPoints);
                pathFound = true;
                break;
            }

            if (openPoints.Count <= 0)
            {
                pathFound = false;
                break;
            }
            if ((openPoints.Count >= maxNodes) ||(closedPoints.Count >= maxNodes))
            {
                pathFound = false;
                break;
            }
        }

        Debug.Log("Nodes created " + closedPoints.Count.ToString());

        DrawPath(openPoints, Color.green, 0.5f);
        DrawPath(closedPoints, Color.blue, 0.5f);
        DrawPath(path, Color.red, 0.5f);

        pathPoints = path;
        return pathFound;
    }

    private static PathPoint NewPathPoint(Vector3Int point, float pathLenghtFromStart, float heuristicEstimatePathLenght, EMoveAction moveAction)
    {
        PathPoint a = new PathPoint();
        a.point = point;
        a.pathLenghtFromStart = pathLenghtFromStart;
        a.heuristicEstimatePathLenght = heuristicEstimatePathLenght;
        a.moveAction = moveAction;
        return a;
    }
    private static PathPoint NewPathPoint(Vector3Int point, float pathLenghtFromStart, float heuristicEstimatePathLenght, EMoveAction moveAction, PathPoint ppoint)
    {
        PathPoint a = new PathPoint();
        a.point = point;
        a.pathLenghtFromStart = pathLenghtFromStart;
        a.heuristicEstimatePathLenght = heuristicEstimatePathLenght;
        a.moveAction = moveAction;
        a.cameFrom = ppoint;
        return a;
    }

    private static int GetMinEstimate(List<PathPoint> points)
    {
        int min = 0;
        for (int i = 0; i<points.Count; i++)
        {
            if (points[i].estimateFullPathLenght<points[min].estimateFullPathLenght)
                min = i;
        }
        return min;
    }

    public static void DrawPath(List<PathPoint> points, Color c, float time)
    {
        for (int i = 0; i<points.Count; i++)
        {
            if (points[i].cameFrom != null)
                Debug.DrawLine(points[i].point, points[i].cameFrom.point, c, time);
        }
    }

    private static bool EndFound(List<PathPoint> points){
        for (int i = 0; i<points.Count; i++)
        {
            if (points[i].heuristicEstimatePathLenght <= 0)
                return true;
        }
        return false;
    }

    private static List<PathPoint> GetPathToTarget(List<PathPoint> points)
    {
        List<PathPoint> path = new List<PathPoint>();
        int targetIndex = 0;
        for (int i = 0; i<points.Count; i++)
        {
            if (points[i].heuristicEstimatePathLenght <= 0)
                targetIndex = i;
        }

        PathPoint ppoint = new PathPoint();
        ppoint = points[targetIndex];
        while (ppoint.pathLenghtFromStart > 0)
        {
            path.Add(ppoint);
            ppoint = ppoint.cameFrom;
        }
        path.Reverse();
        return path;
    }

    private static List<PathPoint> ClosePoint(int index, List<PathPoint> openPoints, List<PathPoint> closedPoints, World world, SPathFinderType pfType, Vector3Int targetPoint)
    {
        List<PathPoint> newOpenPoints = openPoints;
        PathPoint lastPoint = openPoints[index];

        if (SizeCheck(new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z), pfType.characterHeight, world) 
        && VerticalCheck(ref lastPoint, pfType.maxFallDistance, pfType.jumpHeight, world))
        {
            // ---------------------------------------------------------------//north//   /|\//    |// 
            if (!closedPoints.Any(point => point.point == new Vector3Int(lastPoint.point.x + 1, lastPoint.point.y, lastPoint.point.z))
            && (!newOpenPoints.Any(point => point.point == new Vector3Int(lastPoint.point.x + 1, lastPoint.point.y, lastPoint.point.z)))
            && (SizeCheck(new Vector3Int(lastPoint.point.x + 1, lastPoint.point.y, lastPoint.point.z), pfType.characterHeight, world)))
            {
                newOpenPoints.Add(NewPathPoint(new Vector3Int(lastPoint.point.x + 1, lastPoint.point.y, lastPoint.point.z)
                , lastPoint.pathLenghtFromStart + 1
                , Vector3Int.Distance(new Vector3Int(lastPoint.point.x + 1, lastPoint.point.y, lastPoint.point.z), targetPoint)
                , EMoveAction.walk
                , lastPoint));
            }

            // south//    |//   \|///
            if (!closedPoints.Any(point => point.point == new Vector3Int(lastPoint.point.x - 1, lastPoint.point.y, lastPoint.point.z))
            && (!newOpenPoints.Any(point => point.point == new Vector3Int(lastPoint.point.x - 1, lastPoint.point.y, lastPoint.point.z)))
            && (SizeCheck(new Vector3Int(lastPoint.point.x - 1, lastPoint.point.y, lastPoint.point.z), pfType.characterHeight, world)))
            {
                newOpenPoints.Add(NewPathPoint(new Vector3Int(lastPoint.point.x - 1, lastPoint.point.y, lastPoint.point.z)
                , lastPoint.pathLenghtFromStart + 1
                , Vector3Int.Distance(new Vector3Int(lastPoint.point.x - 1, lastPoint.point.y, lastPoint.point.z), targetPoint)
                , EMoveAction.walk
                , lastPoint));
            }
            // east//    ---->//   //
            if (!closedPoints.Any(point => point.point == new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z + 1))
            && (!newOpenPoints.Any(point => point.point == new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z + 1)))
            && (SizeCheck(new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z + 1), pfType.characterHeight, world)))
            {
                newOpenPoints.Add(NewPathPoint(new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z + 1)
                , lastPoint.pathLenghtFromStart + 1
                , Vector3Int.Distance(new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z + 1), targetPoint)
                , EMoveAction.walk
                , lastPoint));
            }
            // west//    <----//   //
            if (!closedPoints.Any(point => point.point == new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z - 1))
            && (!newOpenPoints.Any(point => point.point == new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z - 1)))
            && (SizeCheck(new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z - 1), pfType.characterHeight, world)))
            {
                newOpenPoints.Add(NewPathPoint(new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z - 1)
                , lastPoint.pathLenghtFromStart + 1
                , Vector3Int.Distance(new Vector3Int(lastPoint.point.x, lastPoint.point.y, lastPoint.point.z - 1), targetPoint)
                , EMoveAction.walk
                , lastPoint));
            }
        }
        newOpenPoints.RemoveAt(index);
        return newOpenPoints;
    }

    private static bool SizeCheck(Vector3Int position, int characterHeight, World world)
    {
        for (int i = 0; i < characterHeight; i++)
        {
            BlockType blockType = WorldDataHelper.GetBlock(world, position);
            if (blockType != BlockType.Air && blockType != BlockType.Soft_Barrier)
            {
                return false;
            }   
        }
        return true;
    }

    private static bool VerticalCheck(ref PathPoint pathpoint, int maxFallDistance, int jumpHeight, World world)
    {
        Vector3Int position = pathpoint.point;

        BlockType blockType = WorldDataHelper.GetBlock(world, position);

        if (blockType != BlockType.Air && blockType != BlockType.Soft_Barrier) // Current block is filled so check for jump height
        {
            for (int i = 1; i <= jumpHeight; i++)
            {
                blockType = WorldDataHelper.GetBlock(world, position + new Vector3Int(0, i, 0));
                if (blockType == BlockType.Air || blockType == BlockType.Soft_Barrier)
                {
                    pathpoint.point += new Vector3Int(0, i, 0);
                    return true;
                }
            }
        }
        else // Current block is air so check for fall distance
        {
            for (int i = 1; i <= maxFallDistance; i++)
            {
                blockType = WorldDataHelper.GetBlock(world, position - new Vector3Int(0, i, 0));
                if (blockType != BlockType.Air && blockType != BlockType.Soft_Barrier)
                {
                    pathpoint.point -= new Vector3Int(0, i - 1, 0);
                    return true;
                }
            }
        }
        return false; // The agent cannot fall/jump to this difference in vertical height
    }
}

public struct SPathFinderType
{
    public bool walk, jump, fall;
    public int maxFallDistance, jumpHeight, jumpDistance;
    public int characterHeight;
    public static SPathFinderType normal()
    {
        SPathFinderType n = new SPathFinderType();
        n.walk = true;
        n.jump = true;
        n.fall = true;
        n.maxFallDistance = 1;
        n.jumpHeight = 1;
        n.jumpDistance = 0;
        n.characterHeight = 2;
        return n;
    }
}

