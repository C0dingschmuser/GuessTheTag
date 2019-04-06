using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public GameObject imageSetter, userHandler, networkHandler, userKnockoutObj;
    public bool gameActive = false, royaleShowTags = false, sortShowTags = false;
    public float timer = 0;
    private int originalTimer;
    private bool resetInvoked = false, backPressed = false;
    private int diff = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StartGame(int diff, bool begin = false)
    {
        this.diff = diff;


        userKnockoutObj.SetActive(false);

        int seconds = 5;
        timer = seconds;
        originalTimer = seconds;
        gameActive = true;

        if(begin)
        {
            backPressed = false;
        }
    }

    public void StandardReset()
    {
        userHandler.GetComponent<UserHandler>().FadeOutStats();

        bool bot = true;

        if(userHandler.GetComponent<UserHandler>().networkGameRunning)
        {
            bot = false;
            networkHandler.GetComponent<NetworkHandler>().SendMessage("SendTCPMessage", "6~");
        }

        bool full = false;

        if(MenuData.mode == (int)MenuData.Modes.battleRoyale)
        {
            full = true;
        }

        ResetGame(full, bot);
    }

    private void BotQuit()
    {
        gameActive = false;

        sortShowTags = false;
        royaleShowTags = false;

        int diff = userHandler.GetComponent<UserHandler>().diff;
        float benis = userHandler.GetComponent<UserHandler>().GetUserBenis();

        if (MenuData.mode == (int)MenuData.Modes.battleRoyale)
        { //calc benis
            List<int> uList = userHandler.GetComponent<UserHandler>().SortBattleRoyale();

            int userPos = uList.IndexOf(0);

            int mP = 25 - userPos;

            benis *= mP / 2;
            benis /= 5;

            if(diff == 0)
            {
                benis *= 0.5f;
            } else if(diff == 1)
            {
                benis *= 0.75f;
            } else if(diff == 2)
            {
                benis = benis * 1.1f;
            }

            if(userHandler.GetComponent<UserHandler>().users[0].GetComponent<UserData>().knockout ||
                userPos == 0)
            { //benis gibts nur wenn ausgeknocked oder wenn platz 1

#if UNITY_ANDROID
                Firebase.Analytics.FirebaseAnalytics.LogEvent("BotRoundOver", "WinBenis", (int)benis);
#endif

                UserHandler.SetGlobalUserBenis(UserHandler.GetPlayerUserBenis() + (ulong)benis);
            }
        } else if(MenuData.mode == (int)MenuData.Modes.sort)
        {
            benis = benis / 3f;

            switch (diff)
            { //diff bonus
                case 1: //Normal
                    benis *= 1.1f;
                    break;
                case 2: //Schwer
                    benis *= 1.25f;
                    break;
            }

            int pos = userHandler.GetComponent<UserHandler>().users[0].GetComponent<UserData>().endPos;

            switch(pos)
            { //end pos bonus
                case 0: //nr 1
                    benis *= 1.3f;
                    break;
                case 1:
                    benis *= 1.2f;
                    break;
                case 2:
                    benis *= 1.1f;
                    break;
            }

#if UNITY_ANDROID
            Firebase.Analytics.FirebaseAnalytics.LogEvent("BotRoundOver", "WinBenis", (int)benis);
#endif
            Debug.Log(benis);
            UserHandler.SetGlobalUserBenis(UserHandler.GetPlayerUserBenis() + (ulong)benis);
        }

        imageSetter.GetComponent<ImageSetter>().ResetGame(true);
        userHandler.GetComponent<UserHandler>().ResetGame(true, 0, true);

        Camera.main.transform.DOMove(new Vector3(-381, 642, -10), 0.5f);
        MenuData.state = 0;
    }

    public void ResetGame(bool full = false, bool bot = true, bool quit = false)
    {
        int diff = userHandler.GetComponent<UserHandler>().diff;

        //StartGame(diff);
        resetInvoked = false;

        if (full || !bot)
        {
            gameActive = false;
        }

        if(bot)
        {
            userHandler.GetComponent<UserHandler>().botGameCount++;

            if (userHandler.GetComponent<UserHandler>().botGameCount > 1 || quit || 
                MenuData.mode == (int)MenuData.Modes.battleRoyale ||
                MenuData.mode == (int)MenuData.Modes.sort)
            { //verlasse spiel
                BotQuit();

                return;
            }
        }

        if (bot && !full)
        {
            StartGame(diff);
            Invoke("FadeInAllUsers", .5f);
        }

        imageSetter.GetComponent<ImageSetter>().ResetGame(full);
        userHandler.GetComponent<UserHandler>().ResetGame(false, 0, bot);
    }

    private void FadeInAllUsers()
    {
        userHandler.GetComponent<UserHandler>().FadeInAllUsers();
    }

    private void ThinkTimeout()
    {

        imageSetter.GetComponent<ImageSetter>().thinkDone = true;
        if (MenuData.mode != (int)MenuData.Modes.sort)
        {
            imageSetter.GetComponent<ImageSetter>().inputObj.SetActive(true);
        }
        imageSetter.GetComponent<ImageSetter>().FadeInTags();

        userHandler.GetComponent<UserHandler>().thinkDone = true;

        SetTimer();
    }

    private void SetTimer()
    {
        int seconds = 90;

        if (MenuData.mode == (int)MenuData.Modes.versus)
        {
            if (diff == 1)
            {
                seconds = 60;
            }
            else if (diff == 2)
            {
                seconds = 30;
            }
        }
        else if(MenuData.mode == (int)MenuData.Modes.battleRoyale)
        { //sekunden bis player knockout
            seconds = 30; //leicht
            if (diff == 1)
            { //mittel
                seconds = 20;
            }
            else if (diff == 2)
            { //schwer
                seconds = 10; //10 original
            }
        } else if(MenuData.mode == (int)MenuData.Modes.sort)
        {
            seconds = 100;
        }
        timer = seconds;
        originalTimer = seconds;
    }

    public void SkipRound()
    {
        if (MenuData.mode == (int)MenuData.Modes.versus)
        {
            timer = 0;
            if (userHandler.GetComponent<UserHandler>().networkGameRunning)
            {
                networkHandler.GetComponent<NetworkHandler>().state =
                    (int)NetworkHandler.States.end;
            }
            StartReset();
        } else if(MenuData.mode == (int)MenuData.Modes.battleRoyale)
        {

            royaleShowTags = true;

            imageSetter.GetComponent<ImageSetter>().LoadRandomPost(-1, true);
        } else if(MenuData.mode == (int)MenuData.Modes.sort)
        {

            sortShowTags = true;
            imageSetter.GetComponent<ImageSetter>().LoadRandomPost(-1, true);
        }
    }

    public void StartReset(float forceTime = 5f)
    {
        if (resetInvoked)
        {
            return;
        }

        resetInvoked = true;

        float time = forceTime;

        if (MenuData.mode == (int)MenuData.Modes.versus)
        {

            bool n = imageSetter.GetComponent<ImageSetter>().ShowTags();

            time = 5f;
            if (n)
            {
                time += 5f;
            }
        }

        imageSetter.GetComponent<ImageSetter>().inputObj.SetActive(false);

        if(userHandler.GetComponent<UserHandler>().networkGameRunning)
        {
            time = 7.5f;
        }

        userHandler.GetComponent<UserHandler>().botGameRunning = false;

        userHandler.GetComponent<UserHandler>().RoundFadeOut();


        Invoke("StandardReset", time);
    }

    public void StartRoyaleReset()
    {
        UserHandler uHandler = userHandler.GetComponent<UserHandler>();
        int pos = uHandler.GetLastRoyaleUser();

        uHandler.users[pos].GetComponent<UserData>().DoKnockout();

        if(pos == 0)
        { //user timeout
            userKnockoutObj.SetActive(true);

            GameObject pr0Img = imageSetter.transform.GetChild(0).gameObject;
            pr0Img.GetComponent<Image>().material.SetFloat("_EffectAmount", 1f);

            imageSetter.GetComponent<ImageSetter>().ShowTags();
        }

        userHandler.GetComponent<UserHandler>().SortBattleRoyale();

        int remCount = userHandler.GetComponent<UserHandler>().GetRemainigRoyalePlayers();

        if(remCount == 1)
        {
            int lastPlayerPos = userHandler.GetComponent<UserHandler>().GetLastRoyaleUser();

            if(lastPlayerPos == 0)
            { //wenn user dann +benis

            }

            StartReset();
            return;
        }

        SetTimer();
    }

    public void StartSortReset()
    {
        StartReset();
    }

    private void ResetBackPressed()
    {
        backPressed = false;
    }

    // Update is called once per frame
    void Update()
    {
        NetworkHandler nHandler = networkHandler.GetComponent<NetworkHandler>();
        UserHandler uHandler = userHandler.GetComponent<UserHandler>();

        if(uHandler.networkGameRunning)
        {
            if(nHandler.state == (int)NetworkHandler.States.thinkRunning &&
                !gameActive)
            { //startet timer
                StartGame(2); //später multiplayer diff einbauen
            }
        }

        if(gameActive && 
            imageSetter.GetComponent<ImageSetter>().imageLoaded)
        {
            if(uHandler.botGameRunning)
            {
                if(Input.GetKeyDown(KeyCode.Escape) && 
                    !imageSetter.GetComponent<ImageSetter>().zoomEnabled)
                {
                    if (!backPressed)
                    {
                        backPressed = true;
                        Invoke("ResetBackPressed", 2f);
                    }
                    else
                    { //verlasse match da doppeltap
                        CancelInvoke("ResetBackPressed");
                        ResetGame(false, true, true);
                    }
                }
            }

            timer -= 1 * Time.deltaTime;

            if(timer > 0)
            {
                float p = timer / originalTimer;
                GetComponent<RectTransform>().sizeDelta = new Vector2(685.83f * p, 19.8f);
            } else
            { //timer abgelaufen -> runde vorbei

                if(!imageSetter.GetComponent<ImageSetter>().thinkDone)
                {
                    if (userHandler.GetComponent<UserHandler>().networkGameRunning)
                    {
                        if(nHandler.state == (int)NetworkHandler.States.inGame)
                        {
                            ThinkTimeout();
                        } else if(nHandler.state == (int)NetworkHandler.States.thinkRunning)
                        { //sendet msg an server dass bereit für game
                            nHandler.state = (int)NetworkHandler.States.inGameReady;
                            nHandler.SendMessage("SendTCPMessage", "3~");
                        }
                    }
                    else
                    {
                        ThinkTimeout();
                    }
                    return;
                }

                if (userHandler.GetComponent<UserHandler>().networkGameRunning)
                {
                    if(nHandler.state == (int)NetworkHandler.States.inGame)
                    { //sendet msg an server dass bereit für leaderboard
                        nHandler.state = (int)NetworkHandler.States.endReady;
                        nHandler.SendMessage("SendTCPMessage", "5~");
                    } else if(nHandler.state == (int)NetworkHandler.States.end)
                    {
                        StartReset();
                    }
                }
                else
                {
                    if (MenuData.mode == (int)MenuData.Modes.versus)
                    {
                        StartReset();
                    } else if(MenuData.mode == (int)MenuData.Modes.battleRoyale)
                    {
                        StartRoyaleReset();
                    } else if(MenuData.mode == (int)MenuData.Modes.sort)
                    {
                        StartSortReset();
                    }
                }
            }
        }
    }
}
