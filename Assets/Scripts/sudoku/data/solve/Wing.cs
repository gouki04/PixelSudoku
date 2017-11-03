using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace sudoku.data.solve
{
    public class Wing : Technique
    {
        public bool TrySolve(Context context)
        {
            if (TryXYWing(context)) {
                Debug.Log("TryXYWing");
                return true;
            }

            return false;
        }

        protected bool find_pincer_at(Context ctx, BitSet32 pincer, BitSet128 subset, out int row, out int col)
        {
            foreach (var idx in BitSet128.AllBits(subset)) {
                ctx.Index2RowCol(idx, out row, out col);

                if (ctx.Puzzle.Candidates[row, col] == pincer) {
                    // find
                    return true;
                }
            }

            row = 0;
            col = 0;
            return false;
        }

        protected bool CheckPincerAt(Context ctx, BitSet128 subset, BitSet32 pivot, BitSet32 pincer1, int xy, int xz)
        {
            var pincer2 = pivot.Exclusive(pincer1);
            
            int row, col;
            if (find_pincer_at(ctx, pincer2, subset, out row, out col)) {
                var xy_wing = new BitSet128();
                xy_wing.SetBit(xy);
                xy_wing.SetBit(xz);
                xy_wing.SetBit(ctx.RowCol2Index(row, col));

                int z;
                BitSet32.GetBits(pincer2.Intersect(pincer1), out z);

                var intersect = ctx.GenerateViewGrid(xz).Intersect(ctx.GenerateViewGrid(row, col));
                var eliminated = intersect.Minus(xy_wing);
                if (ctx.EliminateSingleCandidate(eliminated, z)) {
                    return true;
                }
            }

            return false;
        }

        protected bool TryXYWing(Context ctx)
        {
            for (var r = 0; r < ctx.Puzzle.RowCnt; ++r) {
                for (var c = 0; c < ctx.Puzzle.ColCnt; ++c) {
                    var pivot = ctx.Puzzle.Candidates[r, c];
                    if (pivot.BitCount == 2) {
                        // try this cell as xy
                        for (var c2 = c + 1; c2 < ctx.Puzzle.ColCnt; ++c2) {
                            var pincer = ctx.Puzzle.Candidates[r, c2];
                            if (pincer.BitCount == 2 && pivot.Union(pincer).BitCount == 3) {
                                if (CheckPincerAt(ctx, ctx.ColMasks[c], pivot, pincer, ctx.RowCol2Index(r, c), ctx.RowCol2Index(r, c2))) {
                                    return true;
                                }

                                if (CheckPincerAt(ctx, ctx.BoxMasks[ctx.Puzzle.RowCol2Box(r, c)], pivot, pincer, ctx.RowCol2Index(r, c), ctx.RowCol2Index(r, c2))) {
                                    return true;
                                }

                                // swap pivot
                                if (CheckPincerAt(ctx, ctx.ColMasks[c2], pincer, pivot, ctx.RowCol2Index(r, c2), ctx.RowCol2Index(r, c))) {
                                    return true;
                                }

                                if (CheckPincerAt(ctx, ctx.BoxMasks[ctx.Puzzle.RowCol2Box(r, c2)], pincer, pivot, ctx.RowCol2Index(r, c2), ctx.RowCol2Index(r, c))) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            for (var c = 0; c < ctx.Puzzle.ColCnt; ++c) {
                for (var r = 0; r < ctx.Puzzle.RowCnt; ++r) {
                    var pivot = ctx.Puzzle.Candidates[r, c];
                    if (pivot.BitCount == 2) {
                        // try this cell as xy
                        for (var r2 = r + 1; r2 < ctx.Puzzle.RowCnt; ++r2) {
                            var pincer = ctx.Puzzle.Candidates[r2, c];
                            if (pincer.BitCount == 2 && pivot.Union(pincer).BitCount == 3) {
                                if (CheckPincerAt(ctx, ctx.RowMasks[r], pivot, pincer, ctx.RowCol2Index(r, c), ctx.RowCol2Index(r2, c))) {
                                    return true;
                                }

                                if (CheckPincerAt(ctx, ctx.BoxMasks[ctx.Puzzle.RowCol2Box(r, c)], pivot, pincer, ctx.RowCol2Index(r, c), ctx.RowCol2Index(r2, c))) {
                                    return true;
                                }

                                // swap pivot
                                if (CheckPincerAt(ctx, ctx.RowMasks[r2], pincer, pivot, ctx.RowCol2Index(r2, c), ctx.RowCol2Index(r, c))) {
                                    return true;
                                }

                                if (CheckPincerAt(ctx, ctx.BoxMasks[ctx.Puzzle.RowCol2Box(r2, c)], pincer, pivot, ctx.RowCol2Index(r2, c), ctx.RowCol2Index(r, c))) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
