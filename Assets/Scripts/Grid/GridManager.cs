using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor; // for Handles (labels)
#endif

[System.Serializable]
public struct TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;
}

[System.Serializable]
public struct GridItemDTO
{
    public string kind;          // "counter" | "helper" | "decor" ... (type discriminator)
    public string prefabKey;     // must match PrefabRegistry entry
    public int cellX, cellY;     // or a single cellIndex if you prefer
    public TransformData transform;

    // version for migrations
    public int version;

    // object-specific payload as JSON (health, level, inventory, timers, etc.)
    public string customJson;    // nullable/empty OK
}

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [SerializeField] private PrefabRegistry registry;

    [Header("Grid Settings")]
    public Vector2Int gridSize;
    public float cellSize = 1f;
    public Vector3 originPosition;

    [Header("Visuals (runtime)")]
    public GameObject gridCellVisualPrefab;
    public Material validMaterial;
    public Material invalidMaterial;

    // --- Scene View Grid (Gizmos) ---
    [Header("Scene View Grid (Gizmos)")]
    public bool showGrid = true;
    public bool onlyWhenSelected = false;
    public Color gridLineColor = new Color(0.75f, 0.75f, 0.75f, 0.9f);
    public Color boundsColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public bool showCellCenters = false;
    public float centerMarkerSize = 0.05f;
#if UNITY_EDITOR
    public bool showCoordinates = false;
    public Color coordColor = new Color(0.4f, 0.6f, 1f, 0.9f);
