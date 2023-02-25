using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(LineRenderer))]
public class PathLineRenderer : MonoBehaviour
{
    [ShowIf("@!UnityEngine.Application.isPlaying")]
    public bool useStatic = false;
    [ShowIf("@useStatic")]
    public StaticAgent targetStaticAgent;
    [ShowIf("@!useStatic")]
    public Agent targetAgent;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        if (useStatic)
            targetStaticAgent.PathUpdated += UpdateLine;
        else
            targetAgent.PathUpdated += UpdateLine;
    }

    private void OnDisable()
    {
        if (useStatic)
            targetStaticAgent.PathUpdated -= UpdateLine;
        else
            targetAgent.PathUpdated -= UpdateLine;
    }

    public void UpdateLine(List<PathPoint> pathPoints)
    {
        UpdateLine(pathPoints.Select(p => (Vector3)p.point).ToArray());
    }

    public void UpdateLine(Vector3[] pathVectors)
    {
        float offset = 0.35f; // A value between 0-0.5

        List<Vector3> tempList = new List<Vector3>();

        // For each every two vectors
        for (int i = 1; i <= pathVectors.Length; i++)
        {
            Vector3 firstVector = pathVectors[i - 1];

            // If this index is the end
            if (i == pathVectors.Length)
            {
                // Lower the first vector by the offset and break
                firstVector += Vector3.down * offset;
                tempList.Add(firstVector);
                break;
            }

            Vector3 lastVector = pathVectors[i];

            // Check for height difference
            if (firstVector.y != lastVector.y)
            {
                Vector3 difference = lastVector - firstVector;
                Vector3 lowerHeightVector = firstVector.y < lastVector.y ? firstVector : lastVector;
                int direction = firstVector.y < lastVector.y ? 1 : -1;

                // Create new vectors with the height change difference and offsets towards the lower height vector
                Vector3 subVector1 = new Vector3(lowerHeightVector.x + difference.x * offset * direction, firstVector.y, lowerHeightVector.z + difference.z * offset * direction);
                Vector3 subVector2 = new Vector3(lowerHeightVector.x + difference.x *offset * direction, lastVector.y, lowerHeightVector.z + difference.z * offset * direction);

                // Lower the first three points by the offset (exclude last vector because it will be evaluated in the next iteration)
                firstVector += Vector3.down * offset;
                subVector1 += Vector3.down * offset;
                subVector2 += Vector3.down * offset;

                // Add the points to the list
                tempList.Add(firstVector);
                tempList.Add(subVector1);
                tempList.Add(subVector2);
            }
            else
            {
                // Lower the first vector by the offset
                firstVector += Vector3.down * offset;
                tempList.Add(firstVector);
            }
        }

        pathVectors = tempList.ToArray();

        lineRenderer.positionCount = pathVectors.Length;
        lineRenderer.SetPositions(pathVectors);
    }
}
