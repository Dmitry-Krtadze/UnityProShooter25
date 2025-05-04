using UnityEngine;

public class LightController : MonoBehaviour
{
    private Light lampLight;
    private Transform globalLightTransform;
    public string globalLightName = "GlobalLight"; // Имя directional light

    // Порог для включения света ночью (если солнце ниже горизонта)
    public float nightThreshold = 100f;

    void Start()
    {
        // Берём свет лампы
        lampLight = GetComponent<Light>();

        // Находим глобальный directional light
        GameObject globalLight = GameObject.Find(globalLightName);
        if (globalLight != null)
        {
            globalLightTransform = globalLight.transform;
        }
        else
        {
            Debug.LogError("GlobalLight не найден! Убедись, что объект называется точно 'GlobalLight'");
        }

        if (lampLight == null)
        {
            Debug.LogError("Light не найден на объекте лампы");
        }
    }

    void Update()
    {
        if (lampLight == null || globalLightTransform == null) return;

        // Получаем поворот по X
        float xRotation = globalLightTransform.rotation.eulerAngles.x;

        // Считаем, что день — когда X около 90 (над головой), а ночь — когда далеко от 90
        // Можно изменить условие по вкусу
        bool isNight = xRotation > nightThreshold && xRotation < 300f;

        // Включаем или выключаем лампу
        lampLight.enabled = isNight;
    }
}