#endif

    private GridCell[,] grid;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        InitializeGrid(gridSize, cellSize);
        

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

    public GameObject GetGameObjectFromWorldPositin(Vector3 position){
        var gridCell = GetCellFromWorldPosition(position);
        return gridCell.placedObject.gameObject;
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

    //Put all active counters in grid cells
    public void FillGridBySceneCounters()
    {
        // list of active counters in current scene
        BaseCounter[] counters = UnityEngine.Object.FindObjectsByType(
                    typeof(BaseCounter),
                    findObjectsInactive: FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                ) as BaseCounter[];


        foreach (BaseCounter counter in counters)
        {
            Vector3 pos = counter.transform.position;
            int x = Mathf.RoundToInt((pos.x - originPosition.x) / cellSize); //originPosition is location of the left down corner
            int z = Mathf.RoundToInt((pos.z - originPosition.z) / cellSize);

            GridCell cell = GetCellAt(x, z);
            if (cell != null)
            {
                cell.isOccupied = true;
                cell.isWalkable = false;
                cell.placedObject = counter.gameObject;
            }
        }
    }

    //Fill one grid cell by counter
    public void FillGridCellByCounter(BaseCounter counterParam)
    {
        Vector3 counterPosition = counterParam.transform.position;

        var gridCell = GetCellFromWorldPosition(counterPosition);        

        int x = Mathf.RoundToInt((counterPosition.x - originPosition.x) / cellSize); //originPosition is location of the left down corner
        int z = Mathf.RoundToInt((counterPosition.z - originPosition.z) / cellSize);

        GridCell cell = GetCellAt(x, z);
        if (cell != null)
        {
            cell.placedObject = counterParam.gameObject;
        }
    }

    //used for Save/Load
    public GridCell[,] GetGridCells() => grid;

    //used for Save/Load
    public void SetGridCells(GridCell[,] gridCellsParam) => grid = gridCellsParam;

    public IEnumerable<GridCell> GetAllOccupiedCells()
    {
        // Iterates once, allocates only the iterator
        for (int x = 0; x < gridSize.x; x++)
            for (int y = 0; y < gridSize.y; y++)
                if (grid[x, y].isOccupied)
                    yield return grid[x, y];
    }

    public List<GridItemDTO> BuildSnapshot()
    {
        var list = new List<GridItemDTO>();

        foreach (var cell in GetAllOccupiedCells())
        {
            var go = cell.placedObject;
            if (go == null) continue;

            var payloadProvider = go.GetComponent<ISavePayloadProvider>();
            if (payloadProvider == null) continue;

            var payloadObj = payloadProvider.CapturePayload();
            var customJson = payloadObj != null ? JsonConvert.SerializeObject(payloadObj) : null;

            var t = go.transform;

            list.Add(new GridItemDTO
            {
                kind = payloadProvider.GetKind(),
                prefabKey = payloadProvider.GetPrefabKey(),
                cellX = cell.coordinates.x,
                cellY = cell.coordinates.y,
                transform = new TransformData { position = t.position, rotation = t.rotation, localScale = t.localScale },
                version = 1,
                customJson = customJson
            });
        }

        return list;
    }

    public void RestoreFromSnapshot(IEnumerable<GridItemDTO> items)
    {
        InitializeGrid(gridSize, cellSize);// reset grid

        foreach (var dto in items)
        {
            var prefab = registry.GetPrefab(dto.prefabKey);
            if (prefab == null)
            {
                Debug.LogWarning($"No prefab for key '{dto.prefabKey}'");
                continue;
            }

            var go = Instantiate(prefab);
            var t = go.transform;
            t.position = dto.transform.position;
            t.rotation = dto.transform.rotation;
            t.localScale = dto.transform.localScale;

             grid[dto.cellX, dto.cellY].placedObject = go; // your own method
             grid[dto.cellX, dto.cellY].isWalkable = false;
             grid[dto.cellX, dto.cellY].isOccupied = true;

            if (!string.IsNullOrEmpty(dto.customJson))
            {
                var payloadProvider = go.GetComponent<ISavePayloadProvider>();
                if (payloadProvider != null)
                {
                    // Decide the target type based on dto.kind or prefab
                    var targetType = ResolvePayloadType(dto.kind, go);
                    var payloadObj = JsonConvert.DeserializeObject(dto.customJson, targetType);
                    payloadProvider.RestorePayload(payloadObj);
                }
            }
        }
    }

    private System.Type ResolvePayloadType(string kind, GameObject go)
    {
        // simplest: switch on kind
        // or: read a component that exposes Type, or store payloadType in registry
        return kind switch
        {
            "counter" => typeof(CounterSaveData),
            //"helper" => typeof(HelperSaveData),
            _ => typeof(object)
        };
    }

    // =======================
    // Scene view grid drawing
    // =======================
    private void OnDrawGizmos()
    {
        if (!showGrid || onlyWhenSelected) return;
        DrawGridGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGrid || !onlyWhenSelected) return;
        DrawGridGizmos();
    }

    private void DrawGridGizmos()
    {
        if (gridSize.x <= 0 || gridSize.y <= 0 || cellSize <= 0f) return;

        Vector3 origin = originPosition;
        float width = gridSize.x * cellSize;
        float height = gridSize.y * cellSize;

        // Grid lines
        Gizmos.color = gridLineColor;

        // Vertical lines (along +Z)
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0f, 0f);
            Vector3 end = start + new Vector3(0f, 0f, height);
            Gizmos.DrawLine(start, end);
        }

        // Horizontal lines (along +X)
        for (int y = 0; y <= gridSize.y; y++)
        {
            Vector3 start = origin + new Vector3(0f, 0f, y * cellSize);
            Vector3 end = start + new Vector3(width, 0f, 0f);
            Gizmos.DrawLine(start, end);
        }

        // Bounds rectangle (re-draw edges in a darker color)
        Gizmos.color = boundsColor;
        Vector3 bl = origin;                         // bottom-left
        Vector3 br = origin + new Vector3(width, 0, 0);
        Vector3 tl = origin + new Vector3(0, 0, height);
        Vector3 tr = origin + new Vector3(width, 0, height);
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(bl, tl);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(tr, tl);

        // Optional: cell centers + labels
        if (showCellCenters ||
#if UNITY_EDITOR
            showCoordinates
#else
            false
#endif
            )
        {
#if UNITY_EDITOR
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = coordColor }
            };
#endif
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 center = origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);

                    if (showCellCenters)
                    {
                        Gizmos.color = gridLineColor;
                        Gizmos.DrawSphere(center, centerMarkerSize);
                    }

#if UNITY_EDITOR
                    if (showCoordinates)
                    {
                        Handles.color = coordColor;
                        Handles.Label(center + Vector3.up * 0.02f, $"{x},{y}", style);
                    }
#endif
                }
            }
        }
    }
}
