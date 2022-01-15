using TMPro;
using UnityEngine;

public class UI_PlayerHud : MonoBehaviour
{
    private Transform m_container;
    private FPS_Controller m_player;

    [SerializeField] TextMeshProUGUI m_joinCooldownTxt;

    [SerializeField] TextMeshProUGUI m_healthTxt;

    private void Awake()
    {
        if (StartupOptions.isHeadlessServer)
        {
            return;
        }
        m_container = transform.GetChild(0);
    }

    private void Update()
    {
        if (StartupOptions.isHeadlessServer || m_player == null)
        {
            return;
        }

        if (m_player.IsDead)
        {
            float t = m_player.RemainungRespawnTime;
            if (t <= 0.0f)
            {
                m_joinCooldownTxt.text = string.Empty;
            }
            else
            {
                m_joinCooldownTxt.text = m_player.RemainungRespawnTime.ToString("0.0");
            }
        }
    }

    public void PlayerReady(FPS_Controller player)
    {
        if (StartupOptions.isHeadlessServer)
        {
            return;
        }
        m_player = player;

        m_player.Health.PlayerHealthChanged += (hp) =>
        {
            m_healthTxt.text = hp.ToString("0");
        };
    }
}
