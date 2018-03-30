using System.Collections;
using UnityEngine;

namespace sudoku.data
{
    public class PuzzleAutoSolver
    {
        protected Puzzle m_Puzzle;

        protected BitSet128 m_FinishedGridMask;
        protected BitSet128[] m_RowMasks;
        protected BitSet128[] m_ColMasks;
        protected BitSet128[] m_BoxMasks;

        protected BitSet32 m_FinishedDigitMask;

        protected class DigitInfo
        {
            public BitSet128 grid;
            public BitSet32[] rows;
            public BitSet32[] cols;
        }
        protected DigitInfo[] m_DigitInfos;

        public PuzzleAutoSolver(Puzzle puzzle)
        {
            m_Puzzle = puzzle;

            Init();
        }

        protected int RowCol2Index(int row, int col)
        {
            return row * m_Puzzle.Size + col;
        }

        protected void Index2RowCol(int idx, out int row, out int col)
        {
            row = Mathf.FloorToInt(idx / m_Puzzle.Size);
            col = idx % m_Puzzle.Size;
        }

        protected void Init()
        {
            m_RowMasks = new BitSet128[m_Puzzle.Size];
            m_ColMasks = new BitSet128[m_Puzzle.Size];
            m_BoxMasks = new BitSet128[m_Puzzle.Size];

            for (var r = 0; r < m_Puzzle.RowCnt; ++r) {
                for (var c = 0; c < m_Puzzle.ColCnt; ++ c) {
                    var idx = RowCol2Index(r, c);
                    m_RowMasks[r].SetBit(idx);
                    m_ColMasks[c].SetBit(idx);
                    m_BoxMasks[m_Puzzle.RowCol2Box(r, c)].SetBit(idx);

                    m_FinishedGridMask.SetBit(idx);
                }
            }

            for (var digit = 1; digit <= m_Puzzle.Size; ++digit) {
                m_FinishedDigitMask.SetBit(digit);
            }
        }

        protected void Prepare()
        {
            if (m_DigitInfos == null) {
                m_DigitInfos = new DigitInfo[m_Puzzle.Size];
                for (var i = 0; i < m_DigitInfos.Length; ++i) {
                    m_DigitInfos[i] = new DigitInfo
                    {
                        rows = new BitSet32[m_Puzzle.Size],
                        cols = new BitSet32[m_Puzzle.Size]
                    };
                }
            }
            else {
                for (var i = 0; i < m_DigitInfos.Length; ++i) {
                    m_DigitInfos[i].grid.Reset();

                    for (var r = 0; r < m_DigitInfos[i].rows.Length; ++r) {
                        m_DigitInfos[i].rows[r].Reset();
                    }
                    for (var c = 0; c < m_DigitInfos[i].cols.Length; ++c) {
                        m_DigitInfos[i].cols[c].Reset();
                    }
                }
            }

            // mDigitInfos[i].grid保存了数字i+1，其候选数在题板上的分布情况
            // 注意这里的下标是从0开始的，也就是说mDigitInfos[0]是数字1的分布情况
            for (var r = 0; r < m_Puzzle.RowCnt; ++r) {
                for (var c = 0; c < m_Puzzle.ColCnt; ++c) {
                    var candidates = m_Puzzle.Candidates[r, c];
                    if (candidates == 0) {
                        continue;
                    }

                    var idx = RowCol2Index(r, c);
                    foreach (var digit in BitSet32.AllBits(candidates, m_Puzzle.Size)) {
                        m_DigitInfos[digit - 1].grid.SetBit(idx);
                        m_DigitInfos[digit - 1].rows[r].SetBit(c);
                        m_DigitInfos[digit - 1].cols[c].SetBit(r);
                    }
                }
            }
        }

        protected bool CheckError()
        {
            for (var r = 0; r < m_Puzzle.RowCnt; ++r) {
                for (var c = 0; c < m_Puzzle.ColCnt; ++c) {
                    if (m_Puzzle.GivenCells[r, c] == 0 && m_Puzzle[r, c] == 0 && m_Puzzle.Candidates[r, c] == 0) {
                        return true;
                    }
                }
            }

            return false;
        }

        public IEnumerator Run()
        {
            m_Puzzle.FillAllCandidates();

            int step = 1;
            while (!m_Puzzle.IsFinished) {
                yield return new WaitForSeconds(0.2f);

                if (CheckError()) {
                    Debug.Log("CheckError");
                    break;
                }

                //Debug.Log(string.Format("Step " + step));
                step++;

                if (TryNakedSingle()) {
                    Debug.Log("TryNakedSingle");
                    continue;
                }

                Prepare();

                if (TryHiddenSingle()) {
                    Debug.Log("TryHiddenSingle");
                    continue;
                }

                if (TryHiddenPairs()) {
                    Debug.Log("TryHiddenPairs");
                    continue;
                }

                if (TryHiddenTripe()) {
                    Debug.Log("TryHiddenTripe");
                    continue;
                }

                if (TryHiddenQuad()) {
                    Debug.Log("TryHiddenQuad");
                    continue;
                }

                if (TryLockedCandidates()) {
                    Debug.Log("TryLockedCandidates");
                    continue;
                }

                if (TryNakedPairs()) {
                    Debug.Log("TryNakedPairs");
                    continue;
                }

                if (TryNakedTriple()) {
                    Debug.Log("TryNakedTriple");
                    continue;
                }

                if (TryNakedQuad()) {
                    Debug.Log("TryNakedQuad");
                    continue;
                }

                // begin try fish pattern
                if (TryXWing()) {
                    Debug.Log("TryXWing");
                    continue;
                }

                if (TrySkyscraper()) {
                    Debug.Log("TrySkyscraper");
                    continue;
                }

                if (TrySwordfish()) {
                    Debug.Log("TrySwordfish");
                    continue;
                }

                if (TryJellyfish()) {
                    Debug.Log("TryJellyfish");
                    continue;
                }
                // end try finsh pattern

                break;
            }
        }

        protected bool EliminateSingleCandidate(BitSet128 eliminated, int digit)
        {
            var ret = false;
            foreach (var idx in BitSet128.AllBits(eliminated)) {
                int row, col;
                Index2RowCol(idx, out row, out col);

                var candidates = m_Puzzle.Candidates[row, col];
                if (candidates.HasBit(digit)) {
                    // 填入数字后，再次填入是删除操作。。
                    m_Puzzle.TrySetCandidateAt(row, col, digit);
                    ret = true;
                }
            }

            return ret;
        }

