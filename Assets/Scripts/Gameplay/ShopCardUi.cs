using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ShopCardUi : MonoBehaviour
{
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _sellButton;
    [SerializeField] private Image _image;
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
        if (_buyButton != null)
            _sellButton.onClick.RemoveListener(OnSellClicked);
    }
    public void Build(ItemConfig item)
    {
        if (item == null || _buyButton == null)
        {
            gameObject.SetActive(false);
            return;
        }
        _item = item;
        if (_image == null)
            {
            gameObject.SetActive(false);
            return;
            }
        _image.sprite = _item.Icon;
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
