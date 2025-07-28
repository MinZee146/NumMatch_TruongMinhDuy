using System;
using UnityEngine;

[Serializable]
public class Sound
{
    public string Name;
    public AudioClip Clip;
}

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private Sound[] _sounds;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySfx(string name)
    {
        var s = Array.Find(_sounds, s => s.Name == name);
        if (s != null)
        {
            if (_audioSource.clip == s.Clip) return;
            
            _audioSource.PlayOneShot(s.Clip);
        }
    }
}
