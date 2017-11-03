﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace sudoku.asset
{
    [CreateAssetMenu]
    public class Puzzle : ScriptableObject
    {
        [SerializeField]
        public int Size;

        [SerializeField]
        public string PuzzleString = "";

        [NonSerialized]
        protected int[,] mGivenCells;

        public int[,] GivenCells
        {
            get {
                if (mGivenCells == null) {
                    InitGivenCells();
                }

                return mGivenCells;
            }
        }

        protected bool InitGivenCells()
        {
            if (PuzzleString.Length > Size * (Size + 1)) {
                return false;
            }

            foreach (var c in PuzzleString) {
                if (c == '\n') {
                    continue;
                }

                var number = c - '0';
                if (number < 0 || number > Size) {
                    return false;
                }
            }

            mGivenCells = new int[Size, Size];

            var index = 0;
            foreach (var c in PuzzleString) {
                if (c == '\n') {
                    continue;
                }

                var number = c - '0';
                var row = index / Size;
                var col = index % Size;
                mGivenCells[row, col] = number;

                ++index;
            }

            return true;
        }
    }
}
