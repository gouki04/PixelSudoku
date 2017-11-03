using UnityEngine;

namespace sudoku.data.solve
{
    public class Fish : Technique
    {
        public bool TrySolve(Context context)
        {
            if (TryXWing(context)) {
                Debug.Log("TryXWing");
                return true;
            }

            if (TrySwordfish(context)) {
                Debug.Log("TrySwordfish");
                return true;
            }

            if (TryJellyfish(context)) {
                Debug.Log("TryJellyfish");
                return true;
            }

            return false;
        }

        #region x-wing
        protected bool TryXWing(Context context)
        {
            for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                var digit_info = context.DigitInfos[digit - 1];
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
                            var r1_intersect = digit_info.grid.Intersect(context.RowMasks[r1]);
                            var r2_intersect = digit_info.grid.Intersect(context.RowMasks[r2]);
                            var intersect = r1_intersect.Union(r2_intersect);

                            // 计算要剔除的2列
                            var eliminated = new BitSet128();
                            foreach (var c in BitSet32.AllBits(r1_set, context.Puzzle.Size)) {
                                eliminated = eliminated.Union(context.ColMasks[c]);
                            }

                            // 从2列中去掉intersect，就得出了最终要剔除的格子
                            eliminated = eliminated.Minus(intersect);
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
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
                            var c1_intersect = digit_info.grid.Intersect(context.ColMasks[c1]);
                            var c2_intersect = digit_info.grid.Intersect(context.ColMasks[c2]);
                            var intersect = c1_intersect.Union(c2_intersect);

                            // 计算要剔除的2行
                            var eliminated = new BitSet128();
                            foreach (var r in BitSet32.AllBits(c1_set, context.Puzzle.Size)) {
                                eliminated = eliminated.Union(context.RowMasks[r]);
                            }

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
        protected bool TrySwordfish(Context context)
        {
            for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                var digit_info = context.DigitInfos[digit - 1];
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
                                var r1_intersect = digit_info.grid.Intersect(context.RowMasks[r1]);
                                var r2_intersect = digit_info.grid.Intersect(context.RowMasks[r2]);
                                var r3_intersect = digit_info.grid.Intersect(context.RowMasks[r3]);
                                var intersect = r1_intersect.Union(r2_intersect).Union(r3_intersect);

                                // 计算要剔除的3列
                                var eliminated = new BitSet128();
                                foreach (var c in BitSet32.AllBits(r1_r2_r3_set, context.Puzzle.Size)) {
                                    eliminated = eliminated.Union(context.ColMasks[c]);
                                }

                                // 从2列中去掉intersect，就得出了最终要剔除的格子
                                eliminated = eliminated.Minus(intersect);
                                if (context.EliminateSingleCandidate(eliminated, digit)) {
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
                                var c1_intersect = digit_info.grid.Intersect(context.ColMasks[c1]);
                                var c2_intersect = digit_info.grid.Intersect(context.ColMasks[c2]);
                                var c3_intersect = digit_info.grid.Intersect(context.ColMasks[c3]);
                                var intersect = c1_intersect.Union(c2_intersect).Union(c3_intersect);

                                // 计算要剔除的3行
                                var eliminated = new BitSet128();
                                foreach (var r in BitSet32.AllBits(c1_c2_c3_set, context.Puzzle.Size)) {
                                    eliminated = eliminated.Union(context.RowMasks[r]);
                                }

                                // 从2列中去掉intersect，就得出了最终要剔除的格子
                                eliminated = eliminated.Minus(intersect);
                                if (context.EliminateSingleCandidate(eliminated, digit)) {
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
        protected bool TryJellyfish(Context context)
        {
            for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                var digit_info = context.DigitInfos[digit - 1];
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
                                    var r1_intersect = digit_info.grid.Intersect(context.RowMasks[r1]);
                                    var r2_intersect = digit_info.grid.Intersect(context.RowMasks[r2]);
                                    var r3_intersect = digit_info.grid.Intersect(context.RowMasks[r3]);
                                    var r4_intersect = digit_info.grid.Intersect(context.RowMasks[r4]);
                                    var intersect = r1_intersect.Union(r2_intersect).Union(r3_intersect).Union(r4_intersect);

                                    // 计算要剔除的4列
                                    var eliminated = new BitSet128();
                                    foreach (var c in BitSet32.AllBits(r1_r2_r3_r4_set, context.Puzzle.Size)) {
                                        eliminated = eliminated.Union(context.ColMasks[c]);
                                    }

                                    // 从4列中去掉intersect，就得出了最终要剔除的格子
                                    eliminated = eliminated.Minus(intersect);
                                    if (context.EliminateSingleCandidate(eliminated, digit)) {
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
                                    var c1_intersect = digit_info.grid.Intersect(context.ColMasks[c1]);
                                    var c2_intersect = digit_info.grid.Intersect(context.ColMasks[c2]);
                                    var c3_intersect = digit_info.grid.Intersect(context.ColMasks[c3]);
                                    var c4_intersect = digit_info.grid.Intersect(context.ColMasks[c4]);
                                    var intersect = c1_intersect.Union(c2_intersect).Union(c3_intersect).Union(c4_intersect);

                                    // 计算要剔除的4行
                                    var eliminated = new BitSet128();
                                    foreach (var r in BitSet32.AllBits(c1_c2_c3_c4_set, context.Puzzle.Size)) {
                                        eliminated = eliminated.Union(context.RowMasks[r]);
                                    }

                                    // 从4行中去掉intersect，就得出了最终要剔除的格子
                                    eliminated = eliminated.Minus(intersect);
                                    if (context.EliminateSingleCandidate(eliminated, digit)) {
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
    }
}
