using UnityEngine;

public class LightController : MonoBehaviour
{
    private Light lampLight;
    private Transform globalLightTransform;
    public string globalLightName = "GlobalLight";

    void Start()
    {
        lampLight = GetComponent<Light>();

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

        // Считаем, что ночь — когда свет направлен вверх
        bool isNight = Vector3.Dot(globalLightTransform.forward, Vector3.down) < 0f;

        lampLight.enabled = isNight;
    }
}