using Mirror;
using TMPro;
using UnityEngine;
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
        _target = NetworkClient.localPlayer.gameObject.transform;
        _cameraTransform = Camera.main.transform;
        _textObject.text = string.Empty;
        _currentTime = 5f;
        _plant.OnUpdateStage += UpdateTime;
    }

    private void Update()
    {
        if (_target == null) return;
        _currentTime -= Time.deltaTime;
        float sqrDistance = (_target.position - transform.position).sqrMagnitude;
        bool shouldShow = sqrDistance <= (_showDistance * _showDistance);
        if (shouldShow)
        {
            _textObject.transform.rotation = _cameraTransform.rotation;
            if (_currentTime < 0) _currentTime = 0;
            _textObject.text = $"Time {_currentTime.ToString("F1")}";
            if (!isTextVisible)
            {
                isTextVisible = true;
            }
        }
        else
        {
            if (isTextVisible)
            {
                _textObject.text = string.Empty;
                isTextVisible = false;
            }
        }
    }

    private void UpdateTime(EffectState state,float time)
    {
        _currentTime = time;
    }

}