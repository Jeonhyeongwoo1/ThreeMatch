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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SweetSugar.Scripts.Core;
using SweetSugar.Scripts.Items;
using UnityEngine;

namespace SweetSugar.Scripts.System
{
    public class WaitWhileDestroyPipeline : CustomYieldInstruction
    {
        private bool currentDestroyFinished;

        public override bool keepWaiting => !currentDestroyFinished;

        public WaitWhileDestroyPipeline(List<Item> destroyItems, Delays delays)
        {
            // Debug.Log(this.GetType());
            DestroyingPipeline.THIS.DestroyItems(destroyItems, delays, () =>
            {
                currentDestroyFinished = true;
                //            LevelManager.This.FindMatches();
            });
        }
    }

    public class WaitWhileFall : CustomYieldInstruction
    {
        private List<Item> items;
        public override bool keepWaiting
        {
            get
            {
                var ii = items.WhereNotNull().Where(i=>i.gameObject.activeSelf).Any(i => i.falling);
                // if (!ii)
                // 	Debug.Log("Fall finished " + this.GetHashCode() + ii);
//            GenerateAndFall(true);

                return ii;
            }
        }

        public WaitWhileFall(bool generateNewItems = true)
        {
            GenerateAndFall(generateNewItems);
        }

        private void GenerateAndFall(bool generateNewItems)
        {
            items = LevelManager.THIS.field.GetItems(false, null, false);
        }
    }

    public class WaitWhileCollect : CustomYieldInstruction
    {
        private AnimateItems[] items;
        public override bool keepWaiting
        {
            get
            {
                var ii = items.WhereNotNull().Any();
                return ii;
            }
        }

        public WaitWhileCollect()
        {
            items = LevelManager.THIS.animateItems.Where(i=>i.target).ToArray();
        }
    }

    public class WaitWhileFallSide : CustomYieldInstruction
    {
        private List<Item> items;
        public override bool keepWaiting
        {
            get
            {
                var ii = items.WhereNotNull().Where(i=>i.gameObject.activeSelf).Any(i => i.falling);
                var squares = LevelManager.THIS.field.squaresArray.Where(i=>i.isEnterPoint && i.IsFree() && i.Item == null).ToArray();
                if (ii && squares.Any())
                {
                    squares.ForEachY(i => i.GenItem());
                    FallSide();
                }
                return ii;
            }
        }

        public WaitWhileFallSide()
        {
            FallSide();
        }

        private void FallSide()
        {
            items = LevelManager.THIS.field.GetItems(false, null, false).Where(i => i.square).OrderBy(i => i.square.orderInSequence).ToList();
            items.Where(i => !i.destroying && !i.falling && !i.JustCreatedItem).ToList().ForEach(i => i.CheckNearEmptySquares());
        }
    }

    public class WaitForListNull : CustomYieldInstruction
    {
        private List<object> items;
        public override bool keepWaiting => items.AllNull();

        public WaitForListNull(List<object> items_)
        {
            items = items_;
        }
    }

    public class WaitWhileDestroying : CustomYieldInstruction
    {
        private float startTime;
        private List<Item> items;
        public override bool keepWaiting
        {
            get
            {
                if (startTime + 1 < Time.time)
                    items.Where(i => i && i.destroying && i.gameObject.activeSelf).ForEachY(i =>
                    {
                        i.destroying = false;
                        i.DestroyItem();
                    });
                items = items.Where(i => i && i.gameObject.activeSelf).ToList();
                return items.Any(i => i.destroying/* || i.GetComponent<ItemDestroy>() != null*/); //|| !LevelManager.This.destroyAnywayMutlicolor.AllNull();
            }
        }

        public WaitWhileDestroying()
        {
            // Debug.Log(this.GetType());
            startTime = Time.time;
            items = LevelManager.THIS.field.GetItems(false, null, false);
        }
    }

    public class WaitForNextMove : CustomYieldInstruction
    {
        bool nextMove;
        public override bool keepWaiting
        {
            get
            {
                if (nextMove)
                {
                    LevelManager.OnTurnEnd -= OnTurnEnd;
                    return false;
                }

                return true;
            }
        }
        void OnTurnEnd()
        {
            nextMove = true;
        }

        public WaitForNextMove()
        {
            LevelManager.OnTurnEnd += OnTurnEnd;
        }
    }

    public class WaitForSubLevelChange : CustomYieldInstruction
    {
        bool nextMove;
        public override bool keepWaiting
        {
            get
            {
                if (nextMove)
                {
                    LevelManager.OnSublevelChanged -= OnSublevelChanged;
                    return false;
                }

                return true;
            }
        }
        void OnSublevelChanged()
        {
            nextMove = true;
        }

        public WaitForSubLevelChange()
        {
            LevelManager.OnSublevelChanged += OnSublevelChanged;
        }
    }
    public class WaitWhileArrayNotNull : CustomYieldInstruction
    {
        object[] items;
        public override bool keepWaiting => !items.AllNull();

        public WaitWhileArrayNotNull(object[] array)
        {
            //Debug.Log(this.GetType());

            items = array;
        }
    }

    public class WaitWhileAnimations : CustomYieldInstruction
    {
        private bool animationFinished;

        public override bool keepWaiting => !animationFinished;

        public WaitWhileAnimations()
        {
            // Debug.Log(this.GetType());

            StartCoroutine(DestroyAnimatedItems());
        }

        IEnumerator DestroyAnimatedItems()
        {
            var destroyingItems = LevelManager.THIS.field.GetDestroyingItems();
            var i = 0;
            foreach (var item in destroyingItems)
            {
                animationFinished = false;
                item.SecondPartDestroyAnimation(() =>
                {
                    StartCoroutine(SetupFalling());
                    i++;
                    if (i == destroyingItems.Count())
                        animationFinished = true;
                });
                //yield return new WaitUntil(() => animationFinished);
                yield return new WaitForEndOfFrame();
            }
            if (i == destroyingItems.Count())
                animationFinished = true;
        }

        IEnumerator SetupFalling()
        {
            //LevelManager.This.SetupFalling();
            yield return new WaitWhileFall();
            animationFinished = true;
        }

        void StartCoroutine(IEnumerator ienumerator)
        {
            LevelManager.THIS.StartCoroutine(ienumerator);
        }

    }

    public class WaitForSecCustom : CustomYieldInstruction
    {
        private bool stopWait;
        public float s;

        public override bool keepWaiting => !stopWait;

        public WaitForSecCustom()
        {
            //Debug.Log(this.GetType());
            StartCoroutine(WaitForSecCustomCor(s));
        }

        IEnumerator WaitForSecCustomCor(float sec)
        {
            yield return new WaitForSeconds(sec);

        }
        void StartCoroutine(IEnumerator ienumerator)
        {
            LevelManager.THIS.StartCoroutine(ienumerator);
            stopWait = true;
        }
    }
}