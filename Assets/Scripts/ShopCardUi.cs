using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ShopCardUi : MonoBehaviour
{
    private Button _button;
    [SerializeField] private Image _image;
    private ItemConfig _item;
    public event Action<int> OnClick;

    [Inject]
    public void Construct(ItemConfig item)
    {
        _item = item;
    }
    private void Awake()
    {
        if (_button == null)
            _button = GetComponentInChildren<Button>();
        _button.onClick.AddListener(OnClicked);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClicked);
    }
    public void Build(ItemConfig item)
    {
        if (item == null || _button == null)
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

    private void OnClicked()
    {
        if (_item == null)
            return;
        OnClick?.Invoke(_item.Id);
    }
    public class Factory : PlaceholderFactory<ItemConfig, ShopCardUi> { }
}
