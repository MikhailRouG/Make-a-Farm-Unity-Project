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
        db.Init();
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
        if (currentGhost != null)
        {
            currentGhost.transform.position = interaction.LookPoint;
            _canPlant = currentGhost.CheckZone();
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
            currentGhost.Init(_currentSeed);
            _isPlanting = true;
        }
    }
    [Command]
    public void CmdConfirmPlacement()
    {
        this.enabled = false;
        if ( !_canPlant) return;
        Destroy(currentGhost.gameObject);
        ServerConfirmPlacement(_currentSeed.Id, interaction.LookPoint);
        _currentSeed = null;
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
            if (!_inventory.TryRemoveItem(id, 1)) NetworkServer.Destroy(instance);
        }
    }
    private void OnChangedSlot(int i)
    {

    }
    public void CancelPlacement()
    {
        if (currentGhost != null) DestroyImmediate(currentGhost);
        this.enabled = false;
    }

}