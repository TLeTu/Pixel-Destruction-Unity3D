using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject weaponPrefab;
    public GameObject obstaclePrefab;
    public static ObstacleManager instance;
    private Dictionary<GameObject, IWeaponController> obstacleWeaponMap = new Dictionary<GameObject, IWeaponController>();
    private List<WeaponUpgrade> currentWeaponUpgrades = new List<WeaponUpgrade>();
    private int weaponsToPlaceQuota = 0;
    private int weaponsPlacedThisSession = 0;
    void Awake()
    {
        instance = this;
    }
    public void LoadWeaponPrefab(GameObject prefab)
    {
        weaponPrefab = prefab;
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
        weaponsToPlaceQuota = count;
        weaponsPlacedThisSession = 0;

    }
    public void TryPlaceWeaponOn(GameObject obstacle)
    {
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
    Range,
    MoreWeapons
}