using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;


public class UserData : MonoBehaviour
{
    public string uName = "";
    public int benis = 0, internalDiff = 0, postsCompleted = 0, endPos = -1;
    private int benisBackup = 4;
    public float timer = 0, originalTimer = 0;
    public bool guessedAll = false;
    public bool[] royaleGuessed = new bool[4];
    public bool knockout = false;
    private List<Vector3> posList = new List<Vector3>();
    public Vector3 movingTo;
    public bool user = false;

    public void Start()
    {
        internalDiff = Random.Range(0, 6);

        for(int i = 0; i < 4; i++)
        {
            royaleGuessed[i] = false;
        }

        movingTo = transform.position;
        InvokeRepeating("HandlePos", 0f, 0.151f);
    }

    public void AppendPos(Vector3 pos)
    {
        posList.Add(pos);
    }

    public void DoKnockout()
    {
        knockout = true;

        Color c = GetComponent<Image>().color;

        float max = c.r + c.b + c.g;
        float avg = max / 3;

        c.r = avg;
        c.b = avg;
        c.g = avg;

        GetComponent<Image>().color = c; //setzt auf schwarz weiß

        transform.GetChild(3).gameObject.SetActive(true); //setzt knockout-text auf true
    }

    private void HandlePos()
    {
        if(posList.Count > 0)
        {
            Vector3 newPos = posList[0];
            posList.RemoveAt(0);

            if(posList.Count == 0 && knockout)
            {
                CancelInvoke("HandlePos");
            }

            transform.DOMove(newPos, 0.15f);
            movingTo = newPos;
        }
    }

    public bool AllRoyaleGuessed()
    {
        bool ok = true;

        for(int i = 0; i < 4; i++)
        {
            if(!royaleGuessed[i])
            {
                ok = false;
                break;
            }
        }

        return ok;
    }

    public void ResetRoyaleGuessed()
    {
        for (int i = 0; i < 4; i++)
        {
            royaleGuessed[i] = false;
        }
    }

    public void SetTimer(float newTime)
    {
        timer = newTime;
        originalTimer = newTime;
    }

    public void SetName(string name, bool user = false)
    {
        this.uName = name;

        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;

        if(MenuData.mode == (int)MenuData.Modes.battleRoyale)
        {
            if(name.Length > 10 && name.Length < 14)
            {
                transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 12;
            } else if(name.Length > 13 && name.Length < 17)
            {
                transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 10;
            } else if(name.Length > 16)
            {
                transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 8;
            }
        } else if(MenuData.mode == (int)MenuData.Modes.sort)
        {
            transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 14;

            if (name.Length > 10 && name.Length < 14)
            {
                transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 12;
            }
            else if (name.Length > 13 && name.Length < 17)
            {
                transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 10;
            }
            else if (name.Length > 16)
            {
                transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 8;
            }
        }

    }

    public void SetBenis(int benis, bool reset = false)
    {
        if (reset)
        {
            this.benis = benis;
        } else
        {
            this.benis += benis;
            if(this.benis < 0)
            {
                this.benis = 0;
            }
        }
        benisBackup = this.benis + 4;
        transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "B " + this.benis.ToString();
    }

    public void UpdatePostCount()
    {
        transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = postsCompleted.ToString("00");
    }

    public void RoundFadeOut()
    {
        GetComponent<Image>().DOFade(0f, .75f);
        foreach(Transform child in transform)
        {
            child.GetComponent<TextMeshProUGUI>().DOFade(0f, .75f);
        }
    }

    public void RoundFadeIn()
    {
        GetComponent<Image>().DOFade(1f, .75f);
        foreach (Transform child in transform)
        {
            child.GetComponent<TextMeshProUGUI>().DOFade(1f, .75f);
        }
    }

    private void Update()
    {
        if(benis != benisBackup - 4 && user)
        {
            benis = benisBackup - 4;

            Application.Quit();
        }
    }
}
