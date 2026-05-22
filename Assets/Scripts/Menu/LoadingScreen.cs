using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.AsyncVideo.Coroutines.Example4
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _loadingScreen;
        [SerializeField] private Image _loadingCircle;

        private void Awake()
        {
            Hide();
        }

        public void Show() => _loadingScreen.SetActive(true);

        public void Hide() => _loadingScreen.SetActive(false);


        private void Update()
        {
            _loadingCircle.transform.Rotate(Vector3.forward * Time.deltaTime * -100, Space.World);
        }
    }
}