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
        private bool isTextVisible = false;
        private float _currentTime;

        private void OnValidate()
        {
            _plant = GetComponent<Plant>();
            _textObject ??= GetComponentInChildren<TextMeshPro>();
        }
        private void Awake()
        {
            _currentTime = 5f;
            if (_plant != null)
            {
                _plant.OnUpdateStage += UpdateTime;
            }
        }
        private void OnEnable()
        {
            _textObject.text = string.Empty;
            isTextVisible = false;

            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }
        private void Update()
        {
            if (_target == null)
            {
                if (NetworkClient.localPlayer != null)
                {
                    _target = NetworkClient.localPlayer.transform;
                }
                else
                {
                    return;
                }
            }
            if (_cameraTransform == null)
            {
                if (Camera.main != null)
                {
                    _cameraTransform = Camera.main.transform;
                }
                else
                {
                    return;
                }
            }
            float sqrDistance = (_target.position - transform.position).sqrMagnitude;
            bool shouldShow = sqrDistance <= (_showDistance * _showDistance);
            if (!shouldShow)
            {
                HideText();
                return;
            }
            _textObject.transform.rotation = _cameraTransform.rotation;

            if (_currentTime > 0f)
            {
                _currentTime -= Time.deltaTime;

                if (_currentTime < 0f)
                {
                    _currentTime = 0f;
                }

                SetText($"Time {_currentTime:F1}");
            }
        }

        private void UpdateTime(EffectState state, string text)
        {

            if (string.IsNullOrEmpty(text)) return;

            if (state == EffectState.Upgrade)
            {
                if (float.TryParse(text, out float value))
                {
                    _currentTime = value;
                }
            }
            else
            {
                SetText(text);
            }
        }


        private void SetText(string text)
        {
            _textObject.text = text;
            isTextVisible = true;
        }

        private void HideText()
        {
            if (!isTextVisible) return;

            _textObject.text = string.Empty;
            isTextVisible = false;
        }
    }
}