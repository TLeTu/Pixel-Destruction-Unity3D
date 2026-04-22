using UnityEngine;

public interface IWeaponController
{
    void DetectTargets();
    void Pause(bool shouldPause);
    void ApplyUpgrade(WeaponUpgrade upgrade);
}