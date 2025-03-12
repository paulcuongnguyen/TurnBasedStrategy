using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ScreenShakeAction : MonoBehaviour
{
    private void Start()
    {
        GrenadeProjectile.OnAnyGrenadeExloped += GrenadeProjectile_OnAnyGrenadeExloped;
    }

    private void GrenadeProjectile_OnAnyGrenadeExloped(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake(2f);
    }
}
