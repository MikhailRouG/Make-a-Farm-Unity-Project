using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Player.UI.Shop
{
    [RequireComponent(typeof(Toggle))]
    public class ShopTabUi : MonoBehaviour
    {
        [SerializeField] private ShopCategory _category;

        private Toggle _toggle;

        public ShopCategory Category => _category;
        public event Action<ShopCategory> OnSelected;

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
            _toggle.SetIsOnWithoutNotify(isOn);
            UpdateVisual(isOn);
        }

        private void OnValueChanged(bool isOn)
        {
            UpdateVisual(isOn);
            if (isOn) OnSelected?.Invoke(_category);
        }

        private void UpdateVisual(bool isOn)
        {
        }
    }
}