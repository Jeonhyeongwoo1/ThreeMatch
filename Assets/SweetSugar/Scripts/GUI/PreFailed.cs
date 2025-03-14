// // ©2015 - 2024 Candy Smith
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

using System;
using System.Linq;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.Items;
using SweetSugar.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSugar.Scripts.GUI
{
    /// <summary>
    /// Pre failed menu
    /// </summary>
    public class PreFailed : MonoBehaviour
    {
        public Sprite spriteTime;
        public GameObject[] objects;

        /// <summary>
        /// Initialization
        /// </summary>
        public void SetFailed()
        {
            objects[0].SetActive(true);
            if(LevelManager.THIS.levelData.limitType == LIMIT.TIME)
                objects[0].GetComponent<Image>().sprite = spriteTime;
            objects[1].SetActive(false);
        }

        public void SetBombFailed()
        {
            objects[1].SetActive(true);
            objects[0].SetActive(false);
        }
        
        /// <summary>
        /// Continue the game after choose a variant
        /// </summary>
        public void Continue()
        {
            if(IsFail())
            {
                ContinueFailed();
                ContinueBomb();
            }
            else ContinueBomb();
            AnimAction(() => LevelManager.THIS.gameStatus = GameState.Playing);
        }

        /// <summary>
        /// Further animation and game over
        /// </summary>
        public void Close()
        {  
            var timeBombs = FindObjectsOfType<itemTimeBomb>().Where(i=>i.timer<=0);
            if(timeBombs.Count() > 0){
            timeBombs.NextRandom().OnExlodeAnimationFinished += () => LevelManager.THIS.gameStatus = GameState.GameOver;
            AnimAction(() =>
            {
                for (var index = 0; index < timeBombs.Count(); index++)
                {
                    var i = timeBombs.ToList()[index];
                    i.ExlodeAnimation(index != 0,null);
                }
            });}
            else AnimAction(()=>LevelManager.THIS.gameStatus = GameState.GameOver);
        }

        void AnimAction( Action call)
        {
            Animation anim = GetComponent<Animation>();
            var animationState = anim["bannerFailed"];
            animationState.speed = 1;
            anim.Play();
            LeanTween.Framework.LeanTween.delayedCall(anim.GetClip("bannerFailed").length - animationState.time, call);
        }

        private bool IsFail() => objects[0].activeSelf;

        void ContinueBomb()
        {
            FindObjectsOfType<itemTimeBomb>().ForEachY(i =>
            {
                i.timer += 5;
                i.InitItem();
            });
        }

        /// <summary>
        /// Continue the game
        /// </summary>
        private void ContinueFailed()
        {
            if (LevelManager.THIS.levelData.limitType == LIMIT.MOVES)
                LevelManager.THIS.levelData.limit += LevelManager.THIS.ExtraFailedMoves;
            else
                LevelManager.THIS.levelData.limit += LevelManager.THIS.ExtraFailedSecs;
        }
    }
}