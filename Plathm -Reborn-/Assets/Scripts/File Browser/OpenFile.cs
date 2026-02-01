using UnityEngine;
using UnityEngine.UI;
using SFB;
using UnityEngine.Networking;
using System.Collections;

public class OpenFile : MonoBehaviour
{
    public enum FileType
    {
        TYPE_AUDIO,
        TYPE_JACKET,
        TYPE_CHART,
    }

    public AudioSource audioSource;
    public Image jacketArt;

    public void OnClickOpenMusic()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Music", "", "ogg", false);
        if (paths.Length > 0)
        {
            StartCoroutine(OutputRoutineOpen(new System.Uri(paths[0]).AbsoluteUri, FileType.TYPE_AUDIO));
        }
    }

    public void OnClickOpenJacket()
    {
        ExtensionFilter[] extensions =
        {
            new ExtensionFilter("", "png"),
            new ExtensionFilter("", "jpg"),
            new ExtensionFilter("", "jpeg")
        };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Jacket Art", "", extensions, false);
        if (paths.Length > 0)
        {
            StartCoroutine(OutputRoutineOpen(new System.Uri(paths[0]).AbsoluteUri, FileType.TYPE_JACKET));
        }
    }

    IEnumerator OutputRoutineOpen(string url, FileType fileType)
    {
        if (fileType == FileType.TYPE_AUDIO)
        {
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error opening music");
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = audioClip;
            }
        }
        else if (fileType == FileType.TYPE_JACKET)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error opening jacket art");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                jacketArt.sprite = sprite;
                jacketArt.preserveAspect = true;
            }
        }
    }
}
