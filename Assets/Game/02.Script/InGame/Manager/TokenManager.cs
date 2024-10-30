using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Entity;
using UnityEngine;

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
        
        [SerializeField] private InGameResourcesConfigData _resourcesConfigData;
        [SerializeField] private ResourceToken _resourceTokenPrefab;

        private List<ResourceToken> _resourceTokenList;
        
        public void GenerateTokenAsync(CellType cellType, Vector3 spawnPosition, Vector3 destinationPosition,
            ObstacleCellType obstacleCellType, CellImageType cellImageType = CellImageType.None)
        {
            if (_resourceTokenList == null)
            {
                _resourceTokenList = new List<ResourceToken>();
            }
            
            Sprite sprite = GetSprite(cellType, obstacleCellType, cellImageType);
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
            token.Spawn(sprite, scale, spawnPosition, destinationPosition, OnTokenMoveDone);
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

        private void OnTokenMoveDone(ResourceToken resourceToken)
        {
            // _resourceTokenList.Add(resourceToken);
        }

        private ResourceToken TryGetUsableToken()
        {
            return _resourceTokenList.Find(v => !v.gameObject.activeSelf);
        }

        private Sprite GetSprite(CellType cellType, ObstacleCellType obstacleCellType = ObstacleCellType.None, CellImageType cellImageType = CellImageType.None)
        {
            switch (cellType)
            {
                case CellType.Normal:
                    InGameResourcesConfigData.CellImageTypeSpriteData data =
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