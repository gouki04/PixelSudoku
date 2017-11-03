using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace sudoku.data.solve
{
    public class SingleDigitPattern : Technique
    {
        public bool TrySolve(Context context)
        {
            if (TrySkyscraper(context)) {
                Debug.Log("TrySkyscraper");
                return true;
            }

            return false;
        }

        #region skyscraper
        protected bool TrySkyscraper(Context context)
        {
            for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                var digit_info = context.DigitInfos[digit - 1];
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
                            foreach (var c in BitSet32.AllBits(interset_set, context.Puzzle.Size)) {
                                intersect_col = c;
                            }

                            int c1 = 0, c2 = 0;
                            foreach (var c in BitSet32.AllBits(r1_set, context.Puzzle.Size)) {
                                if (c != intersect_col) {
                                    c1 = c;
                                }
                            }
                            foreach (var c in BitSet32.AllBits(r2_set, context.Puzzle.Size)) {
                                if (c != intersect_col) {
                                    c2 = c;
                                }
                            }

                            var cell1_intersect = context.RowMasks[r1].Union(context.ColMasks[c1]).Union(context.BoxMasks[context.Puzzle.RowCol2Box(r1, c1)]);
                            var cell2_intersect = context.RowMasks[r2].Union(context.ColMasks[c2]).Union(context.BoxMasks[context.Puzzle.RowCol2Box(r2, c2)]);

                            var eliminated = digit_info.grid.Intersect(cell1_intersect).Intersect(cell2_intersect);
                            if (eliminated.IsEmpty) {
                                continue;
                            }

                            // 计算组成skyscraper的4个格子，记录在intersect
                            var r1_intersect = digit_info.grid.Intersect(context.RowMasks[r1]);
                            var r2_intersect = digit_info.grid.Intersect(context.RowMasks[r2]);
                            var intersect = r1_intersect.Union(r2_intersect);

                            // 从2列中去掉intersect，就得出了最终要剔除的格子
                            eliminated = eliminated.Minus(intersect);
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
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
                            foreach (var r in BitSet32.AllBits(interset_set, context.Puzzle.Size)) {
                                intersect_row = r;
                            }

                            int r1 = 0, r2 = 0;
                            foreach (var r in BitSet32.AllBits(c1_set, context.Puzzle.Size)) {
                                if (r != intersect_row) {
                                    r1 = r;
                                }
                            }
                            foreach (var r in BitSet32.AllBits(c2_set, context.Puzzle.Size)) {
                                if (r != intersect_row) {
                                    r2 = r;
                                }
                            }

                            var cell1_intersect = context.RowMasks[r1].Union(context.ColMasks[c1]).Union(context.BoxMasks[context.Puzzle.RowCol2Box(r1, c1)]);
                            var cell2_intersect = context.RowMasks[r2].Union(context.ColMasks[c2]).Union(context.BoxMasks[context.Puzzle.RowCol2Box(r2, c2)]);

                            var eliminated = digit_info.grid.Intersect(cell1_intersect).Intersect(cell2_intersect);
                            if (eliminated.IsEmpty) {
                                continue;
                            }

                            // 计算组成skyscraper的4个格子，记录在intersect
                            var c1_intersect = digit_info.grid.Intersect(context.ColMasks[c1]);
                            var c2_intersect = digit_info.grid.Intersect(context.ColMasks[c2]);
                            var intersect = c1_intersect.Union(c2_intersect);

                            // 从2列中去掉intersect，就得出了最终要剔除的格子
                            eliminated = eliminated.Minus(intersect);
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
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
