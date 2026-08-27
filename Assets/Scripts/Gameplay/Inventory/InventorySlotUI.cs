using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _amountText;
    [SerializeField] private Image _rarityImage;
    private int _index;
    private Button _button;

    private Color _idleColor;
    private Color _rarityBase;
    public event Action<int> Clicked;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_button != null)
            _button.onClick.AddListener(OnClicked);

        if (_button != null && _button.image != null)
            _idleColor = _button.image.color;
        if (_rarityImage != null)
            _rarityBase = _rarityImage.color;
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

    public void Set(Sprite icon, InventorySlot slot, Color rarityColor)
    {
        if (_iconImage != null)
        {
            _iconImage.enabled = true;
            _iconImage.sprite = icon;
        }

        if (_rarityImage != null)
            _rarityImage.color = rarityColor;

        if (_amountText == null)
            return;

        if (slot.Weight != 1)
            _amountText.text = $"{slot.Weight:F1} kg";
        else
            _amountText.text = slot.Amount.ToString();
    }

    public void Clear()
    {
        SetHighlight(_idleColor);

        if (_rarityImage != null)
            _rarityImage.color = _rarityBase;

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
        // Dimmed idle colour rather than a fixed grey, so selection reads the same
        // whatever background the prefab uses. Alpha is left alone.
        SetHighlight(new Color(_idleColor.r * 0.6f, _idleColor.g * 0.6f, _idleColor.b * 0.6f, _idleColor.a));
    }

    public void OnUnSelected()
    {
        SetHighlight(_idleColor);
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
