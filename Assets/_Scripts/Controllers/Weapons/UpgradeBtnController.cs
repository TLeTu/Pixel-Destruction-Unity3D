using UnityEngine;

public class UpgradeBtnController : MonoBehaviour
{
    public WeaponUpgrade upgrade;
    public void OnMouseDown()
    {
        Debug.Log("Upgrade button clicked: " + upgrade);
        ObstacleManager.instance.ApplyUpgradeToWeapon(upgrade);
    }
}