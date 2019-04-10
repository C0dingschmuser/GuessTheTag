using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UserHandler : MonoBehaviour
{
    public List<GameObject> users = new List<GameObject>();
    public int ownIndex = 0;
    public GameObject userPrefab, royaleUserPrefab, imageSetter, progressBar, topBar, royaleNumbers, benitrat0r,
        normalMenu, sortNumbers;
    public static GameObject menu;
    public Vector3[] userPositions, statPositions, royaleUserPositions;
    public Color[] userColors;
    public bool botGameRunning = false, thinkDone = false, networkGameRunning = false;
    public AnimationCurve curve;
    public string username;
    public List<string> botNames = new List<string>();
    public Color userColor = new Color32(238, 77, 46, 255);
    private NetworkHandler networkHandler;
    private static ulong userBenis = 0, userBenisBackup = 5;
    public int diff = 1, mpDiff = 1, botGameCount = 0;
    private int fadeCount = 0, fademax = 3, royaleLast = -1;

    // Start is called before the first frame update
    void Start()
    {
        networkHandler = GetComponent<NetworkHandler>();

        /*for(int i = 0; i < 30; i++)
        {
            GameObject newUser = Instantiate(userPrefab);
            newUser.transform.SetParent(this.transform);
            newUser.transform.position = new Vector3(196, -105.55f, 0);
            users[i] = newUser;
        }*/

        menu = normalMenu;

        username = PlayerPrefs.GetString("Player_Username", "");

        SetGlobalUserBenis(ulong.Parse(PlayerPrefs.GetString("UserBenis", "1024")));
        benitrat0r.GetComponent<Benitrat0r>().SetBenis(userBenis);

        //Invoke("StartBotGame", 2f);
        //Invoke("StartNetworkGame", 2f);
    }

    public static ulong GetPlayerUserBenis()
    {
        return userBenis;
    }

    public static void SetGlobalUserBenis(ulong benis)
    {
        userBenis = benis;
        userBenisBackup = benis + 5;
        PlayerPrefs.SetString("UserBenis", userBenis.ToString());

        menu.GetComponent<MenuData>().SetMenuBenisString();
    }

    private void DeleteUsers()
    {
        foreach (GameObject oldUser in users)
        {
            Destroy(oldUser);
        }
        users.Clear();
    }

    public void CreateNormalUsers()
    {
        DeleteUsers();

        for (int i = 0; i < 4; i++)
        {
            GameObject newUser = Instantiate(userPrefab);
            newUser.transform.SetParent(this.transform);
            newUser.transform.position = new Vector3(196, -105.55f, 0);

            if(i == 0)
            {
                newUser.GetComponent<UserData>().user = true;
            }

            users.Add(newUser);
        }
    }

    public void CreateRoyaleUsers()
    {
        DeleteUsers();

        for (int i = 0; i < 25; i++)
        {
            GameObject newUser = Instantiate(royaleUserPrefab);
            newUser.transform.SetParent(this.transform);
            newUser.transform.position = new Vector3(196, -105.55f, 0);

            if (i == 0)
            {
                newUser.GetComponent<UserData>().user = true;
            }

            users.Add(newUser);
        }
    }

    public void CreateSortUsers()
    {
        DeleteUsers();

        for (int i = 0; i < 4; i++)
        {
            GameObject newUser = Instantiate(royaleUserPrefab);
            newUser.transform.SetParent(this.transform);
            newUser.transform.position = new Vector3(196, -105.55f, 0);
            newUser.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            if (i == 0)
            {
                newUser.GetComponent<UserData>().user = true;
            }

            users.Add(newUser);
        }
    }

    public void SetUser(int pos, string name, int reused = 0)
    {
        UserData uData = users[pos].GetComponent<UserData>();
        uData.timer = 0;
        uData.SetName(name);
        uData.guessedAll = false;

        if (MenuData.mode == (int)MenuData.Modes.versus)
        {
            users[pos].transform.position = userPositions[pos];
        } else if(MenuData.mode == (int)MenuData.Modes.battleRoyale)
        {
            users[pos].transform.position = new Vector3(196, -105.55f, 0);
        } else if(MenuData.mode == (int)MenuData.Modes.sort)
        {
            users[pos].transform.position = new Vector3(621, 330.45f - (pos * 85), 0);
        }

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

                if (MenuData.mode == (int)MenuData.Modes.versus)
                {
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
        this.diff = PlayerPrefs.GetInt("Player_SP_Diff", 1);

        SetUser(0, username);

        int uCount = 4;
        if(MenuData.mode == (int)MenuData.Modes.battleRoyale)
        {
            uCount = 25;
            royaleNumbers.SetActive(true);
        } else
        {
            royaleNumbers.SetActive(false);
        }

        if(MenuData.mode == (int)MenuData.Modes.sort)
        {
            sortNumbers.SetActive(true);
        } else
        {
            sortNumbers.SetActive(false);
        }

#if UNITY_ANDROID
        Firebase.Analytics.FirebaseAnalytics.LogEvent("PlayBotRound", "Mode", MenuData.mode);
#endif

        for (int i = 1; i < uCount; i++)
        {
            string name = "";

            bool notFound = true;
            while(notFound)
            {
                notFound = false;
                name = botNames[Random.Range(0, botNames.Count)];

                foreach (GameObject user in users)
                {
                    if(user.GetComponent<UserData>().uName.Equals(name))
                    {
                        notFound = true;
                    }
                }
            }

            SetUser(i, name);
            users[i].GetComponent<UserData>().SetTimer(GenerateTime(diff,
                users[i].GetComponent<UserData>().internalDiff));
        }

        progressBar.SetActive(true);

        botGameRunning = true;
        botGameCount = 0;
        GetComponent<NetworkHandler>().statusText.SetActive(false);

        ShowUsers();

        if (MenuData.mode == (int)MenuData.Modes.battleRoyale)
        {
            SortBattleRoyale();
            foreach(GameObject user in users)
            {
                user.GetComponent<UserData>().UpdatePostCount();
            }
        } else if(MenuData.mode == (int)MenuData.Modes.sort)
        {
            foreach (GameObject user in users)
            {
                user.GetComponent<UserData>().UpdatePostCount();
            }
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
        progressBar.GetComponent<ProgressBar>().userKnockoutObj.SetActive(false);
        progressBar.SetActive(false);

        royaleNumbers.SetActive(false);
        

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

        int uCount = 4;

        if(MenuData.mode == (int)MenuData.Modes.battleRoyale)
        {
            uCount = 25;
        }

        for(int i = 1; i < uCount; i++)
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
        if(MenuData.mode == (int)MenuData.Modes.battleRoyale)
        {
            return;
        }

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

        users[0].GetComponent<UserData>().endPos = sList.IndexOf(users[0]);

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
            if(botGameCount > 0)
            {
                if(sList[i].GetComponent<UserData>().uName == username)
                {
                    float benis = sList[i].GetComponent<UserData>().benis;

                    switch(i)
                    {
                        case 0:
                            benis *= 2;
                            break;
                        case 1:
                            benis *= 1.5f;
                            break;
                        case 2:
                            benis *= 1.25f;
                            break;
                        case 3:
                            benis *= 1.1f;
                            break;
                    }

                    switch(diff)
                    {
                        case 1:
                            benis *= 1.25f;
                            break;
                        case 2:
                            benis *= 1.5f;
                            break;
                    }

                    Debug.Log(benis);
#if UNITY_ANDROID
                    Firebase.Analytics.FirebaseAnalytics.LogEvent("BotRoundOver", "WinBenis", (int)benis);
#endif
                    SetGlobalUserBenis(GetPlayerUserBenis() + (ulong)benis);
                }
            } else if(networkGameRunning)
            {
                float benis = sList[i].GetComponent<UserData>().benis;

                switch (i)
                {
                    case 0:
                        benis *= 2;
                        break;
                    case 1:
                        benis *= 1.5f;
                        break;
                    case 2:
                        benis *= 1.25f;
                        break;
                    case 3:
                        benis *= 1.1f;
                        break;
                }

                benis *= 3; //MP-Bonus

                SetGlobalUserBenis(GetPlayerUserBenis() + (ulong)benis);

#if UNITY_ANDROID
                Firebase.Analytics.FirebaseAnalytics.LogEvent("MPRoundOver", "WinBenis", (int)benis);
#endif
            }

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
        float time = Random.Range(17.5f, 22.5f);

        if(diff == 1)
        { //mittel
            time = Random.Range(12.5f, 17.5f);
        } else if(diff == 2)
        {
            time = Random.Range(7.5f, 12.5f);
        }

        int todo = 0; //+-

        if(internalDiff > 3)
        {
            todo = 1; //+
        } else if(internalDiff < 2)
        {
            todo = -1; //-
        }

        if(todo == 0)
        {
            if(internalDiff == 2)
            { //eher minus
                time += Random.Range(-1f, 0.5f);
            } else if(internalDiff == 3)
            { //eher blus
                time += Random.Range(-0.5f, 1f);
            }
        } else if(todo == 1)
        {
            if(internalDiff == 4)
            { //leicht blus
                time += Random.Range(1.5f, 2.5f);
            } else if(internalDiff == 5)
            {
                time += Random.Range(3f, 5.5f);
            }
        } else if(todo == -1)
        {
            if(internalDiff == 1)
            { //leicht minus
                time -= Random.Range(1.5f, 2.5f);
            } else if(internalDiff == 0)
            {
                time -= Random.Range(3f, 5.5f);
            }
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
        SetBenisRaw(benis);
        users[0].GetComponent<UserData>().timer = 0;

        return benis;
    }

    public void SetBenisRaw(int benis)
    {
        users[0].GetComponent<UserData>().SetBenis(benis);
    }

    public void FlashTag(int tagPos, int userID, int benis, bool force = false)
    {
        if (MenuData.mode == (int)MenuData.Modes.versus)
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
        }

        if (userID > 0)
        {
            users[userID].GetComponent<UserData>().SetBenis(benis, force);
        }
    }

    public void ChatMessage(string text, int userID)
    {
        Color userColor = users[userID].GetComponent<Image>().color;

        string message = users[userID].GetComponent<UserData>().uName + ": " + text;

        imageSetter.GetComponent<ImageSetter>().SpawnChatText(message, 3f, userColor, new Vector3(255, 1043));
    }

    public bool SetGetRoyaleGussed(int tagPos)
    {
        users[0].GetComponent<UserData>().royaleGuessed[tagPos] = true;

        return users[0].GetComponent<UserData>().AllRoyaleGuessed();
    }

    public void ResetRoyaleGuessed()
    {
        for(int i = 0; i < users[0].GetComponent<UserData>().royaleGuessed.Length; i++)
        {
            users[0].GetComponent<UserData>().royaleGuessed[i] = false;
        }
    }

    public void ResetPlayerTimer()
    {
        users[0].GetComponent<UserData>().SetTimer(0);
    }

    void HandleBotRoyale()
    {
        for(int i = 1; i < users.Count; i++)
        {
            if (users[i].GetComponent<UserData>().knockout)
            { //skippen da ausgeknocked
                continue;
            }

            bool allGuessed = users[i].GetComponent<UserData>().AllRoyaleGuessed();

            if(allGuessed)
            { //neue postzuweisung
                users[i].GetComponent<UserData>().ResetRoyaleGuessed();

                users[i].GetComponent<UserData>().SetTimer(GenerateTime(diff,
                    users[i].GetComponent<UserData>().internalDiff));

                users[i].GetComponent<UserData>().postsCompleted++;
                users[i].GetComponent<UserData>().UpdatePostCount();
            } else
            { //tag timer management
                if (users[i].GetComponent<UserData>().timer <= 0)
                { //"rate" tag
                    users[i].GetComponent<UserData>().SetTimer(GenerateTime(diff,
                        users[i].GetComponent<UserData>().internalDiff));

                    bool notFound = true;
                    int tagCount = 3;

                    while (notFound)
                    {
                        notFound = false;
                        int trange = Random.Range(0, 100);
                        int tpos = 0;
                        if (trange > 10 && trange < 30)
                        { //2. top tag
                            tpos = 1;
                        }
                        else if (trange >= 30 && trange < 60)
                        {
                            tpos = 2;
                        }
                        else if (trange >= 60)
                        {
                            tpos = 3;
                            if (tagCount == 5)
                            {
                                tpos = Random.Range(3, 5); //3 oder 4
                            }
                            else if (tagCount == 6)
                            {
                                tpos = Random.Range(3, 6); //3, 4 oder 5
                            }
                        }

                        if (users[i].GetComponent<UserData>().royaleGuessed[tpos] == true)
                        { //wenn tag bereits erraten dann weitersuchen
                            notFound = true;
                        }

                        if (!notFound)
                        { //tag noch nicht erraten -> setzt auf erraten (i ist nutzerID)
                            FlashTag(tpos, i, CalcBenis(i, tpos, diff));
                            users[i].GetComponent<UserData>().royaleGuessed[tpos] = true;

                            //anzeige sortieren
                            if (MenuData.mode == (int)MenuData.Modes.battleRoyale)
                            {
                                SortBattleRoyale();
                            }
                        }
                    }
                } else
                {
                    users[i].GetComponent<UserData>().timer -= 1 * Time.deltaTime;
                }
            }
        }
    }

    public int GetLastRoyaleUser()
    {
        int pos = royaleLast;

        return pos;
    }

    public int GetRemainigRoyalePlayers()
    {
        int count = 0;

        foreach(GameObject user in users)
        {
            if(!user.GetComponent<UserData>().knockout)
            {
                count++;
            }
        }

        return count;
    }

    public List<int> SortBattleRoyale()
    { //sortiert punktestand

        List<int> sortedUsers = new List<int>();

        for (int a = 0; a < 25; a++) //top 25
        { //sucht top 25 user
            int max = 0;
            int pos = -1;

            for (int b = 0; b < users.Count; b++)
            {
                if (users[b].GetComponent<UserData>().benis >= max &&
                    !sortedUsers.Contains(b))
                {
                    max = users[b].GetComponent<UserData>().benis;
                    pos = b;
                }
            }

            sortedUsers.Add(pos);
        }

        for (int a = 0; a < 25; a++)
        {
            Vector3 newPos = royaleUserPositions[a];
            if (!newPos.Equals(users[sortedUsers[a]].GetComponent<UserData>().movingTo))
            {
                users[sortedUsers[a]].GetComponent<UserData>().AppendPos(newPos);
            }
        }

        for(int a = 24; a > 0; a--)
        {
            if(!users[sortedUsers[a]].GetComponent<UserData>().knockout)
            {
                royaleLast = sortedUsers[a];
                break;
            }
        }

        return sortedUsers;
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

            if (MenuData.mode == (int)MenuData.Modes.battleRoyale ||
                MenuData.mode == (int)MenuData.Modes.sort)
            {
                HandleBotRoyale();
                return;
            }

            bool allTagsGuessed = false;

            for (int i = 0; i < 4; i++)
            { //prüft ob alle tags erraten
                for (int a = 0; a < imgSetter.tagObjs.Count; a++)
                {
                    if (!imgSetter.tagObjs[a].GetComponent<TagData>().isGuessed[i])
                    {
                        allTagsGuessed = false;
                    }
                }
            }

            if (allTagsGuessed)
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

        if(userBenis != userBenisBackup - 5)
        {
            userBenis = userBenisBackup - 5;

            Application.Quit();
        }
    }
}
