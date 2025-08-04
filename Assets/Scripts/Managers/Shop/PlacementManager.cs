using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    private GameObject itemToPlace;
    private bool isPlacementMode = false;

    [SerializeField]
    private GameObject parentOfCounter;

    private void Awake()
    {
        Instance = this;
    }

    public void StartPlacement(GameObject item)
    {
        itemToPlace = item;
        isPlacementMode = true;
        ShowValidGridCells();
    }

    private void Update()
    {
        if (isPlacementMode && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PlaceItem(hit.point);
            }
        }
    }

    public void PlaceItem(Vector3 position)
    {
        if (isPlacementMode && IsValidCell(position))
        {
            ReplaceCounter(position, itemToPlace);
            isPlacementMode = false;
            HideGridCells();
        }
    }

    private bool IsValidCell(Vector3 position)
    {
        GridCell cell = GridManager.Instance.GetCellFromWorldPosition(position);
        return cell != null && cell.isOccupied; 
    }

    private void ShowValidGridCells()
    {
        // TODO: Highlight walkable or replaceable cells
    }

    private void HideGridCells()
    {
        // TODO: Clear highlights
    }

    public void ReplaceCounter(Vector3 worldPosition, GameObject newCounterPrefab)
    {
        GridCell cell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);

        if (cell == null)
        {
            Debug.LogWarning("Tried to place counter out of grid bounds.");
            return;
        }

        if (cell.isOccupied)
        {
            GameObject.Destroy(cell.placedObject);
        }

        GameObject newCounter = Instantiate(newCounterPrefab, cell.worldPosition, Quaternion.identity, parentOfCounter.transform);
        cell.placedObject = newCounter;
        cell.isOccupied = true;

        BaseCounter counter = newCounter.GetComponent<BaseCounter>();
        if (counter != null)
        {
            // Optional: counter.gridCoordinates = cell.coordinates;
        }
    }
}
