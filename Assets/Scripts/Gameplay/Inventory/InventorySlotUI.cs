using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _amountText;

    private int _index;
    private Button _button;

    public event Action<int> Clicked;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_button != null)
            _button.onClick.AddListener(OnClicked);

        if (_amountText == null)
            _amountText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (_iconImage == null)
            _iconImage = GetComponentInChildren<Image>(true);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClicked);
    }

    public void Init(int index)
    {
        _index = index;
    }

    public void Set(Sprite icon, InventorySlot slot)
    {
        if (_iconImage != null)
        {
            _iconImage.enabled = true;
            _iconImage.sprite = icon;
        }

        if (_amountText == null)
            return;

        if (slot.Weight != 1)
            _amountText.text = $"{slot.Weight:F1} kg";
        else
            _amountText.text = slot.Amount.ToString();
    }

    public void Clear()
    {
        SetHighlight(Color.white);

        if (_iconImage != null)
        {
            _iconImage.enabled = false;
            _iconImage.sprite = null;
        }

        if (_amountText != null)
            _amountText.text = string.Empty;
    }

    public void OnSelected()
    {
        SetHighlight(Color.gray);
    }

    public void OnUnSelected()
    {
        SetHighlight(Color.white);
    }

    private void SetHighlight(Color color)
    {
        if (_button != null && _button.image != null)
            _button.image.color = color;
    }

    private void OnClicked()
    {
        if (_iconImage == null || _iconImage.sprite == null)
            return;

        Clicked?.Invoke(_index);
    }
}
