using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class MenuData : MonoBehaviour
{
    public GameObject imgSetter, userHandler, progressBar, status, downloadbtn, modusText;
    public GameObject inputName, singleplayerParent, multiplayerParent, colorParent, benitrat0r,
        benitrat0rDigits, options, resButton, blusObj, notifications;
    public bool locked = false;
    private bool downloading = false;
    public AnimationCurve curve;
    public static int mode = 0;
    public static int state = 0;
    private int resolutionMode = 0, menuBenisCount = 0; //Normal

    public enum Modes
    {
        versus = 0,
        battleRoyale = 1,
        sort = 2,
    }

    private void Start()
    {
        inputName.GetComponent<TMP_InputField>().text = PlayerPrefs.GetString("Player_Username");

        int diff = PlayerPrefs.GetInt("Player_SP_Diff", 1);
        int mpDiff = PlayerPrefs.GetInt("Player_MP_Diff", 1);

        resolutionMode = PlayerPrefs.GetInt("Player_Resolution", 0);

        int first = PlayerPrefs.GetInt("FirstStartup", 1);
        PlayerPrefs.SetInt("FirstStartup", 0);

#if UNITY_ANDROID

        if(first == 1)
        {
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("WantsMessages", "1");
        }

        int wants = PlayerPrefs.GetInt("WantsMessages", 1);

        string message = "";

        if (wants == 1)
        {
            message = "Ja";
        } else
        {
            message = "Neim";
        }
        notifications.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = message;

        if (resolutionMode == 0)
        {
            Screen.SetResolution(720, 1280, true);
            resolutionMode = 1;
            resButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "720p";
        }
        else
        {
            Screen.SetResolution(1080, 1920, true);
            resolutionMode = 0;
            resButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Normal";
        }
#endif

        DiffClicked(diff, false);
        DiffClicked(mpDiff, true);
        ColorClicked(true);

        if (PlayerPrefs.GetInt("Offline_Installed", 0) == 1)
        {
            downloadbtn.GetComponent<Button>().interactable = false;
            status.GetComponent<TextMeshProUGUI>().text = "Status: Installiert";
        }
        SetMenuBenisString();
        SpawnMenuBlus();
    }

    public void PushNotificationsClicked()
    {
#if UNITY_ANDROID
        int wants = PlayerPrefs.GetInt("WantsMessages", 1);

        string message = "";

        if(wants == 1)
        {
            wants = 0;
            message = "Neim";
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("WantsMessages", "0");
        } else
        {
            wants = 1;
            message = "Ja";
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("WantsMessages", "1");
        }

        PlayerPrefs.SetInt("WantsMessages", wants);
        notifications.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
#endif
    }

    public void SpawnMenuBlus(int click = 0)
    {
        float newX = Random.Range(-702f, -59f);
        float newY = Random.Range(712.103f, 1103.103f);

        if(click > 0)
        {
            ulong benis = UserHandler.GetPlayerUserBenis();
            UserHandler.SetGlobalUserBenis(benis + 1);
            menuBenisCount++;
        }

        Vector3 newPos = new Vector3(newX, newY);
        blusObj.SetActive(false);
        blusObj.transform.position = newPos;
        Invoke("ShowMenuBlus", 0.25f);
    }

    private void ShowMenuBlus()
    {
        blusObj.SetActive(true);
        Color c = blusObj.transform.GetChild(0).GetComponent<Image>().color;
        c.a = 0;
        blusObj.transform.GetChild(0).GetComponent<Image>().color = c;

        blusObj.transform.GetChild(0).GetComponent<Image>().DOFade(1, 0.25f);
    }

    private void OnApplicationQuit()
    {
#if UNITY_ANDROID
        Firebase.Analytics.FirebaseAnalytics.LogEvent("MenuBenis", "Benis", menuBenisCount);
#endif
    }

    public void SetMenuBenisString()
    {
        benitrat0rDigits.GetComponent<TextMeshProUGUI>().text =
            "B " + UserHandler.GetPlayerUserBenis().ToString("000000000");
    }

    public void Benitrat0rClicked()
    {
        benitrat0r.SetActive(true);
        Camera.main.transform.DOMove(new Vector3(-381, 1943, -10), 0.75f);

        state = 4;

        Invoke("CheckBenitrat0rMusic", 0.25f);
    }

    private void CheckBenitrat0rMusic()
    {

        benitrat0r.GetComponent<Benitrat0r>().sound.GetComponent<AudioScript>().BeginPlayAmbient();
        benitrat0r.GetComponent<Benitrat0r>().SetBenis(UserHandler.GetPlayerUserBenis());
    }

    public void SinglePlayerClicked()
    {
        if (PlayerPrefs.GetString("Player_Username").Equals(""))
        {
            SpawnError(new Vector3(-500, 56, 0), "Bitte wähle zuerst einen Username (Optionen)");
            return;
        }

        bool offline = true;

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            imgSetter.GetComponent<ImageSetter>().offlineMode = true;
        }
        else
        {
            imgSetter.GetComponent<ImageSetter>().offlineMode = false;
            offline = false;
        }

        if(offline && !OfflinePack.offlineInstalled)
        {
            SpawnError(new Vector3(-540, 56, 0), "Kein Offlinepack installiert! (Optionen)");
            return;
        }

        state = 2;

        switch(mode)
        {
            case (int)Modes.versus:
                userHandler.GetComponent<UserHandler>().CreateNormalUsers();
                break;
            case (int)Modes.battleRoyale:
                userHandler.GetComponent<UserHandler>().CreateRoyaleUsers();
                break;
            case (int)Modes.sort:
                userHandler.GetComponent<UserHandler>().CreateSortUsers();
                break;
        }

        GameObject pr0Img = imgSetter.transform.GetChild(0).gameObject;
        pr0Img.GetComponent<Image>().material.SetFloat("_EffectAmount", 0f);

        userHandler.GetComponent<UserHandler>().StartBotGame();
        Camera.main.transform.DOMove(new Vector3(361, 642, -10), 1f);
    }

    public void MultiPlayerClicked()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            SpawnError(new Vector3(-650, 56, 0), "Kein Internet!");
            return;
        }

        if(PlayerPrefs.GetString("Player_Username").Equals(""))
        {
            SpawnError(new Vector3(-500, 56, 0), "Bitte wähle zuerst einen Username (Optionen)");
            return;
        }

        if (mode == (int)Modes.battleRoyale)
        {
            return;
        }

        state = 3;
        userHandler.GetComponent<UserHandler>().StartNetworkGame();
        Camera.main.transform.DOMove(new Vector3(361, 642, -10), 1f);
    }

    public void SpawnError(Vector3 pos, string text)
    {
        imgSetter.GetComponent<ImageSetter>().
            SpawnPointText(text, 10f, Color.red, pos, 200, true);
    }

    public void OptionsClicked()
    {
        options.SetActive(true);
        state = 1;
        Camera.main.transform.DOMove(new Vector3(-381, -665, -10), 0.5f);
    }

    public void OptionsBackClicked()
    {
        state = 0;
        Camera.main.transform.DOMove(new Vector3(-381, 642, -10), 0.5f);
        Invoke("DisableOptions", 0.5f);
    }

    private void DisableOptions()
    {
        options.SetActive(false);
    }

    public void Benitrat0rBack()
    {
        state = 0;
        Camera.main.transform.DOMove(new Vector3(-381, 642, -10), 0.5f);
        SetMenuBenisString();
        Invoke("DisableBenitrat0r", 0.5f);
    }

    private void DisableBenitrat0r()
    {
        benitrat0r.SetActive(false);
    }

    public void ChangeUsername()
    {
        string newUsername = inputName.GetComponent<TMP_InputField>().text;

        bool ok = true;
        foreach(char c in newUsername)
        {
            bool internOk = false;
            if((c >= 'a' && c <= 'z') || 
               (c >= 'A' && c <= 'Z') ||
               (c >= '0' && c <= '9') ||
                c == '_' ||
                c == '.' ||
                c == '*')
            {
                internOk = true;
            }

            if(!internOk)
            {
                ok = false;
            }
        }

        if(newUsername.Length < 3 || newUsername.Length > 18)
        {
            ok = false;
        }

        if(!ok)
        {
            inputName.GetComponent<Image>().DOBlendableColor(Color.red, 0.5f).SetEase(curve);
            inputName.GetComponent<TMP_InputField>().text = "";
        } else
        { //name ok
            inputName.GetComponent<Image>().DOBlendableColor(Color.green, 0.5f).SetEase(curve);
            userHandler.GetComponent<UserHandler>().username = newUsername;
            PlayerPrefs.SetString("Player_Username", newUsername);
        }
    }

    public void SP_Diff(int diff)
    {
        DiffClicked(diff, false);
    }

    public void MP_Diff(int diff)
    {
        DiffClicked(diff, true);
    }

    public void DiffClicked(int diff, bool mp)
    {
        Transform parent = singleplayerParent.transform;

        if(mp)
        {
            parent = multiplayerParent.transform;
        }

        for(int i = 0; i < parent.childCount; i++)
        {
            Color c = parent.GetChild(i).GetComponent<Image>().color;

            if (i != diff)
            {
                c.a = 0.5f;
            } else
            {
                c.a = 1f;
            }

            parent.GetChild(i).GetComponent<Image>().color = c;
        }

        if (!mp)
        {
            userHandler.GetComponent<UserHandler>().diff = diff;
            PlayerPrefs.SetInt("Player_SP_Diff", diff);
        } else
        {
            userHandler.GetComponent<UserHandler>().mpDiff = diff;
            PlayerPrefs.SetInt("Player_MP_Diff", diff);
        }
    }

    public void ColorClicked(bool load = false)
    {
        GameObject btn = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if(load)
        { //lädt farbe
            int loadId = PlayerPrefs.GetInt("Player_Color", 0);

            btn = colorParent.transform.GetChild(loadId).gameObject;
        }

        Color c = btn.GetComponent<Image>().color;

        int id = 0;
        foreach(Transform child in colorParent.transform)
        {
            if(child.gameObject != btn)
            {
                Color tmp = child.GetComponent<Image>().color;
                tmp.a = 0.5f;
                child.GetComponent<Image>().color = tmp;
            } else
            {
                c = child.GetComponent<Image>().color;
                c.a = 1f;
                child.GetComponent<Image>().color = c;

                id = child.GetSiblingIndex();
            }
        }

        if(!load)
        { //speichern
            PlayerPrefs.SetInt("Player_Color", id);
        }

        userHandler.GetComponent<UserHandler>().SetColor(c);
    }

    public void DownloadPack()
    {
        if(OfflinePack.offlineInstalled)
        {
            Debug.Log("Bereits installiert.");
            return;
        }

        bool offline = true;

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            imgSetter.GetComponent<ImageSetter>().offlineMode = true;
        }
        else
        {
            imgSetter.GetComponent<ImageSetter>().offlineMode = false;
            offline = false;
        }

        if(offline)
        {
            OptionsBackClicked();
            SpawnError(new Vector3(-650, 56, 0), "Kein Internet!");
            return;
        }

        downloadbtn.GetComponent<Button>().interactable = false;
        downloading = true;
        imgSetter.GetComponent<OfflinePack>().DownloadPack();
    }

    public void DownloadError()
    {
        downloading = false;
        status.GetComponent<TextMeshProUGUI>().text = "Status: Fehler";
        downloadbtn.GetComponent<Button>().interactable = true;
    }

    public void DownloadDone()
    {
        downloading = false;
        status.GetComponent<TextMeshProUGUI>().text = "Status: Installiert";
    }

    public void ChangeMode()
    {
        string modetext = "Versus    2-4 Spieler";
        if (mode == (int)Modes.versus)
        {
            modetext = "BRoyale  20+ Spieler";
            mode = (int)Modes.battleRoyale;
        } else if(mode == (int)Modes.battleRoyale)
        {
            mode = (int)Modes.sort;
            modetext = "Tagreihenfolge";
        } else if(mode == (int)Modes.sort)
        {
            mode = (int)Modes.versus;
        }
        modusText.GetComponent<TextMeshProUGUI>().text = modetext;
    }

    public void ChangeRes()
    {
#if UNITY_ANDROID
        if(resolutionMode == 0)
        {
            Screen.SetResolution(720, 1280, true);
            resolutionMode = 1;
            resButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "720p";
        } else
        {
            Screen.SetResolution(1080, 1920, true);
            resolutionMode = 0;
            resButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Normal";
        }

        PlayerPrefs.SetInt("Player_Resolution", resolutionMode);
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state == 0)
            { //verlasse game
                Application.Quit();
            }
            else if (state == 1)
            { //zurück zum hm
                OptionsBackClicked();
            }
        }

        if(downloading)
        {
            float tmp = imgSetter.GetComponent<OfflinePack>().dl_progress * 100;
            int progress = (int)tmp;
            status.GetComponent<TextMeshProUGUI>().text = "Status: " + progress.ToString() + "%";
        }
    }

    //private void Res
}
