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
    public void SpawnObstalce(Vector3 position)
    {
        GameObject newObstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);
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
}   