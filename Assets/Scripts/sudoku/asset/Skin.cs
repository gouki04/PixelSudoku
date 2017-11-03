using System.Collections.Generic;
using UnityEngine;

namespace sudoku.asset
{
    [CreateAssetMenu]
    public class Skin : ScriptableObject
    {
        public Sprite CellBorder;
        public Sprite GivenCellBg;
        public Sprite CellBg;
        public Sprite EmptyHighlight;
        public Sprite Highlight;
        public Sprite ErrorHighlight;

        public List<Sprite> CellNumberList = new List<Sprite>();
    }
}