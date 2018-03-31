using UnityEngine;

namespace sudoku.data.solve
{
    public class Single : Technique
    {
        public bool TrySolve(Context context)
        {
            if (TryNakedSingle(context)) {
                Debug.Log("TryNakedSingle");
                return true;
            }

            if (TryHiddenSingle(context)) {
                Debug.Log("TryHiddenSingle");
                return true;
            }

            return false;
        }

        protected bool TryNakedSingle(Context context)
        {
            // naked single
            // 一个格子只剩一个候选数的情况
            // @see http://sudopedia.enjoysudoku.com/Naked_Single.html

            // 检查是否存在一个格子内只有一个可选数字的情况
            for (var r = 0; r < context.Puzzle.RowCnt; ++r) {
                for (var c = 0; c < context.Puzzle.ColCnt; ++c) {
                    var candidates = context.Puzzle.Candidates[r, c];
                    if (candidates.IsEmpty || candidates.BitCount > 1) {
                        continue;
                    }

                    foreach (var digit in BitSet32.AllBits(candidates, context.Puzzle.Size)) {
                        context.Puzzle.TrySetCellAt(r, c, digit);
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool TryHiddenSingle(Context context)
        {
            // hidden single
            // 在某一行、列、宫中，某个数字只能填在一个格子的情况
            // @see http://sudopedia.enjoysudoku.com/Hidden_Single.html

            for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                var digit_grid = context.DigitInfos[digit - 1].grid;
                if (digit_grid.IsEmpty) {
                    continue;
                }

                // 检查是否存在某一house，其中有某一个数字只能填在一个格子的情况
                for (var i = 0; i < context.HouseMasks.Length; ++i) {
                    var intersect_set = context.HouseMasks[i].Intersect(digit_grid);
                    if (intersect_set.IsEmpty) {
                        continue;
                    }

                    if (intersect_set.BitCount == 1) {
                        // find
                        foreach (var idx in BitSet128.AllBits(intersect_set)) {
                            int row, col;
                            context.Index2RowCol(idx, out row, out col);

                            context.Puzzle.TrySetCellAt(row, col, digit);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
