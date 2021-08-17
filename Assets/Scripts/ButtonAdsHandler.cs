using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAdsHandler : MonoBehaviour
{
    public AdvertisingServiceBase advertisingServiceBase;

    public void Start()
    {
        advertisingServiceBase.Init();
    }

    public void RequestBannerAd()
    {
        advertisingServiceBase.RequestBannerAd();
    }

    public void DestroyBannerAd()
    {
        advertisingServiceBase.DestroyBannerAd();
    }

    public void RequestAndLoadRewardedAd()
    {
        advertisingServiceBase.RequestAndLoadRewardedAd();
    }

    public void ShowRewardedAd()
    {
        advertisingServiceBase.ShowRewardedAd();
    }

    public void RequestAndLoadInterstitialAd()
    {
        advertisingServiceBase.RequestAndLoadInterstitialAd();
    }

    public void ShowInterstitialAd()
    {
        advertisingServiceBase.ShowInterstitialAd();
    }

    public void DestroyInterstitialAd()
    {
        advertisingServiceBase.DestroyInterstitialAd();
    }

    public void RequestAndLoadRewardedInterstitialAd()
    {
        advertisingServiceBase.RequestAndLoadRewardedInterstitialAd();
    }

    public void ShowRewardedInterstitialAd()
    {
        advertisingServiceBase.ShowRewardedInterstitialAd();
    }
}