using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;


public class UserData : MonoBehaviour
{
    public string uName = "";
    public int benis = 0, internalDiff = 0;
    public float timer = 0, originalTimer = 0;
    public bool guessedAll = false;

    public void Start()
    {
        internalDiff = Random.Range(0, 3);
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

    }

    public void SetBenis(int benis, bool reset = false)
    {
        if (reset)
        {
            this.benis = benis;
        } else
        {
            this.benis += benis;
        }
        transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = this.benis.ToString();
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
}
