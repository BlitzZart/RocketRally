using TMPro;
using UnityEngine;

public class UI_PlayerHud : MonoBehaviour
{
    private Transform m_container;
    private FPS_Controller m_player;

    [SerializeField] TextMeshProUGUI m_healthTxt;

    private void Awake()
    {
        m_container = transform.GetChild(0);
    }

    public void PlayerReady(FPS_Controller player)
    {
        m_player = player;

        m_player.Health.OnHealthChanged += (hp) =>
        {
            m_healthTxt.text = hp.ToString("0");
        };
    }
}