using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Entity;
using UnityEngine;

namespace ThreeMatch.InGame.Interface
{
    public interface IHitable
    {
        public Health Health { get; }
        bool Hit(CellType cellType);
    }
}