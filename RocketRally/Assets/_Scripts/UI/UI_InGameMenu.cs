using UnityEngine;
using UnityEngine.UI;

public class UI_InGameMenu : MonoBehaviour
{
    private static UI_InGameMenu _instance;

    public static bool IsActive { get => _instance._container.activeSelf; }


    [SerializeField] Button _btnRestartServer, _btnQuitGame;
    private GameObject _container;

    void Start()
    {
        _instance = this;
        _container = transform.GetChild(0).gameObject;

        _btnRestartServer.onClick.AddListener(OnRestartServer);
        _btnQuitGame.onClick.AddListener(OnQuitGame);

        _container.SetActive(false);
    }

    private void OnQuitGame()
    {
        Application.Quit();
    }

    private void OnRestartServer()
    {
        NW_PlayerScript.Instance.RestartServerAndClient();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _container.SetActive(!_container.activeSelf);

            if (_container.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

        }
    }
}
