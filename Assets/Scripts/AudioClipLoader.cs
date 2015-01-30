using System.Collections;
using UnityEngine;

public class AudioClipLoader : MonoBehaviour
{
    public string Url;
    private bool _loop;
    private bool _playOnce;
    private WWW _www;


    public void Start()
    {
        Url = "file://" + Application.streamingAssetsPath + Url;
        _www = new WWW(Url);
        StartCoroutine(WaitForAudioClip());

        transform.position = transform.parent.position;
    }

    public IEnumerator WaitForAudioClip()
    {
        yield return _www;
        audio.clip = _www.audioClip;
        Config.SongLength = audio.clip.length;
    }

    public void Update()
    {
        if (audio.clip != null && audio.clip.isReadyToPlay && _playOnce)
        {
            audio.Play();
            _playOnce = false;
        }
        if (audio.clip != null && !audio.isPlaying && audio.clip.isReadyToPlay && _loop)
        {
            audio.Play();
            _playOnce = false;
        }
    }

    public void Play(AudioPlayMode audioPlayMode = AudioPlayMode.Once)
    {
        switch (audioPlayMode)
        {
            case AudioPlayMode.Loop:
                _loop = true;
                break;
            case AudioPlayMode.Once:
                _playOnce = true;
                break;
        }
    }
}