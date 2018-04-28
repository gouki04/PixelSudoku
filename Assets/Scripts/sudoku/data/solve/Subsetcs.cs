using UnityEngine;

namespace sudoku.data.solve
{
    public class Subsetcs : Technique
    {
        public bool TrySolve(Context context)
        {
            if (TryHiddenPairs(context)) {
                Debug.Log("TryHiddenPairs");
                return true;
            }

            if (TryHiddenTripe(context)) {
                Debug.Log("TryHiddenTripe");
                return true;
            }

            if (TryHiddenQuad(context)) {
                Debug.Log("TryHiddenQuad");
                return true;
            }

            if (TryNakedPairs(context)) {
                Debug.Log("TryNakedPairs");
                return true;
            }

            if (TryNakedTriple(context)) {
                Debug.Log("TryNakedTriple");
                return true;
            }

            if (TryNakedQuad(context)) {
                Debug.Log("TryNakedQuad");
                return true;
            }

            return false;
        }

        #region hidden pairs
        protected bool TryHiddenPairs(Context context)
        {
            for (var i = 0; i < context.HouseMasks.Length; ++i) {
                for (var digit = 1; digit <= context.Puzzle.Size - 1; ++digit) {
                    var digit_grid = context.DigitInfos[digit - 1].grid;
                    if (digit_grid.IsEmpty) {
                        continue;
                    }

                    var intersect_set = context.HouseMasks[i].Intersect(digit_grid);
                    if (intersect_set.IsEmpty || intersect_set.BitCount != 2) {
                        continue;
                    }

                    for (var digit2 = digit + 1; digit2 <= context.Puzzle.Size; ++digit2) {
                        var digit2_grid = context.DigitInfos[digit2 - 1].grid;
                        if (digit2_grid.IsEmpty) {
                            continue;
                        }

                        var intersect_set2 = context.HouseMasks[i].Intersect(digit2_grid);
                        if (intersect_set == intersect_set2) {
                            // find pair
                            // check if pair is hidden
                            var pair_set = (BitSet32)0;
                            pair_set.SetBit(digit);
                            pair_set.SetBit(digit2);

                            var reverse_pair_set = context.FinishedDigitMask - pair_set;
                            if (context.EliminateCandidates(intersect_set, reverse_pair_set)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        #region hidden triple
        protected bool TryHiddenTripe(Context context)
        {
            for (var i = 0; i < context.HouseMasks.Length; ++i) {
                for (var d1 = 1; d1 <= context.Puzzle.Size - 2; ++d1) { // d for digit
                    var d1_grid = context.DigitInfos[d1 - 1].grid;
                    if (d1_grid.IsEmpty) {
                        continue;
                    }

                    var d1_set = context.HouseMasks[i].Intersect(d1_grid);
                    if (d1_set.IsEmpty) {
                        continue;
                    }

                    var d1_bit_count = d1_set.BitCount;
                    if (d1_bit_count < 2 || d1_bit_count > 3) {
                        continue;
                    }

                    for (var d2 = d1 + 1; d2 <= context.Puzzle.Size - 1; ++d2) {
                        var d2_grid = context.DigitInfos[d2 - 1].grid;
                        if (d2_grid.IsEmpty) {
                            continue;
                        }

                        var d2_set = context.HouseMasks[i].Intersect(d2_grid);
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

                        for (var d3 = d2 + 1; d3 <= context.Puzzle.Size; ++d3) {
                            var d3_grid = context.DigitInfos[d3 - 1].grid;
                            if (d3_grid.IsEmpty) {
                                continue;
                            }

                            var d3_set = context.HouseMasks[i].Intersect(d3_grid);
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

                                var reverse_triple_set = context.FinishedDigitMask - triple_set;
                                if (context.EliminateCandidates(d1_d2_d3_set, reverse_triple_set)) {
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

        #region hidden quad
        protected bool TryHiddenQuad(Context context)
        {
            for (var i = 0; i < context.HouseMasks.Length; ++i) {
                for (var d1 = 1; d1 <= context.Puzzle.Size - 3; ++d1) { // d for digit
                    var d1_grid = context.DigitInfos[d1 - 1].grid;
                    if (d1_grid.IsEmpty) {
                        continue;
                    }

                    var d1_set = context.HouseMasks[i].Intersect(d1_grid);
                    if (d1_set.IsEmpty) {
                        continue;
                    }

                    var d1_bit_count = d1_set.BitCount;
                    if (d1_bit_count < 2 || d1_bit_count > 4) {
                        continue;
                    }

                    for (var d2 = d1 + 1; d2 <= context.Puzzle.Size - 2; ++d2) {
                        var d2_grid = context.DigitInfos[d2 - 1].grid;
                        if (d2_grid.IsEmpty) {
                            continue;
                        }

                        var d2_set = context.HouseMasks[i].Intersect(d2_grid);
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

                        for (var d3 = d2 + 1; d3 <= context.Puzzle.Size - 1; ++d3) {
                            var d3_grid = context.DigitInfos[d3 - 1].grid;
                            if (d3_grid.IsEmpty) {
                                continue;
                            }

                            var d3_set = context.HouseMasks[i].Intersect(d3_grid);
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

                            for (var d4 = d3 + 1; d4 <= context.Puzzle.Size; ++d4) {
                                var d4_grid = context.DigitInfos[d4 - 1].grid;
                                if (d4_grid.IsEmpty) {
                                    continue;
                                }

                                var d4_set = context.HouseMasks[i].Intersect(d4_grid);
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

                                    var reverse_quad_set = context.FinishedDigitMask - quad_set;
                                    if (context.EliminateCandidates(d1_d2_d3_d4_set, reverse_quad_set)) {
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

        #region naked pairs
        protected bool TryNakedPairs(Context context)
        {
            for (var i = 0; i < context.HouseMasks.Length; ++i) {
                for (var digit = 1; digit <= context.Puzzle.Size - 1; ++digit) {
                    var digit_grid = context.DigitInfos[digit - 1].grid;
                    if (digit_grid.IsEmpty) {
                        continue;
                    }

                    var intersect_set = context.HouseMasks[i].Intersect(digit_grid);
                    if (intersect_set.IsEmpty || intersect_set.BitCount != 2) {
                        continue;
                    }

                    for (var digit2 = digit + 1; digit2 <= context.Puzzle.Size; ++digit2) {
                        var digit2_grid = context.DigitInfos[digit2 - 1].grid;
                        if (digit2_grid.IsEmpty) {
                            continue;
                        }

                        var intersect_set2 = context.HouseMasks[i].Intersect(digit2_grid);
                        if (intersect_set == intersect_set2) {
                            // find pair
                            // check if pair is naked
                            var pair_set = (BitSet32)0;
                            pair_set.SetBit(digit);
                            pair_set.SetBit(digit2);

                            bool is_naked_pair = true;
                            foreach (var idx in BitSet128.AllBits(intersect_set)) {
                                int row, col;
                                context.Index2RowCol(idx, out row, out col);

                                var candidates = context.Puzzle.Candidates[row, col];
                                if (candidates != pair_set) {
                                    is_naked_pair = false;
                                    break;
                                }
                            }

                            if (is_naked_pair) {
                                var eliminated = context.HouseMasks[i] - intersect_set;
                                if (context.EliminateCandidates(eliminated, pair_set)) {
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

        #region naked triple
        protected bool TryNakedTriple(Context context)
        {
            for (var i = 0; i < context.HouseMasks.Length; ++i) {
                for (var d1 = 1; d1 <= context.Puzzle.Size - 2; ++d1) { // d for digit
                    var d1_grid = context.DigitInfos[d1 - 1].grid;
                    if (d1_grid.IsEmpty) {
                        continue;
                    }

                    var d1_set = context.HouseMasks[i].Intersect(d1_grid);
                    if (d1_set.IsEmpty) {
                        continue;
                    }

                    var d1_bit_count = d1_set.BitCount;
                    if (d1_bit_count < 2 || d1_bit_count > 3) {
                        continue;
                    }

                    for (var d2 = d1 + 1; d2 <= context.Puzzle.Size - 1; ++d2) {
                        var d2_grid = context.DigitInfos[d2 - 1].grid;
                        if (d2_grid.IsEmpty) {
                            continue;
                        }

                        var d2_set = context.HouseMasks[i].Intersect(d2_grid);
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

                        for (var d3 = d2 + 1; d3 <= context.Puzzle.Size; ++d3) {
                            var d3_grid = context.DigitInfos[d3 - 1].grid;
                            if (d3_grid.IsEmpty) {
                                continue;
                            }

                            var d3_set = context.HouseMasks[i].Intersect(d3_grid);
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

                                var eliminated = context.HouseMasks[i] - d1_d2_d3_set;
                                if (context.EliminateCandidates(eliminated, triple_set)) {
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

        #region naked quad
        protected bool TryNakedQuad(Context context)
        {
            for (var i = 0; i < context.HouseMasks.Length; ++i) {
                for (var d1 = 1; d1 <= context.Puzzle.Size - 3; ++d1) { // d for digit
                    var d1_grid = context.DigitInfos[d1 - 1].grid;
                    if (d1_grid.IsEmpty) {
                        continue;
                    }

                    var d1_set = context.HouseMasks[i].Intersect(d1_grid);
                    if (d1_set.IsEmpty) {
                        continue;
                    }

                    var d1_bit_count = d1_set.BitCount;
                    if (d1_bit_count < 2 || d1_bit_count > 4) {
                        continue;
                    }

                    for (var d2 = d1 + 1; d2 <= context.Puzzle.Size - 2; ++d2) {
                        var d2_grid = context.DigitInfos[d2 - 1].grid;
                        if (d2_grid.IsEmpty) {
                            continue;
                        }

                        var d2_set = context.HouseMasks[i].Intersect(d2_grid);
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

                        for (var d3 = d2 + 1; d3 <= context.Puzzle.Size - 1; ++d3) {
                            var d3_grid = context.DigitInfos[d3 - 1].grid;
                            if (d3_grid.IsEmpty) {
                                continue;
                            }

                            var d3_set = context.HouseMasks[i].Intersect(d3_grid);
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

                            for (var d4 = d3 + 1; d4 <= context.Puzzle.Size; ++d4) {
                                var d4_grid = context.DigitInfos[d4 - 1].grid;
                                if (d4_grid.IsEmpty) {
                                    continue;
                                }

                                var d4_set = context.HouseMasks[i].Intersect(d4_grid);
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

                                    var eliminated = context.HouseMasks[i] - d1_d2_d3_d4_set;
                                    if (context.EliminateCandidates(eliminated, quad_set)) {
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
