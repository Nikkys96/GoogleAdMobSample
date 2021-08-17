using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Scripts.Advertising
{
    public class AdvertisingViewer : MonoBehaviour
    {
        private const int TIME_FOR_SERVICE = 6;

        public event Action OnSuccessInit;

        public static AdvertisingViewer Singleton { get; private set; }

        private AdvertisingServiceBase _currentService;

        private void Awake()
        {
            Singleton = this;
            StartInit();
        }

        public void StartInit()
        {
            _currentService = gameObject.AddComponent<CustomAdMobController>();
            InitServices();
        }

        private void InitServices()
        {
            if (IsEditor())
            {
                Debug.Log($"AdvertisingViewer: Ads in the editor disabled");
                OnSuccessInit?.Invoke();
                return;
            }

            Debug.Log($"AdvertisingViewer: InitServices: {_currentService.name}");

            InitService(_currentService);
        }
        

        private void InitService(AdvertisingServiceBase adService)
        {
            Debug.Log($"AdvertisingViewer: InitService service: {adService.name}");
            adService.Init();
        }

        public void ShowSkipping(Action successCallback, Action exceptionCallback = null) =>
            ShowAdvertising(successCallback, exceptionCallback, AdType.Skippable);

        public void Show(Action successCallback, Action exceptionCallback = null) =>
            ShowAdvertising(successCallback, exceptionCallback, AdType.Rewarded);

        private void ShowAdvertising(Action successCallback, Action exceptionCallback, AdType adType)
        {
            if (exceptionCallback == null)
            {
                exceptionCallback = ShowExceptionsMsg;
            }

            if (IsEditor())
            {
                Debug.Log($"AdvertisingViewer: Ads in the editor disabled");
                successCallback?.Invoke();
                return;
            }

            if (!IsServiceValidated())
            {
                exceptionCallback?.Invoke();
                return;
            }

            switch (adType)
            {
                case AdType.Skippable:
                    _currentService.ShowInterstitialAd();
                    break;
                case AdType.Rewarded:
                    _currentService.ShowRewardedAd();
                    break;
            }
        }

        private bool IsServiceValidated()
        {
            bool isHaveService = _currentService != null;

            if (!isHaveService)
            {
                Debug.LogWarning("AdvertisingViewer: Is'n Have Service");
            }

            return isHaveService;
        }

        private void ShowExceptionsMsg()
        {
            Debug.LogError("\"ShowExeptionsMsg\"");
        }

        private bool IsEditor()
        {
            return Application.platform == RuntimePlatform.WindowsEditor;
        }
    }
}