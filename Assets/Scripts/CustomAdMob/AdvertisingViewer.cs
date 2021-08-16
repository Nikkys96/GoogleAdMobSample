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

        public bool IsAdReady(AdType adType) =>
            IsEditor() || _currentService != null && _currentService.IsSupportAndReady(adType);

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
            adService.Init(SuccessCalback);
        }

        private void SuccessCalback()
        {
            Debug.Log($"AdvertisingViewer: Сервис инициализирован");
            OnSuccessInit?.Invoke();
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

            Debug.Log($"AdvertisingViewer: currentService.Name: {_currentService.Name}");
            if (_currentService.IsSupportAndReady(adType))
            {
                switch (adType)
                {
                    case AdType.Skippable:
                        _currentService.ShowSkipping(successCallback);
                        break;
                    case AdType.Rewarded:
                        _currentService.Show(successCallback);
                        break;
                }
            }
            else
            {
                Debug.LogError($"AdvertisingViewer: currentService.Name {_currentService.Name} is not ready");
                exceptionCallback?.Invoke();
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