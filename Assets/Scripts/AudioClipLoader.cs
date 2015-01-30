using System.Collections;
using UnityEngine;

public class AudioClipLoader : MonoBehaviour
{
    private bool loop;
    private bool playOnce;
    public string url;
    private WWW www;


    public void Start()
    {
        url = "file://" + Application.streamingAssetsPath + url;
        www = new WWW(url);
        StartCoroutine(WaitForAudioClip());

        transform.position = transform.parent.position;
    }

    public IEnumerator WaitForAudioClip()
    {
        yield return www;
        audio.clip = www.audioClip;
        Config.SongLength = audio.clip.length;
    }

    public void Update()
    {
        if (audio.clip != null && audio.clip.isReadyToPlay && playOnce)
        {
            audio.Play();
            playOnce = false;
        }
        if (audio.clip != null && !audio.isPlaying && audio.clip.isReadyToPlay && loop)
        {
            audio.Play();
            playOnce = false;
        }
    }

    public void Play(AudioPlayMode audioPlayMode = AudioPlayMode.Once)
    {
        switch (audioPlayMode)
        {
            case AudioPlayMode.Loop:
                loop = true;
                break;
            case AudioPlayMode.Once:
                playOnce = true;
                break;
        }
    }
}