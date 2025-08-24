using System.Collections.Generic;
using UnityEngine;

public class TraysCounterVisual : MonoBehaviour
{
    [SerializeField] private TrayCounter traysCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform trayVisualPrefab;

    private List<GameObject> trayVisualGameObjectList;

    private void Awake() {
        trayVisualGameObjectList = new List<GameObject>();
    }

    private void Start() {
        traysCounter.OnTraySpawned += TraysCounter_OnTraySpawned;
        traysCounter.OnTrayRemoved += TraysCounter_OnTrayRemoved;
    }

    private void TraysCounter_OnTrayRemoved(object sender, System.EventArgs e) {
        GameObject trayGameObject = trayVisualGameObjectList[trayVisualGameObjectList.Count - 1];
        trayVisualGameObjectList.Remove(trayGameObject);
        Destroy(trayGameObject);
    }

    private void TraysCounter_OnTraySpawned(object sender, System.EventArgs e) {
        Transform trayVisualTransform = Instantiate(trayVisualPrefab, counterTopPoint);

        float trayOffsetY = .1f;
        trayVisualTransform.localPosition = new Vector3(0, trayOffsetY * trayVisualGameObjectList.Count ,0);

        trayVisualGameObjectList.Add(trayVisualTransform.gameObject);
    }
}
