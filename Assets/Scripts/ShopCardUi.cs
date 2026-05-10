using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardUi : MonoBehaviour
{
    private Button _button;
    [SerializeField] private Image _image;
    private ItemConfig _item;
    public event Action<int> OnClick;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponentInChildren<Button>();
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClicked);
    }
    public void Build(ItemConfig item)
    {
        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }
        _item = item;
        _button = GetComponentInChildren<Button>();
        if (_button == null)
        {
            gameObject.SetActive(false);
            return;
        }
        _button.onClick.RemoveListener(OnClicked);
        _button.onClick.AddListener(OnClicked);
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
}

