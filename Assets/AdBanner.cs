using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using TMPro;

public class AdBanner : MonoBehaviour
{
    public GameObject benitrat0r, adBtn;
    const string gameID = "3063285";

    private void Start()
    {
        Advertisement.Initialize(gameID);

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            adBtn.GetComponent<Button>().interactable = false;
        }
    }

    public void ShowAd()
    {
        StartCoroutine(ShowRewardedAd());
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
                UserHandler.SetGlobalUserBenis(UserHandler.GetPlayerUserBenis() + 2048);
                benitrat0r.GetComponent<Benitrat0r>().SetBenis(UserHandler.GetPlayerUserBenis());
                break;
        }
    }
}