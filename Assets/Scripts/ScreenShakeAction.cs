using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ScreenShakeAction : MonoBehaviour
{
    private void Start()
    {
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
        GrenadeProjectile.OnAnyGrenadeExloped += GrenadeProjectile_OnAnyGrenadeExloped;
    }

    private void GrenadeProjectile_OnAnyGrenadeExloped(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake(2f);
    }

    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake();
    }   
}
