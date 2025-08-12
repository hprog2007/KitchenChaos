using System.Collections.Generic;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private DeliveryManagerSingleUI recipeTemplate;

    private readonly Dictionary<string, DeliveryManagerSingleUI> cards = new();

    private void Awake()
    {
        recipeTemplate.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        DeliveryManager.instance.OnOrderSpawned += AddCard;
        DeliveryManager.instance.OnOrderRemoved += RemoveCard;
        DeliveryManager.instance.OnOrderTick += TickCard;
    }

    private void OnDisable()
    {
        var dm = DeliveryManager.instance;
        if (dm == null) return;
        dm.OnOrderSpawned -= AddCard;
        dm.OnOrderRemoved -= RemoveCard;
        dm.OnOrderTick -= TickCard;
    }

    private void Start()
    {
        foreach (var t in DeliveryManager.instance.GetWaitingOrders()) AddCard(t);
    }

    private void AddCard(OrderTicket t)
    {
        var ui = Instantiate(recipeTemplate, container);
        ui.gameObject.SetActive(true);
        ui.Bind(t); // implement in your SingleUI
        cards[t.Id] = ui;
    }

    private void RemoveCard(OrderTicket t)
    {
        if (!cards.TryGetValue(t.Id, out var ui)) return;
        Destroy(ui.gameObject);
        cards.Remove(t.Id);
    }

    private void TickCard(OrderTicket t)
    {
        if (cards.TryGetValue(t.Id, out var ui)) ui.OnTick(t);
    }
}
