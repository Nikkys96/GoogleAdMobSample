using System;
using UnityEngine;

namespace Scripts.Advertising
{
    public enum AdType
    {
        Skippable,
        Rewarded,
        Offerwall
    }
    
    public abstract class AdvertisingServiceBase : MonoBehaviour
    {
        protected Action onSuccessfulInit;
        protected Action onRewardedSuccess;
        
        public abstract bool IsSupportAndReady(AdType adType);
        public string Name { get; protected set; }

        public virtual void Init(Action callback)
        {
            onSuccessfulInit = callback;
        }

        public virtual void Show(Action callback)
        {
            onRewardedSuccess = callback;
        }

        public virtual void ShowSkipping(Action callback)
        {
            onRewardedSuccess = callback;
        }
    }
}