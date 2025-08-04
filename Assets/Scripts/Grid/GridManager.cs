using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public Vector2Int gridSize;
    public float cellSize = 1f;
    public GameObject gridCellVisualPrefab;
    public Material validMaterial;
    public Material invalidMaterial;

    public Vector3 originPosition;

    private GridCell[,] grid;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeGrid(gridSize, cellSize);

        PlaceExistingCounters();
    }

    public void InitializeGrid(Vector2Int size, float cellSize)
    {
        this.gridSize = size;
        this.cellSize = cellSize;
        grid = new GridCell[size.x, size.y];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
                grid[x, y] = new GridCell
                {
                    coordinates = new Vector2Int(x, y),
                    isWalkable = true,
                    isOccupied = false,
                    worldPosition = worldPos,
                    placedObject = null
                };

                //Debug.Log("GridCellpos : "+ grid[x, y].worldPosition);
            }
        }
    }

    public GridCell GetCellFromWorldPosition(Vector3 position)
    {
        Vector3 relativePos = position - originPosition;

        int x = Mathf.FloorToInt(relativePos.x / cellSize);
        int y = Mathf.FloorToInt(relativePos.z / cellSize);

        if (x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y)
        {
            return grid[x, y];
        }

        return null;
    }


    public List<GridCell> GetValidPlacementCells(GameObject prefab)
    {
        List<GridCell> validCells = new List<GridCell>();

        foreach (var cell in grid)
        {
            if (!cell.isOccupied && cell.isWalkable)
            {
                validCells.Add(cell);
            }
        }

        return validCells;
    }

    public void HighlightCells(List<GridCell> cells, bool valid)
    {
        foreach (var cell in cells)
        {
            GameObject visual = Instantiate(gridCellVisualPrefab, cell.worldPosition, Quaternion.identity);
            visual.GetComponent<Renderer>().material = valid ? validMaterial : invalidMaterial;
        }
    }

    public Vector3 GridToWorld(Vector2Int coordinates)
    {
        return  originPosition + new Vector3(coordinates.x * cellSize, 0, coordinates.y * cellSize);
    }

    public GridCell GetCellAt(int x, int z)
    {
        if (x >= 0 && x < gridSize.x && z >= 0 && z < gridSize.y)
            return grid[x, z];
        return null;
    }

    void PlaceExistingCounters()
    {
        BaseCounter[] counters = Object.FindObjectsByType(
                    typeof(BaseCounter),
                    findObjectsInactive: FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                ) as BaseCounter[];


        foreach (BaseCounter counter in counters)
        {
            Vector3 pos = counter.transform.position;
            int x = Mathf.RoundToInt((pos.x - originPosition.x) / cellSize);
            int z = Mathf.RoundToInt((pos.z - originPosition.z) / cellSize);

            GridCell cell = GetCellAt(x, z);
            if (cell != null)
            {
                cell.isOccupied = true;
                cell.placedObject = counter.gameObject;
            }
        }
    }
}
