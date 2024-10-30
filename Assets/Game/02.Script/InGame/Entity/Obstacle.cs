using System;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    [RequireComponent(typeof(Health))]
    public class Obstacle : CellBehaviour, IHitable
    {
        private Health _health;
        public Health Health
        {
            get
            {
                if (_health == null)
                {
                    TryGetComponent(out _health);
                }

                return _health;
            }
        }

        public override void Initialize(CellType cellType, Transform parent, Vector3 position,
            ObstacleCellType obstacleCellType = ObstacleCellType.None, CellImageType cellImageType = CellImageType.None,
            CellMatchedType cellMatchedType = CellMatchedType.None)
        {
            base.Initialize(cellType, parent, position, obstacleCellType, cellImageType, cellMatchedType);
            
            switch (obstacleCellType)
            {
                case ObstacleCellType.OneHitBox:
                    break;
                case ObstacleCellType.HitableBox:
                    _backgroundSprite.sprite = _data.HitableBoxSpriteArray[Health.HP - 1];
                    break;
                case ObstacleCellType.Cage:
                    _backgroundSprite.sprite = _data.GetCellImageTypeSpriteData(cellImageType).normalSprite;
                    break;
            }
        }

        public bool Hit(ObstacleCellType obstacleCellType)
        {
            if (Health.IsDead())
            {
                return true;
            }

            int hp = Health.TakeDamage(1);
            if (hp > 0)
            {
                if (obstacleCellType == ObstacleCellType.HitableBox)
                {
                    _backgroundSprite.sprite = _data.HitableBoxSpriteArray[hp - 1];
                }
            }
            else
            {
                switch (obstacleCellType)
                {
                    case ObstacleCellType.HitableBox:
                        Activate(false);
                        break;
                    case ObstacleCellType.Cage:
                        _frontSprite.gameObject.SetActive(false);
                        break;
                }
            }

            return Health.IsDead();
        }
    }
}