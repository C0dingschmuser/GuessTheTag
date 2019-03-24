using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class TagData : MonoBehaviour
{
    public string text;
    public bool userDisplayed = false;
    public bool[] isGuessed = new bool[4];
    private float colorFadeLength = 0.75f;
    public List<Color> colorBuffer = new List<Color>();
    public AnimationCurve curve;

    private bool inUse = false;

    private void DeactivateUse()
    {
        inUse = false;
    }

    private void Update()
    {
        if(colorBuffer.Count > 0)
        { //wenn farben drin
            if(!inUse && !userDisplayed)
            {
                inUse = true;

                GetComponent<Image>().DOColor(colorBuffer[0], colorFadeLength).
                                SetEase(curve);

                if(!isGuessed[0])
                {
                    transform.GetChild(0).GetComponent<TextMeshProUGUI>().
                                    DOColor(colorBuffer[0], colorFadeLength).SetEase(curve);
                }

                colorBuffer.RemoveAt(0);

                Invoke("DeactivateUse", colorFadeLength + 0.01f);
            }
        }

        if(isGuessed[0] && !userDisplayed)
        { //wenn user erraten aber noch nicht angezeigt
            if(!inUse)
            {
                inUse = true;
                userDisplayed = true;
                transform.GetChild(0).GetComponent<TextMeshProUGUI>().
                                    DOColor(Color.white, colorFadeLength);
            }
        }
    }
    //public DOTweenAnimation[] guessAnimations = new DOTweenAnimation[4];
}
