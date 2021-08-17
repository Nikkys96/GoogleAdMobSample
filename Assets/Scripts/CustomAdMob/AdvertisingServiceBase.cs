using System;
using UnityEngine;

public enum AdType
{
    Skippable,
    Rewarded,
    Offerwall
}

public abstract class AdvertisingServiceBase : MonoBehaviour
{
    public abstract void Init();
    public abstract void RequestBannerAd();
    public abstract void DestroyBannerAd();
    public abstract void RequestAndLoadRewardedAd();
    public abstract void ShowRewardedAd();
    public abstract void RequestAndLoadInterstitialAd();
    public abstract void ShowInterstitialAd();
    public abstract void DestroyInterstitialAd();
    public abstract void RequestAndLoadRewardedInterstitialAd();
    public abstract void ShowRewardedInterstitialAd();
}