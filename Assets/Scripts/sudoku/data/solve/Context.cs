using UnityEngine;

namespace sudoku.data.solve
{
    /// <summary>
    /// 解题的上下文信息
    /// 提供给每个解题技巧使用
    /// </summary>
    public class Context
    {
        /// <summary>
        /// 数独题目
        /// </summary>
        public Puzzle Puzzle;

        /// <summary>
        /// 一个已经完成的数独的mask
        /// </summary>
        public BitSet128 FinishedGridMask;

        /// <summary>
        /// 所有行的mask
        /// 下标从0开始
        /// </summary>
        public BitSet128[] RowMasks;

        /// <summary>
        /// 所有列的mask
        /// 下标从0开始
        /// </summary>
        public BitSet128[] ColMasks;

        /// <summary>
        /// 所有宫的mask
        /// 下标从0开始
        /// 下标从左上往右下增加
        /// </summary>
        public BitSet128[] BoxMasks;

        /// <summary>
        /// RowMasks + ColMasks + BoxMasks
        /// </summary>
        public BitSet128[] HouseMasks;

        /// <summary>
        /// 数字1-9完成的mask
        /// </summary>
        public BitSet32 FinishedDigitMask;

        /// <summary>
        /// 保存一个数字的候选数的当前分布信息
        /// </summary>
        public class DigitInfo
        {
            /// <summary>
            /// 所有候选数组成的grid
            /// </summary>
            public BitSet128 grid;

            /// <summary>
            /// 保存每一行的候选数的分布情况
            /// 下标从0开始
            /// 例如row[0] = 0000 0001 0010 0001
            /// 表示第一行有3个候选数，分别在第1列、第6列、第9列
            /// </summary>
            public BitSet32[] rows;

            /// <summary>
            /// 保存每一列的候选数的分布情况
            /// @see rows
            /// </summary>
            public BitSet32[] cols;
        }

        /// <summary>
        /// 所有数字的候选数分布情况
        /// 下标从0开始
        /// DigitInfos[0]表示数字1的分布情况
        /// </summary>
        public DigitInfo[] DigitInfos;

        public Context(Puzzle p)
        {
            Puzzle = p;

            Init();
        }

        /// <summary>
        /// 计算指定行列对应的唯一idx，按照标准数独，基本排布如下
        /// 0  1  2  | 3  4  5  | 6  7  8
        /// 9  10 11 | 12 13 14 | 15 16 17
        /// 18 19 20 | 21 22 23 | 24 25 26
        /// ------------------------------
        /// 27 28 29 | 30 31 32 | 33 34 35
        /// 36 37 38 | 39 40 41 | 42 43 44
        /// 45 46 47 | 48 49 50 | 51 52 53
        /// ------------------------------
        /// 54 55 56 | 57 58 59 | 60 61 62
        /// 63 64 65 | 66 67 68 | 69 70 71
        /// 72 73 74 | 75 76 77 | 78 79 80
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public int RowCol2Index(int row, int col)
        {
            return row * Puzzle.Size + col;
        }

        /// <summary>
        /// 通过唯一idx，反向计算其行列
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public void Index2RowCol(int idx, out int row, out int col)
        {
            row = Mathf.FloorToInt(idx / Puzzle.Size);
            col = idx % Puzzle.Size;
        }

        public void Init()
        {
            RowMasks = new BitSet128[Puzzle.Size];
            ColMasks = new BitSet128[Puzzle.Size];
            BoxMasks = new BitSet128[Puzzle.Size];

            for (var r = 0; r < Puzzle.RowCnt; ++r) {
                for (var c = 0; c < Puzzle.ColCnt; ++c) {
                    var idx = RowCol2Index(r, c);
                    RowMasks[r].SetBit(idx);
                    ColMasks[c].SetBit(idx);
                    BoxMasks[Puzzle.RowCol2Box(r, c)].SetBit(idx);

                    FinishedGridMask.SetBit(idx);
                }
            }

            HouseMasks = new BitSet128[RowMasks.Length + ColMasks.Length + BoxMasks.Length];
            var house_idx = 0;
            for (var i = 0; i < RowMasks.Length; ++i) {
                HouseMasks[house_idx++] = RowMasks[i];
            }

            for (var i = 0; i < ColMasks.Length; ++i) {
                HouseMasks[house_idx++] = ColMasks[i];
            }

            for (var i = 0; i < BoxMasks.Length; ++i) {
                HouseMasks[house_idx++] = BoxMasks[i];
            }

            for (var digit = 1; digit <= Puzzle.Size; ++digit) {
                FinishedDigitMask.SetBit(digit);
            }
        }

