using UnityEngine;
using UnityEngine.UI.Extensions;

public class MoveAlongLine : MonoBehaviour
{
    public UILineRenderer lineRenderer;
    public float moveSpeed = 5f;
    public bool loop = false; // Optional: loop back to the start

    private Vector2[] positions;
    private int currentPositionIndex = 0;
    private bool moving = false;

    void Start()
    {
        if (lineRenderer == null)
        {
            Debug.LogError("Line Renderer is not assigned!");
            enabled = false;
            return;
        }
        positions = new Vector2[lineRenderer.Points.Length];
        positions = lineRenderer.Points;
        StartMoving();
    }

    void Update()
    {
        if (moving)
        {
            MoveObject();
        }
    }

    public void StartMoving()
    {
        moving = true;
        currentPositionIndex = 0; // Start from the beginning
    }

    public void StopMoving()
    {
        moving = false;
    }

    void MoveObject()
    {
        if (currentPositionIndex >= positions.Length)
        {
            if (loop)
            {
                currentPositionIndex = 0; // Reset to the start
            }
            else
            {
                StopMoving(); // Stop at the end
                return;
            }
        }

        Vector3 targetPosition = lineRenderer.transform.TransformPoint(positions[currentPositionIndex]);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if we've reached the current target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentPositionIndex++; // Move to the next position
        }
    }

    // Optional: Visualize the path using Gizmos (for debugging)
    void OnDrawGizmos()
    {
        if (lineRenderer != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < lineRenderer.Points.Length; i++)
            {
                Gizmos.DrawSphere(lineRenderer.transform.TransformPoint(positions[i]), 0.2f);
            }
        }
    }
}