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
        [SerializeField] private TMP_InputField ipInputField;

        [SerializeField] private LoadingScreen loadingScreen;
        private Coroutine timeoutCoroutine;
        private void Start()
        {

            hostButton.onClick.AddListener(StartHost);
            clientButton.onClick.AddListener(StartClient);

            if (ipInputField != null && string.IsNullOrEmpty(ipInputField.text))
            {
                ipInputField.text = "localhost";
            }
        }



        private void StartHost()
        {
            loadingScreen.Show();
            NetworkManager.singleton.StartHost();
        }

        private void StartClient()
        {
            loadingScreen.Show();

            NetworkManager.singleton.networkAddress = ipInputField.text;
            NetworkManager.singleton.StartClient();
            timeoutCoroutine = StartCoroutine(ConnectTimeout(5f));
        }

        private IEnumerator ConnectTimeout(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            if (!NetworkClient.isConnected)
            {
                Debug.Log("Connection timed out");
                NetworkManager.singleton.StopClient();
                ResetUI();
            }

        }
        public void ResetUI()
        {
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            if (loadingScreen != null) loadingScreen.Hide();
        }
    }
}