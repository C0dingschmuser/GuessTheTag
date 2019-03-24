using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class PointText : MonoBehaviour
{
    public AnimationCurve speedCurve, fadeCurve;
    private float time, yTarget;

    public void Initialize(string text, float time, Color c, Vector3 pos, float yTarget = 1161f)
    {
        this.time = time;
        this.yTarget = yTarget;

        transform.position = pos;
        c.a = 0;
        GetComponent<TextMeshProUGUI>().color = c;
        GetComponent<TextMeshProUGUI>().text = text;

        gameObject.SetActive(false);
    }

    public void Go()
    {
        transform.DOMoveY(1161.5f, time).SetEase(speedCurve);
        transform.GetComponent<TextMeshProUGUI>().DOFade(1f, time / 6);
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y > yTarget)
        {
            transform.GetComponent<TextMeshProUGUI>().DOFade(0f, time / 6);
            Destroy(gameObject, (time / 6) + 0.01f);
        }
    }
}
