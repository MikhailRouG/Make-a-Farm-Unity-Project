using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _loadingScreen;
        [SerializeField] private Image _loadingCircle;
        [SerializeField] private float _rotationSpeed = -100f;

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (_loadingCircle == null) return;
            if (_loadingScreen != null && !_loadingScreen.activeInHierarchy) return;

            _loadingCircle.transform.Rotate(
                Vector3.forward * (_rotationSpeed * Time.deltaTime),
                Space.World);
        }

        public void Show()
        {
            if (_loadingScreen != null) _loadingScreen.SetActive(true);
        }

        public void Hide()
        {
            if (_loadingScreen != null) _loadingScreen.SetActive(false);
        }
    }
}
