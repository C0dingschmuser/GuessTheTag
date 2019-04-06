using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class OfflinePack : MonoBehaviour
{
    public static bool offlineInstalled = false;
    private bool doneDownload = false, running = false;
    private ulong dlSize = 77069801; //posts.zip größe in bytes
    public float dl_progress = 0f;
    public GameObject menu, postInfo, updateText;
    float version = 0.3f;
    int postID = 3126416;

    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("Offline_Installed", 0) == 1)
        {
            offlineInstalled = true;
        }

        postID = PlayerPrefs.GetInt("PostID", 3126416);

        StartCoroutine(GetData());
    }

    public void DownloadPack()
    {
        StartCoroutine(GetPack());
    }

    public static void Decompress(string path, string extractPath)
    {
        using (ZipArchive archive = ZipFile.OpenRead(path))
        {
            archive.ExtractToDirectory(extractPath);
        }
    }

    public void OpenPost()
    {
        DontOpenpost();
        string link = "https://pr0gramm.com/new/" + postID.ToString();

        Application.OpenURL(link);
    }

    public void DontOpenpost()
    {
        postInfo.SetActive(false);
    }

    IEnumerator GetData()
    { //checkt updates & co
        string link = "https://drive.google.com/uc?export=download&id=10tSzJkmI0Xj8njpHOff2HtnMpn9bIhAY";
        UnityWebRequest www = UnityWebRequest.Get(link);
        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        } else
        {
            string text = www.downloadHandler.text;
            string[] split = text.Split('|');

            float newVersion = (float)Int32.Parse(split[0]) / 10f;

            if(newVersion > version)
            { //update
                updateText.SetActive(true);
                //updateText.GetComponent<TextMeshProUGUI>().text = newVersion.ToString();
            } else
            {
                updateText.SetActive(false);
            }

            int newPostID = Int32.Parse(split[1]);

            if(newPostID != postID)
            { //neuer post
                PlayerPrefs.SetInt("PostID", newPostID);
                postID = newPostID;
                postInfo.SetActive(true);
            }
        }

        yield return null;
    }

    IEnumerator GetPack()
    {
        string link = "https://drive.google.com/uc?export=download&id=1m5SFIDgNxSLOJsYr5R4PkczjwmMUBvB_";
        UnityWebRequest www = UnityWebRequest.Get(link);
        www.downloadHandler = new DownloadHandlerBuffer();

        running = true;
        StartCoroutine(ShowProgress(www));

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            running = false;
            menu.GetComponent<MenuData>().DownloadError();
        } else
        {
            string path = Application.persistentDataPath + "/posts.zip";
            string extractPath = Application.persistentDataPath + "/";

            bool error = false;

            try
            {

                if (File.Exists(path))
                { //löscht falls download korrupt / abgebrochen
                    File.Delete(path);
                }

                File.WriteAllBytes(path, www.downloadHandler.data);
                Decompress(path, extractPath);

                if (File.Exists(path))
                { //löscht .zip wenn fertig; nicht länger benötigt
                    File.Delete(path);
                }
            } catch (Exception e)
            {
                error = true;
                Debug.Log("DL-Error: " + e);

                running = false;
                menu.GetComponent<MenuData>().DownloadError();
            }

            if(!error)
            {
                doneDownload = true;
                offlineInstalled = true;
                PlayerPrefs.SetInt("Offline_Installed", 1);
                menu.GetComponent<MenuData>().DownloadDone();
            }
        }

        yield return null;
    }

    IEnumerator ShowProgress(UnityWebRequest www)
    {
        while((www.downloadedBytes < dlSize) && running)
        {
            float progress = (float)www.downloadedBytes / (float)dlSize;
            dl_progress = progress;
            yield return new WaitForSeconds(.1f);
        }
    }
}
