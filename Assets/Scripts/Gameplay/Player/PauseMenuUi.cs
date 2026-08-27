using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Player
{
    public class PauseMenuUi : MonoBehaviour, ICloseableUi
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
            _isOpen = false;
        }

        private void OnDestroy()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(Hide);
            if (_exitButton != null)
                _exitButton.onClick.RemoveListener(ExitToMenu);

            if (_player != null)
                _player.onEsc -= Show;
        }

        public void Bind(Player player)
        {
            _player = player;
            _player.onEsc += Show;
        }

        public void Close() => Hide();

        private void Show()
        {
            if (_isOpen) return;
            _isOpen = true;

            if (_panelRoot != null)
                _panelRoot.SetActive(true);

            _player?.SetInputEnabled(false);
            UiManager.Instance?.Register(this);
        }

        private void Hide()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (_panelRoot != null)
                _panelRoot.SetActive(false);

            _player?.SetInputEnabled(true);
            UiManager.Instance?.Unregister(this);
        }

        private void ExitToMenu()
        {
            if (NetworkManager.singleton == null)
                return;

            if (NetworkServer.active && NetworkClient.isConnected)
                NetworkManager.singleton.StopHost();
            else if (NetworkClient.active)
                NetworkManager.singleton.StopClient();
            else if (NetworkServer.active)
                NetworkManager.singleton.StopServer();
        }
    }
}
