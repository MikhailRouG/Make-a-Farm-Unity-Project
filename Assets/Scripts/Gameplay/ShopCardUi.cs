using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ShopCardUi : MonoBehaviour
{
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _sellButton;
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _nameText;

    [Header("Rarity")]
    [SerializeField] private TMP_Text _rarityText;

    [Tooltip("Tinted by rarity. Any Image will do: a frame, a header strip, the card background.")]
    [SerializeField] private Image _rarityBackground;

    [Tooltip("Also tint the rarity label, not just the background.")]
    [SerializeField] private bool _tintRarityText = true;

    [Tooltip("Darkens the label against the background. 1 keeps the rarity colour as is.")]
    [SerializeField, Range(0f, 1f)] private float _rarityTextDim = 0.6f;

    [SerializeField] private TMP_Text _buyPriceText;
    [SerializeField] private TMP_Text _sellPriceText;

    private ItemConfig _item;

    public event Action<int> OnBuyClick;
    public event Action<int> OnSellClick;

    private void Awake()
    {
        if (_buyButton != null)
            _buyButton.onClick.AddListener(OnBuyClicked);
        if (_sellButton != null)
            _sellButton.onClick.AddListener(OnSellClicked);
    }

    private void OnDestroy()
    {
        if (_buyButton != null)
            _buyButton.onClick.RemoveListener(OnBuyClicked);
        if (_sellButton != null)
            _sellButton.onClick.RemoveListener(OnSellClicked);
    }

    public void Build(ItemConfig item)
    {
        if (item == null || _buyButton == null || _image == null)
        {
            gameObject.SetActive(false);
            return;
        }

        _item = item;
        _image.sprite = _item.Icon;

        if (_nameText != null)
            _nameText.text = item.Name;

        ApplyRarity(item.Rarity);

        if (_buyPriceText != null)
            _buyPriceText.text = $"{item.Price}$";

        if (_sellPriceText != null)
            _sellPriceText.text = $"{item.Price}$";
    }

    private void ApplyRarity(ItemRarity rarity)
    {
        Color color = RarityColors.Of(rarity);

        if (_rarityBackground != null)
            _rarityBackground.color = color;

        if (_rarityText == null)
            return;

        _rarityText.text = rarity.ToString();

        // Only RGB is scaled: alpha stays as the label was authored, so a translucent
        // rarity colour meant for the background does not fade the text with it.
        if (_tintRarityText)
        {
            _rarityText.color = new Color(
                color.r * _rarityTextDim,
                color.g * _rarityTextDim,
                color.b * _rarityTextDim,
                _rarityText.color.a);
        }
    }

    private void OnBuyClicked()
    {
        if (_item == null)
            return;

        OnBuyClick?.Invoke(_item.Id);
    }

    private void OnSellClicked()
    {
        if (_item == null)
            return;

        OnSellClick?.Invoke(_item.Id);
    }

    public class Factory : PlaceholderFactory<ShopCardUi> { }
}
