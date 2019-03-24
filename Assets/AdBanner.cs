using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdBanner : MonoBehaviour
{

    string gameID = "3063285";
    string placementID = "banner", placementID2 = "banner2";

    // Start is called before the first frame update
    void Start()
    {
        Advertisement.Initialize(gameID, true);
        StartCoroutine(ShowBannerWhenReady());
        //StartCoroutine(ShowBanner2WhenReady());
    }

    IEnumerator ShowBannerWhenReady()
    {
        while (!Advertisement.IsReady("banner"))
        {
            yield return new WaitForSeconds(0.5f);
        }
        Advertisement.Banner.Show(placementID);
    }

    IEnumerator ShowBanner2WhenReady()
    {
        while (!Advertisement.IsReady("banner2"))
        {
            yield return new WaitForSeconds(0.5f);
        }
        Advertisement.Banner.Show(placementID2);
        Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
