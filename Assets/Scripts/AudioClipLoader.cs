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
        GetComponent<AudioSource>().clip = _www.audioClip;
        Config.SongLength = GetComponent<AudioSource>().clip.length;
    }

    public void Update()
    {
        if (GetComponent<AudioSource>().clip != null && GetComponent<AudioSource>().clip.isReadyToPlay && _playOnce)
        {
            GetComponent<AudioSource>().Play();
            _playOnce = false;
        }
        if (GetComponent<AudioSource>().clip != null && !GetComponent<AudioSource>().isPlaying && GetComponent<AudioSource>().clip.isReadyToPlay && _loop)
        {
            GetComponent<AudioSource>().Play();
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