using UnityEngine;
using System.Collections;
using System.Security.AccessControl;

public class AudioClipLoader : MonoBehaviour {

    public string url;
    private WWW www;
    private bool playOnce = false;
    private bool loop = false;


    public void Start()
    {
        url = "file://" + Application.streamingAssetsPath + url;
        www = new WWW(url);
        StartCoroutine(WaitForAudioClip());
    }

    public IEnumerator WaitForAudioClip()
    {
        yield return www;
        audio.clip = www.audioClip;
    }

    public void Update()
    {
        if (playOnce)
            Debug.Log("Play");
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

    public void Play(AudioPlayMode audioPlayMode)
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
