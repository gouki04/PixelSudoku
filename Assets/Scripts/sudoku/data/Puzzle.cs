using System;
using UnityEngine;

namespace sudoku.data
{
    /// <summary>
    /// 数独题目
    /// </summary>
    public class Puzzle : ICloneable
    {
        /// <summary>
        /// 行数
        /// </summary>
        public int RowCnt
        {
            get {
                return m_Cells.GetLength(0);
            }
        }

        /// <summary>
        /// 列数
        /// </summary>
        public int ColCnt
        {
            get {
                return m_Cells.GetLength(1);
            }
        }

        /// <summary>
        /// 题目的大小
        /// 标准数独是9*9
        /// </summary>
        public int Size
        {
            get {
                return RowCnt;
            }
        }

        /// <summary>
        /// 宫数
        /// </summary>
        public int BoxRowCnt
        {
            get {
                return (int)Math.Sqrt(Size);
            }
        }

        public int BoxColCnt
        {
            get {
                return (int)Math.Sqrt(Size);
            }
        }

        public int BoxCnt
        {
            get {
                return Size;
            }
        }

        /// <summary>
        /// 获取/设置当前题板的指定行列的格子数字
        /// 0表示该格还没填入数字
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public int this[int row, int col]
        {
            get {
                if (m_Cells == null) {
                    return 0;
                }

                if (row < 0 || row >= RowCnt || col < 0 || col >= ColCnt) {
                    return 0;
                }

                return m_Cells[row, col];
            }

            set {
                if (m_Cells == null) {
                    return;
                }

                if (row < 0 || row >= RowCnt || col < 0 || col >= ColCnt) {
                    return;
                }

                m_Cells[row, col] = value;
            }
        }

        protected int[,] m_GivenCells = null;
        protected int[,] m_Cells = null;
        protected int[,] m_TempCells = null;
        protected BitSet32[,] m_Candidates = null;

        protected BitSet32 m_FinishedMask;

        protected bool m_HasError = false;
        protected bool m_IsFinished = false;

        /// <summary>
        /// 题目给定的题板
        /// </summary>
        public int[,] GivenCells
        {
            get {
                return m_GivenCells;
            }
        }

        /// <summary>
        /// 候选数题板
        /// </summary>
        public BitSet32[,] Candidates
        {
            get {
                return m_Candidates;
            }
        }

        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool IsFinished
        {
            get {
                return m_IsFinished;
            }
        }

        /// <summary>
        /// 任意格子的数字发生变化时触发
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="old_number"></param>
        /// <param name="number"></param>
        public delegate void delCellChanged(int row, int col, int old_number, int number);
        public delCellChanged OnCellChanged;

        /// <summary>
        /// 任意格子的候选数发生变化时触发
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="old_candidate"></param>
        /// <param name="candidate"></param>
        public delegate void delCandidateChanged(int row, int col, BitSet32 old_candidate, BitSet32 candidate);
        public delCandidateChanged OnCandidateChanged;

        /// <summary>
        /// 题目出现错误时触发
        /// </summary>
        /// <param name="error_cells"></param>
        public delegate void delPuzzleError(int[,] error_cells);
        public delPuzzleError OnError;

        /// <summary>
        /// 题目完成时触发
        /// </summary>
        public delegate void delPuzzleFinished();
        public delPuzzleFinished OnFinished;

        public void Init(asset.Puzzle puzzle_asset)
        {
            m_GivenCells = ClonePuzzle(puzzle_asset.GivenCells);
            m_Cells = ClonePuzzle(m_GivenCells);

            m_TempCells = new int[m_Cells.GetLength(0), m_Cells.GetLength(1)];

            for (var num = 1; num <= Size; ++num) {
                m_FinishedMask.SetBit(num);
            }

            m_Candidates = new BitSet32[m_Cells.GetLength(0), m_Cells.GetLength(1)];
        }

        /// <summary>
        /// 克隆一份题板
        /// </summary>
        /// <param name="puzzle"></param>
        /// <returns></returns>
        public int[,] ClonePuzzle(int[,] puzzle)
        {
            var new_puzzle = new int[puzzle.GetLength(0), puzzle.GetLength(1)];
            for (var r = 0; r < puzzle.GetLength(0); ++r) {
                for (var c = 0; c < puzzle.GetLength(1); ++c) {
                    new_puzzle[r, c] = puzzle[r, c];
                }
            }

            return new_puzzle;
        }

