using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject weaponPrefab;
    public GameObject obstaclePrefab;
    public static ObstacleManager instance;
    private Dictionary<GameObject, IWeaponController> obstacleWeaponMap = new Dictionary<GameObject, IWeaponController>();
    void Awake()
    {
        instance = this;
    }
    public void LoadWeaponPrefab(GameObject prefab)
    {
        weaponPrefab = prefab;
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
}