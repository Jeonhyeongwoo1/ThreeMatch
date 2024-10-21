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

        public override void Initialize(CellType cellType, Transform parent, Vector3 position, CellImageType cellImageType = CellImageType.None,
            CellMatchedType cellMatchedType = CellMatchedType.None)
        {
            base.Initialize(cellType, parent, position, cellImageType, cellMatchedType);
            
            switch (cellType)
            {
                case CellType.Obstacle_Cage:
                    _backgroundSprite.sprite = _data.GetCellImageTypeSpriteData(cellImageType).normalSprite;
                    break;
                case CellType.Obstacle_IceBox:
                    _backgroundSprite.sprite = _data.IceBoxSpriteArray[Health.HP - 1];
                    break;
                case CellType.Obstacle_Box:
                    break;
            }
        }

        public bool Hit(CellType cellType)
        {
            if (Health.IsDead())
            {
                return true;
            }
            
            int hp = Health.TakeDamage(1);
            if (hp > 0)
            {
                switch (cellType)
                {
                    case CellType.Obstacle_IceBox:
                        _backgroundSprite.sprite = _data.IceBoxSpriteArray[hp - 1];
                        break;
                }
            }
            else
            {
                switch (cellType)
                {
                    case CellType.Obstacle_IceBox:
                        Activate(false);
                        break;
                    case CellType.Obstacle_Cage:
                        _frontSprite.gameObject.SetActive(false);
                        break;
                }
            }
            return Health.IsDead();
        }
    }
}