using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
public class PlayerPlacement : NetworkBehaviour
{
    [SerializeField] private ItemDatabase db;
    private PlayerInteraction interaction;
    private Inventory _inventory;

    private int currentSeedId = -1;
    private GameObject currentGhost;
    private ItemSeed _currentSeed;
    private PlantCheckPosition _component;
    private bool _canPlant;
    public bool _isPlanting { get; private set; }

    private void Awake()
    {
        interaction = GetComponent<PlayerInteraction>();
        _inventory = GetComponent<Inventory>();
        db.Init();
        _isPlanting = false;
        _canPlant = false;
    }

    private void OnEnable()
    {
        _inventory.onUseItem += CmdConfirmPlacement;
    }
    private void OnDisable()
    {
        _inventory.onUseItem -= CmdConfirmPlacement;
    }

    private void Update()
    {
        if (currentGhost != null)
        {
            currentGhost.transform.position = interaction.LookPoint;
            _canPlant = _component.CheckZone();
        }
    }
    public void StartPlanting(int seedId)
    {
        var config = db.Get(seedId);
        if (config is ItemSeed seeds)
        {
            _currentSeed = seeds;
            if(currentGhost != null) Destroy(currentGhost);
            currentGhost = Instantiate(_currentSeed.GhostObject);
            _component = currentGhost.GetComponent<PlantCheckPosition>();
            _component.Init(_currentSeed);
            _isPlanting = true;
        }
    }
    [Command]
    public void CmdConfirmPlacement()
    {
        if (_currentSeed == null || !_canPlant) return;

        ServerConfirmPlacement(_currentSeed.Id, interaction.LookPoint);
        Destroy(currentGhost);
        _currentSeed = null;
    }
    [Server]
    private void ServerConfirmPlacement(int id, Vector3 position)
    {
        if (_inventory.HasItem(id))
        {
            GameObject instance = Instantiate(_currentSeed.HarvestItem[0].Model, position, Quaternion.identity);
            NetworkServer.Spawn(instance, connectionToClient);
            if (_currentSeed.ConsumeOnUse) _inventory.TryRemoveItem(id, 1);
            else NetworkServer.Destroy(instance);
        }
    }
    private void OnCancel()
    {
        if (currentGhost != null) DestroyImmediate(currentGhost);
    }


}