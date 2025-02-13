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

using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.System;
using UnityEngine;

namespace SweetSugar.Scripts.GUI
{
    /// <summary>
    /// Spends a life after game started or offers to buy a life
    /// </summary>
    public class GUIUtils : MonoBehaviour
    {
        public DebugSettings DebugSettings;

        public static GUIUtils THIS;

        private void Start()
        {
            DebugSettings = Resources.Load<DebugSettings>("Scriptable/DebugSettings");
            if (!Equals(THIS, this)) THIS = this;
        }

        public void StartGame()
        {
            if (InitScript.lifes > 0 || DebugSettings.AI)
            {
                InitScript.Instance.SpendLife(1);
                LevelManager.THIS.gameStatus = GameState.PrepareGame;
            }
            else
            {
                BuyLifeShop();
            }

        }

        public void BuyLifeShop()
        {

            if (InitScript.lifes < InitScript.Instance.CapOfLife)
                MenuReference.THIS.LiveShop.gameObject.SetActive(true);

        }
    }
}