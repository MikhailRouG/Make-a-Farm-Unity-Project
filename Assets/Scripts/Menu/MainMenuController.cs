using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_InputField ipInputField;

        [SerializeField] private LoadingScreen loadingScreen;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private float connectTimeout = 5f;

        private Coroutine connectCoroutine;

        private void Start()
        {
            if (hostButton != null) hostButton.onClick.AddListener(StartHost);
            if (clientButton != null) clientButton.onClick.AddListener(StartClient);
            if (exitButton != null) exitButton.onClick.AddListener(QuitGame);

            if (ipInputField != null && string.IsNullOrEmpty(ipInputField.text))
            {
                ipInputField.text = "localhost";
            }

            SetStatus(string.Empty);
        }

        private void OnDestroy()
        {
            if (hostButton != null) hostButton.onClick.RemoveListener(StartHost);
            if (clientButton != null) clientButton.onClick.RemoveListener(StartClient);
            if (exitButton != null) exitButton.onClick.RemoveListener(QuitGame);
        }

        private void StartHost()
        {
            SetStatus(string.Empty);
            if (loadingScreen != null) loadingScreen.Show();

            NetworkManager.singleton.StartHost();

            // StartHost silently does nothing when the port is taken; without this
            // check the loading screen would hang forever.
            if (!NetworkServer.active)
                FailConnection("Failed to start host.");
        }

        private void StartClient()
        {
            SetStatus(string.Empty);
            if (loadingScreen != null) loadingScreen.Show();

            if (ipInputField != null)
                NetworkManager.singleton.networkAddress = ipInputField.text;

            NetworkManager.singleton.StartClient();

            if (connectCoroutine != null) StopCoroutine(connectCoroutine);
            connectCoroutine = StartCoroutine(ConnectRoutine(connectTimeout));
        }

        private void QuitGame()
        {
            // Close the connection first if a host was started or a client
            // connected, or the others only learn about the exit on timeout.
            if (NetworkManager.singleton != null)
            {
                if (NetworkServer.active && NetworkClient.isConnected)
                    NetworkManager.singleton.StopHost();
                else if (NetworkClient.isConnected)
                    NetworkManager.singleton.StopClient();
                else if (NetworkServer.active)
                    NetworkManager.singleton.StopServer();
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // State is polled instead of subscribing to NetworkClient.OnDisconnectedEvent:
        // NetworkManager assigns those events with `=` in RegisterClientMessages(),
        // which wipes out any outside handler.
        private IEnumerator ConnectRoutine(float timeout)
        {
            float deadline = Time.unscaledTime + timeout;

            while (Time.unscaledTime < deadline)
            {
                if (NetworkClient.isConnected)
                {
                    connectCoroutine = null;
                    yield break;
                }

                // Left Connecting without ever connecting: refused, unreachable
                // address or dropped. No point waiting out the timeout.
                if (!NetworkClient.active)
                {
                    connectCoroutine = null;
                    FailConnection("Connection failed.");
                    yield break;
                }

                yield return null;
            }

            connectCoroutine = null;
            FailConnection("Connection timed out.");
        }

        private void FailConnection(string message)
        {
            Debug.LogWarning($"[{nameof(MainMenuController)}] {message}", this);

            if (NetworkManager.singleton != null && NetworkClient.active)
                NetworkManager.singleton.StopClient();

            SetStatus(message);
            ResetUI();
        }

        private void SetStatus(string message)
        {
            if (statusText == null) return;

            statusText.text = message;
            statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        public void ResetUI()
        {
            if (connectCoroutine != null)
            {
                StopCoroutine(connectCoroutine);
                connectCoroutine = null;
            }

            if (loadingScreen != null) loadingScreen.Hide();
        }
    }
}