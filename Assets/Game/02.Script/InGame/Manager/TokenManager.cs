using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Entity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ThreeMatch.InGame.Manager
{
    public class TokenManager : MonoBehaviour
    {
        public static TokenManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TokenManager>();
                }

                return _instance;
            }
        }
        
        private static TokenManager _instance;
        
        [SerializeField] private GameResourcesConfigData _resourcesConfigData;
        [SerializeField] private ResourceToken _resourceTokenPrefab;

        private List<ResourceToken> _resourceTokenList;

        public void GenerateGoldToken(Vector3 spawnPosition, Vector3 destinationPosition, int tokenCount = 8,
            UniTaskCompletionSource source = null)
        {
            _resourceTokenList ??= new List<ResourceToken>();
            Sprite sprite = _resourcesConfigData.GoldSprite;

            for (int i = 0; i < tokenCount; i++)
            {
                ResourceToken token = TryGetUsableToken();
                if (token == null)
                {
                    token = Instantiate(_resourceTokenPrefab, transform);
                    _resourceTokenList.Add(token);
                }

                token.Spawn(sprite, Vector3.one, spawnPosition);
                token.MoveToRandomlyAroundSpawnPosition(false, spawnPosition, destinationPosition, source).Forget();
            }
        }

        public void GenerateCellToken(CellType cellType, Vector3 spawnPosition, Vector3 destinationPosition,
            ObstacleCellType obstacleCellType, CellImageType cellImageType = CellImageType.None, Action callback = null)
        {
            _resourceTokenList ??= new List<ResourceToken>();
            Sprite sprite = GetCellSprite(cellType, obstacleCellType, cellImageType);
            if (sprite == null)
            {
                return;
            }

            ResourceToken token = TryGetUsableToken();
            if (token == null)
            {
                token = Instantiate(_resourceTokenPrefab, transform);
                _resourceTokenList.Add(token);
            }

            Vector3 scale = GetTokenScale(cellType);
            token.Spawn(sprite, scale, spawnPosition, () => callback?.Invoke());
            Vector3 posA = spawnPosition + destinationPosition;
            Vector3 posB = posA * 0.5f;
            Vector3 controlPoint = posA / 2 + new Vector3(Random.Range(-posB.x, posB.x), Random.Range(-posB.y, posB.y));
            token.BezierMoveAsync(spawnPosition, controlPoint, destinationPosition).Forget();
        }

        private Vector3 GetTokenScale(CellType cellType)
        {
            switch (cellType)
            {
                case CellType.Normal:
                case CellType.Obstacle:
                default:
                    return Vector3.one * 0.7f;
                case CellType.Generator:
                    return Vector3.one * 2f;
            }
        }

        private ResourceToken TryGetUsableToken()
        {
            return _resourceTokenList.Find(v => !v.gameObject.activeSelf);
        }

        private Sprite GetCellSprite(CellType cellType, ObstacleCellType obstacleCellType = ObstacleCellType.None, CellImageType cellImageType = CellImageType.None)
        {
            switch (cellType)
            {
                case CellType.Normal:
                    GameResourcesConfigData.CellImageTypeSpriteData data =
                        _resourcesConfigData.GetCellImageTypeSpriteData(cellImageType);
                    return data.normalSprite;
                case CellType.Obstacle:
                    return obstacleCellType switch
                    {
                        ObstacleCellType.OneHitBox => _resourcesConfigData.ObstacleOneHitBoxSprite,
                        ObstacleCellType.HitableBox => _resourcesConfigData.ObstacleHitableBoxSprite,
                        ObstacleCellType.Cage => _resourcesConfigData.ObstacleCageSprite
                    };
                case CellType.Generator:
                    return _resourcesConfigData.StarSprite;
                case CellType.None:
                default:
                    Debug.LogError($"Failed get sprite {cellType}");
                    return null;
            }
        }
    }
}