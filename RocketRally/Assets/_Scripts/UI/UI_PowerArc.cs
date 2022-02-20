using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PowerArc : MonoBehaviour
{
    private float _minArcMax = 10.0f / 100.0f;
    private float _medArcMax = 0.0f;// 20.0f / 100.0f; // UNUSED

    private Image[] _arcs;
    private Gun _gun;

    private void Start()
    {
        _arcs = GetComponentsInChildren<Image>();
    }

    public void PlayerReady(FPS_Controller player)
    {
        _gun = player.Gun;
        _gun.PowerChanged += OnPowerChanged;
        _gun.RocketTypeChanged += OnRocketTypeChagned;

        OnRocketTypeChagned(_gun.CurrentRocketType);
    }

    private void OnRocketTypeChagned(Gun.RocketType obj)
    {
        _minArcMax = _gun.CurrentPowerCost / 100.0f;

        OnPowerChanged(_gun.Power);
    }

    private void OnPowerChanged(float pwr)
    {
        float p = pwr / 100.0f;
        _arcs[0].fillAmount = p;
        _arcs[1].fillAmount = Mathf.Clamp(p, 0, _medArcMax);
        _arcs[2].fillAmount = Mathf.Clamp(p, 0, _minArcMax);
    }
}
