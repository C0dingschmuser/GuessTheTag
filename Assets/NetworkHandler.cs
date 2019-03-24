using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using DG.Tweening;
using TMPro;

public class NetworkHandler : MonoBehaviour
{
    public GameObject waitImage, imageSetter, progressBar, statusText, menuData;
    private TcpClient socketConnection, socketSenderConnection;
    private Thread clientReceiveThread, clientSendThread;
    private List<String> sendData = new List<string>();
    public string username;
    private bool rdySent = false, setupComplete = false, setupLock = false;
    public int state = (int)States.disconnected, diff = 1, otherDiffsAllowed = 1, playersInQueue = 0;
    public float timeout = 4f;
    private bool backPressed = false, connecting = false, shutdown = false;
    private string clientAuth = "";
    UserHandler uHandler;

    public enum NetworkIDs
    {
        login = 0,
    }

    public enum States
    {
        disconnected = 0,
        connected = 1,
        waitingForLogin = 2,
        loggedIn = 3,
        thinkLoading = 4,
        thinkReady = 5,
        thinkRunning = 6,
        inGameReady = 7,
        inGame = 8,
        endReady = 9,
        end = 10,
    }

    public enum ServerCodes
    {
        matchCloseError = -3,
        playerDisconnected = -2,
        loginNOTOK = -1,
        loginOK = 0,
        matchFound = 1,
        startThink = 2,
        startMatch = 3,
        tagGuessed = 4,
        startEnd = 5,
        matchDone = 10,
        timeoutCheck = 11,
        queueUpdate = 12,
        welcome = 13,
    }

    // Start is called before the first frame update
    void Start()
    {
        uHandler = GetComponent<UserHandler>();
    }

    private string GenerateUUID()
    {
        string uuid = "";

        return uuid;
    }

    public void ConnectToServer(string username)
    {
        this.username = username;
        diff = GetComponent<UserHandler>().mpDiff;
        setupComplete = false;
        shutdown = false;

        try
        {
            state = (int)States.connected;
            connecting = true;

            Invoke("ConnectFailed", 5f);

            //socketConnection = new TcpClient("flatterfogel.ddns.net", 6886);

            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();

            //socketSenderConnection = new TcpClient("flatterfogel.ddns.net", 6885);

            clientSendThread = new Thread(new ThreadStart(SendDataLoop));
            clientSendThread.IsBackground = true;
            clientSendThread.Start();
        } catch (Exception e)
        {
            Debug.Log("Connect Exception: " + e);
        }
    }

    public void ResetNetwork()
    {
        rdySent = false;
    }

    private void ConnectFailed()
    {
        menuData.GetComponent<MenuData>().SpawnError(new Vector3(-520, 56, 0), "Verbindung zum Server fehlgeschlagen!");

        StartStopMatch();
    }

    private void Login()
    {
        int id = (int)NetworkIDs.login;

        string message = 
            id.ToString() + "~" + username + "~" + diff.ToString() + "~" +
            otherDiffsAllowed.ToString() + "~" + clientAuth + "~";

        bool ok = SendTCPMessage(message);
        if(ok)
        {
            Debug.Log("Message sent");
        }
    }

    bool SocketConnected(Socket s)
    {
        bool part1 = s.Poll(1000, SelectMode.SelectRead);
        bool part2 = (s.Available == 0);
        if (part1 && part2)
            return false;
        else
            return true;
    }

    public void DisconnectFromServer()
    {

        shutdown = true;

        if (socketConnection != null)
        {
            //socketConnection.Close();
        }

        if (socketSenderConnection != null)
        {
            //socketSenderConnection.Close();
        }

        state = (int)States.disconnected;
        if (clientReceiveThread != null)
        {
            //clientReceiveThread.Abort();
        }

        if(clientSendThread != null)
        {
            //clientSendThread.Abort();
        }
    }

    private void OnApplicationQuit()
    {
        DisconnectFromServer();
    }

