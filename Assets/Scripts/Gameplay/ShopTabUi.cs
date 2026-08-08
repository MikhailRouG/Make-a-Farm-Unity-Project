using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Player.UI.Shop
{
    [RequireComponent(typeof(Toggle))]
    public class ShopTabUi : MonoBehaviour
    {
        [SerializeField] private ItemType _category;

        private Toggle _toggle;

        public ItemType Category => _category;
        public event Action<ItemType> OnSelected;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            if (_toggle != null)
                _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        public void SetSelectedSilently(bool isOn)
        {
            if (_toggle != null)
                _toggle.SetIsOnWithoutNotify(isOn);
        }

        private void OnValueChanged(bool isOn)
        {
            if (isOn) OnSelected?.Invoke(_category);
        }
    }
}