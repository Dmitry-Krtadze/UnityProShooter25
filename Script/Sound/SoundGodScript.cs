using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SoundGodEntity
{
    public string soundType;
    public AudioClip clip;
}

[System.Serializable]
public class SoundCategory
{
    public string categoryName;
    public List<SoundGodEntity> sounds;
}

public class SoundGodScript : MonoBehaviour
{
    public List<SoundCategory> soundCategories;
    private Dictionary<string, Dictionary<string, AudioClip>> soundDictionary;

    void Awake()
    {

        soundDictionary = new Dictionary<string, Dictionary<string, AudioClip>>();
        foreach (var category in soundCategories)
        {
            var categoryDict = new Dictionary<string, AudioClip>();
            foreach (var sound in category.sounds)
            {
                categoryDict[sound.soundType] = sound.clip;
            }
            soundDictionary[category.categoryName] = categoryDict;
        }
    }


    public AudioClip GetSound(string category, string type)
    {
        if (soundDictionary.ContainsKey(category) && soundDictionary[category].ContainsKey(type))
        {
            return soundDictionary[category][type];
        }
        Debug.LogWarning($"Sound '{type}' in category '{category}' not found!");
        return null;
    }
}
