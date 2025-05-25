using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxExposureController : MonoBehaviour
{
    public Material skyboxMaterial;
    public Transform globalLightTransform;
    
    public float dayExposure = 1.3f;
    public float nightExposure = 0.2f;
    public float transitionSpeed = 0.5f;

    private float targetExposure;

    void Start()
    {
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
        }
    }

    void Update()
    {
        if (skyboxMaterial == null || globalLightTransform == null) return;

        // Определяем: ночь или день (если солнце светит вверх — ночь)
        bool isNight = Vector3.Dot(globalLightTransform.forward, Vector3.down) < 0f;
        targetExposure = isNight ? nightExposure : dayExposure;

        float currentExposure = skyboxMaterial.GetFloat("_Exposure");
        float newExposure = Mathf.MoveTowards(currentExposure, targetExposure, transitionSpeed * Time.deltaTime);
        skyboxMaterial.SetFloat("_Exposure", newExposure);

        DynamicGI.UpdateEnvironment(); // Обновить глобальное освещение
    }
}