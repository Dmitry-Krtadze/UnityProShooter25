using UnityEngine;
using Photon.Pun;

public class ObjectSpawner : MonoBehaviourPunCallbacks
{
    public static ObjectSpawner Instance;

    public GameObject[] objectsToSpawn; // Массив префабов для спавна
    public Transform[] spawnPoints;     // Точки спавна
    public float spawnInterval = 5f;      // Интервал между спавнами

    private float nextSpawnTime = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && Time.time >= nextSpawnTime)
        {
            SpawnRandomObject();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    public void SpawnRandomObject()
    {
        int randomObjectIndex = Random.Range(0, objectsToSpawn.Length);
        int randomPointIndex = Random.Range(0, spawnPoints.Length);

        GameObject objectToSpawn = objectsToSpawn[randomObjectIndex];
        Transform spawnPoint = spawnPoints[randomPointIndex];

        // Синхронизированный спавн через PhotonNetwork.Instantiate
        GameObject obj = PhotonNetwork.Instantiate(objectToSpawn.name, spawnPoint.position, spawnPoint.rotation);
        obj.transform.SetParent(transform);
    }
}
