using DG.Tweening.Plugins.Core.PathCore;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class OrderDynamiteTimer : MonoBehaviour
{
    [SerializeField] private UILineRenderer line;
    [SerializeField] private RectTransform sparksRectTransform;
    [SerializeField] private int i = 0;

    private float sparkStartX;
    private float sparkStartY;
    private OrderTicket ticket;

    private float serveTime;
    private float currentTimer = 1f;

    private void Awake()
    {
        ticket = null;
        serveTime = 0f;
    }

    private void Start()
    {
        DeliveryManager.instance.OnOrderSpawned += DeliveryManager_OnOrderSpawned;
        sparkStartX = sparksRectTransform.anchoredPosition.x;    
        sparkStartY = sparksRectTransform.anchoredPosition.y;    
    }

    private void DeliveryManager_OnOrderSpawned(OrderTicket _ticket)
    {
        if (this.ticket != null)
            return;

        this.ticket = _ticket;
        serveTime = this.ticket.CumulativeDuration;
        GeneratePoints((int)serveTime);
    }

    private void OnDestroy()
    {
        DeliveryManager.instance.OnOrderSpawned -= DeliveryManager_OnOrderSpawned;
    }



    private void Update()
    {
        if (ticket == null)
            return;

        //on every second do the job
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            if (serveTime >= 2)
            {
                sparksRectTransform.anchoredPosition = new Vector2(sparkStartX + line.Points[0].x,
                    sparkStartY + line.Points[0].y);

                currentTimer = 1f;
                serveTime--;
                RemoveFirstPoint();
            }
            else
            {
                transform.gameObject.SetActive(false);  
            }
        }
    }

    private void RemoveFirstPoint()
    {
        if (line.Points.Length > 1)
        {
            Vector2[] currentPoints = new Vector2[line.Points.Length - 1];
            for (int i = 1; i < line.Points.Length; i++)
            {
                currentPoints[i-1] = line.Points[i];
            }
            line.Points = currentPoints;
        }
    }

    void GeneratePoints(int totalPoints)
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
}