    private void ListenForData()
    {
        try
        {
            UnityMainThreadDispatcher.Instance().Enqueue(SetStatusText("Verbinde..."));

            socketConnection = new TcpClient();
            var result = socketConnection.BeginConnect("flatterfogel.ddns.net", 6886, null, null);

            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4));
            if (success)
            {
                Byte[] bytes = new Byte[1024];
                while (!shutdown)
                {
                    // Get a stream object for reading 		

                    if (socketConnection == null)
                    {
                        return;
                    }

                    using (NetworkStream stream = socketConnection.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 					
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message. 						
                            string serverMessage = Encoding.ASCII.GetString(incommingData);
                            FetchMessage(serverMessage);
                        }
                    }
                }
            }

            socketConnection.EndConnect(result);
            socketConnection.Close();
        } catch(Exception sError)
        {
            if(!shutdown)
            {
                Debug.Log("Socket Error: " + sError);

                UnityMainThreadDispatcher.Instance().
                    Enqueue(Error(new Vector3(-550, 56, 0), "Verbindung zum Server verloren!"));
                UnityMainThreadDispatcher.Instance().Enqueue(SStop());
            }
        }
    }

    private void SendDataLoop()
    {
        socketSenderConnection = new TcpClient();
        var result = socketSenderConnection.BeginConnect("flatterfogel.ddns.net", 6885, null, null);

        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4));

        if (success)
        {
            while (!shutdown)
            {
                if (sendData.Count > 0)
                {
                    string clientMessage = sendData[0];

                    if (clientMessage != null)
                    {
                        try
                        {
                            Send(clientMessage, socketSenderConnection.GetStream());

                            sendData.RemoveAt(0);
                        }
                        catch (Exception socketException)
                        {
                            if (!shutdown)
                            {
                                Debug.Log("Socket exception: " + socketException);

                                UnityMainThreadDispatcher.Instance().
                                    Enqueue(Error(new Vector3(-550, 56, 0), "Verbindung zum Server verloren!"));
                                UnityMainThreadDispatcher.Instance().Enqueue(SStop());
                            }
                        }
                    }
                }
            }
        }

        try
        {
            socketSenderConnection.EndConnect(result);
            socketSenderConnection.Close();
        } catch (SocketException) { /* Ignore */ }
    }

    private void Send(string message, NetworkStream stream)
    {
        if (stream.CanWrite)
        {
            string clientMessage = message;
            // Convert string message to byte array.                 
            byte[] clientMessageAsByteArray = Encoding.UTF8.GetBytes(clientMessage);
            // Write byte array to socketConnection stream.                 
            stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
        }
    }

    private bool SendTCPMessage(string message)
    {
        timeout = 4f;

        message = "<" + message + ">";
        bool messageSent = false;

        if (socketSenderConnection == null)
        {
            return messageSent;
        }

        try
        {
            sendData.Add(message);
        }
        catch (Exception socketException)
        {
            Debug.Log("Socket exception: " + socketException);

            UnityMainThreadDispatcher.Instance().
                Enqueue(Error(new Vector3(-550, 56, 0), "Verbindung zum Server verloren!"));
            UnityMainThreadDispatcher.Instance().Enqueue(SStop());
        }

        return messageSent;
    }

    private void FetchMessage(string message)
    {

        if (message[0].Equals('<') && message[message.Length - 1].Equals('>'))
        { //nachricht ok
            message = message.Replace("<", "");
            message = message.Replace(">", "");

            //Debug.Log(message);

            string[] split = message.Split('~');

            switch(Int32.Parse(split[0]))
            {
                case (int)ServerCodes.welcome: //login ausführen
                    UnityMainThreadDispatcher.Instance().Enqueue(CancelInvokeString("ConnectFailed"));

                    clientAuth = split[1];

                    Login();
                    break;
                case (int)ServerCodes.matchCloseError: //spieler verlassen + nurnoch 1 spieler = match cancel
                    UnityMainThreadDispatcher.Instance().
                        Enqueue(Error(new Vector3(-500, 56, 0), "Der letzte Mitspieler hat das Spiel verlassen!"));
                    StartStopMatch();
                    break;
                case (int)ServerCodes.playerDisconnected: //spieler verlassen
                    UnityMainThreadDispatcher.Instance().Enqueue(PlayerDisconnect(split[1]));
                    break;
                case (int)ServerCodes.loginNOTOK:
                    //login nicht ok
                    break;
                case (int)ServerCodes.loginOK:
                    //login ok
                    connecting = false;
                    setupComplete = true;

                    UnityMainThreadDispatcher.Instance().Enqueue(SetName(split[1]));

                    UnityMainThreadDispatcher.Instance().Enqueue(SetStatusText("Suche Spiel - Spieler in Warteschlange: " + split[2]));
                    break;
                case (int)ServerCodes.matchFound:

                    state = (int)States.thinkLoading;

                    int reused = Int32.Parse(split[3]); //ob komplett neues match oder nur neue runde
                    int postID = Int32.Parse(split[2]);

                    UnityMainThreadDispatcher.Instance().Enqueue(SetUser(0, username, reused));
                    UnityMainThreadDispatcher.Instance().Enqueue(ShowUsers());

                    UnityMainThreadDispatcher.Instance().Enqueue(LoadPost(postID));
                    UnityMainThreadDispatcher.Instance().Enqueue(WaitImageState(true));
                    UnityMainThreadDispatcher.Instance().Enqueue(SetStatusText("", false));

                    string nameString = split[1];
                    nameString = nameString.Replace(username + ";", "");
                    string[] names = nameString.Split(';');

                    for (int i = 1; i < names.Length; i++)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(SetUser(i, names[i - 1], reused));
                    }

                    break;
                case (int)ServerCodes.startThink: //alle spieler bereit, starte thinktime
                    state = (int)States.thinkRunning;
                    UnityMainThreadDispatcher.Instance().Enqueue(WaitImageState(false, true));
                    break;
                case (int)ServerCodes.startMatch: //beendet anschauzeit + startet match
                    UnityMainThreadDispatcher.Instance().Enqueue(HideShowInput(true));
                    state = (int)States.inGame;
                    break;
                case (int)ServerCodes.tagGuessed: //mitspieler hat tag erraten -> tag aufleuchten + neue punktzahl
                    //name~tagID~benis

                    string userName = split[1];
                    int tagID, benis;
                    bool success1 = Int32.TryParse(split[2], out tagID);
                    bool success2 = Int32.TryParse(split[3], out benis);

                    if (success1 && success2)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(FlashTagSetPoint(tagID, userName, benis));
                    }

                    break;
                case (int)ServerCodes.startEnd: //ratezeit vorbei -> zeige stats
                    UnityMainThreadDispatcher.Instance().Enqueue(HideShowInput(false));
                    state = (int)States.end;
                    break;
                case (int)ServerCodes.matchDone:
                    Debug.Log("MatchDone");
                    StartStopMatch();
                    break;
                case (int)ServerCodes.queueUpdate:
                    playersInQueue = Int32.Parse(split[1]);
                    UnityMainThreadDispatcher.Instance().Enqueue(SetStatusText("Suche Spiel - Spieler in Warteschlange: " + split[1]));
                    break;
            }
        }
    }

    public IEnumerator HideShowInput(bool active)
    {
        imageSetter.GetComponent<ImageSetter>().inputObj.SetActive(active);
        yield return null;
    }

    public IEnumerator SStop()
    {
        StartStopMatch();
        yield return null;
    }

    public IEnumerator CancelInvokeString(string invoke)
    {
        CancelInvoke(invoke);

        yield return null;
    }

    public IEnumerator Error(Vector3 pos, string error)
    {
        menuData.GetComponent<MenuData>().SpawnError(pos, error);
        yield return null;
    }

    public IEnumerator ShowUsers()
    {
        GetComponent<UserHandler>().ShowUsers();
        yield return null;
    }

    public IEnumerator SetStatusText(string text, bool active = true)
    {
        if(!active)
        {
            statusText.SetActive(false);
            yield return null;
        } else
        {
            if(!statusText.activeSelf)
            {
                statusText.SetActive(true);
            }
        }

        statusText.GetComponent<TextMeshProUGUI>().text = text;

        yield return null;
    }

    public IEnumerator SetName(string name)
    {
        username = name;
        GetComponent<UserHandler>().users[0].GetComponent<UserData>().SetName(name, true);
        yield return null;
    }

    public void StartStopMatch()
    {
        if (state == (int)States.disconnected)
        {
            return;
        }

        DisconnectFromServer();
        connecting = false;

        UnityMainThreadDispatcher.Instance().Enqueue(EndMatch());
    }

    public IEnumerator EndMatch()
    {
        state = (int)States.disconnected;
        UserHandler uHandler = GetComponent<UserHandler>();
        ProgressBar pBar = progressBar.GetComponent<ProgressBar>();
        ImageSetter imgSetter = imageSetter.GetComponent<ImageSetter>();

        for (int i = 0; i < uHandler.users.Length; i++)
        {
            if (uHandler.users[i].GetComponent<UserData>().uName != "")
            {
                uHandler.ResetUser(i);
            }
        }

        Debug.Log("stop");

        pBar.ResetGame(true, false);
        Camera.main.transform.DOMove(new Vector3(-381, 642, -10), 0.5f);
        MenuData.state = 0;
        waitImage.SetActive(false);

        yield return null;
    }

    public IEnumerator PlayerDisconnect(string username)
    {
        int userID = 1;

        UserHandler uHandler = GetComponent<UserHandler>();

        for (int i = 1; i < uHandler.users.Length; i++)
        {
            if (uHandler.users[i].GetComponent<UserData>().uName != "")
            {
                if (uHandler.users[i].GetComponent<UserData>().uName == username)
                {
                    break;
                }
                else
                {
                    userID++;
                }
            }
        }

        ImageSetter imgSetter = imageSetter.GetComponent<ImageSetter>();
        foreach(GameObject tag in imgSetter.tagObjs)
        {
            tag.GetComponent<TagData>().isGuessed[userID] = false;
        }

        uHandler.ResetUser(userID);

        yield return null;
    }

    public IEnumerator FlashTagSetPoint(int tagID, string userName, int benis)
    {
        int userID = 1;

        UserHandler uHandler = GetComponent<UserHandler>();

        for (int i = 1; i < uHandler.users.Length; i++)
        {
            if (uHandler.users[i].GetComponent<UserData>().uName != "")
            {
                if (uHandler.users[i].GetComponent<UserData>().uName == userName)
                {
                    break;
                }
                else
                {
                    userID++;
                }
            }
        }

        uHandler.FlashTag(tagID, userID, benis, true);
        yield return null;
    }

    public IEnumerator WaitImageState(bool state, bool pBar = false)
    {
        if(pBar)
        {
            progressBar.SetActive(true);
        }

        waitImage.SetActive(state);
        yield return null;
    }

    public IEnumerator LoadPost(int id)
    {
        imageSetter.GetComponent<ImageSetter>().LoadRandomPost(id);
        yield return null;
    }

    public IEnumerator SetUser(int i, string name, int reused)
    {
        uHandler.SetUser(i, name, reused);
        yield return null;
    }

    private void ResetBackPressed()
    {
        backPressed = false;
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            if(GetComponent<UserHandler>().networkGameRunning)
            {

            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if(GetComponent<UserHandler>().networkGameRunning && 
            state != (int)States.disconnected &&
            setupComplete)
        {
            timeout -= Time.deltaTime;
            if(timeout < 0)
            {
                timeout = 4f;
                SendTCPMessage("-11~");
            }

            if(Input.GetKeyDown(KeyCode.Escape) && 
                !imageSetter.GetComponent<ImageSetter>().zoomEnabled)
            {
                if(!backPressed)
                {
                    backPressed = true;
                    Invoke("ResetBackPressed", 2f);
                } else
                { //verlasse match
                    StartStopMatch();
                }
            }
        }
    }
}
