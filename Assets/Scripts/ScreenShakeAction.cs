using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ScreenShakeAction : MonoBehaviour
{
    private void Start()
    {
        // ShootAction.OnAnyShoot += ShootAction_OnAnyShoot;
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
    }

    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake();
    }

    // private void ShootAction_OnAnyShoot(object sender, ShootAction.OnShootEventArgs e)
    // {
    //     ScreenShake.Instance.Shake();
    // }
}
