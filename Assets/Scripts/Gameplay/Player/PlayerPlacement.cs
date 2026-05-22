using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
public class PlayerPlacement : NetworkBehaviour
{
    [SerializeField] private ItemDatabase db;
    private PlayerInteraction interaction;
    private PlayerInventory _player;
    private Inventory _inventory;

    private int currentSeedId = -1;
    private ItemSeed _currentSeed;
    private PlantCheckPosition currentGhost;
    private bool _canPlant;
    public bool _isPlanting { get; private set; }

    private void Awake()
    {
        interaction = GetComponent<PlayerInteraction>();
        _inventory = GetComponent<Inventory>();
        _player = GetComponent<PlayerInventory>();
        _isPlanting = false;
        _canPlant = false;
    }

    private void OnEnable()
    {
        _player.OnSelectedSlotChangedEvent += OnChangedSlot;
    }
    private void OnDisable()
    {
        _player.OnSelectedSlotChangedEvent -= OnChangedSlot;
    }

    private void Update()
    {
        if (!isLocalPlayer || !_isPlanting) return;
        if (currentGhost != null)
        {
            currentGhost.transform.position = interaction.LookPoint;
            _canPlant = currentGhost.CheckZone();
        }
    }
    public void StartPlanting(int seedId)
    {
        if (!isLocalPlayer) return;
        var config = db.Get(seedId);
        if (config is ItemSeed seeds)
        {
            _currentSeed = seeds;
            if(currentGhost != null) Destroy(currentGhost);
            currentGhost = Instantiate(_currentSeed.GhostObject);
            currentGhost.Init(_currentSeed);
            _isPlanting = true;
        }
    }

    public void ConfirmPlacement()
    {
        if (!isLocalPlayer || !_isPlanting) return;
        if (!_canPlant)
        {
            CleanUpGhost();
            return;
        }
        ServerConfirmPlacement(_currentSeed.Id, interaction.LookPoint);
        CleanUpGhost();
    }
    [Command]
    private void CmdConfirmPlacement(int seedId, Vector3 spawnPosition)
    {
        var config = db.Get(seedId);
        if (config is ItemSeed seedConfig)
        {
            ServerConfirmPlacement(seedConfig.Id, spawnPosition);
        }
    }
    [Server]
    private void ServerConfirmPlacement(int id, Vector3 position)
    {
        if (_inventory.HasItem(id))
        {
            Plant i = Instantiate(_currentSeed.PlantStartObject, position, Quaternion.identity);
            GameObject instance = i.gameObject;
            NetworkServer.Spawn(instance, connectionToClient);
            i.Init(netId, _currentSeed);
            if (!_inventory.TryRemoveItem(id, 1))
            {
                NetworkServer.Destroy(instance);
            }
        }
    }
    private void OnChangedSlot(int i)
    {

    }
    public void CancelPlacement()
    {
        if (currentGhost != null) Destroy(currentGhost);
        this.enabled = false;
    }
    private void CleanUpGhost()
    {
        if (currentGhost != null)
        {
            Destroy(currentGhost.gameObject);
            currentGhost = null;
        }
        _currentSeed = null;
        _isPlanting = false;
        _canPlant = false;
    }
}