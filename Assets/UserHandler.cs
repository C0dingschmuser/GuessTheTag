using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UserHandler : MonoBehaviour
{
    public GameObject[] users = new GameObject[4];
    public int ownIndex = 0;
    public GameObject userPrefab, imageSetter, progressBar, topBar;
    public Vector3[] userPositions, statPositions;
    public Color[] userColors;
    public bool botGameRunning = false, thinkDone = false, networkGameRunning = false;
    public AnimationCurve curve;
    public string username;
    public Color userColor = new Color32(238, 77, 46, 255);
    private NetworkHandler networkHandler;
    public int diff = 1, mpDiff = 1, botGameCount = 0;
    private int fadeCount = 0, fademax = 3;

    // Start is called before the first frame update
    void Start()
    {
        networkHandler = GetComponent<NetworkHandler>();

        for(int i = 0; i < 4; i++)
        {
            GameObject newUser = Instantiate(userPrefab);
            newUser.transform.SetParent(this.transform);
            newUser.transform.position = new Vector3(196, -105.55f, 0);
            users[i] = newUser;
        }

        username = PlayerPrefs.GetString("Player_Username", "");

        //Invoke("StartBotGame", 2f);
        //Invoke("StartNetworkGame", 2f);
    }

    public void SetUser(int pos, string name, int reused = 0)
    {
        UserData uData = users[pos].GetComponent<UserData>();
        uData.timer = 0;
        uData.SetName(name);
        uData.guessedAll = false;

        users[pos].transform.position = userPositions[pos];
        users[pos].GetComponent<UserData>().RoundFadeIn();

        users[pos].SetActive(true);

        if (reused == 1)
        {
            return;
        }

        uData.SetBenis(0, true);

        Color newColor = Color.black;

        if(pos == 0)
        {
            newColor = userColor;
        } else
        {
            bool notFound = true;
            while (notFound)
            {
                notFound = false;
                newColor = userColors[Random.Range(0, userColors.Length)];

                for (int i = 0; i < 4; i++)
                {
                    if (i != pos)
                    {
                        if (users[i].GetComponent<Image>().color == newColor)
                        {
                            notFound = true;
                            break;
                        }
                    }
                }
            }
        }

        users[pos].GetComponent<Image>().color = newColor;
    }

    public Color GetUserColor(int pos = 0)
    {
        return users[pos].GetComponent<Image>().color;
    }

    public void ResetUser(int pos)
    {
        UserData uData = users[pos].GetComponent<UserData>();
        uData.timer = 0;
        uData.SetBenis(0, true);
        uData.SetName("");
        uData.guessedAll = false;

        users[pos].transform.position = new Vector3(196, -105.55f, 0);
    }

    public void StartBotGame()
    {
        SetUser(0, username);
        for(int i = 1; i < 4; i++)
        {
            SetUser(i, "Bot " + i.ToString());
            users[i].GetComponent<UserData>().SetTimer(GenerateTime(diff,
                users[i].GetComponent<UserData>().internalDiff));
        }

        progressBar.SetActive(true);

        botGameRunning = true;
        botGameCount = 0;
        GetComponent<NetworkHandler>().statusText.SetActive(false);

        ShowUsers();

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            imageSetter.GetComponent<ImageSetter>().offlineMode = true;
        } else
        {
            imageSetter.GetComponent<ImageSetter>().offlineMode = false;
        }

        foreach (Transform child in transform.GetChild(0))
        {
            child.gameObject.SetActive(false);
        }

        imageSetter.GetComponent<ImageSetter>().inputObj.SetActive(false);
        imageSetter.GetComponent<ImageSetter>().SetImage(null, true);
        imageSetter.GetComponent<ImageSetter>().LoadRandomPost();
        progressBar.GetComponent<ProgressBar>().StartGame(diff, true);
    }

    public void ShowUsers()
    {
        foreach(GameObject user in users)
        {
            if(!user.GetComponent<UserData>().uName.Equals(""))
            {
                user.SetActive(true);
            }
        }
    }

    public void StartNetworkGame()
    {
        imageSetter.GetComponent<ImageSetter>().inputObj.SetActive(false);
        imageSetter.GetComponent<ImageSetter>().SetImage(null, true);
        progressBar.SetActive(false);

        foreach (GameObject user in users)
        {
            user.SetActive(false);
        }

        foreach (Transform child in transform.GetChild(0))
        {
            child.gameObject.SetActive(false);
        }

        networkGameRunning = true;
        networkHandler.ConnectToServer(username);
    }

    public void ResetGame(bool full = false, int newDiff = 0, bool bot = true)
    {
        diff = newDiff;
        ReGenerateInternalDiff();

        users[0].GetComponent<UserData>().SetTimer(0);

        for(int i = 1; i < 4; i++)
        { //generiert neue timerzeit
            users[i].GetComponent<UserData>().SetTimer(GenerateTime(diff,
                users[i].GetComponent<UserData>().internalDiff));
        }

        if (bot)
        {
            botGameRunning = true;
        }
        thinkDone = false;
    }

    public void SetColor(Color c)
    {
        userColor = c;

        topBar.GetComponent<Image>().color = c;
        progressBar.GetComponent<Image>().color = c;
    }

    public void RoundFadeOut()
    {
        foreach(GameObject user in users)
        {
            user.GetComponent<UserData>().RoundFadeOut();
        }

        int userCount = 0;
        foreach(GameObject user in users)
        {
            if(user.GetComponent<UserData>().uName != "")
            {
                userCount++;
            }
        }

        List<GameObject> sList = new List<GameObject>();
        bool done = false;
        while(!done)
        { //sortiert nach benis
            int max = 0;
            int maxid = -1;
            int id = 0;

            foreach(GameObject user in users)
            {

                if (user.GetComponent<UserData>().uName != "")
                {
                    if (user.GetComponent<UserData>().benis >= max &&
                        !sList.Contains(user))
                    {
                        max = user.GetComponent<UserData>().benis;
                        maxid = id;
                    }
                }

                id++;
            }
            if (maxid > -1)
            {
                sList.Add(users[maxid]);
            }
            if(sList.Count == userCount)
            {
                done = true;
            }
        }

        bool won = false;

        if(sList[0].GetComponent<UserData>().uName == username)
        {
            if(sList[0].GetComponent<UserData>().benis > 0)
            {
                won = true;
            }
        }

        if(won)
        {
            if(networkGameRunning)
            {
                GetComponent<Stats>().mpRoundsWon++;
            } else
            {
                GetComponent<Stats>().roundsWon++;
            }
        } else
        {
            if(networkGameRunning)
            {
                GetComponent<Stats>().mpRoundsPlayed++;
            } else
            {
                GetComponent<Stats>().roundsPlayed++;
            }
        }

        for(int i = 0; i < sList.Count; i++)
        {
            transform.GetChild(0).GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                sList[i].GetComponent<UserData>().uName;
            transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                sList[i].GetComponent<UserData>().benis.ToString();
        }

        fadeCount = 0;
        fademax = sList.Count;

        InvokeRepeating("FadeInStats", .75f, .5f);
    }

    public void FadeOutStats()
    {
        for(int i = 0; i < fademax; i++)
        {
            transform.GetChild(0).GetChild(i).GetComponent<TextMeshProUGUI>().DOFade(0, .5f);
            transform.GetChild(0).GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(0, .5f);
            transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(0, .5f);
        }
    }

    public void FadeInAllUsers()
    {
        foreach(GameObject user in users)
        {
            if(user.GetComponent<UserData>().uName != "")
            {
                user.GetComponent<UserData>().RoundFadeIn();
            }
        }
    }

    private void FadeInStats()
    {
        if(fadeCount == fademax)
        {
            CancelInvoke("FadeInStats");
            return;
        }

        transform.GetChild(0).GetChild(fadeCount).gameObject.SetActive(true);
        transform.GetChild(0).GetChild(fadeCount).position = 
            new Vector3(statPositions[0].x, statPositions[3].y - 200f, statPositions[0].z);

        Color a = transform.GetChild(0).GetChild(fadeCount).GetComponent<TextMeshProUGUI>().color;
        a.a = 0f;
        transform.GetChild(0).GetChild(fadeCount).GetComponent<TextMeshProUGUI>().color = a;
        transform.GetChild(0).GetChild(fadeCount).GetChild(0).GetComponent<TextMeshProUGUI>().color = a;
        transform.GetChild(0).GetChild(fadeCount).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().color = a;

        transform.GetChild(0).GetChild(fadeCount).GetComponent<TextMeshProUGUI>().DOFade(1f, .75f);
        transform.GetChild(0).GetChild(fadeCount).GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(1f, .75f);
        transform.GetChild(0).GetChild(fadeCount).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(1f, .75f);
        transform.GetChild(0).GetChild(fadeCount).DOMove(statPositions[fadeCount], .5f);
        fadeCount++;
    }

    private void ReGenerateInternalDiff()
    {
        for(int i = 1; i < 4; i++)
        {
            users[i].GetComponent<UserData>().internalDiff =
                Random.Range(0, 3);
        }
    }

    public float GenerateTime(int diff, int internalDiff)
    {
        float time = Random.Range(15, 20);

        if(diff == 1)
        { //mittel
            time = Random.Range(10, 15);
        } else if(diff == 2)
        {
            time = Random.Range(5, 10);
        }

        if (internalDiff == 0)
        {
            time += Random.Range(1f, 3f);
        }
        else if (internalDiff == 1)
        {
            int rand = Random.Range(0, 2);
            if (rand == 0)
            { //kürzer
                time -= Random.Range(0, 2f);
            }
            else
            { //länger
                time += Random.Range(0, 2f);
            }
        }
        else if (internalDiff == 2)
        {
            time -= Random.Range(1f, 3f);
        }

        return time;
    }

    public int CalcBenis(int userID, int tpos, int diff)
    {
        int benis = (6 - tpos) * 10;
        int diffMax = 20;

        if (diff == 1)
        {
            diffMax = 15;
        } else if(diff == 2)
        {
            diffMax = 10;
        }

        float dR = ((float)diffMax /
            (float)users[userID].GetComponent<UserData>().originalTimer);

        if(userID == 0)
        { //player
            dR = ((float)diffMax /
                (float)users[userID].GetComponent<UserData>().timer);
        }

        float benisF = (float)benis * dR;
        //Debug.Log(dR + " " + benisF);

        benis = (int)benisF;

        return benis;
    }

    public int GetUserBenis()
    {
        return users[0].GetComponent<UserData>().benis;
    }

    public int SetUserBenis(int tpos)
    {
        int benis = CalcBenis(0, tpos, diff);
        users[0].GetComponent<UserData>().SetBenis(benis);
        users[0].GetComponent<UserData>().timer = 0;

        return benis;
    }

    public void FlashTag(int tagPos, int userID, int benis, bool force = false)
    {
        TagData tag = imageSetter.GetComponent<ImageSetter>().
            tagObjs[tagPos].GetComponent<TagData>();

        tag.isGuessed[userID] = true;

        Color userColor = users[userID].GetComponent<Image>().color;

        tag.colorBuffer.Add(userColor);

        int tmpTagPos = tagPos + 1;

        imageSetter.GetComponent<ImageSetter>().
            SpawnPointText(users[userID].GetComponent<UserData>().uName + " hat Tag #" + tmpTagPos.ToString() + " erraten!",
            3f, userColor, new Vector3(360, 1043));

        users[userID].GetComponent<UserData>().SetBenis(benis, force);
    }

    // Update is called once per frame
    void Update()
    {
        //return;

        if(networkGameRunning)
        {
            if(networkHandler.state == (int)NetworkHandler.States.thinkLoading)
            {
                if(imageSetter.GetComponent<ImageSetter>().imageLoaded)
                {
                    networkHandler.SendMessage("SendTCPMessage", "2~");
                    networkHandler.state = (int)NetworkHandler.States.thinkReady;
                }
            } else if(networkHandler.state == (int)NetworkHandler.States.inGame)
            {
                users[0].GetComponent<UserData>().timer += Time.deltaTime;
            }
        } else if(botGameRunning)
        {
            ImageSetter imgSetter = imageSetter.GetComponent<ImageSetter>();

            if(!imgSetter.imageLoaded ||
                !thinkDone)
            {
                return;
            }

            users[0].GetComponent<UserData>().timer += Time.deltaTime;

            bool allTagsGuessed = false;

            for(int i = 0; i < 4; i++)
            { //prüft ob alle tags erraten
                for (int a = 0; a < imgSetter.tagObjs.Count; a++)
                {
                    if (!imgSetter.tagObjs[a].GetComponent<TagData>().isGuessed[i])
                    {
                        allTagsGuessed = false;
                    }
                }
            }

            if(allTagsGuessed)
            {
                return;
            }

            for (int i = 1; i < 4; i++)
            { //looped durch user
                //Debug.Log(i.ToString() + " Timer: " +
                //    users[i].GetComponent<UserData>().timer);

                bool allGuessed = true;
                for(int a = 0; a < imgSetter.tagObjs.Count; a++)
                {
                    if(!imgSetter.tagObjs[a].GetComponent<TagData>().isGuessed[i])
                    {
                        allGuessed = false;
                    }
                }

                users[i].GetComponent<UserData>().guessedAll = allGuessed;

                if(users[i].GetComponent<UserData>().timer <= 0 &&
                    !users[i].GetComponent<UserData>().guessedAll)
                { //guess tag algo -> aufgerufen wenn timer abgelaufen
                    users[i].GetComponent<UserData>().SetTimer(GenerateTime(diff,
                        users[i].GetComponent<UserData>().internalDiff));

                    int tagCount = imgSetter.tagObjs.Count;

                    bool notFound = true;
                    while(notFound)
                    {
                        notFound = false;
                        int trange = Random.Range(0, 100);
                        int tpos = 0;
                        if(trange > 10 && trange < 30)
                        { //2. top tag
                            tpos = 1;
                        } else if(trange >= 30 && trange < 60)
                        {
                            tpos = 2;
                        } else if(trange >= 60)
                        {
                            tpos = 3;
                            if(tagCount == 5)
                            {
                                tpos = Random.Range(3, 5); //3 oder 4
                            } else if(tagCount == 6)
                            {
                                tpos = Random.Range(3, 6); //3, 4 oder 5
                            }
                        }

                        if(imgSetter.tagObjs[tpos].GetComponent<TagData>().isGuessed[i] == true)
                        { //wenn tag bereits erraten dann weitersuchen
                            notFound = true;
                        }

                        if(!notFound)
                        { //tag noch nicht erraten -> setzt auf erraten (i ist nutzerID)
                            FlashTag(tpos, i, CalcBenis(i, tpos, diff));

                            //Debug.Log(tpos + " " + i);
                            //Debug.Break();
                        }
                    }

                } else
                {
                    users[i].GetComponent<UserData>().timer -= 1 * Time.deltaTime;
                }
            }
        }
    }
}
