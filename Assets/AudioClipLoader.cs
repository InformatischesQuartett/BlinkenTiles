using UnityEngine;
using System.Collections;

public class AudioClipLoader : MonoBehaviour {

    public string url;
    private WWW www;

    public void Start()
    {
        url = "file://" + Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")) + url;
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
