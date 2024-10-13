using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ThreeMatch.InGame
{
    public class Block
    {
        public Vector3 Position => _blockBehaviour.transform.position;
        public BlockType BlockType => _blockType;
        public BlockBehaviour BlockBehaviour => _blockBehaviour;
        public int Row => _row;
        public int Column => _column;
        
        private int _column;
        private int _row;
        private BlockBehaviour _blockBehaviour;
        private BlockType _blockType;
        
        public Block(int row, int column, BlockType blockType)
        {
            _column = column;
            _row = row;
            _blockType = blockType;
        }

        public void CreateBlockBehaviour(GameObject prefab, Vector2 centerPosition, int row, int column, bool isOdd)
        {
            var obj = Object.Instantiate(prefab);
            obj.name = $"Block {_row} / {_column}";
            _blockBehaviour = obj.GetOrAddComponent<BlockBehaviour>();
            Vector2 size = _blockBehaviour.Size;
            Vector2 startPosition = centerPosition -
                                    new Vector2(size.x * (column * 0.5f) - size.x * 0.5f,
                                        size.y * (row * 0.5f) - size.y * 0.5f);
            Vector2 position = startPosition + new Vector2(size.x * _column, size.y * _row);
            _blockBehaviour.UpdatePosition(position);
            _blockBehaviour.UpdateUI(isOdd, _blockType);
        }
    }
}