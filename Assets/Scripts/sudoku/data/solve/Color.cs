using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace sudoku.data.solve
{
    public class Color : Technique
    {
        protected Context context;

        protected class GridNode
        {
            public BitSet128 grid;
            public List<int> rc_idxs = new List<int>();
            public GridNode opposite_node;
            public LinkedGrid parent;
        }

        protected class LinkedGrid
        {
            public GridNode true_node;
            public GridNode false_node;
            public LinkedGrid prev;
            public LinkedGrid next;
        }

        protected class DigitColor
        {
            public Dictionary<int, GridNode> rc2grid = new Dictionary<int, GridNode>();
            public LinkedGrid head = null;
            public LinkedGrid tail = null;

            public void AddConjugatePair(int rc1, int rc2)
            {
                GridNode grid1, grid2;
                rc2grid.TryGetValue(rc1, out grid1);
                rc2grid.TryGetValue(rc2, out grid2);

                if (grid1 == null && grid2 == null) {
                    var linked_node = new LinkedGrid();

                    linked_node.true_node = new GridNode();
                    linked_node.false_node = new GridNode();

                    linked_node.true_node.parent = linked_node;
                    linked_node.false_node.parent = linked_node;

                    linked_node.true_node.opposite_node = linked_node.false_node;
                    linked_node.false_node.opposite_node = linked_node.true_node;

                    linked_node.true_node.rc_idxs.Add(rc1);
                    rc2grid[rc1] = linked_node.true_node;

                    linked_node.true_node.grid.SetBit(rc1);

                    linked_node.false_node.rc_idxs.Add(rc2);
                    rc2grid[rc2] = linked_node.false_node;

                    linked_node.false_node.grid.SetBit(rc2);

                    // add to head
                    if (head == null) {
                        head = linked_node;
                        tail = linked_node;
                    }
                    else {
                        // add to tail
                        tail.next = linked_node;
                        linked_node.prev = tail;

                        tail = linked_node;
                    }
                }
                else if (grid1 != null && grid2 != null) {
                    if (grid1.opposite_node == grid2) {
                        // maybe error ?
                    }
                    else {
                        grid1.opposite_node.grid = grid1.opposite_node.grid.Union(grid2.grid);
                        grid1.grid = grid1.grid.Union(grid2.opposite_node.grid);

                        grid1.opposite_node.rc_idxs.AddRange(grid2.rc_idxs);
                        grid1.rc_idxs.AddRange(grid2.opposite_node.rc_idxs);

                        foreach (var idx in grid2.rc_idxs) {
                            rc2grid[idx] = grid1.opposite_node;
                        }

                        foreach (var idx in grid2.opposite_node.rc_idxs) {
                            rc2grid[idx] = grid1;
                        }

                        var linked_node = grid2.parent;
                        if (linked_node == head) {
                            if (linked_node.next != null) {
                                linked_node.next.prev = null;
                            }

                            head = linked_node.next;
                        }
                        else if (linked_node == tail) { // 这里不会出现linked_node既是head又是tail的情况，因为至少有2个节点
                            if (linked_node.prev != null) {
                                linked_node.prev.next = null;
                            }

                            tail = linked_node.prev;
                        }
                        else {
                            if (linked_node.prev != null) {
                                linked_node.prev.next = linked_node.next;
                            }
                            if (linked_node.next != null) {
                                linked_node.next.prev = linked_node.prev;
                            }
                        }
                    }
                }
                else {
                    if (grid1 != null) {
                        grid1.opposite_node.rc_idxs.Add(rc2);
                        grid1.opposite_node.grid.SetBit(rc2);

                        rc2grid[rc2] = grid1.opposite_node;
                    }
                    else {
                        grid2.opposite_node.rc_idxs.Add(rc1);
                        grid2.opposite_node.grid.SetBit(rc1);

                        rc2grid[rc1] = grid2.opposite_node;
                    }
                }
            }

            public void AddConjugatePair(BitSet128 grid)
            {
                var conjugate_pair = new int[2];
                var arr_idx = 0;
                foreach (var rc_idx in BitSet128.AllBits(grid)) {
                    conjugate_pair[arr_idx++] = rc_idx;
                }

                AddConjugatePair(conjugate_pair[0], conjugate_pair[1]);
            }
        }

        protected DigitColor[] digit_colors;

        protected void prepare()
        {
            digit_colors = new DigitColor[context.Puzzle.Size];
            for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                var dc = new DigitColor();
                digit_colors[digit - 1] = dc;

                var grid = context.DigitInfos[digit - 1].grid;
                for (var i = 0; i < context.Puzzle.Size; ++i) {
                    var intersect = grid.Intersect(context.RowMasks[i]);
                    if (intersect.BitCount == 2) {
                        dc.AddConjugatePair(intersect);
                    }

                    intersect = grid.Intersect(context.ColMasks[i]);
                    if (intersect.BitCount == 2) {
                        dc.AddConjugatePair(intersect);
                    }

                    intersect = grid.Intersect(context.BoxMasks[i]);
                    if (intersect.BitCount == 2) {
                        dc.AddConjugatePair(intersect);
                    }
                }
            }
        }

        public bool TrySolve(Context ctx)
        {
            context = ctx;
            prepare();

            if (TrySimpleColor()) {
                Debug.Log("TrySimpleColor");
                return true;
            }

            if (TryMultiColors()) {
                Debug.Log("TryMultiColors");
                return true;
            }

            return false;
        }

        protected bool TrySimpleColor()
        {
            for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                var dc = digit_colors[digit - 1];
                var p = dc.head;
                while (p != null) {
                    var intersect = context.GenerateIntersectViewGridBetween(p.true_node.grid, p.false_node.grid);

                    if (context.EliminateSingleCandidate(intersect, digit)) {
                        return true;
                    }

                    p = p.next;
                }
            }

            return false;
        }

        protected bool TryMultiColors()
        {
            for (var digit = 1; digit <= context.Puzzle.Size; ++digit) {
                var dc = digit_colors[digit - 1];
                if (dc.head == null) {
                    continue;
                }

                var first = dc.head;
                while (first != null) {
                    var second = first.next;

                    var first_true_view = context.GenerateViewGrid(first.true_node.grid);
                    var first_false_view = context.GenerateViewGrid(first.false_node.grid);
                    while (second != null) {
                        var intersect = first_true_view.Intersect(second.true_node.grid);
                        if (intersect.BitCount != 0) {
                            // find
                            var eliminated = context.GenerateIntersectViewGridBetween(first.false_node.grid, second.false_node.grid);
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }

                        intersect = first_true_view.Intersect(second.false_node.grid);
                        if (intersect.BitCount != 0) {
                            // find
                            var eliminated = context.GenerateIntersectViewGridBetween(first.false_node.grid, second.true_node.grid);
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }

                        intersect = first_false_view.Intersect(second.true_node.grid);
                        if (intersect.BitCount != 0) {
                            // find
                            var eliminated = context.GenerateIntersectViewGridBetween(first.true_node.grid, second.false_node.grid);
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }

                        intersect = first_false_view.Intersect(second.false_node.grid);
                        if (intersect.BitCount != 0) {
                            // find
                            var eliminated = context.GenerateIntersectViewGridBetween(first.true_node.grid, second.true_node.grid);
                            if (context.EliminateSingleCandidate(eliminated, digit)) {
                                return true;
                            }
                        }

                        second = second.next;
                    }

                    first = first.next;
                }
            }

            return false;
        }
    }
}
