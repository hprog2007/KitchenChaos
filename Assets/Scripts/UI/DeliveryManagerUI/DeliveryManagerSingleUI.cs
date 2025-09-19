using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;
    [SerializeField] private Transform clockHandleRed;
    [SerializeField] private TextMeshProUGUI remainingTimeText;

    [SerializeField] private UILineRenderer line;
    [SerializeField] private RectTransform sparksRectTransform;

    private float sparkStartX; //starting point for fuse spark particle
    private float sparkStartY;
    private OrderTicket ticket;

    public void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
        sparkStartX = sparksRectTransform.anchoredPosition.x;
        sparkStartY = sparksRectTransform.anchoredPosition.y;
        

    }

    private void Start()
    {
    }

    public void Bind(OrderTicket orderTicket)
    {
        //bind to OrderTicket
        ticket = orderTicket;
        //init fuse points
        line.Points = GeneratePoints(line, (int)ticket.RemainingTime);
        //set recipe
        SetRecipeSO(orderTicket.Recipe);        

        // Set initial values
        OnTick(ticket);  // Update immediately with the current time values

    }

    // This method is called by DeliveryManagerUI on each tick that triggered by OrderManager
    // Method to update the timer visuals (called on every tick)
    public void OnTick(OrderTicket orderTicket)
    {
        ////////////// update timer text and clock handle position
        remainingTimeText.text = ((int)orderTicket.RemainingTime).ToString() ;
        // Rotate the clock hand each frame based on the time passed (1 second = full tick)
        float rotationAmount = 360f / 60f * Time.deltaTime;
        clockHandleRed.Rotate(0f, 0f, rotationAmount);

        UpdateFuseLength();
        UpdateSparkPosition();
    }


    // Set the recipe UI (unchanged from your original method)
    public void SetRecipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.recipeName;

        // Clear any existing icons
        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }

        // Add the icons for each ingredient
        foreach (KitchenObjectSO kitchenObjectSO in recipeSO.kitchenObjectSOList)
        {
            Transform iconTranform = Instantiate(iconTemplate, iconContainer);
            iconTranform.gameObject.SetActive(true);
            iconTranform.GetComponent<Image>().sprite = kitchenObjectSO.sprite;
        }
    }

    //Redistribute points over fuse (UILineRenderer) based on OrderTicket CumulativeTime
    //UILineRenderer is divided to totalPoints then we can resize the line by removing the points
    public void GeneratePoints(int totalPoints)
    {
        if (line.Points == null || line.Points.Length < 2)
        {
            Debug.LogError("At least 2 control points are required!");
            return;
        }

        // Calculate the number of segments (between control points)
        int segments = line.Points.Length - 1;

        // Calculate how many points per segment (distribute totalPoints across segments)
        int pointsPerSegment = Mathf.Max(1, totalPoints / segments);
        totalPoints = pointsPerSegment * segments; // Adjust total points to be divisible by segments

        // Generate points using Lerp
        Vector2[] generatedPoints = new Vector2[totalPoints];
        int pointIndex = 0;

        for (int i = 0; i < segments; i++)
        {
            Vector2 startPoint = line.Points[i];
            Vector2 endPoint = line.Points[i + 1];

            // Generate points for the current segment
            for (int j = 0; j < pointsPerSegment; j++)
            {
                // Calculate interpolation parameter t (0 to 1)
                float t = (float)j / (pointsPerSegment - 1);
                if (pointsPerSegment == 1) t = 0; // Handle single-point segments

                // Interpolate between start and end points
                Vector2 interpolatedPoint = Vector2.Lerp(startPoint, endPoint, t);
                generatedPoints[pointIndex] = new Vector2(interpolatedPoint.x, interpolatedPoint.y);
                pointIndex++;
            }
        }

        // Assign the generated points to the Line Renderer
        line.Points = generatedPoints;
    }

    /// <summary>
    /// Returns 'totalPoints' points spaced at equal arc-length intervals
    /// along the shape drawn by the given UILineRenderer.
    /// Works with Bezier enabled (cubic chain) or plain polylines.
    /// Output points are in the UILineRenderer's local (UI) space.
    /// </summary>
    public static Vector2[] GeneratePoints(UILineRenderer uiLine, int totalPoints)
    {
        if (uiLine == null) { Debug.LogWarning("GeneratePoints: uiLine is null"); return null; }
        if (totalPoints < 2) { Debug.LogWarning("GeneratePoints: totalPoints must be >= 2"); return null; }

        // 1) Build a dense polyline that closely matches what UILineRenderer draws.
        //    If Bezier is on: treat points as a *cubic Bezier chain* P0,P1,P2,P3 | P3,P4,P5,P6 | ...
        //    Step size uses uiLine.BezierSegmentsPerCurve when available; else default to 32 samples per cubic.
        var src = uiLine.Points; // Unity UI Extensions exposes 'Points' as Vector2[]
        if (src == null || src.Length == 0) return new Vector2[0];

        List<Vector2> dense = new List<Vector2>(1024);

        bool bezierOn = uiLine.BezierMode != UILineRenderer.BezierType.None; // true when "Bezier" toggle is enabled
        int segsPerCurve = Mathf.Max(4, uiLine.BezierSegmentsPerCurve); // fallback in case user set tiny values

        if (bezierOn && src.Length >= 4)
        {
            // Treat as cubic chain: (0,1,2,3), then (3,4,5,6), step by 3 until we run out.
            for (int i = 0; i <= src.Length - 4; i += 3)
            {
                Vector2 p0 = src[i];
                Vector2 p1 = src[i + 1];
                Vector2 p2 = src[i + 2];
                Vector2 p3 = src[i + 3];

                // Sample this cubic segment densely
                for (int s = 0; s < segsPerCurve; s++)
                {
                    float t = s / (float)segsPerCurve; // [0, 1)
                    dense.Add(EvalCubic(p0, p1, p2, p3, t));
                }
            }
            // Ensure we add the final endpoint exactly once
            dense.Add(src[src.Length - 1]);
        }
        else
        {
            // Polyline mode: use given points as-is
            dense.AddRange(src);
        }

        // Guard: if dense has fewer than 2 distinct points, return duplicates
        if (dense.Count < 2)
        {
            var single = dense.Count == 1 ? dense[0] : Vector2.zero;
            var outArr = new Vector2[totalPoints];
            for (int k = 0; k < totalPoints; k++) outArr[k] = single;
            return outArr;
        }

        // 2) Build cumulative arc-length table over the dense polyline
        int M = dense.Count;
        float[] cum = new float[M];
        cum[0] = 0f;
        float totalLen = 0f;
        for (int i = 1; i < M; i++)
        {
            float d = Vector2.Distance(dense[i - 1], dense[i]);
            totalLen += d;
            cum[i] = totalLen;
        }

        if (totalLen <= 1e-6f)
        {
            // Degenerate: all points coincide
            var outArr = new Vector2[totalPoints];
            for (int k = 0; k < totalPoints; k++) outArr[k] = dense[0];
            return outArr;
        }

        // 3) Even spacing targets (open curve: include both endpoints)
        Vector2[] result = new Vector2[totalPoints];
        float step = totalLen / (totalPoints - 1);

        int j = 0; // segment index over dense polyline
        for (int k = 0; k < totalPoints; k++)
        {
            float target = Mathf.Min(k * step, totalLen);

            // Advance j until cum[j] <= target <= cum[j+1]
            while (j < M - 2 && cum[j + 1] < target) j++;

            float segLen = Mathf.Max(cum[j + 1] - cum[j], 1e-8f);
            float t = (target - cum[j]) / segLen; // [0,1]
            result[k] = Vector2.Lerp(dense[j], dense[j + 1], t);
        }

        return result;
    }

    // Cubic Bezier evaluation (De Casteljau)
    private static Vector2 EvalCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 a = Vector2.Lerp(p0, p1, t);
        Vector2 b = Vector2.Lerp(p1, p2, t);
        Vector2 c = Vector2.Lerp(p2, p3, t);
        Vector2 d = Vector2.Lerp(a, b, t);
        Vector2 e = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(d, e, t);
    }


    //shorten the fuse (UILineRenderer) point by point
    private void UpdateFuseLength()
    {
        if (line.Points.Length > 3)
        {
            Vector2[] currentPoints = new Vector2[line.Points.Length - 1];
            for (int i = 1; i < line.Points.Length; i++)
            {
                currentPoints[i - 1] = line.Points[i];
            }
            line.Points = currentPoints;
        }
    }

    // update fuse spark position
    private void UpdateSparkPosition()
    {
        //Update spark particle position
        sparksRectTransform.anchoredPosition = new Vector2(sparkStartX + line.Points[0].x,
                    sparkStartY + line.Points[0].y);
    }

}
