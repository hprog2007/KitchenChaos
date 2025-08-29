using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    public static DeliveryManagerUI instance { get; private set; }


    public event Action<OrderTicket> OnOrderCardAdded;

    [SerializeField] private Transform container;
    [SerializeField] private DeliveryManagerSingleUI recipeTemplate;

    private readonly Dictionary<string, DeliveryManagerSingleUI> cards = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);   // kill duplicates
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // if you intend it to persist across scenes
    }

    private void OnEnable()
    {
        if (OrderManager.instance == null)
            return;

        OrderManager.instance.OnOrderSpawned += AddCard;
        OrderManager.instance.OnOrderRemoved += RemoveCard;
        OrderManager.instance.OnOrderTick += TickCard;
    }

    private void OnDisable()
    {
        if (OrderManager.instance == null) 
            return;

        OrderManager.instance.OnOrderSpawned -= AddCard;
        OrderManager.instance.OnOrderRemoved -= RemoveCard;
        OrderManager.instance.OnOrderTick -= TickCard;
    }

    private void Start()
    {
        foreach (var t in OrderManager.instance.GetWaitingOrders())
        {
            AddCard(t);
        }
    }

    private void AddCard(OrderTicket ticket)
    {
        var ui = Instantiate(recipeTemplate, container);
        ui.gameObject.SetActive(true);
        ui.Bind(ticket); // implement in your SingleUI
        cards[ticket.Id] = ui;


        OnOrderCardAdded?.Invoke(ticket);
    }

    private void RemoveCard(OrderTicket t)
    {
        if (!cards.TryGetValue(t.Id, out var ui)) 
            return;

        if (t.RemainingTime <= 0)
        {
            ui.GetComponent<UIExplosionParticles>().Explode();
        }

        Destroy(ui.gameObject, 3f);
        cards.Remove(t.Id);
    }

    private void TickCard(OrderTicket t)
    {
        if (cards.TryGetValue(t.Id, out var ui)) ui.OnTick(t);
    }
}
