using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PowerArc : MonoBehaviour
{
    private float _minArcMax = 10.0f / 100.0f;
    private float _medArcMax = 20.0f / 100.0f;

    private Image[] _arcs;

    private void Start()
    {
        _arcs = GetComponentsInChildren<Image>();
    }

    public void PlayerReady(FPS_Controller player)
    {
        player.Gun.PowerChanged += OnPowerChanged;
    }

    private void OnPowerChanged(float pwr)
    {
        float p = pwr / 100.0f;
        _arcs[0].fillAmount = p;
        _arcs[1].fillAmount = Mathf.Clamp(p, 0, _medArcMax);
        _arcs[2].fillAmount = Mathf.Clamp(p, 0, _minArcMax);
    }
}
