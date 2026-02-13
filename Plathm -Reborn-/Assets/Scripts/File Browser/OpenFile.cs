using SFB;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class OpenFile : MonoBehaviour
{
    [SerializeField] UIManager uiManager;

    [Header("Song Information")]
    [SerializeField] TMP_Text directoryText;
    [SerializeField] TMP_InputField songNameInputField;
    [SerializeField] TMP_InputField songArtistInputField;
    [SerializeField] TMP_InputField charterNameInputField;
    [SerializeField] TMP_InputField chartOffsetInputField;
    [SerializeField] TMP_InputField chartSpeedInputField;

    public enum FileType
    {
        TYPE_AUDIO,
        TYPE_JACKET,
        TYPE_PROJECT,
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

    public void OnClickOpenProject()
    {
        string path = StandaloneFileBrowser.OpenFolderPanel("Select Project Folder", "", false)[0];

        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(OutputRoutineOpen(path, FileType.TYPE_PROJECT));
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
        else if (fileType == FileType.TYPE_PROJECT)
        {
            if (!Directory.Exists(url))
            {
                Debug.Log("Project does not exist: " + url);
                yield break;
            }

            directoryText.text = url;
            uiManager.RemoveAllTimings();

            string[] files = Directory.GetFiles(url);
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                Debug.Log("Found file: " + file);

                if (extension == ".ogg")
                {
                    StartCoroutine(OutputRoutineOpen(new System.Uri(file).AbsoluteUri, FileType.TYPE_AUDIO));
                }
                else if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                {
                    StartCoroutine(OutputRoutineOpen(new System.Uri(file).AbsoluteUri, FileType.TYPE_JACKET));
                }
                else if (extension == ".ptmf")
                {
                    Debug.Log("The chart file is suitable.");
                }
                else if (extension == ".ptminf")
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line == string.Empty)
                            {
                                continue;
                            }

                            if (line.StartsWith(ValueStorer.songNameString))
                            {
                                string songName = line.Substring(ValueStorer.songNameString.Length);
                                songNameInputField.text = songName;
                                continue;
                            }

                            if (line.StartsWith(ValueStorer.songArtistString))
                            {
                                string songArtist = line.Substring(ValueStorer.songArtistString.Length);
                                songArtistInputField.text = songArtist;
                                continue;
                            }

                            if (line.StartsWith(ValueStorer.charterNameString))
                            {
                                string charterName = line.Substring(ValueStorer.charterNameString.Length);
                                charterNameInputField.text = charterName;
                                continue;
                            }

                            if (line.StartsWith(ValueStorer.chartOffsetString))
                            {
                                string chartOffset = line.Substring(ValueStorer.chartOffsetString.Length);
                                chartOffsetInputField.text = chartOffset;
                                continue;
                            }

                            if (line.StartsWith(ValueStorer.chartSpeedString))
                            {
                                string chartSpeed = line.Substring(ValueStorer.chartSpeedString.Length);
                                chartSpeedInputField.text = chartSpeed;
                                continue;
                            }

                            if (line.StartsWith(ValueStorer.timingString))
                            {
                                string content = line.Replace(ValueStorer.timingString, "").Replace(")", "");
                                string[] values = content.Split(',');

                                if (float.TryParse(values[0], out float timing) &&
                                    (float.TryParse(values[1], out float BPM)))
                                {
                                    uiManager.AddTimingItem(timing, BPM);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("The file is not supported");
                }
            }
        }
    }
}
