using Assets.AsyncVideo.Coroutines.Example4;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField ipInputField;

    [SerializeField] private LoadingScreen loadingScreen;

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

        NetworkManager.singleton.networkAddress = "localhost";
        NetworkManager.singleton.StartClient();
    }

}
