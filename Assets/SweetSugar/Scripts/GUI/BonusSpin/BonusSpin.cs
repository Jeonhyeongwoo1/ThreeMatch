﻿// // ©2015 - 2024 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System.Collections;
using System.Linq;
using SweetSugar.Scripts.AdsEvents;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.GUI.Boost;
using SweetSugar.Scripts.Localization;
using SweetSugar.Scripts.System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SweetSugar.Scripts.GUI.BonusSpin
{
    /// <summary>
    /// Bonus spin manager. 
    /// </summary>
    public class BonusSpin : MonoBehaviour
    {
        public GameObject wheel;
        private bool spin;
        private bool stopspin;
        public GameObject coins;
        public RewardWheel[] boosts;
        Rigidbody2D rb;
        public float velocity = -3000;
        public float stoptime = 3;
        public GameObject rewardWindow;
        public TextMeshProUGUI priceButton;
        [Header("Prices range for first and seconds spins")]
        public int[] spinPrice;

    public GameObject closeButton;
    public UnityEvent OnSpin;
    void OnEnable()
    {
        transform.Find("Image/BuyPlay").GetComponent<Button>().interactable = true;
        spin = false;
        stopspin = false;
        closeButton.GetComponent<Button>().interactable = true;
        var i =  Mathf.Clamp( PlayerPrefs.GetInt("Spinned", 0),0,spinPrice.Length-1);
        if(i>0)
        {
            priceButton.text = "" + spinPrice[i];
            coins.SetActive(true);
        }
        else
        {
            priceButton.text = LocalizationManager.GetText(82, "Free");
            coins.SetActive(false);

            }
        
    }
    /// <summary>
    /// Purchasing of one spin
    /// </summary>
    public void BuyStartSpin()
    {
        transform.Find("Image/BuyPlay").GetComponent<Button>().interactable = false;
        if (priceButton.text == LocalizationManager.GetText(82, "Free"))
        {
#if UNITY_ADS
            AdsManager.OnRewardedShown += () =>
            {
                StartSpin();
            };
#else
                StartSpin();
#endif
            InitScript.Instance.currentReward = RewardsType.FreeAction;
            AdsManager.THIS.ShowRewardedAds();
         
            return;
        }
        if (InitScript.Gems >= int.Parse(priceButton.text))
        {
            InitScript.Instance.SpendGems(int.Parse(priceButton.text));
            StartSpin();
        }
        else
        {
            transform.Find("Image/BuyPlay").GetComponent<Button>().interactable = true;
            MenuReference.THIS.GemsShop.gameObject.SetActive(true);
        }
    }
        public void StartSpin()
        {
            if (!spin && !stopspin)
            {
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(Spinning());
                }
            }
        }

        IEnumerator Spinning()
        {
            closeButton.GetComponent<Button>().interactable = false;
            PlayerPrefs.SetInt("Spinned", PlayerPrefs.GetInt("Spinned", 0)+1);
            spin = true;
            rb = wheel.GetComponent<Rigidbody2D>();
            rb.angularVelocity = velocity;
            yield return new WaitForSeconds(stoptime + Random.Range(0, 2f));
            spin = false;
            stopspin = true;
            yield return new WaitUntil(() => rb.angularVelocity != 0);
            yield return new WaitUntil(() => rb.angularVelocity != 0);
            yield return new WaitForSeconds(3);
            stopspin = false;

            CheckSpin();
            OnSpin?.Invoke();
            gameObject.SetActive(false);

        }

        void FixedUpdate()
        {
            if (spin)
                rb.angularVelocity = velocity;
            else if (stopspin && rb.angularVelocity < 0)
                rb.angularVelocity += Time.fixedDeltaTime;

        }
/// <summary>
/// Check getting bonus
/// </summary>
        private void CheckSpin()
        {
            var boost = boosts.OrderByDescending(i => i.transform.position.x).OrderByDescending(i => i.transform.position.y).First();
            if (boost.type != BoostType.None)
                InitScript.Instance.BuyBoost(boost.type, 0, boost.count);
            else
                InitScript.Instance.AddGems(boost.count);
            rewardWindow.SetActive(true);
            rewardWindow.GetComponent<RewardIcon>().SetWheelReward(boost);

        }

    }
}
