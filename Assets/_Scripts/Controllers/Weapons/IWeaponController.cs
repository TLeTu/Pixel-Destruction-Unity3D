using UnityEngine;

public interface IWeaponController
{
    void DetectTargets();
    void Pause(bool shouldPause);
}