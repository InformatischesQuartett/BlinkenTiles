using UnityEngine;
using System.Collections;

public class AudioClipLoader : MonoBehaviour {

    public string url;
    private WWW www;

    public void Start()
    {
        url = "file://" + Application.streamingAssetsPath + url;
        Debug.Log(url);
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
        if (audio.clip != null && !audio.isPlaying && audio.clip.isReadyToPlay)
        {
            audio.Play();
        }
    }
}
