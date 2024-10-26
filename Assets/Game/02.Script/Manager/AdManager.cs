using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThreeMatch.Manager
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AdManager>();
                }

                return _instance;
            }
        }

        private static AdManager _instance;

        private InterstitialAd interstitial;
        
        // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
        private const string _sampleInterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField] private string _interstitialAdUnitId = "";
#elif UNITY_IPHONE
        private const string _sampleInterstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
        private const string _sampleInterstitialAdUnitId = "unused";
#endif

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            MobileAds.Initialize(initStatus => { });
            LoadInterstitialAd();
            StartCoroutine(LoadRewardAdCor());
            // StartCoroutine(ShowInterstitialAdCor());
        }

        [Button]
        public void Debug_ShowInterstitialAd()
        {
            ShowInterstitialAd();
        }
        //
        // [Button]
        // public void Debug_ShowRewardAd()
        // {
        //     ShowRewardAd();
        // }

        #region InterstitialAds
  /// <summary>
        /// UI element activated when an ad is ready to show.
        /// </summary>
        public GameObject AdLoadedStatus;

        private InterstitialAd _interstitialAd;

        private IEnumerator ShowInterstitialAdCor()
        {
            while (true)
            {
                if (_interstitialAd == null || !_interstitialAd.CanShowAd())
                {
                    LoadInterstitialAd();
                }
                
                yield return new WaitForSeconds(120);
                
                ShowInterstitialAd();
            }
        }

        /// <summary>
        /// Loads the ad.
        /// </summary>
        public void LoadInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            // if (_interstitialAd != null)
            // {
            //     DestroyInterstitialAd();
            // }

            Debug.Log("Loading interstitial ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            string id = _useSampleUnitId ? _sampleInterstitialAdUnitId : _interstitialAdUnitId;
            // Send the request to load the ad.
            InterstitialAd.Load(id, adRequest, (InterstitialAd ad, LoadAdError error) =>
            {
                // If the operation failed with a reason.
                if (error != null)
                {
                    Debug.LogError("Interstitial ad failed to load an ad with error : " + error);
                    return;
                }
                // If the operation failed for unknown reasons.
                // This is an unexpected error, please report this bug if it happens.
                if (ad == null)
                {
                    Debug.LogError("Unexpected error: Interstitial load event fired with null ad and null error.");
                    return;
                }

                // The operation completed successfully.
                Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                _interstitialAd = ad;

                // Register to ad events to extend functionality.
                RegisterInterstitialAdEventHandlers(ad);

                // Inform the UI that the ad is ready.
                // AdLoadedStatus?.SetActive(true);
            });
        }

        /// <summary>
        /// Shows the ad.
        /// </summary>
        public void ShowInterstitialAd()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                _interstitialAd.Show();
            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
            }

            // Inform the UI that the ad is not ready.
            // AdLoadedStatus?.SetActive(false);
        }

        /// <summary>
        /// Destroys the ad.
        /// </summary>
        public void DestroyInterstitialAd()
        {
            if (_interstitialAd != null)
            {
                Debug.Log("Destroying interstitial ad.");
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            // Inform the UI that the ad is not ready.
            // AdLoadedStatus?.SetActive(false);
        }

        /// <summary>
        /// Logs the ResponseInfo.
        /// </summary>
        public void LogResponseInfo()
        {
            if (_interstitialAd != null)
            {
                var responseInfo = _interstitialAd.GetResponseInfo();
                UnityEngine.Debug.Log(responseInfo);
            }
        }

        private void RegisterInterstitialAdEventHandlers(InterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content with error : "
                    + error);
            };
        }
        
#endregion InterstitialAds

#region Rewarded Ads

        public GameObject rewardAdLoadedStatus;

        // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
        private const string _sampleRewardAdUnitId = "ca-app-pub-3940256099942544/5224354917";
        [SerializeField] private string _rewardAdUnitId = "";
#elif UNITY_IPHONE
        private const string _sampleRewardAdUnitId = "ca-app-pub-3940256099942544/1712485313";
        [SerializeField] private string _rewardAdUnitId = "";
#else
        private const string _sampleRewardAdUnitId = "unused";
        [SerializeField] private string _rewardAdUnitId = "";
#endif

        [SerializeField] private bool _useSampleUnitId = true;

        private RewardedAd _rewardedAd;

        private IEnumerator LoadRewardAdCor()
        {
            while (true)
            {
                if (_rewardedAd == null || !_rewardedAd.CanShowAd())
                {
                    LoadRewardAd();
                }

                yield return new WaitForSeconds(1f);
            }
        }
        
        /// <summary>
        /// Loads the ad.
        /// </summary>
        public void LoadRewardAd()
        {
            // Clean up the old ad before loading a new one.
            if (_rewardedAd != null)
            {
                DestroyRewardAd();
            }

            Debug.Log("Loading rewarded ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            string id = _useSampleUnitId ? _sampleRewardAdUnitId : _rewardAdUnitId;
            // Send the request to load the ad.
            RewardedAd.Load(id, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                // If the operation failed with a reason.
                if (error != null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                    return;
                }
                // If the operation failed for unknown reasons.
                // This is an unexpected error, please report this bug if it happens.
                if (ad == null)
                {
                    Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
                    return;
                }

                // The operation completed successfully.
                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
                _rewardedAd = ad;

                // Register to ad events to extend functionality.
                RegisterEventHandlers(ad);

                // Inform the UI that the ad is ready.
                // rewardAdLoadedStatus?.SetActive(true);
            });
        }

        private Action onSuccessCallback;
        private Action onFailCallback;

        /// <summary>
        /// Shows the ad.
        /// </summary>
        public void ShowRewardAd(Action successCallback, Action failCallback)
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                onSuccessCallback = successCallback;
                onFailCallback = failCallback;
                Debug.Log("Showing rewarded ad.");
                _rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
                                            reward.Amount,
                                            reward.Type));
                    
                });
            }
            else
            {
                Debug.LogError("Rewarded ad is not ready yet.");
                DestroyRewardAd();
            }

            // Inform the UI that the ad is not ready.
            // rewardAdLoadedStatus?.SetActive(false);
        }

        /// <summary>
        /// Destroys the ad.
        /// </summary>
        public void DestroyRewardAd()
        {
            if (_rewardedAd != null)
            {
                Debug.Log("Destroying rewarded ad.");
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            // Inform the UI that the ad is not ready.
            // rewardAdLoadedStatus?.SetActive(false);
        }

        /// <summary>
        /// Logs the ResponseInfo.
        /// </summary>
        public void LogRewardAdResponseInfo()
        {
            if (_rewardedAd != null)
            {
                var responseInfo = _rewardedAd.GetResponseInfo();
                UnityEngine.Debug.Log(responseInfo);
            }
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when the ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
                onSuccessCallback?.Invoke();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content with error : "
                    + error);
                onFailCallback?.Invoke();
            };
        }

#endregion
        
    }
}