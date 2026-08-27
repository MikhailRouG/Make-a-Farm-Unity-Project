using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Farm
{
    /// <summary>
    /// Shows the icon of the item a plant is waiting for. Holds no care state of its
    /// own: PlantCare tells it what is needed, Update only decides whether the icon
    /// is close enough to be seen and turns it towards the camera.
    /// </summary>
    public class PlantNeedIcon : MonoBehaviour
    {
        [SerializeField] private PlantCare _care;
        [SerializeField] private Image _icon;

        [SerializeField] private float _showDistance = 5f;

        [SerializeField] private bool _faceCamera = true;

        private Transform _cameraTransform;
        private Transform _target;
        private Sprite _neededSprite;
        private Sprite _shownSprite;

        private void OnValidate()
        {
            _care ??= GetComponentInParent<PlantCare>();
            _icon ??= GetComponentInChildren<Image>(true);
        }

        private void OnEnable()
        {
            _shownSprite = null;
            Hide();

            if (_care != null)
            {
                _care.OnChangedReqirement += OnNeedChanged;

                // The hook stays silent when the incoming value matches what the
                // object already holds, so the current state has to be read once.
                OnNeedChanged(_care.CurrentNeed, _care.NeedsCare);
            }

            if (Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        private void OnDisable()
        {
            if (_care != null)
                _care.OnChangedReqirement -= OnNeedChanged;
        }

        private void OnNeedChanged(PlantRequirement need, bool needed)
        {
            _neededSprite = needed && need != null && need.RequiredItem != null
                ? need.RequiredItem.Icon
                : null;
        }

        private void Update()
        {
            if (_icon == null) return;

            if (_neededSprite == null)
            {
                Hide();
                return;
            }

            if (_target == null)
            {
                if (NetworkClient.localPlayer == null) return;
                _target = NetworkClient.localPlayer.transform;
            }

            float sqrDistance = (_target.position - transform.position).sqrMagnitude;

            if (sqrDistance > _showDistance * _showDistance)
            {
                Hide();
                return;
            }

            Show(_neededSprite);
            FaceCamera();
        }

        // Assigning Image.sprite dirties the canvas and forces a rebuild, so only a
        // real change is pushed rather than the same sprite every frame.
        private void Show(Sprite sprite)
        {
            if (_shownSprite != sprite)
            {
                _shownSprite = sprite;
                _icon.sprite = sprite;
            }

            if (!_icon.enabled)
                _icon.enabled = true;
        }

        private void Hide()
        {
            if (_icon == null) return;

            if (_icon.enabled)
                _icon.enabled = false;
        }

        private void FaceCamera()
        {
            if (!_faceCamera) return;

            if (_cameraTransform == null)
            {
                if (Camera.main == null) return;
                _cameraTransform = Camera.main.transform;
            }

            _icon.transform.rotation = _cameraTransform.rotation;
        }
    }
}
