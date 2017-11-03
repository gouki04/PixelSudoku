using UnityEngine;

namespace sudoku.data.solve
{
    public class Intersection : Technique
    {
        public bool TrySolve(Context context)
        {
            if (TryLockedCandidates(context)) {
                Debug.Log("TryLockedCandidates");
                return true;
            }

            return false;
        }

        protected bool TryLockedCandidates(Context context)
        {
            // check Type-1 (Pointing)
            for (var box = 0; box < context.Puzzle.Size; ++box) {
                for (var r = 0; r < context.Puzzle.RowCnt; ++r) {
                    var intersect_set = context.BoxMasks[box].Intersect(context.RowMasks[r]);
                    // 判断当前宫和当前行是否有交集
                    if (intersect_set.IsEmpty) {
                        continue;
                    }

                    for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                        var grid_flag = context.DigitInfos[digit - 1].grid;

                        // 判断当前数字是否已经没有候选数了
                        if (grid_flag.IsEmpty) {
                            continue;
                        }

                        var box_flag = grid_flag.Intersect(context.BoxMasks[box]);
                        if (box_flag.IsEmpty) {
                            continue;
                        }

                        var diff = box_flag - intersect_set;
                        if (diff.IsEmpty) {
                            // 当前数字，在当前宫中只存在于当前行内
                            // 则可以把当前行内其他格子对当前数字的候选数删掉
                            var eliminated = context.RowMasks[r] - intersect_set;
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }

                for (var c = 0; c < context.Puzzle.ColCnt; ++c) {
                    var intersect_set = context.BoxMasks[box].Intersect(context.ColMasks[c]);
                    if (intersect_set.IsEmpty) {
                        continue;
                    }

                    for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                        var grid_flag = context.DigitInfos[digit - 1].grid;
                        if (grid_flag.IsEmpty) {
                            continue;
                        }

                        var box_flag = grid_flag.Intersect(context.BoxMasks[box]);
                        if (box_flag.IsEmpty) {
                            continue;
                        }

                        var diff = box_flag - intersect_set;
                        if (diff.IsEmpty) {
                            // 当前数字，在当前宫中只存在于当前列内
                            // 则可以把当前列内其他格子对当前数字的候选数删掉
                            var eliminated = context.ColMasks[c] - intersect_set;
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }
            }

            // Type 2 (Claiming or Box-Line Reduction)
            for (var r = 0; r < context.Puzzle.RowCnt; ++r) {
                for (var box = 0; box < context.Puzzle.Size; ++box) {
                    var intersect_set = context.RowMasks[r].Intersect(context.BoxMasks[box]);
                    if (intersect_set.IsEmpty) {
                        continue;
                    }

                    for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                        var grid_flag = context.DigitInfos[digit - 1].grid;
                        if (grid_flag.IsEmpty) {
                            continue;
                        }

                        var row_flag = grid_flag.Intersect(context.RowMasks[r]);
                        if (row_flag.IsEmpty) {
                            continue;
                        }

                        var diff = row_flag - intersect_set;
                        if (diff.IsEmpty) {
                            // 当前数字，在当前行中只存在于当前宫内
                            // 则可以把当前宫内其他格子对当前数字的候选数删掉
                            var eliminated = context.BoxMasks[box] - intersect_set;
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }
            }

            for (var c = 0; c < context.Puzzle.ColCnt; ++c) {
                for (var box = 0; box < context.Puzzle.Size; ++box) {
                    var intersect_set = context.ColMasks[c].Intersect(context.BoxMasks[box]);
                    if (intersect_set.IsEmpty) {
                        continue;
                    }

                    for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                        var grid_flag = context.DigitInfos[digit - 1].grid;
                        if (grid_flag.IsEmpty) {
                            continue;
                        }

                        var col_flag = grid_flag.Intersect(context.ColMasks[c]);
                        if (col_flag.IsEmpty) {
                            continue;
                        }

                        var diff = col_flag - intersect_set;
                        if (diff.IsEmpty) {
                            // 当前数字，在当前列中只存在于当前宫内
                            // 则可以把当前宫内其他格子对当前数字的候选数删掉
                            var eliminated = context.BoxMasks[box] - intersect_set;
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
