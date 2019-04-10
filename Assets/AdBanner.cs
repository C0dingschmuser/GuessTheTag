using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using TMPro;

public class AdBanner : MonoBehaviour
{
    public GameObject benitrat0r, adBtn, adInfo;
    const string gameID = "3063285";
    private bool adWatching = false, appInstalled = false;
    private DateTime startTime;

    private void Start()
    {
        Advertisement.Initialize(gameID);

        bool ok = false;

#if UNITY_ANDROID
        ok = true;
#endif

        if (Application.internetReachability == NetworkReachability.NotReachable || !ok)
        {
            adBtn.GetComponent<Button>().interactable = false;
        }
    }

    public void ShowAd()
    {
        adInfo.SetActive(false);
        adWatching = true;

        StartCoroutine(ShowRewardedAd());
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            if(adWatching)
            {
                startTime = DateTime.Now;
            }
        }
    }

    IEnumerator ShowRewardedAd()
    {

        while(!Advertisement.IsReady("rewardedVideo"))
        {
            yield return null;
        }

        var options = new ShowOptions { resultCallback = HandleShowResult };
        Advertisement.Show("rewardedVideo", options);
    }

    private void HandleShowResult(ShowResult result)
    {

        switch (result)
        {
            case ShowResult.Finished:
#if UNITY_ANDROID
                Firebase.Analytics.FirebaseAnalytics.LogEvent("AdShowed");
#endif
                adWatching = false;

                DateTime cTime = DateTime.Now;

                if(startTime == null)
                {
                    startTime = DateTime.Now;
                }

                TimeSpan adTime = cTime - startTime;

                int benis = 2048;

                if(adTime.TotalSeconds > 35)
                {
                    benis += 3072;
                }

                UserHandler.SetGlobalUserBenis(UserHandler.GetPlayerUserBenis() + (ulong)benis);
                benitrat0r.GetComponent<Benitrat0r>().SetBenis(UserHandler.GetPlayerUserBenis());
                break;
        }
    }
}