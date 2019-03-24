using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ProgressBar : MonoBehaviour
{
    public GameObject imageSetter, userHandler, networkHandler;
    public bool gameActive = false;
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

        int seconds = 10;
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

        ResetGame(false, bot);
    }

    private void BotQuit()
    {
        gameActive = false;

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

            if (userHandler.GetComponent<UserHandler>().botGameCount > 1 || quit)
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
        imageSetter.GetComponent<ImageSetter>().inputObj.SetActive(true);
        imageSetter.GetComponent<ImageSetter>().FadeInTags();

        userHandler.GetComponent<UserHandler>().thinkDone = true;

        int seconds = 120;
        if (diff == 1)
        {
            seconds = 75;
        }
        else if (diff == 2)
        {
            seconds = 45;
        }
        timer = seconds;
        originalTimer = seconds;
    }

    public void StartReset()
    {
        if (resetInvoked)
        {
            return;
        }

        resetInvoked = true;
        bool n = imageSetter.GetComponent<ImageSetter>().ShowTags();

        float time = 5f;
        if (n)
        {
            time += 10f;
        }

        imageSetter.GetComponent<ImageSetter>().inputObj.SetActive(false);

        if(userHandler.GetComponent<UserHandler>().networkGameRunning)
        {
            time = 10f;
        }

        userHandler.GetComponent<UserHandler>().botGameRunning = false;

        userHandler.GetComponent<UserHandler>().RoundFadeOut();


        Invoke("StandardReset", time);
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
                    StartReset();
                }
            }
        }
    }
}
