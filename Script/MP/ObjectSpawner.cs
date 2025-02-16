using UnityEngine;
using Photon.Pun;

public class ObjectSpawner : MonoBehaviourPunCallbacks
{
    public GameObject[] objectsToSpawn; // Массив объектов для спавна
    public Transform[] spawnPoints; // Массив точек, где можно спавнить объекты
    public float spawnInterval = 5f; // Интервал между спавнами в секундах

    private float nextSpawnTime = 0f;

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            nextSpawnTime = Time.time + spawnInterval; // Устанавливаем время для первого спавна
        }
    }

    private void Update()
    {
        // Проверяем, если текущее время больше или равно времени следующего спавна
        if (PhotonNetwork.IsConnected && Time.time >= nextSpawnTime)
        {
            SpawnRandomObject();
            nextSpawnTime = Time.time + spawnInterval; // Обновляем время для следующего спавна
        }
    }

    private void SpawnRandomObject()
    {
        // Выбираем случайный объект из массива
        int randomObjectIndex = Random.Range(0, objectsToSpawn.Length);
        GameObject objectToSpawn = objectsToSpawn[randomObjectIndex];

        // Выбираем случайную точку для спавна
        int randomPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomPointIndex];

        // Спавним объект через Photon с использованием PhotonNetwork.Instantiate
        PhotonNetwork.Instantiate(objectToSpawn.name, spawnPoint.position, spawnPoint.rotation);
    }
}
