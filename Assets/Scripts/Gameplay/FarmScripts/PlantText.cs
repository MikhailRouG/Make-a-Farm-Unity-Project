using Mirror;
using TMPro;
using UnityEngine;

namespace Gameplay.Farm
{
    public class PlantText : MonoBehaviour
    {
        [SerializeField] private Plant _plant;
        [SerializeField] private TextMeshPro _textObject;
        [SerializeField] private float _showDistance = 5f;

        private Transform _cameraTransform;
        private Transform _target;
        private bool _isTextVisible;
        private float _currentTime;

        private void OnValidate()
        {
            _plant ??= GetComponent<Plant>();
            _textObject ??= GetComponentInChildren<TextMeshPro>();
        }

        private void Awake()
        {
            _currentTime = 5f;

            if (_plant != null)
                _plant.OnUpdateStage += UpdateTime;
        }

        private void OnDestroy()
        {
            if (_plant != null)
                _plant.OnUpdateStage -= UpdateTime;
        }

        private void OnEnable()
        {
            if (_textObject != null)
                _textObject.text = string.Empty;

            _isTextVisible = false;

            if (Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (_textObject == null) return;

            if (_target == null)
            {
                if (NetworkClient.localPlayer == null)
                    return;

                _target = NetworkClient.localPlayer.transform;
            }

            if (_cameraTransform == null)
            {
                if (Camera.main == null)
                    return;

                _cameraTransform = Camera.main.transform;
            }

            float sqrDistance = (_target.position - transform.position).sqrMagnitude;

            if (sqrDistance > _showDistance * _showDistance)
            {
                HideText();
                return;
            }

            _textObject.transform.rotation = _cameraTransform.rotation;

            if (_currentTime > 0f)
            {
                _currentTime = Mathf.Max(0f, _currentTime - Time.deltaTime);
                SetText($"Time {_currentTime:F1}");
            }
        }

        private void UpdateTime(EffectState state, string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (state == EffectState.Upgrade)
            {
                if (float.TryParse(text, out float value))
                    _currentTime = value;
            }
            else
            {
                SetText(text);
            }
        }

        private void SetText(string text)
        {
            _textObject.text = text;
            _isTextVisible = true;
        }

        private void HideText()
        {
            if (!_isTextVisible) return;

            _textObject.text = string.Empty;
            _isTextVisible = false;
        }
    }
}
