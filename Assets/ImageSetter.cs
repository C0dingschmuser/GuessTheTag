using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Networking;

public class ImageSetter : MonoBehaviour
{
    public GameObject userHandler, pointPrefab, pointErrorPrefab;
    public Sprite ver;
    public GameObject verObj, tagPrefab, inputObj, statusText;
    public List<GameObject> pointBuffer = new List<GameObject>();
    public AspectRatioFitter aFit;
    public bool inputActivated = false, offlineMode = false,
        imageLoaded = false, thinkDone = false, zoomEnabled = false;
    public List<GameObject> tagObjs = new List<GameObject>();
    public float posX, posYtop, posYbottom;
    public Transform tagParent, pointParent;
    public TextAsset postData;
    private string[] postList;
    Color hideColor = new Color32(68, 68, 68, 255);
    public AnimationCurve curve;
    public TMP_InputField inputFieldRef;
    private bool input_wasFocused = false;
    TouchScreenKeyboard keyboard;

    // Start is called before the first frame update
    void Start()
    {

        postList = postData.text.Split('|');

        SetImage(ver);
        InvokeRepeating("HandlePointBuffer", 0f, .5f);

        //LoadRandomPost();
    }

    private void HandlePointBuffer()
    {
        if(pointBuffer.Count < 1)
        {
            return;
        }

        GameObject obj = pointBuffer[pointBuffer.Count - 1];
        obj.SetActive(true);
        obj.GetComponent<PointText>().Go();

        pointBuffer.Remove(obj);
    }

    public void SelectInput()
    {
        inputActivated = true;
    }

    public void DeSelectInput()
    {
        inputActivated = false;
    }

    public void LoadRandomPost(int overrideID = -1)
    {
        int pCount = Random.Range(0, 512);

        if(!offlineMode)
        {
            pCount = Random.Range(512, postList.Length);
        }

        if(overrideID > -1)
        {
            pCount = overrideID;
        }

        string[] postSplit = postList[pCount].Split(';');

        if(postSplit.Length != 6)
        { //fehler beim splitten -> neuer post
            LoadRandomPost();
            return;
        }

        string[] tags = postSplit[4].Split(',');

        int maxLength = 4;

        for(int i = 0; i < maxLength; i++)
        {
            GameObject tag = Instantiate(tagPrefab, tagParent);
            tagObjs.Add(tag);

            SetTag(i, tags[i]);
            tag.SetActive(false);
        }

        Sprite image = null;

        if (pCount < 512)
        {
            image = Resources.Load<Sprite>("Sprites/posts/img_" +
                postSplit[1] + "_" + postSplit[0]);
            SetImage(image);
            imageLoaded = true;
        } else
        { //get img from pr0

            string linkString = "https://img.pr0gramm.com/" + postSplit[5];

            StartCoroutine(GetTexture(linkString));
        }
    }

