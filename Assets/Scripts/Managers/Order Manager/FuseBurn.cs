using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class FuseBurn : MonoBehaviour
{
    public RectTransform[] waypoints;   // TIP -> DYNAMITE
    public RectTransform spark;         // mover
    public int duration = 32;           // seconds (also number of steps)
    public UILineRenderer lineRenderer;
    public float trimPoints = 1f; //for UI LineRenderer
    public Transform explostion;

    private float moveSpeed = 1f;


    private void Awake()
    {
        //explostion.gameObject.SetActive(false);
    }

    void OnEnable() { StartCoroutine(Burn()); }

    IEnumerator Burn()
    {
        if (spark == null || waypoints == null || waypoints.Length < 2 || duration < 1)
            yield break;

        var pointsByDuration = GenerateSparksWorldPointsByDuration();
        GenerateUILineRendererPointsByDuration();

        // Move once per second
        for (int i = 0; i < pointsByDuration.Length; i++)
        {
            lineRenderer.Points = TrimStartByPoints(lineRenderer.Points, (int)trimPoints); //trim from tip of fuse line renderer
            spark.position = pointsByDuration[i]; //move particle effect
            yield return new WaitForSeconds(1f);
        }

        //explosion
        //explostion.gameObject.SetActive(true);
        //Destroy(explostion.gameObject, .5f);
    }

    //Trim Points of UILineRenderer
    public static Vector2[] TrimStartByPoints(Vector2[] pts, int trimPoints)
    {
        var res = new Vector2[pts.Length - trimPoints];
        for (int i = trimPoints, k = 0; i < pts.Length; i++, k++)
        {
            res[k] = pts[i];
        }
        return res;
    }

    private Vector3[] GenerateSparksWorldPointsByDuration()
    {
        // Collect world points
        var worldPoints = new Vector3[waypoints.Length];
        for (int i = 0; i < worldPoints.Length; i++)
        {
            worldPoints[i] = waypoints[i].position;
        }

        // Segment lengths + total
        var distanceSegments = new float[worldPoints.Length - 1];
        float totalDistance = 0f;
        for (int i = 0; i < distanceSegments.Length; i++)
        {
            distanceSegments[i] = Vector3.Distance(worldPoints[i], worldPoints[i + 1]);
            totalDistance += distanceSegments[i];
        }

        // Generate equally spaced points (duration+1: includes start & end) for Sparks Position       
        var pointsByDuration = new Vector3[duration + 1];
        pointsByDuration[0] = worldPoints[0];  //First Point
        pointsByDuration[duration] = worldPoints[worldPoints.Length - 1]; //Last Point
        float step = totalDistance / duration;
        int k = 0;
        float walked = 0f;

        for (int i = 1; i < duration; i++)
        {
            float target = step * i;
            while (k < distanceSegments.Length - 1 && walked + distanceSegments[k] < target)
            {
                walked += distanceSegments[k];
                k++;
            }

            float t = distanceSegments[k] > 0f ? (target - walked) / distanceSegments[k] : 0f;
            pointsByDuration[i] = Vector3.Lerp(worldPoints[k], worldPoints[k + 1], t);
        }

        return pointsByDuration;
    }

    private void GenerateUILineRendererPointsByDuration()
    {
        // Segment lengths + total for line renderer
        var distanceSegments = new float[lineRenderer.Points.Length - 1];
        float totalDistance = 0f;
        for (int i = 0; i < distanceSegments.Length; i++)
        {
            distanceSegments[i] = Vector2.Distance(lineRenderer.Points[i], lineRenderer.Points[i + 1]);
            totalDistance += distanceSegments[i];
        }

        // Generate equally spaced points (duration+1: includes start & end) for LineRenderer Points
        var lineRendererPointsByDuration = new Vector2[duration + 1];
        lineRendererPointsByDuration[0] = lineRenderer.Points[0];  //First Point
        lineRendererPointsByDuration[duration] = lineRenderer.Points[lineRenderer.Points.Length - 1]; //Last Point
        float stepLinerenderer = totalDistance / duration;
        int k = 0;
        float walked = 0f;

        for (int i = 1; i < duration; i++)
        {
            float target = stepLinerenderer * i;
            while (k < distanceSegments.Length - 1 && walked + distanceSegments[k] < target)
            {
                walked += distanceSegments[k];
                k++;
            }

            float t = distanceSegments[k] > 0f ? (target - walked) / distanceSegments[k] : 0f;
            lineRendererPointsByDuration[i] = Vector2.Lerp(lineRenderer.Points[k], lineRenderer.Points[k + 1], t);
        }

        lineRenderer.Points = lineRendererPointsByDuration;

    }
}
