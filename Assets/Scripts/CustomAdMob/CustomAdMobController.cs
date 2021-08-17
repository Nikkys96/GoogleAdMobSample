using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

public class CustomAdMobController : AdvertisingServiceBase
{
    private readonly string AD_INTERSTIAL_ID =
#if UNITY_EDITOR
            "unused";
#elif UNITY_ANDROID
                "ca-app-pub-5343588632125776/2742226915";
#elif UNITY_IOS
                "ca-app-pub-5343588632125776/9508611519"; 
#else
                "unexpected_platform";
#endif

    private readonly string AD_REWARD_ID =
#if UNITY_EDITOR
            "unused";
#elif UNITY_ANDROID
                "ca-app-pub-5343588632125776/8593319390";
#elif UNITY_IOS
                "ca-app-pub-5343588632125776/7074019868";
#else
                "unexpected_platform";
#endif

    public static Action OnAdsLoad;

    private bool _isInitialized;
    private InterstitialAd _interstitialAd;
    private bool _isInterstitialLoading;
    private bool _isRewardedLoading;
    private RewardedAd _rewardedAd;
    private bool _isInterstitialWatched;
    private bool _isRewardedWatched;

    public override void Init()
    {
        MobileAds.SetiOSAppPauseOnBackground(true);

        List<string> deviceIds = new List<string>() { AdRequest.TestDeviceSimulator };

#if UNITY_IOS
        deviceIds.Add("96e23e80653bb28980d3f40beb58915c");
#elif UNITY_ANDROID
        deviceIds.Add("CA81CE9F6BC3246FDF38F08018A6E5AD");
        deviceIds.Add("00a479e1-2cd2-4930-9058-864122a63d52");
#endif

        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder()
                .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
                .SetTestDeviceIds(deviceIds)
                .build();

        MobileAds.SetRequestConfiguration(requestConfiguration);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(HandleInitCompleteAction);
    }

    private void HandleInitCompleteAction(InitializationStatus initStatus)
    {
        ExecuteInMainThread(() =>
        { 
            Debug.LogError("init adapters success");
            _isInitialized = true;

            RequestAndLoadInterstitialAd();
            RequestAndLoadRewardedAd();
        });
    }

    public override void RequestAndLoadInterstitialAd()
    {
        if (_isInterstitialLoading || !_isInitialized)
        {
            Debug.LogError(
                $"AdMobManager::RequestAndLoadInterstitialAd return state _isInterstitialLoading {_isInterstitialLoading}; _isInitialized {_isInitialized}");
            return;
        }

        // Clean up interstitial before using it
        _interstitialAd?.Destroy();

        _interstitialAd = new InterstitialAd(AD_INTERSTIAL_ID);

        // Load an interstitial ad
        _interstitialAd.OnAdFailedToLoad += (sender, args) =>
        {
            ExecuteInMainThread(() =>
            {
                Debug.LogError(
                    $"AdMobManager::RequestAndLoadInterstitialAd::OnAdFailedToLoad message {args.LoadAdError.GetMessage()}");
                _isInterstitialLoading = false;
            });
        };
        _interstitialAd.OnAdLoaded += (sender, args) =>
            ExecuteInMainThread(() => _isInterstitialLoading = false);

        _isInterstitialLoading = true;
        _isInterstitialWatched = false;

        _interstitialAd.LoadAd(CreateAdRequest());
    }

    public override void RequestAndLoadRewardedAd()
    {
        if (_isRewardedLoading || !_isInitialized)
        {
            Debug.LogError(
                $"AdMobManager::RequestAndLoadRewardedAd return state _isRewardedLoading {_isRewardedLoading}; _isInitialized {_isInitialized}");
            return;
        }

        _rewardedAd = new RewardedAd(AD_REWARD_ID);

        _rewardedAd.OnAdFailedToLoad += (sender, args) =>
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.LogError(
                    $"AdMobManager::RequestAndLoadRewardedAd::OnAdFailedToLoad message {args.LoadAdError.GetMessage()}");
                _isRewardedLoading = false;
                OnAdsLoad?.Invoke();
            });

        _rewardedAd.OnAdLoaded += (sender, args) =>
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                _isRewardedLoading = false;
                OnAdsLoad?.Invoke();
            });

        _isRewardedLoading = true;
        _isRewardedWatched = false;

        _rewardedAd.LoadAd(CreateAdRequest());
    }

    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .Build();
    }

    public override void ShowRewardedAd()
    {
        if (_rewardedAd == null || !_rewardedAd.IsLoaded() || _isRewardedWatched)
        {
            return;
        }

        _rewardedAd.OnUserEarnedReward += (sender, args) =>
        {
            ExecuteInMainThread(() =>
            {
                Debug.Log("AdMobManager::Show OnUserEarnedReward");
                Invoke(nameof(RequestAndLoadRewardedAd), 3f);
            });
        };
        _rewardedAd.OnAdClosed += (sender, args) =>
        {
            ExecuteInMainThread(() =>
            {
                Debug.Log("AdMobManager::Show OnAdClosed");
            });
        };

        _isRewardedWatched = true;
        _rewardedAd.Show();
    }

    public override void ShowInterstitialAd()
    {
        if (_interstitialAd == null || !_interstitialAd.IsLoaded() || _isInterstitialWatched)
        {
            return;
        }

        _interstitialAd.OnAdClosed += (sender, args) =>
        {
            ExecuteInMainThread(() =>
            {
                Debug.Log("AdMobManager::ShowSkipping OnAdClosed");
                Invoke(nameof(RequestAndLoadInterstitialAd), 3f);
            });
        };

        _isInterstitialWatched = true;
        _interstitialAd.Show();
    }

    private void ExecuteInMainThread(Action execute) => MobileAdsEventExecutor.ExecuteInUpdate(() => { execute?.Invoke(); });

    public override void RequestBannerAd()
    {
        throw new NotImplementedException();
    }

    public override void DestroyBannerAd()
    {
        throw new NotImplementedException();
    }

    public override void DestroyInterstitialAd()
    {
        throw new NotImplementedException();
    }

    public override void RequestAndLoadRewardedInterstitialAd()
    {
        throw new NotImplementedException();
    }

    public override void ShowRewardedInterstitialAd()
    {
        throw new NotImplementedException();
    }
}