using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject weaponPrefab;
    public GameObject obstaclePrefab;
    public static ObstacleManager instance;
    private Dictionary<GameObject, IWeaponController> obstacleWeaponMap = new Dictionary<GameObject, IWeaponController>();
    private int weaponsToPlaceQuota = 0;
    private int weaponsPlacedThisSession = 0;
    private WeaponUpgrade[] nonMoreWeaponsUpgrades = new WeaponUpgrade[] { WeaponUpgrade.Damage, WeaponUpgrade.Time, WeaponUpgrade.Range };
    private WeaponUpgrade[] allUpgrades = (WeaponUpgrade[])Enum.GetValues(typeof(WeaponUpgrade));
    private List<WeaponUpgrade> currentUpgrades = new List<WeaponUpgrade>();
    void Awake()
    {
        instance = this;
    }
    public void LoadWeaponPrefab(GameObject prefab)
    {
        weaponPrefab = prefab;
    }
    public int GetObstaclesCount()
    {
        return obstacleWeaponMap.Count;
    }
    public int AvailableWeaponSlots()
    {
        int count = 0;
        foreach (var kvp in obstacleWeaponMap)
        {
            if (kvp.Value == null)
            {
                count++;
            }
        }
        return count;
    }
    public List<GameObject> GetObstaclesWithoutWeapons()
    {
        List<GameObject> obstaclesWithoutWeapons = new List<GameObject>();
        foreach (var kvp in obstacleWeaponMap)
        {
            if (kvp.Value == null)
            {
                obstaclesWithoutWeapons.Add(kvp.Key);
            }
        }
        return obstaclesWithoutWeapons;
    }
    public void SpawnObstacle(Vector3 position)
    {
        GameObject newObstacle = Instantiate(obstaclePrefab, position, obstaclePrefab.transform.rotation);
        obstacleWeaponMap[newObstacle] = null;
    }
    public void PlaceWeaponOnObstacle(GameObject obstacle)
    {
        GameObject newWeapon = Instantiate(weaponPrefab, obstacle.transform.position, Quaternion.identity);
        IWeaponController weaponController = newWeapon.GetComponent<IWeaponController>();
        if (obstacleWeaponMap.ContainsKey(obstacle))
        {
            obstacleWeaponMap[obstacle] = weaponController;
        }
        obstacle.SetActive(false);
        if(currentUpgrades.Count > 0)
        {
            foreach(WeaponUpgrade upgrade in currentUpgrades)
            {
                weaponController.ApplyUpgrade(upgrade);
            }
        }
        Debug.Log("Weapon placed on obstacle: " + obstacle.name);
    }
    public void PauseWeapons(bool shouldPause)
    {
        foreach (var kvp in obstacleWeaponMap)
        {
            IWeaponController weaponController = kvp.Value;
            if (weaponController != null)
            {
                weaponController.Pause(shouldPause);
            }
        }
    }
    public void StartPlacingSession(int count)
    {
        weaponsToPlaceQuota = Mathf.Max(0, count);
        weaponsPlacedThisSession = 0;

    }
    public void TryPlaceWeaponOn(GameObject obstacle)
    {
        if (weaponsToPlaceQuota <= 0)
        {
            return;
        }

        if (obstacleWeaponMap.ContainsKey(obstacle) && obstacleWeaponMap[obstacle] == null)
        {
            Debug.Log("Placing weapon on obstacle: " + obstacle.name);
            PlaceWeaponOnObstacle(obstacle);

            weaponsPlacedThisSession++;

            if (weaponsPlacedThisSession >= weaponsToPlaceQuota)
            {
                FinishPlacingSession();
            }
        }
    }
    public void ApplyUpgradeToWeapon(WeaponUpgrade upgrade)
    {
        foreach (var kvp in obstacleWeaponMap)
        {
            IWeaponController weaponController = kvp.Value;
            if (weaponController != null)
            {
                weaponController.ApplyUpgrade(upgrade);
            }
        }
        currentUpgrades.Add(upgrade);
    }
    public void CleanUp()
    {
        // Destroy all obstacles and weapons
        foreach (var kvp in obstacleWeaponMap)        {
            GameObject obstacle = kvp.Key;
            IWeaponController weaponController = kvp.Value;
            if (weaponController != null)            {
                GameObject weaponObj = ((MonoBehaviour)weaponController).gameObject;
                Destroy(weaponObj);
            }
            Destroy(obstacle);
        }
        obstacleWeaponMap.Clear();
        weaponsToPlaceQuota = 0;
        weaponsPlacedThisSession = 0;
        currentUpgrades.Clear();
    }
    public WeaponUpgrade GetRandomUpgrade()
    {
        if(AvailableWeaponSlots() > 0)
        {
            return allUpgrades[UnityEngine.Random.Range(0, allUpgrades.Length)];
        }
        else
        {
            return nonMoreWeaponsUpgrades[UnityEngine.Random.Range(0, nonMoreWeaponsUpgrades.Length)];
        }
    }

    private void FinishPlacingSession()
    {
        weaponsToPlaceQuota = 0;
        weaponsPlacedThisSession = 0;

        GameManager.instance.SetGameState(GameState.Playing);
    }
}

public enum WeaponUpgrade
{
    Damage,
    Time,
    Range,
    MoreWeapons
}