using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Player
{
    public class PauseMenuUi : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _exitButton;

        private Player _player;
        private bool _isOpen;

        private void Awake()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(Hide);
            if (_exitButton != null)
                _exitButton.onClick.AddListener(ExitToMenu);

            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(Hide);
            if (_exitButton != null)
                _exitButton.onClick.RemoveListener(ExitToMenu);

            if (_player != null)
                _player.onEsc -= Toggle;
        }

        public void Bind(Player player)
        {
            _player = player;
            _player.onEsc += Toggle;
        }

        private void Toggle()
        {
            if (_isOpen) Hide();
            else Show();
        }

        private void Show()
        {
            _isOpen = true;

            if (_panelRoot != null)
                _panelRoot.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _player?.SetInputEnabled(false);
        }

        private void Hide()
        {
            _isOpen = false;

            if (_panelRoot != null)
                _panelRoot.SetActive(false);

            _player?.SetInputEnabled(true);
        }

        private void ExitToMenu()
        {
            if (NetworkManager.singleton != null)
                NetworkManager.singleton.StopHost();
        }
    }
}
