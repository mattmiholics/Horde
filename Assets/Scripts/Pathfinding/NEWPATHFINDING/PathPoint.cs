using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EMoveAction { walk, jump, fall, swim };
public class PathPoint
{
    public Vector3Int point { get; set; }
    public float pathLenghtFromStart { get; set; }
    public float heuristicEstimatePathLenght { get; set; }
    public float estimateFullPathLenght
    {
        get
        {
            return this.heuristicEstimatePathLenght + this.pathLenghtFromStart / 10;
        }
    }
    public EMoveAction moveAction = EMoveAction.walk;
    public PathPoint cameFrom;
}
