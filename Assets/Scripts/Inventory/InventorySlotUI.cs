using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _amountText;
    [SerializeField] private InventorySlot _slot;
    private int _index;
    private Button _button;
    public event Action<int> Clicked;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null) _button.onClick.AddListener(OnClicked);
        _amountText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (_iconImage == null)
            _iconImage = GetComponentInChildren<Image>(true);
    }

    public void Init(int index)
    {
        _index = index;
    }
    public void Set(
       Sprite icon,
       InventorySlot slot)
    {
        _iconImage.enabled = true;
        _iconImage.sprite = icon;
        _amountText.text = slot.Amount.ToString();
    }
    public void Clear()
    {
        _button.image.color = Color.white;
        _iconImage.enabled = false;
        _iconImage.sprite = null;
        _amountText.text = "";
    }
    private void OnClicked()
    {
        if (_iconImage.sprite == null) return;
        Clicked?.Invoke(_index);
    }
    public void OnSelected()
    {
        _button.image.color = Color.gray;
    }
    public void OnUnSelected()
    {
        _button.image.color = Color.white;
    }
}