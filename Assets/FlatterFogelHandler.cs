using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class FlatterFogelHandler : MonoBehaviour
{
    public GameObject player, topPipe, bottomPipe, flash, scoreText, goBtn;
    public Transform pipeParent;
    public bool gameActive = false;
    public AnimationCurve flashCurve;
    private List<GameObject> pipes = new List<GameObject>();
    public List<GameObject> bottomObjs = new List<GameObject>();
    private int score = 0;

    public static int state = 0;

    public enum FF_States {
        Idle = 0,
        Playing = 1,
        End = 2,
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void StartGame()
    {
        state = (int)FF_States.Playing;

        foreach(GameObject pipe in pipes)
        {
            Destroy(pipe);
        }

        pipes.Clear();

        goBtn.SetActive(false);

        player.transform.position = new Vector3(-1439, 187);
        player.transform.DOMove(new Vector3(-1122, 640), 1f);

        player.GetComponent<FF_PlayerData>().PlayerGo(false);
        player.GetComponent<Rigidbody2D>().DORotate(45, 0.2f);

        SetScore(0);
        scoreText.SetActive(false);

        Invoke("ResetRotation", 0.7f);
        Invoke("EndStart", 2f);
    }

    private void ResetRotation()
    {
        player.GetComponent<Rigidbody2D>().DORotate(0, 0.2f);
    }

    private void EndStart()
    {
        gameActive = true;
        player.GetComponent<FF_PlayerData>().PlayerGo(true);

        scoreText.SetActive(true);

        InvokeRepeating("HandleRotation", 0f, 0.05f);
        InvokeRepeating("SpawnPipes", 1f, 4f);
    }

    public void SetScore(int score)
    {
        this.score = score;
        scoreText.GetComponent<TextMeshProUGUI>().text = score.ToString();
    }

    private void SpawnBlus()
    {
        //blus.GetComponent<FF_Blus>().SpawnBlus(new Vector3(-657f, 628f), +100);
    }

    private void SpawnPipes()
    {
        GameObject pipeTop = Instantiate(topPipe, pipeParent);
        GameObject pipeBottom = Instantiate(bottomPipe, pipeParent);

        float yPos = Random.Range(415f, 915f);
        float xPos = -655f;

        pipeTop.transform.position = new Vector3(xPos, yPos + 150);
        pipeBottom.transform.position = new Vector3(xPos, yPos - 150);

        float topDiff = 1284 - pipeTop.transform.position.y;
        float bottomDiff = pipeBottom.transform.position.y - 1;

        pipeTop.GetComponent<RectTransform>().sizeDelta = new Vector2(75, topDiff);
        pipeTop.GetComponent<BoxCollider2D>().size = new Vector2(75, topDiff);
        pipeTop.GetComponent<BoxCollider2D>().offset = new Vector2(0, topDiff / 2);

        pipeBottom.GetComponent<RectTransform>().sizeDelta = new Vector2(75, bottomDiff);
        pipeBottom.GetComponent<BoxCollider2D>().size = new Vector2(75, bottomDiff);
        pipeBottom.GetComponent<BoxCollider2D>().offset = new Vector2(0, -(bottomDiff / 2));

        pipes.Add(pipeTop);
        pipes.Add(pipeBottom);
    }

    public void PlayerDeath()
    {
        state = (int)FF_States.End;

        gameActive = false;

        flash.GetComponent<Image>().DOFade(.75f, 0.2f).SetEase(flashCurve);

        CancelInvoke("HandleRotation");
        CancelInvoke("SpawnPipes");

        Invoke("ShowBtn", 1f);
    }

    private void ShowBtn()
    {
        goBtn.SetActive(true);
    }

    private void HandleRotation()
    {
        Vector2 vel = player.GetComponent<Rigidbody2D>().velocity;
        float rotation = (vel.y / 750) * 45;
        if (rotation < -45)
        {
            rotation = -45;
        }
        player.GetComponent<Rigidbody2D>().DORotate(rotation, 0.05f);
    }

    // Update is called once per frame
    void Update()
    {
        if(gameActive)
        {
            if (Input.GetMouseButtonUp(0))
            {
                player.GetComponent<FF_PlayerData>().PlayerFly();
            }

            List<GameObject> delList = new List<GameObject>();

            for(int i = 0; i < pipes.Count; i++)
            {
                Vector3 pos = pipes[i].transform.position;

                pos.x -= 100 * Time.deltaTime;

                if(pipes[i].tag.Equals("FF_PipeTop"))
                {
                    if(pos.x + 37.5f < player.transform.position.x &&
                        !pipes[i].GetComponent<PipeData>().isChecked)
                    {
                        SetScore(score + 1);
                        pipes[i].GetComponent<PipeData>().isChecked = true;
                    }
                }

                if(pos.x < -1541)
                {
                    delList.Add(pipes[i]);
                }

                pipes[i].transform.position = pos;
            }

            foreach(GameObject delPipe in delList)
            {
                pipes.Remove(delPipe);
                Destroy(delPipe);
            }
            delList.Clear();

            for(int i = 0; i < bottomObjs.Count; i++)
            {
                Vector3 pos = bottomObjs[i].transform.position;

                if(pos.x <= -1842)
                {

                    if(i == 0)
                    {
                        pos.x = (-100 * Time.deltaTime) + bottomObjs[1].transform.position.x + 720;
                    } else if(i == 1)
                    {
                        pos.x = bottomObjs[0].transform.position.x + 720;
                    }
                } else
                {
                    pos.x -= 100 * Time.deltaTime;
                }

                bottomObjs[i].transform.position = pos;
            }
        }
    }
}