    IEnumerator GetTexture(string link)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(link);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            SetImage(Sprite.Create((Texture2D)myTexture, 
                new Rect(0,0,myTexture.width,myTexture.height), 
                new Vector2(0,0)));
            imageLoaded = true;
        }
    }

    public void ResetImage()
    {
        SetImage(ver);
        SetStatusText("Verbinde...");
    }

    public void SetStatusText(string text)
    {
        statusText.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void ResetGame(bool full = false)
    {
        foreach(GameObject tag in tagObjs)
        {
            tag.GetComponent<DOTweenAnimation>().DORestartById("FadeOut");
            Destroy(tag, 1f);
        }

        imageLoaded = false;
        thinkDone = false;

        foreach(GameObject tag in tagObjs)
        {
            Destroy(tag);
        }

        tagObjs.Clear();

        if(keyboard != null)
        {
            keyboard.active = false;
        }

        //SetImage(ver);

        if (!full && !userHandler.GetComponent<UserHandler>().networkGameRunning)
        {
            LoadRandomPost();
        }
    }

    private void SetTag(int count, string text)
    {
        
        tagObjs[count].GetComponent<TagData>().text = text;
        tagObjs[count].transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(text);
        tagObjs[count].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = hideColor;

        if (count == 0)
        {
            tagObjs[count].transform.position = new Vector3(17f, 501.33f, 0);
        } else
        {
            GameObject pTag = tagObjs[count - 1];

            float lastX = pTag.transform.position.x +
                pTag.transform.GetChild(0).GetComponent<TextMeshProUGUI>().preferredWidth;

            float newY = pTag.transform.position.y;
            float newX = lastX + 38f;

            tagObjs[count].transform.position = new Vector3(newX, newY, 0);

            lastX = tagObjs[count].transform.position.x +
                tagObjs[count].transform.GetChild(0).GetComponent<TextMeshProUGUI>().preferredWidth;

            if (lastX > 690f)
            {
                newX = 17f;
                newY -= 49f;
                tagObjs[count].transform.position = new Vector3(newX, newY, 0);
            } 

        }
    }

    private string GetKeyboardText()
    {
        string text = "";

        if(keyboard != null)
        {
            text = keyboard.text;
        }
        return text;
    }

    private bool IsKeyboardDone()
    {
        bool done = false;

        if (keyboard != null)
        {
            if (keyboard.status == TouchScreenKeyboard.Status.Done)
            {
                done = true;
            }
        }

        return done;
    }

    public void OpenKeyboard()
    {
        if(userHandler.GetComponent<UserHandler>().networkGameRunning)
        { //multiplayer game
            if(userHandler.GetComponent<NetworkHandler>().state != (int)NetworkHandler.States.inGame)
            {
                return;
            }
        } else
        { //bot game
            if(!thinkDone)
            {
                return;
            }
        }
        //keyboard = TouchScreenKeyboard.Open("");
    }

    public void EndEdit()
    {
        if(input_wasFocused)
        {
            CheckTag();
        }
    }

    private void CheckTag()
    {
        if(!imageLoaded || !thinkDone)
        {
            return;
        }

        int tpos = 0;

        int guessed = -1;

        string tagText = inputFieldRef.text;
        inputFieldRef.text = "";

        for(int i = 0; i < tagObjs.Count; i++) {
            GameObject tempTag = tagObjs[i];

            if(!tempTag.GetComponent<TagData>().isGuessed[0])
            {
                tagText = tagText.ToLower();
                if(tagText.Equals(tempTag.GetComponent<TagData>().text.ToLower()))
                { //wenn richtiger tag
                    guessed = i;
                    break;
                }
            }
            tpos++;
        }

        if(guessed > -1)
        { //tag erraten
            GameObject tempTag = tagObjs[guessed];

            int addBenis = userHandler.GetComponent<UserHandler>().SetUserBenis(tpos);

            if (userHandler.GetComponent<UserHandler>().networkGameRunning)
            {
                int benis = userHandler.GetComponent<UserHandler>().GetUserBenis();
                userHandler.GetComponent<NetworkHandler>().
                    SendMessage("SendTCPMessage", "4~" + tpos.ToString() + "~" + benis.ToString() + "~");

                userHandler.GetComponent<Stats>().mpPointsCollected += (ulong)addBenis;
                userHandler.GetComponent<Stats>().mpTagsCorrectGuessed++;
            }
            else
            {
                userHandler.GetComponent<Stats>().pointsCollected += (ulong)addBenis;
                userHandler.GetComponent<Stats>().tagsCorrectGuessed++;
            }

            tempTag.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
            tempTag.GetComponent<TagData>().isGuessed[0] = true;

            SpawnPointText(userHandler.GetComponent<NetworkHandler>().username,
                3f, userHandler.GetComponent<UserHandler>().GetUserColor(),
                new Vector3(360, 672.6f));

            inputObj.GetComponent<Image>().DOBlendableColor(Color.green, 0.5f).SetEase(curve);
        } else
        {
            if (userHandler.GetComponent<UserHandler>().networkGameRunning)
            {
                userHandler.GetComponent<Stats>().mpTagsGuessed++;
            }
            else
            {
                userHandler.GetComponent<Stats>().tagsGuessed++;
            }

            inputObj.GetComponent<Image>().DOBlendableColor(Color.red, 0.5f).SetEase(curve);
        }
    }

    public void SpawnPointText(string text, float time, Color c, Vector3 pos, float yTarget = 1161f, bool error = false)
    {
        GameObject prefab = pointPrefab;

        if(error)
        {
            prefab = pointErrorPrefab;
        }

        GameObject pObj = Instantiate(prefab, pointParent);
        pObj.GetComponent<PointText>().Initialize(text, time, c, pos, yTarget); //grün
        pObj.SetActive(false);

        pointBuffer.Add(pObj);
    }

    public bool ShowTags()
    {
        bool needed = false;

        foreach(GameObject tag in tagObjs)
        {
            if(!tag.GetComponent<TagData>().isGuessed[0])
            { //wenn user tag noch nicht erraten -> anzeigen
                needed = true;
                tag.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOColor(Color.white, 0.75f);
            }
        }

        return needed;
    }

    public void FadeInTags()
    {
        foreach(GameObject tag in tagObjs)
        {
            tag.SetActive(true);
            tag.GetComponent<DOTweenAnimation>().DORestartById("FadeIn");
        }
    }

    public void SetImage(Sprite image, bool logo = false)
    {
        if(logo)
        {
            image = ver;
        }

        float aspect = image.rect.width / image.rect.height;
        GameObject pr0Img = transform.GetChild(0).gameObject;

        pr0Img.GetComponent<AspectRatioFitter>().aspectRatio = aspect;

        Sprite oldImg = pr0Img.GetComponent<Image>().sprite;

        pr0Img.GetComponent<Image>().sprite = image;

        oldImg = null;
        Resources.UnloadUnusedAssets();
    }

    public void Enlarge()
    {
        Sprite image = transform.GetChild(0).GetComponent<Image>().sprite;
        float aspect = image.rect.width / image.rect.height;

        verObj.GetComponent<AspectRatioFitter>().aspectRatio = aspect;
        verObj.GetComponent<Image>().sprite = image;
        verObj.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("Enlarge");
        Invoke("ActivateZoom", 0.2f);
    }

    private void ActivateZoom()
    {
        verObj.GetComponent<Zoom>().enabled = true;
        zoomEnabled = true;
        aFit.enabled = false;
    }

    private void DisableZoom()
    {
        aFit.enabled = true;
        verObj.GetComponent<Zoom>().enabled = false;

        Camera.main.orthographicSize = 637.7938f; //setzt zoom zurück falls vorhanden

        verObj.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("Reduce");
    }

    // Update is called once per frame
    void Update()
    {
        if(zoomEnabled)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                zoomEnabled = false;
                DisableZoom();
            }
        }

        if(IsKeyboardDone())
        {
            inputActivated = false;
            //CheckTag();
        }

        input_wasFocused = inputFieldRef.isFocused;

        if(Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            bool ok = true;
            if (userHandler.GetComponent<UserHandler>().networkGameRunning)
            { //multiplayer game
                if (userHandler.GetComponent<NetworkHandler>().state != (int)NetworkHandler.States.inGame)
                {
                    ok = false;
                }
            }
            else
            { //bot game
                if (!thinkDone)
                {
                    ok = false;
                }
            }

            ok = false;

            if(ok)
            {
                Color tmp = inputObj.GetComponent<Image>().color;
                if(tmp.a.Equals(1))
                {
                    //CheckTag();
                } 
            }
        }
    }
}
