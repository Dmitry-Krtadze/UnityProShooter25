using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
    public Material blendedSkybox;
    public Transform globalLightTransform;

    [Range(0f, 1f)]
    public float blendSpeed = 0.5f;

    private float targetBlend = 0f;

    void Start()
    {
        RenderSettings.skybox = blendedSkybox;
        DynamicGI.UpdateEnvironment();
    }

    void Update()
    {
        if (blendedSkybox == null || globalLightTransform == null) return;

        // Ночь — если солнце направлено вверх
        bool isNight = Vector3.Dot(globalLightTransform.forward, Vector3.down) < 0f;
        targetBlend = isNight ? 1f : 0f;

        // Плавный переход между 0 и 1
        float currentBlend = blendedSkybox.GetFloat("_Blend");
        currentBlend = Mathf.MoveTowards(currentBlend, targetBlend, blendSpeed * Time.deltaTime);
        blendedSkybox.SetFloat("_Blend", currentBlend);

        DynamicGI.UpdateEnvironment();
    }
}