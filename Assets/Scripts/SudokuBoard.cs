using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class SudokuBoard : ScriptableObject
{
    public int RowCnt
    {
        get {
            return OriginBoard.Count;
        }
    }

    public int ColCnt
    {
        get {
            return OriginBoard[0].Count;
        }
    }

    public int CellCnt
    {
        get {
            return RowCnt;
        }
    }

    public int this[int row, int col]
    {
        get {
            if (OriginBoard == null) {
                return 0;
            }

            if (row < 0 || row >= RowCnt || col < 0 || col >= ColCnt) {
                return 0;
            }

            return OriginBoard[row][col];
        }

        set {
            if (OriginBoard == null) {
                return;
            }

            if (row < 0 || row >= RowCnt || col < 0 || col >= ColCnt) {
                return;
            }

            OriginBoard[row][col] = value;
        }
    }

    [Serializable]
    public class RowList : List<int>
    {
        public RowList()
        { }

        public RowList(int capacity)
            : base(capacity)
        {

        }
    }

    [SerializeField]
    public List<RowList> OriginBoard = null;

    [NonSerialized]
    public List<RowList> Board = null;

    public UnityEvent<int, int, int, int> OnCellChanged;

    public void Init(int size)
    {
        OriginBoard = new List<SudokuBoard.RowList>();
        for (var i = 0; i < size; ++i) {
            var row = new SudokuBoard.RowList();
            for (var j = 0; j < size; ++j) {
                row.Add(0);
            }

            OriginBoard.Add(row);
        }

        Board = CloneBoard(OriginBoard);
    }

    public List<SudokuBoard.RowList> CloneBoard(List<SudokuBoard.RowList> board)
    {
        var new_board = new List<SudokuBoard.RowList>();
        for (var i = 0; i < board.Count; ++i) {
            var row = new SudokuBoard.RowList();
            for (var j = 0; j < board.Count; ++j) {
                row.Add(board[i][j]);
            }

            Board.Add(row);
        }

        return new_board;
    }

    public bool ValidateNumber(int num)
    {
        if (num <= 0 || num > CellCnt) {
            return false;
        }

        return true;
    }

    public bool SetCell(int row, int col, int num)
    {
        if (!ValidateNumber(num)) {
            return false;
        }

        if (this[row, col] != 0) {
            return false;
        }

        var old_num = this[row, col];
        this[row, col] = num;

        OnCellChanged.Invoke(row, col, old_num, num);
        return true;
    }

    public bool DeleteCell(int row, int col)
    {
        return SetCell(row, col, 0);
    }
}
