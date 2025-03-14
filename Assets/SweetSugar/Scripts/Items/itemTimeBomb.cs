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

using System.Linq;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SweetSugar.Scripts.Items
{
    /// <summary>
    /// Time bomb item
    /// </summary>
    public class itemTimeBomb :Item, IItemInterface
    {
        [SerializeField] private bool ActivateByExplosion;
        [SerializeField] private bool StaticOnStart;
        public int timer;
        public int startTimer = 5;
        public TextMeshProUGUI timerLabel;
        [SerializeField] public GameObject sprite;
        public GameObject explosion;

        public void Destroy(Item item1, Item item2)
        {
                GetItem.square.DestroyBlock();
            if (GetItem.square.type == SquareTypes.WireBlock)
            {
                GetItem.StopDestroy();
            }
            SoundBase.Instance
                .PlayOneShot(SoundBase.Instance.explosion);
            Instantiate(explosion, transform.position, Quaternion.identity);
            DestroyBehaviour();
        }
        
        private new Item GetItem => GetComponentInParent<Item>();

        public void OnEnable()
        {
            // Old Code
            /*
              if (LevelManager.THIS.gameStatus == GameState.PrepareGame)
            {
                startTimer = LevelManager.THIS.field.fieldData.bombTimer;
                timer = startTimer;
            }
             */
            
            
            //added_feature
            {
                startTimer = LevelManager.THIS.field?.fieldData.bombTimer?? 15;
                if (startTimer < 10)
                    startTimer = 10;
                timer = startTimer;
                // Debug.Log(timer + " : " + name);
            }
            
            
            LevelManager.OnTurnEnd += OnTurnEnd;
            InitItem();
        }

        public override void InitItem()
        {
            timerLabel.color = new Color(73f/255f,73f/255f,73f/255f);
            // sprite.GetComponent<SpriteRenderer>().sortingOrder = 2;
            GetComponentInChildren<Canvas>().enabled = true;
            sprite.SetActive(true);
            base.InitItem();
            UpdateTimer();
        }

        private void OnDisable()
        {
            LevelManager.OnTurnEnd -= OnTurnEnd;
        }

        private void OnTurnEnd()
        {
            timer--;
            // if(timer == 1) Warning();
            if (timer <= 0)
            {
                timer = 0;
                // LevelManager.THIS.gameStatus = GameState.BombFailed;
           
            }
            UpdateTimer();
        }

        private void Warning()
        {
            timerLabel.color = Color.red;
            var seq = LeanTween.Framework.LeanTween.sequence();
            float t = 0.3f;
            seq.append(LeanTween.Framework.LeanTween.scale(timerLabel.gameObject, Vector3.one * 1.2f, t));
            seq.append(LeanTween.Framework.LeanTween.scale(timerLabel.gameObject, Vector3.one, t));
            seq.append(LeanTween.Framework.LeanTween.scale(timerLabel.gameObject, Vector3.one * 1.2f, t));
            seq.append(LeanTween.Framework.LeanTween.scale(timerLabel.gameObject, Vector3.one, t));
            SoundBase.Instance.PlayLimitSound(SoundBase.Instance.timeOut);
        }

        public UnityAction OnExlodeAnimationFinished;
        public void ExlodeAnimation(bool hide, UnityAction callback)
        {
            LevelManager.THIS.levelData.limit = 0;

            if(callback == null)
                callback = OnExlodeAnimationFinished;
            anim.enabled = false;
            GetComponentInChildren<Canvas>().enabled = false;
            GetComponent<BombFailedAnimation>().BombFailed(Vector3.zero, 10, hide, callback);
        }

        void UpdateTimer()
        {
            timerLabel.text = timer.ToString();
        }

        public GameObject GetGameobject()
        {
            return gameObject;
        }

        public bool IsCombinable()
        {
            return Combinable;
        }

        public bool IsExplodable()
        {
            return ActivateByExplosion;
        }

        public void SetExplodable(bool setExplodable)
        {
            ActivateByExplosion = setExplodable;
        }

        public bool IsStaticOnStart()
        {
            return StaticOnStart;
        }

        public void SetOrder(int i)
        {
            var spriteRenderers = GetSpriteRenderers();
            var orderedEnumerable = spriteRenderers.OrderBy(x => x.sortingOrder).ToArray();
            for (int index = 0; index < orderedEnumerable.Length; index++)
            {
                var spr = orderedEnumerable[index];
                spr.sortingOrder = i + index;
            }
        }

        public Item GetParentItem()
        {
            return transform.GetComponentInParent<Item>();
        }
        
        
    }
}