        /// <summary>
        /// 准备下基本的候选数分布情况
        /// </summary>
        public void Prepare()
        {
            if (DigitInfos == null) {
                DigitInfos = new DigitInfo[Puzzle.Size];
                for (var i = 0; i < DigitInfos.Length; ++i) {
                    DigitInfos[i] = new DigitInfo();
                    DigitInfos[i].rows = new BitSet32[Puzzle.Size];
                    DigitInfos[i].cols = new BitSet32[Puzzle.Size];
                }
            }
            else {
                for (var i = 0; i < DigitInfos.Length; ++i) {
                    DigitInfos[i].grid.Reset();

                    for (var r = 0; r < DigitInfos[i].rows.Length; ++r) {
                        DigitInfos[i].rows[r].Reset();
                    }
                    for (var c = 0; c < DigitInfos[i].cols.Length; ++c) {
                        DigitInfos[i].cols[c].Reset();
                    }
                }
            }

            // mDigitInfos[i].grid保存了数字i+1，其候选数在题板上的分布情况
            // 注意这里的下标是从0开始的，也就是说mDigitInfos[0]是数字1的分布情况

            for (var r = 0; r < Puzzle.RowCnt; ++r) {
                for (var c = 0; c < Puzzle.ColCnt; ++c) {
                    var candidates = Puzzle.Candidates[r, c];
                    if (candidates == 0) {
                        continue;
                    }

                    var idx = RowCol2Index(r, c);
                    foreach (var digit in BitSet32.AllBits(candidates, Puzzle.Size)) {
                        DigitInfos[digit - 1].grid.SetBit(idx);
                        DigitInfos[digit - 1].rows[r].SetBit(c);
                        DigitInfos[digit - 1].cols[c].SetBit(r);
                    }
                }
            }
        }

        /// <summary>
        /// 从eliminated中屏除掉候选数digit
        /// 如果出现成功摒除，返回true
        /// </summary>
        /// <param name="eliminated"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public bool EliminateSingleCandidate(BitSet128 eliminated, int digit)
        {
            var ret = false;
            foreach (var idx in BitSet128.AllBits(eliminated)) {
                int row, col;
                Index2RowCol(idx, out row, out col);

                var candidates = Puzzle.Candidates[row, col];
                if (candidates.HasBit(digit)) {
                    // 填入数字后，再次填入是删除操作。。
                    Puzzle.TrySetCandidateAt(row, col, digit);
                    ret = true;
                }
            }

            return ret;
        }

        /// <summary>
        /// 从eliminated中屏除掉候选数列candidates
        /// 如果出现成功摒除，返回true
        /// </summary>
        /// <param name="eliminated"></param>
        /// <param name="candidates">和Puzzle里候选数排布一样，数字i在i位（由于数独是1-9，所以0位不用）</param>
        /// <returns></returns>
        public bool EliminateCandidates(BitSet128 eliminated, BitSet32 candidates)
        {
            var ret = false;
            foreach (var idx in BitSet128.AllBits(eliminated)) {
                int row, col;
                Index2RowCol(idx, out row, out col);

                var cur_candidates = Puzzle.Candidates[row, col];
                var need_eliminate_candidates = cur_candidates.Intersect(candidates);

                if (!need_eliminate_candidates.IsEmpty) {
                    ret = true;
                    foreach (var digit in BitSet32.AllBits(need_eliminate_candidates, Puzzle.Size)) {
                        // 填入数字后，再次填入是删除操作。。
                        Puzzle.TrySetCandidateAt(row, col, digit);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 生成指定位置的视野grid
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public BitSet128 GenerateViewGrid(int row, int col)
        {
            return RowMasks[row].Union(ColMasks[col]).Union(BoxMasks[Puzzle.RowCol2Box(row, col)]);
        }

        /// <summary>
        /// 生成指定位置的视野grid
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public BitSet128 GenerateViewGrid(int idx)
        {
            int row, col;
            Index2RowCol(idx, out row, out col);

            return GenerateViewGrid(row, col);
        }

        /// <summary>
        /// 生成指定grid的视野grid
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public BitSet128 GenerateViewGrid(BitSet128 grid)
        {
            var sum = new BitSet128();
            foreach (var idx in BitSet128.AllBits(grid)) {
                sum = sum.Union(GenerateViewGrid(idx));
            }

            return sum;
        }

        /// <summary>
        /// 计算2个grid的视野交集
        /// </summary>
        /// <param name="grid_a"></param>
        /// <param name="grid_b"></param>
        /// <returns></returns>
        public BitSet128 GenerateIntersectViewGridBetween(BitSet128 grid_a, BitSet128 grid_b)
        {
            return GenerateViewGrid(grid_a).Intersect(GenerateViewGrid(grid_b)).Minus(grid_a).Minus(grid_b);
        }
    }

    public interface Technique
    {
        bool TrySolve(Context context);
    }
}