        /// <summary>
        /// 验证指定数字的正确性
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool ValidateNumber(int num)
        {
            if (num <= 0 || num > Size) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查是否完成题目
        /// </summary>
        /// <returns></returns>
        public bool CheckFinish()
        {
            var flag = (BitSet32)0;

            // check all rows
            for (var r = 0; r < RowCnt; ++r) {
                flag = 0;

                for (var c = 0; c < ColCnt; ++c) {
                    var number = m_Cells[r, c];
                    if (number == 0) {
                        continue;
                    }

                    flag.SetBit(number);
                }

                // 0000 0011 1111 1110
                if (flag != m_FinishedMask) {
                    return false;
                }
            }

            // check all columns
            for (var c = 0; c < ColCnt; ++c) {
                flag = 0;

                for (var r = 0; r < RowCnt; ++r) {
                    var number = m_Cells[r, c];
                    if (number == 0) {
                        continue;
                    }

                    flag.SetBit(number);
                }

                // 0000 0011 1111 1110
                if (flag != m_FinishedMask) {
                    return false;
                }
            }

            // check all boxes
            var box_cnt = BoxRowCnt;
            for (var i = 0; i < Size; ++i) {
                var box_r = Mathf.FloorToInt(i / box_cnt);
                var box_c = i % box_cnt;

                flag = 0;
                for (var r = box_r * box_cnt; r < box_r * box_cnt + box_cnt; ++r) {
                    for (var c = box_c * box_cnt; c < box_c * box_cnt + box_cnt; ++c) {
                        var number = m_Cells[r, c];
                        if (number == 0) {
                            continue;
                        }

                        flag.SetBit(number);
                    }
                }

                // 0000 0011 1111 1110
                if (flag != m_FinishedMask) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查是否出现错误
        /// 出现错误时，所有错误的位置都会记录在mTempCells中
        /// </summary>
        /// <returns></returns>
        public bool CheckError(int[,] error_cells)
        {
            var has_error = false;

            var flag = (BitSet32)0;
            var error_flag = (BitSet32)0;

            // clear error
            for (var r = 0; r < RowCnt; ++r) {
                for (var c = 0; c < ColCnt; ++c) {
                    error_cells[r, c] = 0;
                }
            }

            // check all rows
            for (var r = 0; r < RowCnt; ++r) {
                flag.Reset();
                error_flag.Reset();

                for (var c = 0; c < ColCnt; ++c) {
                    var number = m_Cells[r, c];
                    if (number == 0) {
                        continue;
                    }

                    if (flag.HasBit(number)) {
                        // error
                        error_flag.SetBit(number);
                    } else {
                        flag.SetBit(number);
                    }
                }

                if (error_flag != 0) {
                    has_error = true;

                    for (var c = 0; c < ColCnt; ++c) {
                        var number = m_Cells[r, c];
                        if (error_flag.HasBit(number)) {
                            error_cells[r, c] = number;
                        }
                    }
                }
            }

            // check all columns
            for (var c = 0; c < ColCnt; ++c) {
                flag = 0;
                error_flag = 0;

                for (var r = 0; r < RowCnt; ++r) {
                    var number = m_Cells[r, c];
                    if (number == 0) {
                        continue;
                    }

                    if (flag.HasBit(number)) {
                        // error
                        error_flag.SetBit(number);
                    } else {
                        flag.SetBit(number);
                    }
                }

                if (error_flag != 0) {
                    has_error = true;

                    for (var r = 0; r < RowCnt; ++r) {
                        var number = m_Cells[r, c];
                        if (error_flag.HasBit(number)) {
                            error_cells[r, c] = number;
                        }
                    }
                }
            }

            // check all boxes
            var box_cnt = BoxRowCnt;
            for (var i = 0; i < Size; ++i) {
                var box_r = Mathf.FloorToInt(i / box_cnt);
                var box_c = i % box_cnt;

                flag = 0;
                error_flag = 0;
                for (var r = box_r * box_cnt; r < box_r * box_cnt + box_cnt; ++r) {
                    for (var c = box_c * box_cnt; c < box_c * box_cnt + box_cnt; ++c) {
                        var number = m_Cells[r, c];
                        if (number == 0) {
                            continue;
                        }

                        if (flag.HasBit(number)) {
                            // error
                            error_flag.SetBit(number);
                        } else {
                            flag.SetBit(number);
                        }
                    }
                }

                if (error_flag != 0) {
                    has_error = true;

                    for (var r = box_r * box_cnt; r < box_r * box_cnt + box_cnt; ++r) {
                        for (var c = box_c * box_cnt; c < box_c * box_cnt + box_cnt; ++c) {
                            var number = m_Cells[r, c];
                            if (error_flag.HasBit(number)) {
                                error_cells[r, c] = number;
                            }
                        }
                    }
                }
            }

            return has_error;
        }

        /// <summary>
        /// 当[row, col]位置填入数字时，更新其关联的所有候选数
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="col">列</param>
        /// <param name="num">填入的数字</param>
        /// <returns></returns>
        protected bool UpdateCandidateHelper(int row, int col, int num)
        {
            SetCandidateImpl(row, col, 0);

            for (var r = 0; r < RowCnt; ++r) {
                RemoveCandidate(r, col, num);
            }

            for (var c = 0; c < ColCnt; ++c) {
                RemoveCandidate(row, c, num);
            }

            var box = RowCol2Box(row, col);
            int box_r, box_c;
            Box2RowCol(box, out box_r, out box_c);

            var box_cnt = BoxRowCnt;
            for (var r = box_r; r < box_r + box_cnt; ++r) {
                for (var c = box_c; c < box_c + box_cnt; ++c) {
                    RemoveCandidate(r, c, num);
                }
            }

            return true;
        }

        /// <summary>
        /// 删除[row, col]位置的候选数num
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        protected bool RemoveCandidate(int row, int col, int num)
        {
            var old_candidates = m_Candidates[row, col];
            
            if (old_candidates.HasBit(num)) { // 该数字之前就填了
                m_Candidates[row, col].UnSetBit(num); // 去掉该数字

                if (OnCandidateChanged != null) {
                    OnCandidateChanged(row, col, old_candidates, m_Candidates[row, col]);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 在[row, col]位置填入候选数num
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="num"></param>
        protected void SetCandidateImpl(int row, int col, int num)
        {
            var old_candidates = m_Candidates[row, col];
            if (num == 0) { // clear
                m_Candidates[row, col].Reset();

                if (!old_candidates.IsEmpty) {
                    if (OnCandidateChanged != null) {
                        OnCandidateChanged(row, col, old_candidates, m_Candidates[row, col]);
                    }
                }
            } else {
                if (old_candidates.HasBit(num)) { // 该数字之前就填了
                    m_Candidates[row, col].UnSetBit(num); // 去掉该数字
                } else {
                    m_Candidates[row, col].SetBit(num); // 添加该数字
                }

                if (OnCandidateChanged != null) {
                    OnCandidateChanged(row, col, old_candidates, m_Candidates[row, col]);
                }
            }
        }

        /// <summary>
        /// 尝试在[row, col]位置填入候选数num
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool TrySetCandidateAt(int row, int col, int num)
        {
            if (!ValidateNumber(num)) {
                return false;
            }

            if (m_Cells[row, col] != 0) {
                return false;
            }

            SetCandidateImpl(row, col, num);

            return true;
        }

        /// <summary>
        /// 在所有未填入数字的格子中填入候选数（已经填了候选数的格子，其候选数会被刷新）
        /// 填入的候选数只根据基本的判断填入所有可以填的候选数，不会采用格外的公式来删减候选数
        /// </summary>
        /// <returns></returns>
        public bool FillAllCandidates()
        {
            // @note
            // 下面算法通过记录每一行、每一列、每一宫内的数字填入情况（记录在对应的bit_set中）
            // 那么[r,c]位置的格子的候选数，就是r行、c列、RowCol2Box(r,c)宫的所有数字的并集再求补集
            // 例如第一行已填入数字[1,3,7,8]，第1列已填入数字[2,4,7]，第1宫已填入数字[2,7,8]
            // 那么对于[1,1]格子来说，其并集为[1,2,3,4,7,8]，其补集（也就是其候选数）为[5,6,9]
            var row_bit_sets = new BitSet32[RowCnt];
            var col_bit_sets = new BitSet32[ColCnt];
            var box_bit_sets = new BitSet32[Size];

            // check all rows
            for (var r = 0; r < RowCnt; ++r) {
                for (var c = 0; c < ColCnt; ++c) {
                    var number = m_Cells[r, c];
                    if (number == 0) {
                        continue;
                    }

                    row_bit_sets[r].SetBit(number);
                }
            }

            // check all columns
            for (var c = 0; c < ColCnt; ++c) {
                for (var r = 0; r < RowCnt; ++r) {
                    var number = m_Cells[r, c];
                    if (number == 0) {
                        continue;
                    }

                    col_bit_sets[c].SetBit(number);
                }
            }

            // check all grids
            var box_cnt = BoxRowCnt;
            for (var i = 0; i < Size; ++i) {
                var box_r = Mathf.FloorToInt(i / box_cnt);
                var box_c = i % box_cnt;

                for (var r = box_r * box_cnt; r < box_r * box_cnt + box_cnt; ++r) {
                    for (var c = box_c * box_cnt; c < box_c * box_cnt + box_cnt; ++c) {
                        var number = m_Cells[r, c];
                        if (number == 0) {
                            continue;
                        }

                        box_bit_sets[i].SetBit(number);
                    }
                }
            }

            for (var r = 0; r < RowCnt; ++r) {
                for (var c = 0; c < ColCnt; ++c) {
                    var number = m_Cells[r, c];
                    if (number != 0) {
                        continue;
                    }

                    var diff = m_FinishedMask - (row_bit_sets[r] + col_bit_sets[c] + box_bit_sets[RowCol2Box(r, c)]);

                    var old_candidates = m_Candidates[r, c];
                    m_Candidates[r, c] = diff;

                    if (OnCandidateChanged != null) {
                        OnCandidateChanged(r, c, old_candidates, m_Candidates[r, c]);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 在[row,col]位置填入数字
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="num"></param>
        protected void SetCellImpl(int row, int col, int num)
        {
            var old_num = this[row, col];
            if (old_num == num) {
                num = 0;
            }

            this[row, col] = num;

            if (OnCellChanged != null) {
                OnCellChanged(row, col, old_num, num);
            }
        }

        /// <summary>
        /// 尝试在[row,col]位置填入数字num
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool TrySetCellAt(int row, int col, int num)
        {
            if (!ValidateNumber(num)) {
                return false;
            }

            if (m_GivenCells[row, col] != 0) {
                return false;
            }

            SetCellImpl(row, col, num);
            UpdateCandidateHelper(row, col, num);

            if (CheckFinish()) {
                // clear error
                if (m_HasError) {
                    m_HasError = false;

                    if (OnError != null) {
                        OnError(null);
                    }
                }

                m_IsFinished = true;
                if (OnFinished != null) {
                    OnFinished();
                }
            }
            else {
                if (CheckError(m_TempCells)) {
                    m_HasError = true;

                    if (OnError != null) {
                        OnError(m_TempCells);
                    }
                } else if (m_HasError) {
                    m_HasError = false;

                    if (OnError != null) {
                        OnError(null);
                    }
                }
            }

            return true;
        }

        public bool DeleteCell(int row, int col)
        {
            SetCellImpl(row, col, 0);
            return true;
        }

        #region utility
        public int RowCol2Box(int row, int col)
        {
            var box_cnt = BoxRowCnt;
            var box_r = Mathf.FloorToInt(row / box_cnt);
            var box_c = Mathf.FloorToInt(col / box_cnt);

            return box_r * box_cnt + box_c;
        }

        public void Box2RowCol(int box, out int row, out int col)
        {
            var box_cnt = BoxRowCnt;
            var box_r = Mathf.FloorToInt(box / box_cnt);
            var box_c = box % box_cnt;

            row = box_r * box_cnt;
            col = box_c * box_cnt;
        }

        #endregion

        public object Clone()
        {
            var ret = new Puzzle
            {
                m_GivenCells = ClonePuzzle(m_GivenCells),
                m_Cells = ClonePuzzle(m_Cells),

                m_TempCells = new int[RowCnt, ColCnt],
                m_FinishedMask = m_FinishedMask,

                m_HasError = m_HasError,

                OnCellChanged = OnCellChanged,
                OnError = OnError,
                OnFinished = OnFinished,
                OnCandidateChanged = OnCandidateChanged,

                m_Candidates = new BitSet32[RowCnt, ColCnt]
            };

            for (var r = 0; r < RowCnt; ++r) {
                for (var c = 0; c < ColCnt; ++c) {
                    ret.m_Candidates[r, c] = m_Candidates[r, c];
                }
            }

            return ret;
        }
    }
}