        protected bool EliminateCandidates(BitSet128 eliminated, BitSet32 candidates)
        {
            var ret = false;
            foreach (var idx in BitSet128.AllBits(eliminated)) {
                int row, col;
                Index2RowCol(idx, out row, out col);

                var cur_candidates = m_Puzzle.Candidates[row, col];
                var need_eliminate_candidates = cur_candidates.Intersect(candidates);

                if (!need_eliminate_candidates.IsEmpty) {
                    ret = true;
                    foreach (var digit in BitSet32.AllBits(need_eliminate_candidates, m_Puzzle.Size)) {
                        // 填入数字后，再次填入是删除操作。。
                        m_Puzzle.TrySetCandidateAt(row, col, digit);
                    }
                }
            }

            return ret;
        }

        #region naked single

        protected bool TryNakedSingle()
        {
            // naked single

            // 检查是否存在一个格子内只有一个可选数字的情况
            for (var r = 0; r < m_Puzzle.RowCnt; ++r) {
                for (var c = 0; c < m_Puzzle.ColCnt; ++c) {
                    var candidates = m_Puzzle.Candidates[r, c];
                    if (candidates.IsEmpty || candidates.BitCount > 1) {
                        continue;
                    }

                    foreach (var digit in BitSet32.AllBits(candidates, m_Puzzle.Size)) {
                        m_Puzzle.TrySetCellAt(r, c, digit);
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region hidden single
        protected bool HiddenSingle_CheckIntersect(int digit, BitSet128 digit_grid, BitSet128[] sub_sets)
        {
            for (var i = 0; i < sub_sets.Length; ++i) {
                var intersect_set = sub_sets[i].Intersect(digit_grid);
                if (intersect_set.IsEmpty) {
                    continue;
                }

                if (intersect_set.BitCount == 1) {
                    // find
                    foreach (var idx in BitSet128.AllBits(intersect_set)) {
                        int row, col;
                        Index2RowCol(idx, out row, out col);

                        m_Puzzle.TrySetCellAt(row, col, digit);
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool TryHiddenSingle()
        {
            // hidden single

            for (var digit = 1; digit <= m_Puzzle.Size; ++digit) {
                var digit_grid = m_DigitInfos[digit - 1].grid;
                if (digit_grid.IsEmpty) {
                    continue;
                }

                // 检查是否存在某一行，其中有某一个数字只能填在一个格子的情况
                if (HiddenSingle_CheckIntersect(digit, digit_grid, m_RowMasks)) {
                    return true;
                }

                // 检查是否存在某一列，其中有某一个数字只能填在一个格子的情况
                if (HiddenSingle_CheckIntersect(digit, digit_grid, m_ColMasks)) {
                    return true;
                }

                // 检查是否存在某一宫，其中有某一个数字只能填在一个格子的情况
                if (HiddenSingle_CheckIntersect(digit, digit_grid, m_BoxMasks)) {
                    return true;
                }
            }
            
            return false;
        }
        #endregion

        #region hidden pairs
        protected bool HiddenPairs_CheckIntersect(BitSet128[] sub_sets)
        {
            for (var i = 0; i < sub_sets.Length; ++i) {
                for (var digit = 1; digit <= m_Puzzle.Size - 1; ++digit) {
                    var digit_grid = m_DigitInfos[digit - 1].grid;
                    if (digit_grid.IsEmpty) {
                        continue;
                    }

                    var intersect_set = sub_sets[i].Intersect(digit_grid);
                    if (intersect_set.IsEmpty || intersect_set.BitCount != 2) {
                        continue;
                    }

                    for (var digit2 = digit + 1; digit2 <= m_Puzzle.Size; ++digit2) {
                        var digit2_grid = m_DigitInfos[digit2 - 1].grid;
                        if (digit2_grid.IsEmpty) {
                            continue;
                        }

                        var intersect_set2 = sub_sets[i].Intersect(digit2_grid);
                        if (intersect_set == intersect_set2) {
                            // find pair
                            // check if pair is hidden
                            var pair_set = (BitSet32)0;
                            pair_set.SetBit(digit);
                            pair_set.SetBit(digit2);

                            var reverse_pair_set = m_FinishedDigitMask - pair_set;
                            if (EliminateCandidates(intersect_set, reverse_pair_set)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected bool TryHiddenPairs()
        {
            if (HiddenPairs_CheckIntersect(m_RowMasks)) {
                return true;
            }

            if (HiddenPairs_CheckIntersect(m_ColMasks)) {
                return true;
            }

            if (HiddenPairs_CheckIntersect(m_BoxMasks)) {
                return true;
            }

            return false;
        }
        #endregion

        #region hidden triple
        protected bool HiddenTriple_CheckIntersect(BitSet128[] sub_sets)
        {
            for (var i = 0; i < sub_sets.Length; ++i) {
                for (var d1 = 1; d1 <= m_Puzzle.Size - 2; ++d1) { // d for digit
                    var d1_grid = m_DigitInfos[d1 - 1].grid;
                    if (d1_grid.IsEmpty) {
                        continue;
                    }

                    var d1_set = sub_sets[i].Intersect(d1_grid);
                    if (d1_set.IsEmpty) {
                        continue;
                    }

                    var d1_bit_count = d1_set.BitCount;
                    if (d1_bit_count < 2 || d1_bit_count > 3) {
                        continue;
                    }

                    for (var d2 = d1 + 1; d2 <= m_Puzzle.Size - 1; ++d2) {
                        var d2_grid = m_DigitInfos[d2 - 1].grid;
                        if (d2_grid.IsEmpty) {
                            continue;
                        }

                        var d2_set = sub_sets[i].Intersect(d2_grid);
                        if (d2_set.IsEmpty) {
                            continue;
                        }

                        var d2_bit_count = d2_set.BitCount;
                        if (d2_bit_count < 2 || d2_bit_count > 3) {
                            continue;
                        }

                        var d1_d2_set = d1_set.Union(d2_set);
                        if (d1_d2_set.BitCount != 3) {
                            continue;
                        }

                        for (var d3 = d2 + 1; d3 <= m_Puzzle.Size; ++d3) {
                            var d3_grid = m_DigitInfos[d3 - 1].grid;
                            if (d3_grid.IsEmpty) {
                                continue;
                            }

                            var d3_set = sub_sets[i].Intersect(d3_grid);
                            if (d3_set.IsEmpty) {
                                continue;
                            }

                            var d3_bit_count = d3_set.BitCount;
                            if (d3_bit_count < 2 || d3_bit_count > 3) {
                                continue;
                            }

                            var d1_d2_d3_set = d1_d2_set.Union(d3_set);
                            if (d1_d2_d3_set.BitCount == 3) {
                                // find triple
                                // check if triple is hidden
                                var triple_set = (BitSet32)0;
                                triple_set.SetBit(d1);
                                triple_set.SetBit(d2);
                                triple_set.SetBit(d3);

                                var reverse_triple_set = m_FinishedDigitMask - triple_set;
                                if (EliminateCandidates(d1_d2_d3_set, reverse_triple_set)) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected bool TryHiddenTripe()
        {
            if (HiddenTriple_CheckIntersect(m_RowMasks)) {
                return true;
            }

            if (HiddenTriple_CheckIntersect(m_ColMasks)) {
                return true;
            }

            if (HiddenTriple_CheckIntersect(m_BoxMasks)) {
                return true;
            }

            return false;
        }
        #endregion

        #region hidden quad
        protected bool HiddenQuad_CheckIntersect(BitSet128[] sub_sets)
        {
            for (var i = 0; i < sub_sets.Length; ++i) {
                for (var d1 = 1; d1 <= m_Puzzle.Size - 3; ++d1) { // d for digit
                    var d1_grid = m_DigitInfos[d1 - 1].grid;
                    if (d1_grid.IsEmpty) {
                        continue;
                    }

                    var d1_set = sub_sets[i].Intersect(d1_grid);
                    if (d1_set.IsEmpty) {
                        continue;
                    }

                    var d1_bit_count = d1_set.BitCount;
                    if (d1_bit_count < 2 || d1_bit_count > 4) {
                        continue;
                    }

                    for (var d2 = d1 + 1; d2 <= m_Puzzle.Size - 2; ++d2) {
                        var d2_grid = m_DigitInfos[d2 - 1].grid;
                        if (d2_grid.IsEmpty) {
                            continue;
                        }

                        var d2_set = sub_sets[i].Intersect(d2_grid);
                        if (d2_set.IsEmpty) {
                            continue;
                        }

                        var d2_bit_count = d2_set.BitCount;
                        if (d2_bit_count < 2 || d2_bit_count > 4) {
                            continue;
                        }

                        var d1_d2_set = d1_set.Union(d2_set);
                        var d1_d2_bit_count = d1_d2_set.BitCount;
                        if (d1_d2_bit_count < 3 || d1_d2_bit_count > 4) {
                            continue;
                        }

                        for (var d3 = d2 + 1; d3 <= m_Puzzle.Size - 1; ++d3) {
                            var d3_grid = m_DigitInfos[d3 - 1].grid;
                            if (d3_grid.IsEmpty) {
                                continue;
                            }

                            var d3_set = sub_sets[i].Intersect(d3_grid);
                            if (d3_set.IsEmpty) {
                                continue;
                            }

                            var d3_bit_count = d3_set.BitCount;
                            if (d3_bit_count < 2 || d3_bit_count > 4) {
                                continue;
                            }

                            var d1_d2_d3_set = d1_d2_set.Union(d3_set);
                            if (d1_d2_d3_set.BitCount != 4) {
                                continue;
                            }

                            for (var d4 = d3 + 1; d4 <= m_Puzzle.Size; ++d4) {
                                var d4_grid = m_DigitInfos[d4 - 1].grid;
                                if (d4_grid.IsEmpty) {
                                    continue;
                                }

                                var d4_set = sub_sets[i].Intersect(d4_grid);
                                if (d4_set.IsEmpty) {
                                    continue;
                                }

                                var d4_bit_count = d4_set.BitCount;
                                if (d4_bit_count < 2 || d4_bit_count > 4) {
                                    continue;
                                }

                                var d1_d2_d3_d4_set = d1_d2_d3_set.Union(d4_set);
                                if (d1_d2_d3_d4_set.BitCount == 4) {
                                    // find quad
                                    // check if quad is hidden
                                    var quad_set = (BitSet32)0;
                                    quad_set.SetBit(d1);
                                    quad_set.SetBit(d2);
                                    quad_set.SetBit(d3);
                                    quad_set.SetBit(d4);

                                    var reverse_quad_set = m_FinishedDigitMask - quad_set;
                                    if (EliminateCandidates(d1_d2_d3_d4_set, reverse_quad_set)) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected bool TryHiddenQuad()
        {
            if (HiddenQuad_CheckIntersect(m_RowMasks)) {
                return true;
            }

            if (HiddenQuad_CheckIntersect(m_ColMasks)) {
                return true;
            }

            if (HiddenQuad_CheckIntersect(m_BoxMasks)) {
                return true;
            }

            return false;
        }
        #endregion

        #region naked pairs
        protected bool NakedPairs_CheckIntersect(BitSet128[] sub_sets)
        {
            for (var i = 0; i < sub_sets.Length; ++i) {
                for (var digit = 1; digit <= m_Puzzle.Size - 1; ++digit) {
                    var digit_grid = m_DigitInfos[digit - 1].grid;
                    if (digit_grid.IsEmpty) {
                        continue;
                    }

                    var intersect_set = sub_sets[i].Intersect(digit_grid);
                    if (intersect_set.IsEmpty || intersect_set.BitCount != 2) {
                        continue;
                    }

                    for (var digit2 = digit + 1; digit2 <= m_Puzzle.Size; ++digit2) {
                        var digit2_grid = m_DigitInfos[digit2 - 1].grid;
                        if (digit2_grid.IsEmpty) {
                            continue;
                        }

                        var intersect_set2 = sub_sets[i].Intersect(digit2_grid);
                        if (intersect_set == intersect_set2) {
                            // find pair
                            // check if pair is naked
                            var pair_set = (BitSet32)0;
                            pair_set.SetBit(digit);
                            pair_set.SetBit(digit2);

                            bool is_naked_pair = true;
                            foreach (var idx in BitSet128.AllBits(intersect_set)) {
                                int row, col;
                                Index2RowCol(idx, out row, out col);

                                var candidates = m_Puzzle.Candidates[row, col];
                                if (candidates != pair_set) {
                                    is_naked_pair = false;
                                    break;
                                }
                            }

                            if (is_naked_pair) {
                                var eliminated = sub_sets[i] - intersect_set;
                                if (EliminateCandidates(eliminated, pair_set)) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected bool TryNakedPairs()
        {
            if (NakedPairs_CheckIntersect(m_RowMasks)) {
                return true;
            }

            if (NakedPairs_CheckIntersect(m_ColMasks)) {
                return true;
            }

            if (NakedPairs_CheckIntersect(m_BoxMasks)) {
                return true;
            }

            return false;
        }
        #endregion

        #region naked triple
        protected bool NakedTriple_CheckIntersect(BitSet128[] sub_sets)
        {
            for (var i = 0; i < sub_sets.Length; ++i) {
                for (var d1 = 1; d1 <= m_Puzzle.Size - 2; ++d1) { // d for digit
                    var d1_grid = m_DigitInfos[d1 - 1].grid;
                    if (d1_grid.IsEmpty) {
                        continue;
                    }

                    var d1_set = sub_sets[i].Intersect(d1_grid);
                    if (d1_set.IsEmpty) {
                        continue;
                    }

                    var d1_bit_count = d1_set.BitCount;
                    if (d1_bit_count < 2 || d1_bit_count > 3) {
                        continue;
                    }

                    for (var d2 = d1 + 1; d2 <= m_Puzzle.Size - 1; ++d2) {
                        var d2_grid = m_DigitInfos[d2 - 1].grid;
                        if (d2_grid.IsEmpty) {
                            continue;
                        }

                        var d2_set = sub_sets[i].Intersect(d2_grid);
                        if (d2_set.IsEmpty) {
                            continue;
                        }

                        var d2_bit_count = d2_set.BitCount;
                        if (d2_bit_count < 2 || d2_bit_count > 3) {
                            continue;
                        }

                        var d1_d2_set = d1_set.Union(d2_set);
                        if (d1_d2_set.BitCount != 3) {
                            continue;
                        }

                        for (var d3 = d2 + 1; d3 <= m_Puzzle.Size; ++d3) {
                            var d3_grid = m_DigitInfos[d3 - 1].grid;
                            if (d3_grid.IsEmpty) {
                                continue;
                            }

                            var d3_set = sub_sets[i].Intersect(d3_grid);
                            if (d3_set.IsEmpty) {
                                continue;
                            }

                            var d3_bit_count = d3_set.BitCount;
                            if (d3_bit_count < 2 || d3_bit_count > 3) {
                                continue;
                            }

                            var d1_d2_d3_set = d1_d2_set.Union(d3_set);
                            if (d1_d2_d3_set.BitCount == 3) {
                                // find triple
                                var triple_set = (BitSet32)0;
                                triple_set.SetBit(d1);
                                triple_set.SetBit(d2);
                                triple_set.SetBit(d3);

                                var eliminated = sub_sets[i] - d1_d2_d3_set;
                                if (EliminateCandidates(eliminated, triple_set)) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected bool TryNakedTriple()
        {
            if (NakedTriple_CheckIntersect(m_RowMasks)) {
                return true;
            }

            if (NakedTriple_CheckIntersect(m_ColMasks)) {
                return true;
            }

            if (NakedTriple_CheckIntersect(m_BoxMasks)) {
                return true;
            }

            return false;
        }
        #endregion

        #region naked quad
        protected bool NakedQuad_CheckIntersect(BitSet128[] sub_sets)
        {
            for (var i = 0; i < sub_sets.Length; ++i) {
                for (var d1 = 1; d1 <= m_Puzzle.Size - 3; ++d1) { // d for digit
                    var d1_grid = m_DigitInfos[d1 - 1].grid;
                    if (d1_grid.IsEmpty) {
                        continue;
                    }

                    var d1_set = sub_sets[i].Intersect(d1_grid);
                    if (d1_set.IsEmpty) {
                        continue;
                    }

                    var d1_bit_count = d1_set.BitCount;
                    if (d1_bit_count < 2 || d1_bit_count > 4) {
                        continue;
                    }

                    for (var d2 = d1 + 1; d2 <= m_Puzzle.Size - 2; ++d2) {
                        var d2_grid = m_DigitInfos[d2 - 1].grid;
                        if (d2_grid.IsEmpty) {
                            continue;
                        }

                        var d2_set = sub_sets[i].Intersect(d2_grid);
                        if (d2_set.IsEmpty) {
                            continue;
                        }

                        var d2_bit_count = d2_set.BitCount;
                        if (d2_bit_count < 2 || d2_bit_count > 4) {
                            continue;
                        }

                        var d1_d2_set = d1_set.Union(d2_set);
                        var d1_d2_bit_count = d1_d2_set.BitCount;
                        if (d1_d2_bit_count < 3 || d1_d2_bit_count > 4) {
                            continue;
                        }

                        for (var d3 = d2 + 1; d3 <= m_Puzzle.Size - 1; ++d3) {
                            var d3_grid = m_DigitInfos[d3 - 1].grid;
                            if (d3_grid.IsEmpty) {
                                continue;
                            }

                            var d3_set = sub_sets[i].Intersect(d3_grid);
                            if (d3_set.IsEmpty) {
                                continue;
                            }

                            var d3_bit_count = d3_set.BitCount;
                            if (d3_bit_count < 2 || d3_bit_count > 4) {
                                continue;
                            }

                            var d1_d2_d3_set = d1_d2_set.Union(d3_set);
                            if (d1_d2_d3_set.BitCount != 4) {
                                continue;
                            }

                            for (var d4 = d3 + 1; d4 <= m_Puzzle.Size; ++d4) {
                                var d4_grid = m_DigitInfos[d4 - 1].grid;
                                if (d4_grid.IsEmpty) {
                                    continue;
                                }

                                var d4_set = sub_sets[i].Intersect(d4_grid);
                                if (d4_set.IsEmpty) {
                                    continue;
                                }

                                var d4_bit_count = d4_set.BitCount;
                                if (d4_bit_count < 2 || d4_bit_count > 4) {
                                    continue;
                                }

                                var d1_d2_d3_d4_set = d1_d2_d3_set.Union(d4_set);
                                if (d1_d2_d3_d4_set.BitCount == 4) {
                                    // find quad
                                    // check if quad is hidden
                                    var quad_set = (BitSet32)0;
                                    quad_set.SetBit(d1);
                                    quad_set.SetBit(d2);
                                    quad_set.SetBit(d3);
                                    quad_set.SetBit(d4);

                                    var eliminated = sub_sets[i] - d1_d2_d3_d4_set;
                                    if (EliminateCandidates(eliminated, quad_set)) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected bool TryNakedQuad()
        {
            if (NakedQuad_CheckIntersect(m_RowMasks)) {
                return true;
            }

            if (NakedQuad_CheckIntersect(m_ColMasks)) {
                return true;
            }

            if (NakedQuad_CheckIntersect(m_BoxMasks)) {
                return true;
            }

            return false;
        }
        #endregion

        #region locked candidates
        protected bool TryLockedCandidates()
        {
            // check Type-1 (Pointing)
            for (var box = 0; box < m_Puzzle.Size; ++box) {
                for (var r = 0; r < m_Puzzle.RowCnt; ++r) {
                    var intersect_set = m_BoxMasks[box].Intersect(m_RowMasks[r]);
                    // 判断当前宫和当前行是否有交集
                    if (intersect_set.IsEmpty) {
                        continue;
                    }

                    for (var digit = 1; digit <= m_Puzzle.Size; ++digit) {
                        var grid_flag = m_DigitInfos[digit - 1].grid;

                        // 判断当前数字是否已经没有候选数了
                        if (grid_flag.IsEmpty) {
                            continue;
                        }

                        var box_flag = grid_flag.Intersect(m_BoxMasks[box]);
                        if (box_flag.IsEmpty) {
                            continue;
                        }

                        var diff = box_flag - intersect_set;
                        if (diff.IsEmpty) {
                            // 当前数字，在当前宫中只存在于当前行内
                            // 则可以把当前行内其他格子对当前数字的候选数删掉
                            var eliminated = m_RowMasks[r] - intersect_set;
                            if (EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }

                for (var c = 0; c < m_Puzzle.ColCnt; ++c) {
                    var intersect_set = m_BoxMasks[box].Intersect(m_ColMasks[c]);
                    if (intersect_set.IsEmpty) {
                        continue;
                    }

                    for (var digit = 1; digit <= m_Puzzle.Size; ++digit) {
                        var grid_flag = m_DigitInfos[digit - 1].grid;
                        if (grid_flag.IsEmpty) {
                            continue;
                        }

                        var box_flag = grid_flag.Intersect(m_BoxMasks[box]);
                        if (box_flag.IsEmpty) {
                            continue;
                        }

                        var diff = box_flag - intersect_set;
                        if (diff.IsEmpty) {
                            // 当前数字，在当前宫中只存在于当前列内
                            // 则可以把当前列内其他格子对当前数字的候选数删掉
                            var eliminated = m_ColMasks[c] - intersect_set;
                            if (EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }
            }

            // Type 2 (Claiming or Box-Line Reduction)
            for (var r = 0; r < m_Puzzle.RowCnt; ++r) {
                for (var box = 0; box < m_Puzzle.Size; ++box) {
                    var intersect_set = m_RowMasks[r].Intersect(m_BoxMasks[box]);
                    if (intersect_set.IsEmpty) {
                        continue;
                    }

                    for (var digit = 1; digit <= m_Puzzle.Size; ++digit) {
                        var grid_flag = m_DigitInfos[digit - 1].grid;
                        if (grid_flag.IsEmpty) {
                            continue;
                        }

                        var row_flag = grid_flag.Intersect(m_RowMasks[r]);
                        if (row_flag.IsEmpty) {
                            continue;
                        }

                        var diff = row_flag - intersect_set;
                        if (diff.IsEmpty) {
                            // 当前数字，在当前行中只存在于当前宫内
                            // 则可以把当前宫内其他格子对当前数字的候选数删掉
                            var eliminated = m_BoxMasks[box] - intersect_set;
                            if (EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }
            }

            for (var c = 0; c < m_Puzzle.ColCnt; ++c) {
                for (var box = 0; box < m_Puzzle.Size; ++box) {
                    var intersect_set = m_ColMasks[c].Intersect(m_BoxMasks[box]);
                    if (intersect_set.IsEmpty) {
                        continue;
                    }

                    for (var digit = 1; digit <= m_Puzzle.Size; ++digit) {
                        var grid_flag = m_DigitInfos[digit - 1].grid;
                        if (grid_flag.IsEmpty) {
                            continue;
                        }

                        var col_flag = grid_flag.Intersect(m_ColMasks[c]);
                        if (col_flag.IsEmpty) {
                            continue;
                        }

                        var diff = col_flag - intersect_set;
                        if (diff.IsEmpty) {
                            // 当前数字，在当前列中只存在于当前宫内
                            // 则可以把当前宫内其他格子对当前数字的候选数删掉
                            var eliminated = m_BoxMasks[box] - intersect_set;
                            if (EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        #region x-wing
        protected bool TryXWing()
        {
            for (var digit = 1; digit <= m_Puzzle.Size; ++digit) {
                var digit_info = m_DigitInfos[digit - 1];
                if (digit_info.grid.IsEmpty) {
                    continue;
                }

                // x-wing at row
                for (var r1 = 0; r1 < digit_info.rows.Length - 1; ++r1) {
                    var r1_set = digit_info.rows[r1];
                    if (r1_set.IsEmpty || r1_set.BitCount != 2) {
                        continue;
                    }

                    for (var r2 = r1 + 1; r2 < digit_info.rows.Length; ++r2) {
                        var r2_set = digit_info.rows[r2];
                        if (r1_set == r2_set) {
                            // find
                            // 计算组成x-wing的4个格子，记录在intersect
                            var r1_intersect = digit_info.grid.Intersect(m_RowMasks[r1]);
                            var r2_intersect = digit_info.grid.Intersect(m_RowMasks[r2]);
                            var intersect = r1_intersect.Union(r2_intersect);

                            // 计算要剔除的2列
                            var eliminated = new BitSet128();
                            foreach (var c in BitSet32.AllBits(r1_set, m_Puzzle.Size)) {
                                eliminated = eliminated.Union(m_ColMasks[c]);
                            }

                            // 从2列中去掉intersect，就得出了最终要剔除的格子
                            eliminated = eliminated.Minus(intersect);
                            if (EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }

                // x-wing at column
                for (var c1 = 0; c1 < digit_info.cols.Length - 1; ++c1) {
                    var c1_set = digit_info.cols[c1];
                    if (c1_set.IsEmpty || c1_set.BitCount != 2) {
                        continue;
                    }

                    for (var c2 = c1 + 1; c2 < digit_info.cols.Length; ++c2) {
                        var c2_set = digit_info.cols[c2];
                        if (c1_set == c2_set) {
                            // find
                            // 计算组成x-wing的4个格子，记录在intersect
                            var c1_intersect = digit_info.grid.Intersect(m_ColMasks[c1]);
                            var c2_intersect = digit_info.grid.Intersect(m_ColMasks[c2]);
                            var intersect = c1_intersect.Union(c2_intersect);

                            // 计算要剔除的2行
                            var eliminated = new BitSet128();
                            foreach (var r in BitSet32.AllBits(c1_set, m_Puzzle.Size)) {
                                eliminated = eliminated.Union(m_RowMasks[r]);
                            }

                            // 从2列中去掉intersect，就得出了最终要剔除的格子
                            eliminated = eliminated.Minus(intersect);
                            if (EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        #region swordfish

        /// <summary>
        /// @note 本函数自身是不完备的，也就是说如果直接调用本函数，可以能出错误的sowrdfish
        /// 但如果在找不到x-wing的情况下，本函数就能保证能出正确结果。理由如下：
        /// 
        /// 下面以swordfish的行形式说明
        /// swordfish本身除了要求候选数覆盖3行3列外，其实还要求每一列至少有2个元素，例如下面这种情况是不属于swordfish的
        /// **
        /// * *
        /// **
        /// 可以看到其实1，3行组成了x-wing，但由于已经检查过x-wing了，所以不会出现这种情况
        /// </summary>
        /// <returns></returns>
        protected bool TrySwordfish()
        {
            for (var digit = 1; digit <= m_Puzzle.Size; ++digit) {
                var digit_info = m_DigitInfos[digit - 1];
                if (digit_info.grid.IsEmpty) {
                    continue;
                }

                // swordfish at row
                for (var r1 = 0; r1 < digit_info.rows.Length - 2; ++r1) {
                    var r1_set = digit_info.rows[r1];
                    if (r1_set.IsEmpty) {
                        continue;
                    }

                    var r1_col_count = r1_set.BitCount;
                    if (r1_col_count < 2 || r1_col_count > 3) {
                        continue;
                    }

                    for (var r2 = r1 + 1; r2 < digit_info.rows.Length - 1; ++r2) {
                        var r2_set = digit_info.rows[r2];
                        if (r2_set.IsEmpty) {
                            continue;
                        }

                        var r2_col_count = r2_set.BitCount;
                        if (r2_col_count < 2 || r2_col_count > 3) {
                            continue;
                        }

                        var r1_r2_set = r1_set.Union(r2_set);
                        if (r1_r2_set.BitCount != 3) {
                            continue;
                        }

                        for (var r3 = r2 + 1; r3 < digit_info.rows.Length; ++r3) {
                            var r3_set = digit_info.rows[r3];
                            if (r3_set.IsEmpty) {
                                continue;
                            }

                            var r3_col_count = r3_set.BitCount;
                            if (r3_col_count < 2 || r3_col_count > 3) {
                                continue;
                            }

                            var r1_r2_r3_set = r1_r2_set.Union(r3_set);
                            if (r1_r2_r3_set.BitCount == 3) {
                                // find
                                // 计算组成swordfish的6-9个格子，记录在intersect
                                var r1_intersect = digit_info.grid.Intersect(m_RowMasks[r1]);
                                var r2_intersect = digit_info.grid.Intersect(m_RowMasks[r2]);
                                var r3_intersect = digit_info.grid.Intersect(m_RowMasks[r3]);
                                var intersect = r1_intersect.Union(r2_intersect).Union(r3_intersect);

                                // 计算要剔除的3列
                                var eliminated = new BitSet128();
                                foreach (var c in BitSet32.AllBits(r1_r2_r3_set, m_Puzzle.Size)) {
                                    eliminated = eliminated.Union(m_ColMasks[c]);
                                }

                                // 从2列中去掉intersect，就得出了最终要剔除的格子
                                eliminated = eliminated.Minus(intersect);
                                if (EliminateSingleCandidate(eliminated, digit)) {
                                    return true;
                                }
                            }
                        }
                    }
                }

                // swordfish at column
                for (var c1 = 0; c1 < digit_info.cols.Length - 2; ++c1) {
                    var c1_set = digit_info.cols[c1];
                    if (c1_set.IsEmpty) {
                        continue;
                    }

                    var c1_row_count = c1_set.BitCount;
                    if (c1_row_count < 2 || c1_row_count > 3) {
                        continue;
                    }

                    for (var c2 = c1 + 1; c2 < digit_info.cols.Length - 1; ++c2) {
                        var c2_set = digit_info.cols[c2];
                        if (c2_set.IsEmpty) {
                            continue;
                        }

                        var c2_row_count = c2_set.BitCount;
                        if (c2_row_count < 2 || c2_row_count > 3) {
                            continue;
                        }

                        var c1_c2_set = c1_set.Union(c2_set);
                        if (c1_c2_set.BitCount != 3) {
                            continue;
                        }

                        for (var c3 = c2 + 1; c3 < digit_info.cols.Length; ++c3) {
                            var c3_set = digit_info.cols[c3];
                            if (c3_set.IsEmpty) {
                                continue;
                            }

                            var c3_row_count = c3_set.BitCount;
                            if (c3_row_count < 2 || c3_row_count > 3) {
                                continue;
                            }

                            var c1_c2_c3_set = c1_c2_set.Union(c3_set);
                            if (c1_c2_c3_set.BitCount == 3) {
                                // find
                                // 计算组成swordfish的6-9个格子，记录在intersect
                                var c1_intersect = digit_info.grid.Intersect(m_ColMasks[c1]);
                                var c2_intersect = digit_info.grid.Intersect(m_ColMasks[c2]);
                                var c3_intersect = digit_info.grid.Intersect(m_ColMasks[c3]);
                                var intersect = c1_intersect.Union(c2_intersect).Union(c3_intersect);

                                // 计算要剔除的3行
                                var eliminated = new BitSet128();
                                foreach (var r in BitSet32.AllBits(c1_c2_c3_set, m_Puzzle.Size)) {
                                    eliminated = eliminated.Union(m_RowMasks[r]);
                                }

                                // 从2列中去掉intersect，就得出了最终要剔除的格子
                                eliminated = eliminated.Minus(intersect);
                                if (EliminateSingleCandidate(eliminated, digit)) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        #region jellyfish
        /// <summary>
        /// @note 本函数自身也是不完备的，也就是说如果直接调用本函数，可以能出错误的jellyfish
        /// 但如果在找不到x-wing和swordfish的情况下，本函数就能保证能出正确结果。理由如下：
        /// 
        /// 下面以jellyfish的行形式说明
        /// jellyfish本身除了要求候选数覆盖4行4列外，其实还要求每一列至少有2个元素，例如下面这种情况是不属于swordfish的
        /// **
        ///  **
        ///   **
        ///  * *
        /// 如果我们假设现在有一列只有1个元素（注意不是说这一列只有1个候选数，而是说在组成jellyfish的格子里，这一列只有1个元素）
        /// 那么我们可以去掉包含这一列的那一行，那么剩下的3行就正好组成了一个swordfish（刚好3行3列），但由于我们已经检查过没有swordfish了，所以不会出现这种情况
        /// 也就是说不会出现有一列只有1个元素的情况，所以能得出正确的jellyfish
        /// </summary>
        /// <returns></returns>
        protected bool TryJellyfish()
        {
            for (var digit = 1; digit <= m_Puzzle.Size; ++digit) {
                var digit_info = m_DigitInfos[digit - 1];
                if (digit_info.grid.IsEmpty) {
                    continue;
                }

                // jellyfish at row
                for (var r1 = 0; r1 < digit_info.rows.Length - 3; ++r1) {
                    var r1_set = digit_info.rows[r1];
                    if (r1_set.IsEmpty) {
                        continue;
                    }

                    var r1_col_count = r1_set.BitCount;
                    if (r1_col_count < 2 || r1_col_count > 4) {
                        continue;
                    }

                    for (var r2 = r1 + 1; r2 < digit_info.rows.Length - 2; ++r2) {
                        var r2_set = digit_info.rows[r2];
                        if (r2_set.IsEmpty) {
                            continue;
                        }

                        var r2_col_count = r2_set.BitCount;
                        if (r2_col_count < 2 || r2_col_count > 4) {
                            continue;
                        }

                        var r1_r2_set = r1_set.Union(r2_set);
                        var r1_r2_col_count = r1_r2_set.BitCount;
                        if (r1_r2_col_count < 3 || r1_r2_col_count > 4) {
                            // 其实这里r1_r2_col_count==2也是可以的，但这样的话r1,r2就形成了x-wing，这在之前已经检测过 
                            // 所以这里也过滤掉
                            continue;
                        }

                        for (var r3 = r2 + 1; r3 < digit_info.rows.Length - 1; ++r3) {
                            var r3_set = digit_info.rows[r3];
                            if (r3_set.IsEmpty) {
                                continue;
                            }

                            var r3_col_count = r3_set.BitCount;
                            if (r3_col_count < 2 || r3_col_count > 3) {
                                continue;
                            }

                            var r1_r2_r3_set = r1_r2_set.Union(r3_set);
                            if (r1_r2_r3_set.BitCount != 4) {
                                continue;
                            }

                            for (var r4 = r3 + 1; r4 < digit_info.rows.Length; ++r4) {
                                var r4_set = digit_info.rows[r4];
                                if (r4_set.IsEmpty) {
                                    continue;
                                }

                                var r1_r2_r3_r4_set = r1_r2_r3_set.Union(r4_set);
                                if (r1_r2_r3_r4_set.BitCount == 4) {
                                    // find
                                    // 计算组成jellyfish的8-16个格子，记录在intersect
                                    var r1_intersect = digit_info.grid.Intersect(m_RowMasks[r1]);
                                    var r2_intersect = digit_info.grid.Intersect(m_RowMasks[r2]);
                                    var r3_intersect = digit_info.grid.Intersect(m_RowMasks[r3]);
                                    var r4_intersect = digit_info.grid.Intersect(m_RowMasks[r4]);
                                    var intersect = r1_intersect.Union(r2_intersect).Union(r3_intersect).Union(r4_intersect);

                                    // 计算要剔除的4列
                                    var eliminated = new BitSet128();
                                    foreach (var c in BitSet32.AllBits(r1_r2_r3_r4_set, m_Puzzle.Size)) {
                                        eliminated = eliminated.Union(m_ColMasks[c]);
                                    }

                                    // 从4列中去掉intersect，就得出了最终要剔除的格子
                                    eliminated = eliminated.Minus(intersect);
                                    if (EliminateSingleCandidate(eliminated, digit)) {
                                        return true;
                                    }
                                }

                            }
                        }
                    }
                }

                // jellyfish at column
                for (var c1 = 0; c1 < digit_info.cols.Length - 3; ++c1) {
                    var c1_set = digit_info.cols[c1];
                    if (c1_set.IsEmpty) {
                        continue;
                    }

                    var c1_row_count = c1_set.BitCount;
                    if (c1_row_count < 2 || c1_row_count > 4) {
                        continue;
                    }

                    for (var c2 = c1 + 1; c2 < digit_info.cols.Length - 2; ++c2) {
                        var c2_set = digit_info.cols[c2];
                        if (c2_set.IsEmpty) {
                            continue;
                        }

                        var c2_row_count = c2_set.BitCount;
                        if (c2_row_count < 2 || c2_row_count > 4) {
                            continue;
                        }

                        var c1_c2_set = c1_set.Union(c2_set);
                        var c1_c2_row_count = c1_c2_set.BitCount;
                        if (c1_c2_row_count < 3 || c1_c2_row_count > 4) {
                            continue;
                        }

                        for (var c3 = c2 + 1; c3 < digit_info.cols.Length - 1; ++c3) {
                            var c3_set = digit_info.cols[c3];
                            if (c3_set.IsEmpty) {
                                continue;
                            }

                            var c3_col_count = c3_set.BitCount;
                            if (c3_col_count < 2 || c3_col_count > 3) {
                                continue;
                            }

                            var c1_c2_c3_set = c1_c2_set.Union(c3_set);
                            if (c1_c2_c3_set.BitCount != 4) {
                                continue;
                            }

                            for (var c4 = c3 + 1; c4 < digit_info.cols.Length; ++c4) {
                                var c4_set = digit_info.cols[c4];
                                if (c4_set.IsEmpty) {
                                    continue;
                                }

                                var c1_c2_c3_c4_set = c1_c2_c3_set.Union(c4_set);
                                if (c1_c2_c3_c4_set.BitCount == 4) {
                                    // find
                                    // 计算组成jellyfish的8-16个格子，记录在intersect
                                    var c1_intersect = digit_info.grid.Intersect(m_ColMasks[c1]);
                                    var c2_intersect = digit_info.grid.Intersect(m_ColMasks[c2]);
                                    var c3_intersect = digit_info.grid.Intersect(m_ColMasks[c3]);
                                    var c4_intersect = digit_info.grid.Intersect(m_ColMasks[c4]);
                                    var intersect = c1_intersect.Union(c2_intersect).Union(c3_intersect).Union(c4_intersect);

                                    // 计算要剔除的4行
                                    var eliminated = new BitSet128();
                                    foreach (var r in BitSet32.AllBits(c1_c2_c3_c4_set, m_Puzzle.Size)) {
                                        eliminated = eliminated.Union(m_RowMasks[r]);
                                    }

                                    // 从4行中去掉intersect，就得出了最终要剔除的格子
                                    eliminated = eliminated.Minus(intersect);
                                    if (EliminateSingleCandidate(eliminated, digit)) {
                                        return true;
                                    }
                                }

                            }
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        #region skyscraper
        protected bool TrySkyscraper()
        {
            for (var digit = 1; digit <= m_Puzzle.Size; ++digit) {
                var digit_info = m_DigitInfos[digit - 1];
                if (digit_info.grid.IsEmpty) {
                    continue;
                }

                // skyscraper at row
                for (var r1 = 0; r1 < digit_info.rows.Length - 1; ++r1) {
                    var r1_set = digit_info.rows[r1];
                    if (r1_set.IsEmpty || r1_set.BitCount != 2) {
                        continue;
                    }

                    for (var r2 = r1 + 1; r2 < digit_info.rows.Length; ++r2) {
                        var r2_set = digit_info.rows[r2];
                        if (r2_set.IsEmpty || r2_set.BitCount != 2) {
                            continue;
                        }

                        var r1_r2_set = r1_set.Union(r2_set);
                        if (r1_r2_set.BitCount == 3) {
                            // find
                            // 计算相同的那一列
                            var interset_set = r1_set.Intersect(r2_set);

                            int intersect_col = 0;
                            foreach (var c in BitSet32.AllBits(interset_set, m_Puzzle.Size)) {
                                intersect_col = c;
                            }

                            int c1 = 0, c2 = 0;
                            foreach (var c in BitSet32.AllBits(r1_set, m_Puzzle.Size)) {
                                if (c != intersect_col) {
                                    c1 = c;
                                }
                            }
                            foreach (var c in BitSet32.AllBits(r2_set, m_Puzzle.Size)) {
                                if (c != intersect_col) {
                                    c2 = c;
                                }
                            }

                            var cell1_intersect = m_RowMasks[r1].Union(m_ColMasks[c1]).Union(m_BoxMasks[m_Puzzle.RowCol2Box(r1, c1)]);
                            var cell2_intersect = m_RowMasks[r2].Union(m_ColMasks[c2]).Union(m_BoxMasks[m_Puzzle.RowCol2Box(r2, c2)]);

                            var eliminated = digit_info.grid.Intersect(cell1_intersect).Intersect(cell2_intersect);
                            if (eliminated.IsEmpty) {
                                continue;
                            }

                            // 计算组成skyscraper的4个格子，记录在intersect
                            var r1_intersect = digit_info.grid.Intersect(m_RowMasks[r1]);
                            var r2_intersect = digit_info.grid.Intersect(m_RowMasks[r2]);
                            var intersect = r1_intersect.Union(r2_intersect);

                            // 从2列中去掉intersect，就得出了最终要剔除的格子
                            eliminated = eliminated.Minus(intersect);
                            if (EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }

                // skyscraper at column
                for (var c1 = 0; c1 < digit_info.cols.Length - 1; ++c1) {
                    var c1_set = digit_info.cols[c1];
                    if (c1_set.IsEmpty || c1_set.BitCount != 2) {
                        continue;
                    }

                    for (var c2 = c1 + 1; c2 < digit_info.cols.Length; ++c2) {
                        var c2_set = digit_info.cols[c2];
                        if (c2_set.IsEmpty || c2_set.BitCount != 2) {
                            continue;
                        }

                        var c1_c2_set = c1_set.Union(c2_set);
                        if (c1_c2_set.BitCount == 3) {
                            // find
                            // 计算相同的那一行
                            var interset_set = c1_set.Intersect(c2_set);

                            int intersect_row = 0;
                            foreach (var r in BitSet32.AllBits(interset_set, m_Puzzle.Size)) {
                                intersect_row = r;
                            }

                            int r1 = 0, r2 = 0;
                            foreach (var r in BitSet32.AllBits(c1_set, m_Puzzle.Size)) {
                                if (r != intersect_row) {
                                    r1 = r;
                                }
                            }
                            foreach (var r in BitSet32.AllBits(c2_set, m_Puzzle.Size)) {
                                if (r != intersect_row) {
                                    r2 = r;
                                }
                            }

                            var cell1_intersect = m_RowMasks[r1].Union(m_ColMasks[c1]).Union(m_BoxMasks[m_Puzzle.RowCol2Box(r1, c1)]);
                            var cell2_intersect = m_RowMasks[r2].Union(m_ColMasks[c2]).Union(m_BoxMasks[m_Puzzle.RowCol2Box(r2, c2)]);

                            var eliminated = digit_info.grid.Intersect(cell1_intersect).Intersect(cell2_intersect);
                            if (eliminated.IsEmpty) {
                                continue;
                            }

                            // 计算组成skyscraper的4个格子，记录在intersect
                            var c1_intersect = digit_info.grid.Intersect(m_ColMasks[c1]);
                            var c2_intersect = digit_info.grid.Intersect(m_ColMasks[c2]);
                            var intersect = c1_intersect.Union(c2_intersect);

                            // 从2列中去掉intersect，就得出了最终要剔除的格子
                            eliminated = eliminated.Minus(intersect);
                            if (EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        #endregion
    }
}
