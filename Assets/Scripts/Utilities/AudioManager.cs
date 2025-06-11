using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class AudioData
{
    public string name;
    public AudioClip clip;
    public float volume = 1;
}

[CreateAssetMenu(fileName = "AudioManager", menuName = "Scriptable Objects/Audio Manager")]
public class AudioManager : SingletonScriptableObject<AudioManager>
{
    [SerializeField] private List<AudioData> _sfxList;

    private AudioData GetSFX(string name)
    {
        foreach (AudioData sfx in _sfxList) {
            if (sfx.name == name)
                return sfx;
        }
        return null;
    }

    public void PlaySFXInScreen(string name)
    {
        AudioData audio = GetSFX(name);
        if (audio == null) return;
        AudioSource camSource = Camera.main.GetComponent<AudioSource>();
        if (camSource == null)
        {
            camSource = Camera.main.AddComponent<AudioSource>();
        }
        camSource.PlayOneShot(audio.clip, audio.volume);
    }
}
