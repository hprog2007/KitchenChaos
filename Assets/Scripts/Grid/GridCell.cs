using System;
using UnityEngine;

[Serializable]
public class GridCell
{
    public Vector2Int coordinates;
    public Vector3 worldPosition;
    public bool isWalkable;
    public bool isOccupied;
    public GameObject placedObject;
}
