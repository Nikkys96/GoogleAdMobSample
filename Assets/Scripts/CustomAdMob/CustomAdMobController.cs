using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace Scripts.Advertising
{
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

        public override void Init(Action callback)
        {
            base.Init(callback);
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
                StartCoroutine(InitAdapters(initStatus));
            });
        }

        private IEnumerator InitAdapters(InitializationStatus initStatus)
        {
            Dictionary<string, AdapterStatus> adapters = initStatus.getAdapterStatusMap();
            bool isAllNetworksReady = false;

            while (!isAllNetworksReady)
            {
                isAllNetworksReady = true;
                Debug.LogError("AdMobManager::InitAdapters cycle, round start");
                yield return new WaitForSeconds(1f);

                foreach (KeyValuePair<string, AdapterStatus> keyValuePair in adapters)
                {
                    string className = keyValuePair.Key;
                    AdapterStatus status = keyValuePair.Value;

                    Debug.LogError(
                        $"AdMobManager::WaitAllNetworkReady Class {className} {status.InitializationState} (description {status.Description})");

                    if (status.InitializationState == AdapterState.NotReady)
                    {
                        isAllNetworksReady = false;
                        break;
                    }
                }
            }

            Debug.LogWarning("AdMobManager::InitAdapters onSuccessfulInit");

            _isInitialized = true;
            onSuccessfulInit?.Invoke();

            RequestAndLoadInterstitialAd();
            RequestAndLoadRewardedAd();
        }

        private AdRequest CreateAdRequest()
        {
            return new AdRequest.Builder()
                .Build();
        }

        private void RequestAndLoadInterstitialAd()
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
            Debug.LogError("AdMobManager::RequestAndLoadInterstitialAd ad requested");
        }

        private void RequestAndLoadRewardedAd()
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
            Debug.LogError("AdMobManager::RequestAndLoadRewardedAd ad requested");
        }

        public override bool IsSupportAndReady(AdType adType)
        {
            bool isReady = false;

#if UNITY_WEBGL
            return isReady;
#endif
            Debug.LogWarning($"AdMobManager::IsSupportAndReady adType {adType}");
            switch (adType)
            {
                case AdType.Skippable:
                    {
                        isReady = _interstitialAd != null && _interstitialAd.IsLoaded();
                        Debug.LogWarning(
                            $"AdMobManager::IsSupportAndReady interstitialAd != null {_interstitialAd != null}; interstitialAd.IsLoaded() {_interstitialAd.IsLoaded()}; _isInterstitialLoading {_isInterstitialLoading}");
                        if (!isReady)
                        {
                            RequestAndLoadInterstitialAd();
                        }

                        break;
                    }
                case AdType.Rewarded:
                    {
                        isReady = _rewardedAd != null && _rewardedAd.IsLoaded();
                        Debug.LogWarning($"AdMobManager::IsSupportAndReady rewardedAd != null {_rewardedAd != null};");
                        Debug.LogWarning($"rewardedAd.IsLoaded() {_rewardedAd?.IsLoaded()}");
                        Debug.LogWarning($"_isRewardedLoading {_isRewardedLoading}");

                        if (!isReady)
                        {
                            RequestAndLoadRewardedAd();
                        }
                        break;
                    }
            }

            return isReady;
        }

        public override void Show(Action callback)
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
                    callback?.Invoke();
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

            Debug.Log("AdMobManager::Show");
            _isRewardedWatched = true;
            _rewardedAd.Show();
        }

        public override void ShowSkipping(Action callback)
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
                    callback();
                    Invoke(nameof(RequestAndLoadInterstitialAd), 3f);
                });
            };

            _isInterstitialWatched = true;
            _interstitialAd.Show();
        }

        private void ExecuteInMainThread(Action execute) => MobileAdsEventExecutor.ExecuteInUpdate(() => { execute?.Invoke(); });
    }
}