using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

public class OfflinePack : MonoBehaviour
{
    public static bool offlineInstalled = false;
    private bool doneDownload = false, running = false;
    private ulong dlSize = 77069801; //posts.zip größe in bytes
    public float dl_progress = 0f;
    public GameObject menu;

    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("Offline_Installed", 0) == 1)
        {
            offlineInstalled = true;
        }
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
