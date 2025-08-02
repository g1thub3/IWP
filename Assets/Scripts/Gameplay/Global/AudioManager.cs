using System.Collections;
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
    AudioData _currBGM;
    private float _sfxVol, _bgmVol;
    public float SFXVolume
    {
        get { return _sfxVol; }
        set { 
            _sfxVol = value;
            GlobalCanvasManager.Instance.SFXSource.volume = _sfxVol;
        }
    }
    public float BGMVolume
    {
        get { return _bgmVol; }
        set { 
            _bgmVol = value;
            float scale = 0.0f;
            if (_currBGM != null)
                scale = _currBGM.volume;
            GlobalCanvasManager.Instance.BGMSource.volume = _bgmVol * scale;
        }
    }

    private int _bgmCounter;
    private Dictionary<string, AudioData> _sfxDictionary;
    private Dictionary<string, AudioData> _bgmDictionary;
    [SerializeField] private List<AudioData> _bgmList;
    [SerializeField] private List<AudioData> _sfxList;

    private void OnEnable()
    {
        
        _bgmCounter = 0;
        _sfxDictionary = new Dictionary<string, AudioData>();
        if (_sfxList != null) {
            foreach (AudioData sfx in _sfxList)
            {
                _sfxDictionary.Add(sfx.name, sfx);
            }
        }

        _bgmDictionary = new Dictionary<string, AudioData>();
        if (_bgmList != null)
        {
            foreach (AudioData bgm in _bgmList)
            {
                _bgmDictionary.Add(bgm.name, bgm);
            }
        }
    }

    private AudioData GetSFX(string name)
    {
        if (_sfxDictionary == null) return null;
        return _sfxDictionary[name];
    }
    private AudioData GetBGM(string name)
    {
        if (_bgmDictionary == null) return null;
        return _bgmDictionary[name];
    }

    public void PlaySFXInScreen(string name)
    {
        AudioData audio = GetSFX(name);
        if (audio == null) return;
        GlobalCanvasManager.Instance.SFXSource.PlayOneShot(audio.clip, audio.volume);
    }

    public void PlayFromObject(AudioSource source, string name)
    {
        AudioData audio = GetSFX(name);
        if (audio == null) return;
        source.PlayOneShot(audio.clip, audio.volume);
    }

    public void PlayBGM(string name, float fadeTime = 1.0f)
    {
        AudioData audio = GetBGM(name);
        if (audio == null) return;
        if (GlobalCanvasManager.Instance.BGMSource.clip == audio.clip) return;
        _bgmCounter++;
        GlobalCanvasManager.Instance.StartCoroutine(FadeCoroutine(audio, fadeTime));
    }

    public void PlayBGM(string name)
    {
        PlayBGM(name, 1.0f);
    }

    private IEnumerator FadeCoroutine(AudioData audio, float fadeTime)
    {
        int counter = _bgmCounter;
        Transition newTrans = new Transition();
        newTrans.max = fadeTime;
        newTrans.t = newTrans.max;

        if (newTrans.max > 0.0f)
        {
            var curr = GlobalCanvasManager.Instance.BGMSource.volume;
            while (newTrans.Progression > 0.0f)
            {
                if (counter != _bgmCounter) break;
                newTrans.Revert();
                GlobalCanvasManager.Instance.BGMSource.volume = curr * newTrans.Progression;
                yield return new WaitForEndOfFrame();
            }
        }

        if (counter == _bgmCounter) {
            GlobalCanvasManager.Instance.BGMSource.volume = 0;
        }
        GlobalCanvasManager.Instance.BGMSource.clip = audio.clip;
        GlobalCanvasManager.Instance.BGMSource.Play();
        _currBGM = audio;

        if (newTrans.max > 0.0f)
        {
            var curr = audio.volume * _bgmVol;
            while (newTrans.Progression < 1.0f)
            {
                if (counter != _bgmCounter) break;
                newTrans.Progress();
                GlobalCanvasManager.Instance.BGMSource.volume = curr * newTrans.Progression;
                yield return new WaitForEndOfFrame();
            }
        } else
        {
            var curr = audio.volume * _bgmVol;
            GlobalCanvasManager.Instance.BGMSource.volume = curr;
        }
    }
}
