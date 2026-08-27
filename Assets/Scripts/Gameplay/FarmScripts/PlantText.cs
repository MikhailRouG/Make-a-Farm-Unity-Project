using Mirror;
using TMPro;
using UnityEngine;

namespace Gameplay.Farm
{
    public class PlantText : MonoBehaviour
    {
        private const string ReadyText = "Can be collected";

        [SerializeField] private Plant _plant;
        [SerializeField] private PlantCare _care;
        [SerializeField] private TextMeshPro _textObject;
        [SerializeField] private float _showDistance = 5f;

        private Transform _cameraTransform;
        private Transform _target;
        private string _shownText = string.Empty;
        private PlantRequirement _need;

        private void OnValidate()
        {
            _plant ??= GetComponent<Plant>();
            _care ??= GetComponent<PlantCare>();
            _textObject ??= GetComponentInChildren<TextMeshPro>();
        }

        private void OnEnable()
        {
            _shownText = null;
            SetText(string.Empty);

            if (_care != null)
            {
                _care.OnChangedReqirement += OnNeedChanged;

                // The hook stays silent when the incoming value matches what the
                // object already holds, so the current state has to be read once.
                OnNeedChanged(_care.CurrentNeed, _care.NeedsCare);
            }

            if (Camera.main != null) _cameraTransform = Camera.main.transform;
        }

        private void OnDisable()
        {
            if (_care != null)
                _care.OnChangedReqirement -= OnNeedChanged;
        }

        private void OnNeedChanged(PlantRequirement need, bool needed)
        {
            _need = needed ? need : null;
        }

        private void Update()
        {
            if (_textObject == null || _plant == null) return;

            if (_target == null)
            {
                if (NetworkClient.localPlayer == null) return;
                _target = NetworkClient.localPlayer.transform;
            }

            if (_cameraTransform == null)
            {
                if (Camera.main == null) return;
                _cameraTransform = Camera.main.transform;
            }

            float sqrDistance = (_target.position - transform.position).sqrMagnitude;

            if (sqrDistance > _showDistance * _showDistance)
            {
                SetText(string.Empty);
                return;
            }

            _textObject.transform.rotation = _cameraTransform.rotation;

            SetText(BuildLabel());
        }

        // An overdue need outranks the growth timer: it is the only thing the player
        // can act on, and ignoring it costs the plant its health. Care is optional,
        // so a plant without a PlantCare component just shows its timer.
        private string BuildLabel()
        {
            if (_need != null) return $"{_need.Prompt}{HealthSuffix()}";
            if (_plant.IsFullyGrown) return ReadyText;
            if (_plant.HasStageDeadline) return $"Time {_plant.RemainingStageTime:F1}{HealthSuffix()}";

            return string.Empty;
        }

        private string HealthSuffix()
        {
            if (_care == null) return string.Empty;

            float fraction = _care.HealthFraction;

            return fraction < 1f ? $"  HP {Mathf.RoundToInt(fraction * 100f)}%" : string.Empty;
        }

        private void SetText(string text)
        {
            if (_shownText == text || _textObject == null) return;
            _shownText = text;
            _textObject.text = text;
        }
    }
}
