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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SweetSugar.Scriptable.Rewards;
using SweetSugar.Scripts.AdsEvents;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Effects;
using SweetSugar.Scripts.GUI;
using SweetSugar.Scripts.GUI.Boost;
using SweetSugar.Scripts.Integrations.Network;
using SweetSugar.Scripts.Items;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.MapScripts;
using SweetSugar.Scripts.System;
using SweetSugar.Scripts.System.Combiner;
using SweetSugar.Scripts.System.Orientation;
using SweetSugar.Scripts.TargetScripts.TargetSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace SweetSugar.Scripts.Core
{
//game state enum
    public enum GameState
    {
        Map,
        PrepareGame,
        RegenLevel,
        Tutorial,
        Pause,
        Playing,
        PreFailed,
        GameOver,
        ChangeSubLevel,
        PreWinAnimations,
        Win,
        WaitForPopup,
        WaitAfterClose,
        BlockedGame,
        BombFailed
    }

    /// <summary>
    /// core-game class, using for handle game states, blocking, sync animations and search mathing and map
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager THIS;
        //life shop reference
        public LifeShop lifeShop;
        //true if Unity in-apps is enable and imported
        public bool enableInApps;
        //square width for border placement
        public float squareWidth = 1.2f;
        //item which was dragged recently 
        public Item lastDraggedItem;
        //item which was switched succesfully recently 
        public Item lastSwitchedItem;
        //makes scores visible in the game
        public GameObject popupScore;
        //current game level
        public int currentLevel = 1;
        //current sub-level
        private int currentSubLevel = 1;
        public int CurrentSubLevel
        {
            get { return currentSubLevel; }
            set
            {
                currentSubLevel = value;
                levelData.currentSublevelIndex = currentSubLevel - 1;
            }
        }
        //current field reference
        public FieldBoard field => fieldBoards.Count > 0? fieldBoards[CurrentSubLevel - 1]: null;
        //EDITOR: cost of continue after failing 
        public int FailedCost;
        //EDITOR: moves gived to continue
        public int ExtraFailedMoves = 5;
        //EDITOR: time gived to continue
        public int ExtraFailedSecs = 30;
        //in-app products for purchasing
        public List<GemProduct> gemsProducts = new List<GemProduct>();
        //in-apps product IDs
        public string[] InAppIDs;
        //true if thriving block destroyed on current move
        public bool thrivingBlockDestroyed;
        
        
        //Added_feature
        public GameObject dispenserSpawner;
        public bool thrivingBlockMachine;
        public Dictionary<string, List<Square>> IfanythrivingBlock = new Dictionary<string, List<Square>>();
        private int Res;

        public void GenerateRandom()
        {
            Res = Random.Range(1, IfanythrivingBlock.Count + 1);
        }

        public int Getint { get { return Res; } }

        public string SubScribeChocoSpread()
        {
            string res = "" + (IfanythrivingBlock.Count + 1);

            IfanythrivingBlock.Add(res, new List<Square>());

            return res;
        }
        public void UnSubscribeChocoSpread(string Key)
        {
            if (IfanythrivingBlock.ContainsKey(Key))
                IfanythrivingBlock.Remove(Key);
        }
        
        
        //variables for boost which place bonus candies on the field
        public int BoostColorfullBomb;
        public int BoostPackage;
        public int BoostStriped;
        public int BoostMarmalade;
        //TODO: check sharing path
        public string androidSharingPath;
        public string iosSharingPath;
        //put some marmalade bears to the field on start
        public bool enableMarmalade => levelData.enableMarmalade;
        //empty boost reference for system
        public BoostIcon emptyBoostIcon;
        //debug settings reference
        public DebugSettings DebugSettings;
        //additional gameplay settings reference
        public AdditionalSettings AdditionalSettings;
        //activate boost in game
        public BoostIcon activatedBoost;
        public BoostIcon ActivatedBoost
        {
            get { return activatedBoost == null ? emptyBoostIcon : activatedBoost; }
            set
            {
                if (value == null)
                {
                    if (activatedBoost != null && gameStatus == GameState.Playing)
                    {
                        InitScript.Instance.SpendBoost(activatedBoost.type);
                        UnLockBoosts();
                    }
                }

                //        if (activatedBoost != null) return;
                activatedBoost = value;

                if (value != null)
                {
                    LockBoosts();
                }

                if (activatedBoost == null) return;
                if (activatedBoost.type != BoostType.ExtraMoves && activatedBoost.type != BoostType.ExtraTime) return;
                if (THIS.levelData.limitType == LIMIT.MOVES)
                    THIS.levelData.limit += 5;
                else
                    THIS.levelData.limit += 30;

                ActivatedBoost = null;
            }
        }
        //score gain on this game
        public static int Score;
        //stars gain on this game
        public int stars;
        //striped effect reference
        public GameObject stripesEffect;
        //show popup score on field
        public bool showPopupScores;
        //popup score color
        public Color[] scoresColors;
        //popup score outline
        public Color[] scoresColorsOutline;
        //congratulation words popup reference
        public GameObject[] gratzWords;
        //Level gameobject reference
        public GameObject Level;
        //Gameobject reference
        public GameObject LevelsMap;
        //Gameobject reference
        public GameObject FieldsParent;
        //Gameobject reference
        public GameObject NoMoreMatches;
        //Gameobject reference
        public GameObject CompleteWord;
        //Gameobject reference
        public GameObject FailedWord;
        //in game boost reference
        public BoostIcon[] InGameBoosts;
        //levels passed for the current session
        //reference to orientation handler
        public OrientationGameCameraHandle orientationGameCameraHandle;
        [HideInInspector]public List<AnimateItems> animateItems = new List<AnimateItems>();
        //blocking to drag items for a time
        public bool dragBlocked;
        private Coroutine resetBlockCourotine;
        private float blockTime;
        public bool DragBlocked
        {
            get
            {
                if (dragBlocked && blockTime > 0 && Time.time - blockTime >= 10 && gameStatus == GameState.Playing)
                    dragBlocked = false;
                return dragBlocked;
            }
            set
            {
                if(!value)
                {
                    Item.usedItem = null;
                    Item.usedItemSecond = null;
                }
                dragBlocked = value;
                blockTime = Time.time;
                var moveID = THIS.moveID;
                if (dragBlocked)
                    resetBlockCourotine = StartCoroutine(ResetBlock(moveID));
                else if(resetBlockCourotine != null)
                    StopCoroutine(resetBlockCourotine);
            }
        }

        private IEnumerator ResetBlock(int moveID)
        {
            yield return new WaitForSeconds(3);
            if (moveID == THIS.moveID && dragBlocked)
            {
                dragBlocked = false;
                Item.usedItem = null;
                Item.usedItemSecond = null;
            }
        }

        //current move ID
        public int moveID;
        //value for regeneration, items with falling or not
        public bool onlyFalling;
        //level loaded, wait until true for some courotines
        public bool levelLoaded;
        //true if Facebook plugin installed
        public bool FacebookEnable;
        //combine manager listener
        public CombineManager combineManager;
        //true if search of matches has started
        public bool findMatchesStarted;
        //true if need to check matches again
        private bool checkMatchesAgain;
        //if true - start the level avoind the map for debug
        public bool testByPlay;

        //game events
        #region EVENTS

        public delegate void GameStateEvents();
        public static event GameStateEvents OnMapState;
        public static event GameStateEvents OnEnterGame;
        public static event GameStateEvents OnLevelLoaded;
        public static event GameStateEvents OnWaitForTutorial;
        public static event GameStateEvents OnMenuPlay;
        public static event GameStateEvents OnSublevelChanged;
        public static event GameStateEvents OnMenuComplete;
        public static event GameStateEvents OnStartPlay;
        public static event GameStateEvents OnWin;
        public static event GameStateEvents OnLose;
        public static event GameStateEvents OnTurnEnd;
        public static event GameStateEvents OnCombo;
        public static event GameStateEvents OnMove;

        //current game state
        private GameState GameStatus;
        public GameState gameStatus
        {
            get { return GameStatus; }
            set
            {
                GameStatus = value;
                AdsManager.THIS?.CheckAdsEvents(value);
                switch (value)
                {
                    case GameState.PrepareGame://preparing and initializing  the game
                        StartCoroutine(AI.THIS.CheckPossibleCombines());
                        CrosssceneData.passLevelCounter++;
                        PrepareGame();
                        // var firstItemPrefab = THIS.levelData.target.prefabs.FirstOrDefault();
                        // if (firstItemPrefab && firstItemPrefab.GetComponent<Item>() && !firstItemPrefab.GetComponent<ItemSimple>())
                        //     collectIngredients = true;
                        _colorGenerator = new ColorGenerator();
                        GenerateLevel(_colorGenerator);
                        levelLoaded = true;
                        OnLevelLoaded?.Invoke();
                        break;
                    case GameState.WaitForPopup://waiting for pre game banners
                        StopCoroutine(IdleItemsDirection());
                        StartCoroutine(IdleItemsDirection());
                        var find = GameObject.Find("CanvasBack");
                        if (find != null) find.GetComponent<GraphicRaycaster>().enabled = false;
                        if (orientationGameCameraHandle != null)
                        {
                            OrientationGameCameraHandle.CameraParameters cameraParameters = orientationGameCameraHandle.GetCameraParameters();
                            Vector2 cameraCenter = orientationGameCameraHandle.GetCenterOffset();
                            StartCoroutine(AnimateField(field.GetPosition() + cameraCenter, cameraParameters.size));
                        }

                        break;
                    case GameState.Tutorial://tutorial state
                        OnWaitForTutorial?.Invoke();
                        break;
                    case GameState.PreFailed://chance to continue the game, shows menu PreFailed
                        AdsManager.THIS.CacheRewarded();
                        LeanTween.Framework.LeanTween.delayedCall(1, ()=>FailedWord.SetActive(true));
                        LeanTween.Framework.LeanTween.delayedCall(3, () =>
                        {
                            var preFailedGameObject = MenuReference.THIS.PreFailed.gameObject;
                            preFailedGameObject.GetComponent<PreFailed>().SetFailed();
                            preFailedGameObject.SetActive(true);
                        });
                        break;
                    case GameState.BombFailed:
                        LeanTween.Framework.LeanTween.delayedCall(0.3f, () =>
                        {
                            var preFailedGameObject = MenuReference.THIS.PreFailed.gameObject;
                            preFailedGameObject.GetComponent<PreFailed>().SetBombFailed();
                            preFailedGameObject.SetActive(true);
                        });
                        break;
                    case GameState.Map://map state
                        //open map or test level
                        if (PlayerPrefs.GetInt("OpenLevelTest") <= 0 || FindObjectOfType<RestartLevel>())
                        {
                            EnableMap(true);
                            OnMapState?.Invoke();
                        }
                        else
                        {
                            THIS.gameStatus = GameState.PrepareGame;
                            if (!testByPlay)
                                PlayerPrefs.SetInt("OpenLevelTest", 0);
                            PlayerPrefs.Save();
                        }
                        if (CrosssceneData.passLevelCounter > 0 && InitScript.Instance.ShowRateEvery > 0)
                        {
                            if (CrosssceneData.passLevelCounter % InitScript.Instance.ShowRateEvery == 0 &&
                                InitScript.Instance.ShowRateEvery > 0 && PlayerPrefs.GetInt("Rated", 0) == 0)
                                InitScript.Instance.ShowRate();
                        }
                        break;
                    case GameState.Playing://playing state
                        StartCoroutine(AI.THIS.CheckPossibleCombines());
                        break;
                    case GameState.GameOver://game over
                        MenuReference.THIS.MenuFailed.gameObject.SetActive(true);
                        OnLose?.Invoke();
                        break;
                    case GameState.PreWinAnimations://animations after win
                        StartCoroutine(PreWinAnimationsCor());
                        break;
                    case GameState.ChangeSubLevel://changing sub level state
                        if (CurrentSubLevel != GetLastSubLevel())
                            ChangeSubLevel();
                        break;
                    case GameState.Win://shows MenuComplete
                        OnMenuComplete?.Invoke();
                        MenuReference.THIS.MenuComplete.gameObject.SetActive(true);
                        SoundBase.Instance.PlayOneShot(SoundBase.Instance.complete[1]);
                        if(winRewardAmount > 0)
                            InitScript.Instance.ShowGemsReward(winRewardAmount);// InitScript.Instance.ShowGemsReward(10);
                        OnWin();
                        break;
                }
            }
        }

        //Combine manager reference
        public CombineManager CombineManager
        {
            get
            {
                if(combineManager == null)         combineManager = new CombineManager();
                return combineManager;
            }
        }

        //if true - pausing all falling animations
        public bool StopFall => _stopFall.Any();
        //returns last sub-level of this level
        private int GetLastSubLevel()
        {
            return fieldBoards.Count;
        }
        //returns current game state
        public static GameState GetGameStatus()
        {
            return THIS.gameStatus;
        }
        //menu play enabled invokes event
        public void MenuPlayEvent()
        {
            OnMenuPlay?.Invoke();
        }
        //Switch sub level to next 
        private void ChangeSubLevel()
        {
            CurrentSubLevel++;
            OrientationGameCameraHandle.CameraParameters cameraParameters = orientationGameCameraHandle.GetCameraParameters();
            Vector2 cameraCenter = orientationGameCameraHandle.GetCenterOffset();
            StartCoroutine(AnimateField(field.GetPosition() + cameraCenter, cameraParameters.size));
        }

        #endregion
        //Lock boosts
        private void LockBoosts()
        {
            foreach (var item in InGameBoosts)
            {
                if (item.type != ActivatedBoost.type)
                    item.LockBoost();
            }
        }
        //unlock boosts
        public void UnLockBoosts()
        {
            foreach (var item in InGameBoosts)
            {
                item.UnLockBoost();
            }
        }
        //Load the level from "OpenLevel" player pref
        public void LoadLevel()
        {
            currentLevel = PlayerPrefs.GetInt("OpenLevel");
            if (currentLevel == 0)
                currentLevel = 1;
            LoadLevel(currentLevel);
        }
        //enable map
        public void EnableMap(bool enable)
        {
            bool isRun = false;
            if (Camera.main == null)
                return;
            Camera.main.orthographicSize = 5.3f;
            if (enable)
            {
                Camera.main.GetComponent<MapCamera>()
                    .SetPosition(new Vector2(0, GetComponent<Camera>().transform.position.y));
                if (FindObjectOfType<RestartLevel>() == null && DebugSettings.AI && DebugSettings.testLevel > 0)
                {
                    PlayerPrefs.SetInt("OpenLevel", DebugSettings.testLevel);
                    RestartLevel();
                }
                else
                {
                    if (Camera.main.GetComponent<MapCamera>().enabled == enable)
                    {
                        isRun = true;
                        setNumbers();
                    }
                }
            }
            else
            {
                GetComponent<Camera>().orthographicSize = 4;
                if (ServerTime.THIS.dateReceived)
                    InitScript.DateOfExit = ServerTime.THIS.serverTime.ToString();
                MenuReference.THIS.GetComponent<GraphicRaycaster>().enabled = false;
                MenuReference.THIS.GetComponent<GraphicRaycaster>().enabled = true;
                Level.transform.Find("Canvas").GetComponent<GraphicRaycaster>().enabled = false;
                Level.transform.Find("Canvas").GetComponent<GraphicRaycaster>().enabled = true;
            }

            Camera.main.GetComponent<MapCamera>().enabled = enable;
            LevelsMap.SetActive(!enable);
            LevelsMap.SetActive(enable);
            Level.SetActive(!enable);

            if (!isRun && Camera.main.GetComponent<MapCamera>().isActiveAndEnabled)
                setNumbers();

            if (!enable)
                Camera.main.transform.position = new Vector3(0, 0, -10) - (Vector3)orientationGameCameraHandle.offsetFieldPosition;

            foreach (var item in fieldBoards)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            isRun = false;
        }

        private void setNumbers()
        {
            var numberObject = Resources.Load<GameObject>("Prefabs/Number");
            var parentObj = FindObjectOfType<Path>();
            StartCoroutine(SetLevelNumberToMapObject(numberObject, parentObj.Waypoints));
        }

        private IEnumerator SetLevelNumberToMapObject(GameObject numberObj, List<Transform> levelParentObject)
        {
            yield return new WaitForSeconds(0.01f);
            int counter = 0;
            foreach (var t in levelParentObject)
            {
                counter++;
                if (Camera.main.GetComponent<MapCamera>().isActiveAndEnabled)
                {
                    var obj = Instantiate(numberObj, t.parent, true);
                    obj.GetComponent<Canvas>().worldCamera = Camera.main;
                    obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + counter;
                    var rt = obj.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(0.011f, 0.235f); /*new Rect(, , );*/
                    rt.sizeDelta = new Vector2(30.25f, 28.47f);
                }
                else
                    break;
            }
        }
        
        private void Awake()
        {
            THIS = this;
            testByPlay = false;
            // testByPlay = true;//enable to instant level run
        }

        // Use this for initialization
        private void Start()
        {
            inputHandler = gameObject.AddComponent<InputHandler>();
            InputHandler.OnDown += MouseDown;
            InputHandler.OnUp += MouseUp;
            InputHandler.OnDownRight += MouseDownRight;
            DebugSettings = Resources.Load<DebugSettings>("Scriptable/DebugSettings");
            AdditionalSettings = Resources.Load<AdditionalSettings>("Scriptable/AdditionalSettings");
            LeanTween.Framework.LeanTween.init( 800 );
            LeanTween.Framework.LeanTween.reset();
#if FACEBOOK
            FacebookEnable = true;
            /*if (FacebookEnable && (!NetworkManager.THIS?.IsLoggedIn ?? false))
                FacebookManager.THIS.CallFBInit();
            else Debug.LogError("Facebook not initialized, please, install database service");*/
#else
        FacebookEnable = false;

#endif
#if UNITY_INAPPS
            gameObject.AddComponent<UnityInAppsIntegration>();
            enableInApps = true;
#else
        enableInApps = false;

#endif

//        if (!THIS.enableInApps)
//            GameObject.Find("CanvasMap/SafeArea/Gems").gameObject.SetActive(false);

            gameStatus = GameState.Map;
            winRewardAmount = Resources.Load<WinReward>("Scriptable/WinReward").winRewardAmount;

        }

        private void OnDisable()
        {
            InputHandler.OnDown -= MouseDown;
            InputHandler.OnUp -= MouseUp;
            InputHandler.OnDownRight -= MouseDownRight;
        }

        private void PrepareGame()
        {
     
            ActivatedBoost = null;
            Score = 0;
            stars = 0;
            moveID = 0;
            fieldBoards = new List<FieldBoard>();
            transform.position += Vector3.down * 1000;
            // targetUIObject.SetActive(false);
            LevelManager.THIS.thrivingBlockDestroyed = false;

            EnableMap(false);

            LoadLevel();

            CurrentSubLevel = 1;
            if (ProgressBarScript.Instance != null)
                ProgressBarScript.Instance.InitBar();

            if (levelData.limitType == LIMIT.MOVES)
            {
                InGameBoosts.Where(i => i.type == BoostType.ExtraMoves).ToList().ForEach(i => i.gameObject.SetActive(true));
                InGameBoosts.Where(i => i.type == BoostType.ExtraTime).ToList().ForEach(i => i.gameObject.SetActive(false));
            }
            else
            {
                InGameBoosts.Where(i => i.type == BoostType.ExtraMoves).ToList().ForEach(i => i.gameObject.SetActive(false));
                InGameBoosts.Where(i => i.type == BoostType.ExtraTime).ToList().ForEach(i => i.gameObject.SetActive(true));
            }

            OnEnterGame?.Invoke();
        }

        public List<FieldBoard> fieldBoards = new List<FieldBoard>();
        public GameObject FieldBoardPrefab;
        public LevelData levelData;
        internal bool tutorialTime;

        //Generate loaded level
        public void GenerateLevel(IColorGenerator colorGenerator)
        {
            var fieldPos = new Vector3(-0.9f, 0, -10);
            var latestFieldPos = Vector3.right * ((GetLastSubLevel() - 1) * 10) + Vector3.back * 10;

            var i = 0;
            foreach (var item in fieldBoards)
            {
                var _field = item.gameObject;
                _field.transform.SetParent(FieldsParent?.transform);
                _field.transform.position = fieldPos + Vector3.right * (i * 15);
                var fboard = _field.GetComponent<FieldBoard>();

                fboard.CreateField(colorGenerator);
                latestFieldPos = fboard.GetPosition();

                i++;
            }
            levelData.TargetCounters.RemoveAll(x => x.targetLevel.setCount == SetCount.FromLevel && x.GetCount()==0);

            if (orientationGameCameraHandle != null)
            {
                transform.position = latestFieldPos + Vector3.right * 10 + Vector3.back * 10 - (Vector3)orientationGameCameraHandle.offsetFieldPosition;
            }

            SetPreBoosts();
        }

        public bool animStarted;
        /// <summary>
        /// move camera to the field
        /// </summary>
        /// <param name="destPos">position of the field</param>
        /// <param name="cameraParametersSize">camera size</param>
        /// <returns></returns>
        private IEnumerator AnimateField(Vector3 destPos, float cameraParametersSize)
        {
            var _camera = GetComponent<Camera>();
            if(animStarted) yield break;
            animStarted = true;
            //var duration = 2f;
            var speed = 10f;
            var startPos = transform.position;
            var distance = Vector2.Distance(startPos, destPos);
            var time = distance / speed;
            var curveX = new AnimationCurve(new Keyframe(0, startPos.x), new Keyframe(time, destPos.x));
            var startTime = Time.time;
            float distCovered = 0;
            while (distCovered < distance)
            {
                distCovered = (Time.time - startTime) * speed;
                transform.localPosition = new Vector3(curveX.Evaluate(Time.time - startTime), transform.position.y, 0);
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, cameraParametersSize, Time.deltaTime*5);
                yield return new WaitForFixedUpdate();
            }

            _camera.orthographicSize = cameraParametersSize;
            transform.position = destPos;
            yield return new WaitForSeconds(0.5f);
            animStarted = false;
            GameStart();
        }

        //game start
        private void GameStart()
        {
            if (CurrentSubLevel - 1 == 0)
                MenuReference.THIS.PrePlay.gameObject.SetActive(true);
            else
            {
                OnSublevelChanged?.Invoke();
                gameStatus = GameState.Playing;
            }
        }
        /// Cloud effect animation for different direction levels
        private IEnumerator IdleItemsDirection()
        {
            if (field.squaresArray.Select(i => i.direction).Distinct().Count() > 1)
            {
                while (true)
                {
                    yield return new WaitForSeconds(3);
                    if (gameStatus == GameState.Playing && !findMatchesStarted)
                    {

                        // var orderedEnumerableCol = DirectionCloudEffect.GetItems();
                        var orderedEnumerableCol = THIS.field.GetItems()
                            .GroupBy(i => i.square.squaresGroup).ToList()
                            .Select(x => new
                            {
                                items = x,
                                Num = x.Max(i => i.square.orderInSequence),
                                Count = x.Count(),
                                x.Key
                            }).OrderByDescending(i => i.Num).ToList();

                        // Debug.WatchInstance(orderedEnumerableCol);
                        foreach (var items in orderedEnumerableCol)
                        {
                            var animationFinished = false;
                            foreach (var item in items.items)
                            {
                                if(item.destroying) continue;
                                StartCoroutine(item.DirectionAnimation(() => { animationFinished = true; }));
                            }

                            yield return new WaitUntil(() => animationFinished);
                        }
                    }

                    yield return new WaitForSeconds(1);
                }
            }
        }

        //Check win or lose conditions
        public void CheckWinLose()
        {
            var lose = false;
            var win = false;

            if (levelData.limit <= 0)
            {
                levelData.limit = 0;

                if (!levelData.IsTotalTargetReached())
                    lose = true;
                else win = true;
            }

            else
            {
                if (levelData.IsTotalTargetReached() && !levelData.WaitForMoveOut())
                {
                    win = true;
                }
                else if (levelData.IsTargetReachedSublevel() && fieldBoards.Count>1)
                    gameStatus = GameState.ChangeSubLevel;
                else if(noTip && !lose && fieldBoards.Count>1 && levelData.GetField().switchSublevelNoMatch)
                {
                    noTip = false;
                    gameStatus = GameState.ChangeSubLevel;
                }
            }

            if (lose)
            {
                gameStatus = GameState.PreFailed;
            }

            else if (win && !lose)
                gameStatus = GameState.PreWinAnimations;
            else if (!win && !lose && FindObjectsOfType<itemTimeBomb>().Any(i => i.timer <= 0))
            {
                LevelManager.THIS.gameStatus = GameState.BombFailed;
            }

            if (DebugSettings.AI && (win || lose))
            {
                Debug.Log((win ? "win " : "lose ") + " score " + Score+ " stars " + stars + " moves/time rest " + THIS.levelData.limit);
                RestartLevel();
            }
        }

        public GameObject winTrail;
        public Transform movesTransform;
        public int destLoopIterations;
        //Animations after win
          private IEnumerator PreWinAnimationsCor()
          { 
              tapToSkip = Instantiate((GameObject) Resources.Load("Prefabs/TapToSkip"),MenuReference.THIS.transform);
            if (!InitScript.Instance.losingLifeEveryGame && InitScript.lifes < InitScript.Instance.CapOfLife)
                InitScript.Instance.AddLife(1);
            CompleteWord.SetActive(true);
            var limit = Mathf.Clamp(levelData.limit, 0, 5);

            if(!skipWin)
            {
                var c1 = StartCoroutine(PreWinLoop(limit));
                yield return c1;
            }
            if(skipWin)
            {
                Score += limit * Random.Range(500, 3000) / levelData.colorLimit;
                CheckStars();
                skipWin = false;
                Destroy(tapToSkip);
            }
            if (PlayerPrefs.GetInt($"Level.{currentLevel:000}.StarsCount", 0) < stars)
                PlayerPrefs.SetInt($"Level.{currentLevel:000}.StarsCount", stars);
            if (Score > PlayerPrefs.GetInt("Score" + currentLevel))
            {
                PlayerPrefs.SetInt("Score" + currentLevel, Score);
            }
            if(PlayerPrefs.GetInt("ReachedLevel") <= currentLevel)
                PlayerPrefs.SetInt("ReachedLevel",currentLevel+1);
            PlayerPrefs.Save();
            if(Application.isEditor)
                Debug.Log("Level " + currentLevel + " score " + Score + " stars " + stars);
            CrosssceneData.win = true;
#if PLAYFAB || GAMESPARKS
            NetworkManager.dataManager.SetPlayerScore(currentLevel, Score);
            NetworkManager.dataManager.SetPlayerLevel(currentLevel + 1);
            NetworkManager.dataManager.SetStars(currentLevel);
#elif EPSILON
              NetworkManager.dataManager.SetPlayerLevel(new EpsilonLevel(currentLevel, stars, Score));
#endif
              
            gameStatus = GameState.Win;
        }

        private IEnumerator PreWinLoop(int limit)
        {
            var items = field.GetRandomItems(levelData.limitType == LIMIT.MOVES ? limit : 5);
            var list = new List<object>();
            for (var i = 0; i < items.Count; i++)
            {
                if (items?[i] == null) continue;
                var go = Instantiate(winTrail);
                go.transform.position = movesTransform.position;
                go.GetComponent<TrailEffect>().target = items[i];
                list.Add(go);
                go.GetComponent<TrailEffect>().StartAnim(target =>
                {
                    if (levelData.limitType == LIMIT.MOVES)
                    {
                        levelData.limit--;
                    }

                    if (target != null && target.gameObject.activeSelf)
                    {
                        target.NextType = (ItemsTypes) Random.Range(4, 6);
                        target.ChangeType();
                    }
                });
                if(skipWin) yield break;
                yield return new WaitForSeconds(0.3f);
            }


            if (list.Count > 0)
            {
                if(skipWin) yield break;
                yield return new WaitForListNull(list);
            }
            levelData.limit = 0;

            do
            {
                while (field.GetAllExtaItems().Count > 0 && gameStatus != GameState.Win)
                {
                    var item = field.GetAllExtaItems()[0];
                    if (item != null && item.currentType != ItemsTypes.MULTICOLOR) item.DestroyItem();
                    else if (item.currentType == ItemsTypes.MULTICOLOR) item.Check(item, field.GetRandomItems(1).First());
                    dragBlocked = true;
                    if(skipWin) yield break;
                    yield return new WaitForSeconds(0.1f);
                    FindMatches();
                }
                if(skipWin) yield break;

                yield return new WaitWhile(() => findMatchesStarted);
            } while (field.GetAllExtaItems().Count > 0);

            FindMatches();
            while (dragBlocked)
                yield return new WaitForFixedUpdate();

            if(skipWin) yield break;
            yield return new WaitForSeconds(1f);

            MenuReference.THIS.PreCompleteBanner.gameObject.SetActive(true);
            yield return new WaitForSeconds(3);
            MenuReference.THIS.PreCompleteBanner.gameObject.SetActive(false);
        }

        private void Update()
        {
            //  AvctivatedBoostView = ActivatedBoost;
            if (Input.GetKeyDown(DebugSettings.Regen) && DebugSettings.enableHotkeys)
            {
                NoMatches();
            }

            if (Input.GetKeyDown(DebugSettings.Win) && DebugSettings.enableHotkeys)
            {
                stars = Mathf.Clamp(stars,1, stars);
                gameStatus = GameState.PreWinAnimations;
            }

            if (Input.GetKeyDown(DebugSettings.Lose) && DebugSettings.enableHotkeys)
            {
                levelData.limit = 1;
            }

            if (Input.GetKeyDown(DebugSettings.Restart) && DebugSettings.enableHotkeys)
            {
                InitScript.Instance.AddLife(1);
                RestartLevel();
            }

#if !TEST
            if (gameStatus == GameState.Playing)
                Time.timeScale = DebugSettings.TimeScaleItems;
            else
                Time.timeScale = DebugSettings.TimeScaleUI;
#endif

            //if (Input.GetKeyDown(KeyCode.D))
            //{
            //	DestroyAnimatedItems();
            //}
            if (Input.GetKeyDown(DebugSettings.SubSwitch) && DebugSettings.enableHotkeys)
            {
                gameStatus = GameState.ChangeSubLevel;
            }

            if (Input.GetKeyUp(DebugSettings.Back) && DebugSettings.enableHotkeys)
            {
                if (THIS.gameStatus == GameState.Playing)
                    GameObject.Find("CanvasGlobal").transform.Find("MenuPause").gameObject.SetActive(true);
                else if (THIS.gameStatus == GameState.Map)
                    Application.Quit();
            }
            
            
        }

        void MouseDown(Vector2 pos)
        {
            if (gameStatus == GameState.PreWinAnimations)
            {
                skipWin = true;
            }
            if (gameStatus != GameState.Playing && gameStatus != GameState.Tutorial)
                return;
            if (EventSystem.current.IsPointerOverGameObject(-1) && gameStatus == GameState.Playing)
                return;

            var hit = Physics2D.OverlapPoint(pos,
                1 << LayerMask.NameToLayer("Item"));
            if (hit == null && AdditionalSettings.SelectableSolidBlock)
            {
                var hit1 = Physics2D.OverlapPoint(pos,
                    1 << LayerMask.NameToLayer("Square"));
                if (hit1 != null)
                {
                    var o = hit.gameObject;
                    Debug.Log("hit 0:::: " + o,o);
                    var fakeItemSquare = hit1.GetComponent<Square>();
                    if(fakeItemSquare.type == SquareTypes.SolidBlock || fakeItemSquare.type == SquareTypes.SpiralBlock || fakeItemSquare.type == SquareTypes.ThrivingBlock)
                    {
                        var fakeItem = fakeItemSquare.GenItem(false);
                        fakeItem.HideSprites(true);
                        fakeItem.square = fakeItemSquare;
                        fakeItemSquare.Item = fakeItem;
                        hit = fakeItem.GetComponent<Collider2D>();
                    }
                }
            }
            if (hit != null)
            {
                var o = hit.gameObject;
                lastTouchedItem = hit.gameObject.GetComponent<Item>();
                        
                if (tutorialTime && !lastTouchedItem.tutorialUsableItem) return;
                if (!THIS.DragBlocked &&
                    (gameStatus == GameState.Playing || gameStatus == GameState.Tutorial))
                {
                    OnStartPlay?.Invoke();
                    tutorialTime = false;
                    if (THIS.ActivatedBoost.type == BoostType.ExplodeArea &&
                        lastTouchedItem.currentType != ItemsTypes.MULTICOLOR && lastTouchedItem.currentType != ItemsTypes.INGREDIENT)
                    {
                        SoundBase.Instance.PlayOneShot(SoundBase.Instance.boostBomb);
                        THIS.DragBlocked = true;
                        var obj = Instantiate(Resources.Load("Boosts/area_explosion"), lastTouchedItem.transform.position,
                            lastTouchedItem.transform.rotation) as GameObject;
                        obj.GetComponent<SpriteRenderer>().sortingOrder = 4;
                        obj.GetComponent<BoostAnimation>().square = lastTouchedItem.square;
                        THIS.ActivatedBoost = null;
                    }
                    else if (THIS.ActivatedBoost.type == BoostType.Bomb &&
                             lastTouchedItem.currentType != ItemsTypes.MULTICOLOR && lastTouchedItem.currentType != ItemsTypes.INGREDIENT)
                    {
                        SoundBase.Instance
                            .PlayOneShot(SoundBase.Instance.boostColorReplace);
                        THIS.DragBlocked = true;
                        var obj = Instantiate(Resources.Load("Boosts/simple_explosion"), lastTouchedItem.transform.position,
                            lastTouchedItem.transform.rotation) as GameObject;
                        obj.GetComponent<BoostAnimation>().square = lastTouchedItem.square;
                        obj.GetComponent<SpriteRenderer>().sortingOrder = 4;
                        THIS.ActivatedBoost = null;
                    }
                    else if (lastTouchedItem.square.GetSubSquare().CanGoOut()/* && lastTouchedItem.Combinable*/)
                    {
                        var oo = hit.gameObject;
                                
                        lastTouchedItem.dragThis = true;
                        lastTouchedItem.mousePos = pos;
                        lastTouchedItem.deltaPos = Vector3.zero;
                    }
                }
            }
        }

        void MouseUp(Vector2 pos)
        {
            if (gameStatus != GameState.Playing && gameStatus != GameState.Tutorial)
                return;
            if (EventSystem.current.IsPointerOverGameObject(-1) && gameStatus == GameState.Playing)
                return;
            if (lastTouchedItem != null)
            {
                lastTouchedItem.dragThis = false;
                lastTouchedItem.switchDirection = Vector3.zero;
            }
        }

        void MouseDownRight(Vector2 pos)
        {
            if (gameStatus != GameState.Playing && gameStatus != GameState.Tutorial)
                return;
            if (EventSystem.current.IsPointerOverGameObject(-1) && gameStatus == GameState.Playing)
                return;
            var hit = Physics2D.OverlapPoint(pos,
                1 << LayerMask.NameToLayer("Item"));
            if (hit != null)
            {
                var item = hit.gameObject.GetComponent<Item>();
                Camera.main.GetComponent<ItemChangerTest>().ShowMenuItems(item);

            }
        }

        private void RestartLevel()
        {
            GameObject gm = new GameObject();
            gm.AddComponent<RestartLevel>();
            string scene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(scene);
        }

        #region RegenerateLevel
        //No matches detected, regenerate level
        public void NoMatches()
        {
            if (field.fieldData.noRegenLevel)
            {
                if (GameStatus == GameState.Playing)
                {
                    noTip = true;
                    CheckWinLose();
                }
                return;
            }
            StartCoroutine(NoMatchesCor());
        }

        private IEnumerator NoMatchesCor()
        {
            if (gameStatus == GameState.Playing)
            {
                DragBlocked = true;

                SoundBase.Instance.PlayOneShot(SoundBase.Instance.noMatch);

                NoMoreMatches.gameObject.SetActive(true);
                gameStatus = GameState.RegenLevel;
                yield return new WaitForSeconds(1);
                ReGenLevel();
            }
        }

        public void ReGenLevel()
        {
            DragBlocked = true;
            //if (gameStatus != GameState.Playing && gameStatus != GameState.RegenLevel)
            //DestroyAnimatedItems();
//        if (gameStatus == GameState.RegenLevel)
//            DestroyItems(true);
            StartCoroutine(RegenMatches());
            OnLevelLoaded();
        }

        private IEnumerator RegenMatches(bool onlyFalling = false)
        {
            if (gameStatus == GameState.RegenLevel)
            {
                yield return new WaitForSeconds(0.5f);
            }
            if (!onlyFalling)
                field.RegenItems(false);
            else
                THIS.onlyFalling = true;
            field.GetItems().ForEach(i=>i.Hide(true));
            yield return new WaitForFixedUpdate();

            var combs = new List<List<Item>>();
            do
            {
                yield return new WaitWhileFall();
                combs = CombineManager.GetCombinedItems(field);
                combs.Add( AI.THIS.GetMarmaladeCombines()?.items);
                combs = combs.WhereNotNull().Where(i => i.Count() > 0).ToList();
                yield return new WaitForEndOfFrame();
                foreach (var comb in combs)
                {
                    foreach (var item in comb)
                    {
                        item.GenColor(_colorGenerator);
                    }
                }
            } while (combs.Count > 0);

            SetPreBoosts();
            if (!onlyFalling)
                DragBlocked = false;
            THIS.onlyFalling = false;
            if (gameStatus == GameState.RegenLevel)
                gameStatus = GameState.Playing;
            yield return new WaitForEndOfFrame();
            field.GetItems().ForEach(i=>
            {
                var transformLocalScale = i.transform.localScale;
                i.transform.localScale = Vector3.zero;
                LeanTween.Framework.LeanTween.scale(i.gameObject, transformLocalScale, .3f); i.Hide(false);  });
        }
    
        public void DestroyItems(bool withoutEffects = false)
        {
            var items = field.GetItems();
            foreach (var item in items)
            {
                if (item != null)
                {
                    if (item.GetComponent<Item>().currentType != ItemsTypes.INGREDIENT &&
                        item.GetComponent<Item>().currentType == ItemsTypes.NONE)
                    {
                        if (!withoutEffects)
                            item.GetComponent<Item>().DestroyItem();
                        else
                            item.GetComponent<Item>().anim.SetTrigger("disappear");
                    }
                }
            }
        }
    
        #endregion


        /// <summary>
        /// Place boosts in the field which bought before in Play menu
        /// </summary>
        private void SetPreBoosts()
        {
            if (BoostPackage > 0)
            {
                InitScript.Instance.SpendBoost(BoostType.Packages);
                foreach (var item in field.GetRandomItems(BoostPackage))
                {
                    item.NextType = ItemsTypes.PACKAGE;
                    item.ChangeType(null, true, false);
                }

                BoostPackage = 0;
            }

            if (BoostColorfullBomb > 0)
            {
                InitScript.Instance.SpendBoost(BoostType.MulticolorCandy);
                foreach (var item in field.GetRandomItems(BoostColorfullBomb))
                {
                    item.NextType = ItemsTypes.MULTICOLOR;
                    item.ChangeType(null, true, false);
                }

                BoostColorfullBomb = 0;
            }

            if (BoostStriped > 0)
            {
                InitScript.Instance.SpendBoost(BoostType.Stripes);
                foreach (var item in field.GetRandomItems(BoostStriped))
                {
                    item.NextType = (ItemsTypes)Random.Range(4, 6);
                    item.ChangeType(null, true, false);
                }

                BoostStriped = 0;
            }

            if (BoostMarmalade > 0)
            {
                InitScript.Instance.SpendBoost(BoostType.Marmalade);
                foreach (var item in field.GetRandomItems(BoostMarmalade))
                {
                    item.NextType = ItemsTypes.MARMALADE;
                    item.ChangeType(null, true, false);
                }

                BoostStriped = 0;
            }
        }


        //Find matches with delay
        public IEnumerator FindMatchDelay()
        {
            yield return new WaitForSeconds(0.2f);
            THIS.FindMatches();
        }

        //start sync matches search then wait while items destroy and fall
        public void FindMatches()
        {
            if (!findMatchesStarted)
                StartCoroutine(FallingDown());
            else
                checkMatchesAgain = true;
        }

        internal List<GameBlocker> _stopFall = new List<GameBlocker>();
        public int combo;
        public AnimationCurve fallingCurve = AnimationCurve.Linear(0, 10, 1, 10);
        public float waitAfterFall = 0.02f;
        [HideInInspector] private bool collectIngredients;

        public Item lastTouchedItem;
        private int winRewardAmount;
        public bool skipWin;
        private GameObject tapToSkip;
        private bool noTip;
        public bool checkTarget;
        private InputHandler inputHandler;
        private ColorGenerator _colorGenerator;

        private IEnumerator FallingDown()
        {
            OnMove?.Invoke();
            findMatchesStarted = true;
//        Debug.Log("@@@ Next Move search matches @@@");
            THIS.thrivingBlockDestroyed = false;
            combo = 0;
            AI.THIS.allowShowTip = false;
            var it = field.GetItems();
            for (var i = 0; i < it.Count; i++)
            {
                var item = it[i];
                if (item != null)
                {
                    item.anim.StopPlayback();
                }
            }

            destLoopIterations = 0;
            // while (true)
            {
                destLoopIterations++;
                checkMatchesAgain = false;

                var destroyItemsListed = field.GetItems().Where(i => i.destroyNext).ToList();
                if (destroyItemsListed.Count > 0)
                    yield return new WaitWhileDestroyPipeline(destroyItemsListed, new Delays());
                yield return new WaitWhileDestroying();
                yield return new WaitWhile(()=>StopFall);
                yield return new WaitWhileFall();
                yield return new WaitWhileCollect();

                if (!LevelManager.THIS.animateItems.Any())
                    levelData.TargetCounters.ForEachY(i => i.GetCount());
                CheckBigBlocksCleared();
            }

            if (combo > 2 && gameStatus == GameState.Playing)
            {
                gratzWords[Random.Range(0, gratzWords.Length)].SetActive(true);
                combo = 0;
                OnCombo?.Invoke();
            }

            //CheckItemsPositions();
            DragBlocked = false;
            findMatchesStarted = false;
            checkMatchesAgain = false;
            if (gameStatus == GameState.Playing)
                StartCoroutine(AI.THIS.CheckPossibleCombines());

//        Debug.Log("<-next turn->");
            if (gameStatus == GameState.Playing)
            {
                if (!LevelManager.THIS.animateItems.Any())
                {
                    OnTurnEnd?.Invoke();
                    THIS.CheckWinLose();
                }
                else
                {
                    yield return new WaitWhile(()=>LevelManager.THIS.animateItems.Any() || LevelManager.THIS.checkTarget);
                    OnTurnEnd?.Invoke();
                    THIS.CheckWinLose();
                }
            }
        }

        private void CheckBigBlocksCleared()
        {
            field.GetSquares().ForEach(i => i.CheckBigBlockCleared());
        }

        /// <summary>
        /// Get square by position
        /// </summary>
        public Square GetSquare(int col, int row, bool safe = false)
        {
            return field.GetSquare(col, row, safe);
        }
        /// <summary>
        /// Get bunch of squares by row number
        /// </summary>
        public List<Square> GetRowSquare(int row)
        {
            var itemsList = new List<Square>();
            for (var col = 0; col < levelData.maxCols; col++)
            {
                Square square = GetSquare(col, row, true);
                if(square.type == SquareTypes.UndestrBlock && LevelManager.THIS.AdditionalSettings.StripedStopByUndestroyable) break;
                if (!square.IsNone()) 
                    itemsList.Add(square);
            }

            return itemsList;
        }
        /// Get bunch of squares by column number
        public List<Square> GetColumnSquare(int col)
        {
            var itemsList = new List<Square>();
            for (var row = 0; row < levelData.maxRows; row++)
            {
                Square square = GetSquare(col, row, true);
                if(square.type == SquareTypes.UndestrBlock && LevelManager.THIS.AdditionalSettings.StripedStopByUndestroyable) break;
                if (!square.IsNone()) 
                    itemsList.Add(square);
            }

            return itemsList;
        }
        /// Get bunch of items by row number
        public List<Item> GetRow(Square square)
        {
            var itemsList = new List<Item>();
            for (var col = square.col; col < levelData.maxCols; col++)
            {
                var square1 = GetSquare(col, square.row, true);
                if(square1.type == SquareTypes.UndestrBlock && LevelManager.THIS.AdditionalSettings.StripedStopByUndestroyable) break;
                itemsList.Add(square1.Item);
            }
            for (var col =  square.col; col >= 0; col--)
            {
                var square1 = GetSquare(col, square.row, true);
                if(square1.type == SquareTypes.UndestrBlock && LevelManager.THIS.AdditionalSettings.StripedStopByUndestroyable) break;
                itemsList.Add(square1.Item);
            }
            return itemsList.WhereNotNull().ToList();
        }
        /// Get bunch of items by column number
        public List<Item> GetColumn(Square square)
        {
            var itemsList = new List<Item>();
            for (var row = square.row; row < levelData.maxRows; row++)
            {
                var square1 = GetSquare(square.col, row, true);
                if(square1.type == SquareTypes.UndestrBlock && LevelManager.THIS.AdditionalSettings.StripedStopByUndestroyable) break;
                itemsList.Add(square1.Item);
            }
            
            for (var row =  square.row; row >= 0; row--)
            {
                var square1 = GetSquare(square.col, row, true);
                if(square1.type == SquareTypes.UndestrBlock && LevelManager.THIS.AdditionalSettings.StripedStopByUndestroyable) break;
                itemsList.Add(square1.Item);
            }

            return itemsList.WhereNotNull().ToList();
        }
        
        public List<Item> GetColumn(int col)
        {
            var itemsList = new List<Item>();
            for (var row = 0; row < levelData.maxRows; row++)
            {
                var square1 = GetSquare(col, row, true);
                itemsList.Add(square1.Item);
            }
            return itemsList.WhereNotNull().ToList();
        }
        /// <summary>
        /// Get squares around the square
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        public List<Square> GetSquaresAroundSquare(Square square)
        {
            var col = square.col;
            var row = square.row;
            var itemsList = new List<Square>();
            for (var r = row - 1; r <= row + 1; r++)
            {
                for (var c = col - 1; c <= col + 1; c++)
                {
                    itemsList.Add(GetSquare(c, r, true));
                }
            }

            return itemsList;
        }

        /// <summary>
        /// get 9 items around the square
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        public List<Item> GetItemsAround9(Square square)
        {
            var itemsList = GetItemsAround8(square);
            itemsList.Add(square.Item);
            return itemsList;
        }

        /// get 8 items around the square
        public List<Item> GetItemsAround8(Square square)
        {
            var col = square.col;
            var row = square.row;
            var itemsList = new List<Item>();
            for (var r = row - 1; r <= row + 1; r++)
            {
                for (var c = col - 1; c <= col + 1; c++)
                {
                    var pos = GetSquare(c, r, true).transform.position;
                    RaycastHit2D[] result = new RaycastHit2D[3];
                    Physics2D.LinecastNonAlloc(square.transform.position, pos, result,1<< LayerMask.NameToLayer("Item"));
                    var hit = result.Where(i => i.transform).OrderBy(i => (i.transform.position - pos).magnitude).FirstOrDefault();
                    if(hit && hit.transform)
                        itemsList.Add(hit.transform.GetComponent<Item>());
                }
            }

            return itemsList;
        }

        //striped effect
        public void StripedShow(GameObject obj, bool horrizontal)
        {
            if (stripesEffect != null)
            {
                var effect = Instantiate(stripesEffect, obj.transform.position, Quaternion.identity);
                if (!horrizontal)
                    effect.transform.Rotate(Vector3.back, 90);
                Destroy(effect, 1);
            }
        }

        //popup score, to use - enable "Popup score" in editor
        public void ShowPopupScore(int value, Vector3 pos, int color)
        {
            // UpdateBar();
            if (showPopupScores)
            {
                var parent = GameObject.Find("CanvasScore").transform;
                var poptxt = Instantiate(popupScore, pos, Quaternion.identity);
                poptxt.transform.GetComponentInChildren<Text>().text = "" + value;
                if (color <= scoresColors.Length - 1)
                {
                    poptxt.transform.GetComponentInChildren<Text>().color = scoresColors[color];
                    poptxt.transform.GetComponentInChildren<Outline>().effectColor = scoresColorsOutline[color];
                }

                poptxt.transform.SetParent(parent);
                //   poptxt.transform.position += Vector3.right * 1;
                poptxt.transform.localScale = Vector3.one / 1.5f;
                Destroy(poptxt, 0.3f);
            }
        }

        /// <summary>
        /// check gained stars
        /// </summary>
        public void CheckStars()
        {
            if (Score >= levelData.star1 && stars <= 0)
            {
                stars = 1;
            }

            if (Score >= levelData.star2 && stars <= 1)
            {
                stars = 2;
            }

            if (Score >= levelData.star3 && stars <= 2)
            {
                stars = 3;
            }
        }

        /// <summary>
        /// load level from 
        /// </summary>
        /// <param name="currentLevel"></param>
        public void LoadLevel(int currentLevel)
        {
            levelLoaded = false;
            levelData = LoadingManager.LoadForPlay(currentLevel);

            if (gameStatus != GameState.Map)
            {
                foreach (var fieldData in levelData.fields)
                {
                    var _field = Instantiate(FieldBoardPrefab);
                    var fboard = _field.GetComponent<FieldBoard>();
                    fboard.fieldData = fieldData;
                    fboard.squaresArray = new Square[fieldData.maxCols * fieldData.maxRows];
                    fieldBoards.Add(fboard);
                }
            }
        }
        
        public void delayedCall(float sec, Action action)
        {
            StartCoroutine(DelayedCallCor(sec, action));
        }

        IEnumerator DelayedCallCor(float sec, Action action)
        {
            yield return new WaitForSeconds(sec);
            action?.Invoke();
        }


    }
}