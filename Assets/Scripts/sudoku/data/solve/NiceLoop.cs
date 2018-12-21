using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace sudoku.data.solve
{
    /// <summary>
    /// 关于链接和推论：
    ///     * 弱推论表示：如果单元格A包含数字x，那么单元格B不能包含。
    ///     * 强推论表示：如果单元格A不包含数字x，那么单元B必须包含。
    ///     * 推论都是双向的：当你说“如果单元格A包含4，那么单元格B不能包含4”，同时你也可以说“如果单元格B包含4，那么单元格A不能包含4”
    ///     * 强推论也可以认为是弱推论：当你说“如果单元格A不包含4，那么单元B必须包含4”，同时你也可以说“如果单元格A包含4，那么单元格B不能包含4”。反之亦然。
    ///     
    /// Key points to remember about links and their inferences:
    ///     * A weak inference means 'if square A contains value x then square B can't'.
    ///     * A strong inference means  'if square A doesn't contain value x then square B must'.
    ///     * An inference is always bi-directional - when you can say 'if square A contains 4 then square B can't', you can also say 'if square B contains 4 then square A can't'.
    ///     * When a strong inference exists, a weak inference also exists - when you can say 'if square A doesn't contain 4 then square B must', you can also say 'if square A contains 4 then square B can't' (and vice-versa).
    /// 
    /// 传递规则：
    ///     * 当一个单元格有2条弱链，那这个单元格必须是bivalue（只含2个候选数），同时2条弱链的数字必须不同。
    ///         * 例子：-8-[R8C9]-6-（……R8C9不能是8，因为它是bivalue，所以它是6，如果R8C9是6……）
    ///     * 当一个单元格有2条强链，同时2条强链的数字必须不同。
    ///         * 例子：=6=[R4C7]=3=（……R4C7必须是6，如果R4C7不是3……）
    ///     * 当一个单元格有1条强链、1条弱链（任意顺序），那么这2条链的数字必须相同。
    ///         * 例子：=5=[R5C4]-5-（……R5C4必须是5，如果R5C4是5……）
    ///         * 例子：-5-[R5C4]=5=（……R5C4不能是5，如果R5C4不是5……）
    /// 
    /// Propagation rules:
    ///     * If a square has two weak links, then it must be bivalue(two candidates) and the link candidates must be different.
    ///         * Example: -8-[R8C9]-6- (...R8C9 can't be 8, and because it's bivalue it must therefore be 6, and if R8C9 is 6...)
    ///     * If a square has two strong links, then the links must have different candidates.
    ///         * Example: =6=[R4C7]=3=  (... R4C7 must be 6, and if R4C7 isn't 3...).
    ///     * If a square has one strong and one weak link(in either combination), then the link candidates must be the same.
    ///         * Example 1:  =5=[R5C4]-5-  (... R5C4 must be 5, and if R5C4 is 5...)
    ///         * Example 2:  -5-[R5C4]=5=  (... R5C4 can't be 5, and if R5C4 isn't 5...)
    /// 
    /// 不连续环类型：
    ///     * 类型1：如果第一个单元格有2条弱链，且弱链的数字相同，那么这个数字可以被这个单元格摒除。
    ///     * 类型2：如果第一个单元格有2条强链，且强链的数字相同，那么这个数字可以直接填入单元格。
    ///     * 类型3：如果第一个单元格有1条强链和1条弱链，且2条链的数字不同，那么弱链的数字可以被这个单元格摒除。
    ///     
    ///     * 总结：如果传递规则被破坏时存在弱链，那么弱链的数字可以被第一个单元格摒除，否则将强链的数字填入第一个单元格。
    /// 
    /// Discontinuous Nice Loop Types:
    ///     * Type 1. If the first square has two weak links for the same candidate, that candidate can be eliminated from the square.
    ///     * Type 2. If the first square has two strong links with the same candidate, then it can be solved with the links' candidate.
    ///     * Type 3. If the first square has a weak link and a strong link with different candidates, then the weak link's candidate can be eliminated from the square. 
    ///     
    ///     * Summary: If a propagation rule has been broken and there's a weak link involved, eliminate the weak link's candidate from the first square, otherwise solve the square with the strong links' candidate.
    /// 
    /// 连续环：
    ///     * 对于环内的任意单元格，如果其有2条强链且强链的数字不同，那么这个单元格内除了这2个数字的其他数字可以被摒除。
    ///     * 对于环内的任意2个单元格，如果其由弱链相连，那么弱链的数字可以被这2个单元格视野（related）内的单元格摒除。
    /// 
    /// Continuous Nice Loops:
    ///     * If a square has two strong links with different candidates, then all candidates except those two can be eliminated from the square.
    ///     * If two squares are joined by a weak link, then the link's candidate can be eliminated from any squares that are related to both of them.
    ///     
    /// 寻找环：
    ///     * 如果进入的是弱链（这个单元格不能是x），那么出去的链可以是：
    ///         * 相同数字的强链（如果这个单元格不是x） - 你需要找到这个单元格的共轭对
    ///         * 不同数字的弱链（如果这个单元格是y） - 单元格必须是bivalue
    ///     * 如果进入的是强链（这个单元格必须是x），那么出去的链可以是：
    ///         * 相同数字的弱链（如果这个单元格是x）
    ///         * 不同数字的强链（如果这个单元格是y） - 再次，你需要找到这个单元格的共轭对
    /// 
    /// Finding Nice Loops:
    ///     * If the incoming link is weak ('this square can't be X'), then the outgoing link from this square can be:
    ///         * A strong link on the same candidate('if this square isn't X')  - you'll need to find a conjugate pair with this square.
    ///         * If this square is bivalue, a weak link on its other candidate ('if this square is Y').
    ///     * If the incoming link is strong('this square must be X'), then the outgoing link from this square can be:
    ///         * A weak link on the same candidate ('if this square is X').
    ///         * A strong link on any of this square's other candidates ('if this square isn't Y') - again, you'll need to find a conjugate pair.
    ///         
    /// fake code
    /// 
    /// choose a cell
    /// loop.link(cell, link_type.head)
    /// foreach c in cell.candidates do
    ///     // try strong link
    ///     foreach cell2 in cell.conjugate_pairs[c] do
    ///         loop.link(cell2, c, link_type.strong)
    ///         next(loop)
    ///     end
    ///     
    ///     // try weak link
    ///     foreach cell2 in cell.related[c] do
    ///         loop.link(cell2, c, link_type.weak)
    ///         next(loop)
    ///     end
    /// end
    /// 
    /// function next(loop)
    ///     if loop.last == loop.head then
    ///         print (find loop)
    ///         return
    ///     end
    /// 
    ///     local cell = loop.last.cell
    ///     local c = loop.last.candidate
    ///     
    ///     // 如果进入的是强链（这个单元格必须是x），那么出去的链可以是：
    ///     if loop.last.link_type == link_type.strong then
    ///         // 相同数字的弱链（如果这个单元格是x）
    ///         foreach cell2 in cell.related[c] do
    ///             if cell2 == loop.last.prev.cell then
    ///                 continue
    ///             elseif not cell2.candidate.contains(c) then
    ///                 continue
    ///             else
    ///                 loop.link(cell2, c, link_type.weak)
    ///                 next(loop)
    ///             end
    ///         end
    ///         
    ///         // 不同数字的强链（如果这个单元格是y） - 再次，你需要找到这个单元格的共轭对
    ///         foreach c2 in cell.candidates do
    ///             if c2 == c then
    ///                 continue
    ///             else
    ///                 foreach cell2 in cell.conjugate_pairs[c2] do
    ///                     if cell2 == loop.last.prev.cell then
    ///                         continue
    ///                     else
    ///                         loop.link(cell2, c2, link_type.strong)
    ///                         next(loop)
    ///                     end
    ///                 end
    ///             end
    ///         end
    ///     // 如果进入的是弱链（这个单元格不能是x），那么出去的链可以是：
    ///     else
    ///         // 相同数字的强链（如果这个单元格不是x） - 你需要找到这个单元格的共轭对
    ///         foreach cell2 in cell.conjugate_pairs[c] do
    ///             if cell2 == loop.last.prev.cell then
    ///                 continue
    ///             else
    ///                 loop.link(cell2, c, link_type.strong)
    ///                 next(loop)
    ///             end
    ///         end
    ///         
    ///         // 不同数字的弱链（如果这个单元格是y） - 单元格必须是bivalue
    ///         if cell.is_bivalue then
    ///             foreach c2 in cell.candidates do
    ///                 if c2 == c then
    ///                     continue
    ///                 else
    ///                     foreach cell2 in cell.related[c2] do
    ///                         if cell2 == loop.last.prev.cell then
    ///                             continue
    ///                         else
    ///                             loop.link(cell2, c2, link_type.weak)
    ///                             next(loop)
    ///                         end
    ///                     end
    ///                 end
    ///             end
    ///         end
    ///     end
    /// end
    /// 
    /// 654989
    /// </summary>
    public class NiceLoop : Technique
    {
        public enum ELinkType
        {
            None = 0,
            Strong = 1,
            Weak = 2,
        }

        public class Cell
        {
            public Context Context = null;

            public int CellIdx;
            public int Row;
            public int Col;
            public int Digit;

            public void Init(int cell_idx, int digit)
            {
                CellIdx = cell_idx;
                Context.Index2RowCol(CellIdx, out Row, out Col);

                Digit = digit;
            }

            public bool IsAtSamePosition(Cell c)
            {
                return c.CellIdx == CellIdx;
            }

            public bool IsAtSamePosition(int cell_idx)
            {
                return cell_idx == CellIdx;
            }

            public bool IsBiValue()
            {
                return Context.Puzzle.Candidates[Row, Col].BitCount == 2;
            }

            public bool ContainsDigit(int digit)
            {
                return Context.Puzzle.Candidates[Row, Col].HasBit(digit);
            }

            public BitSet32 GetOtherCondidates()
            {
                var candidates = Context.Puzzle.Candidates[Row, Col];
                candidates.UnSetBit(Digit);

                return candidates;
            }

            public BitSet128 GetRelated(int digit)
            {
                if (digit != Digit && !ContainsDigit(digit)) {
                    throw new Exception(string.Format("Invalid digit{0} in GetRelated", digit));
                }

                // 计算出自身的视野
                var view = Context.GenerateViewGrid(CellIdx);

                // 去掉视野内不存在指定数字的格子
                view = view.Intersect(Context.DigitInfos[digit - 1].grid);

                // 去掉自身格子
                var self_mask = new BitSet128();
                self_mask.SetBit(CellIdx);
                view = view.Minus(self_mask);

                return view;
            }

            public BitSet128 GetConjugatePairs(int digit)
            {
                if (digit != Digit && !ContainsDigit(digit)) {
                    throw new Exception(string.Format("Invalid number{0} in GetConjugatePairs", digit));
                }

                var ret = new BitSet128();

                var digit_info = Context.DigitInfos[digit - 1];

                // 计算行、列、宫的共轭对
                var row = digit_info.grid.Intersect(Context.RowMasks[Row]);
                if (row.BitCount == 2) {
                    ret = ret.Union(row);
                }

                var col = digit_info.grid.Intersect(Context.ColMasks[Col]);
                if (col.BitCount == 2) {
                    ret = ret.Union(col);
                }

                var box = digit_info.grid.Intersect(Context.BoxMasks[Context.Puzzle.RowCol2Box(Row, Col)]);
                if (box.BitCount == 2) {
                    ret = ret.Union(box);
                }

                // 去掉自身格子
                var self_mask = new BitSet128();
                self_mask.SetBit(CellIdx);
                ret = ret.Minus(self_mask);

                return ret;
            }

            public string Print()
            {
                return string.Format("[R{0}C{1}]", Row + 1, Col + 1);
            }
        }

        public class Node
        {
            public Node Prev = null;
            public Node Next = null;

            public Cell Cell = null;
            public ELinkType LinkType;

            public string Print()
            {
                return string.Format("{0}{1}{0}{2}", LinkType == ELinkType.Strong ? '=' : '-',
                    Cell.Digit, Cell.Print());
            }
        }

        public class Loop
        {
            public Context Context = null;
            public Node Head = null;
            public Node Tail = null;

            public BitSet128 FilledMask;

            protected int m_BreakTime = int.MaxValue;

            protected Queue<Cell> m_CellCache = new Queue<Cell>();
            public Cell CreateCell(int cell_idx, int digit)
            {
                Cell ret = null;
                if (m_CellCache.Count > 0) {
                    ret = m_CellCache.Dequeue();
                }
                else {
                    ret = new Cell();
                    ret.Context = Context;
                }

                ret.Init(cell_idx, digit);
                return ret;
            }

            public void DestroyCell(Cell cell)
            {
                m_CellCache.Enqueue(cell);
            }

            protected Queue<Node> m_NodeCache = new Queue<Node>();
            public Node CreateNode(Cell cell, ELinkType link_type = ELinkType.None)
            {
                Node ret = null;
                if (m_NodeCache.Count > 0) {
                    ret = m_NodeCache.Dequeue();
                }
                else {
                    ret = new Node();
                }

                ret.Cell = cell;
                ret.LinkType = link_type;
                ret.Prev = null;
                ret.Next = null;

                return ret;
            }

            public void DestroyNode(Node node)
            {
                m_NodeCache.Enqueue(node);
            }

            public void SetHead(Cell cell)
            {
                var node = CreateNode(cell);

                Head = node;
                Tail = node;

                FilledMask.Reset();
                FilledMask.SetBit(cell.CellIdx);
            }

            protected bool EliminateType1()
            {
                // 如果第一个单元格有2条弱链，且弱链的数字相同，那么这个数字可以被这个单元格摒除。
                Context.EliminateSingleCandidate(new BitSet128(Head.Cell.CellIdx), Head.Next.Cell.Digit);
                return true;
            }

            protected bool EliminateType2()
            {
                // 如果第一个单元格有2条强链，且强链的数字相同，那么这个数字可以直接填入单元格。
                Context.Puzzle.TrySetCellAt(Head.Cell.Row, Head.Cell.Col, Head.Next.Cell.Digit);
                return true;
            }

            protected bool EliminateType3()
            {
                // 如果第一个单元格有1条强链和1条弱链，且2条链的数字不同，那么弱链的数字可以被这个单元格摒除。
                if (Head.Next.LinkType == ELinkType.Weak) {
                    Context.EliminateSingleCandidate(new BitSet128(Head.Cell.CellIdx), Head.Next.Cell.Digit);
                }
                else {
                    Context.EliminateSingleCandidate(new BitSet128(Tail.Cell.CellIdx), Tail.Cell.Digit);
                }
                return true;
            }

            protected bool EliminateNiceLoop()
            {
                var p = Head.Next;
                do {
                    if (p.LinkType == ELinkType.Strong) {
                        if (p.Next != null && p.Next.LinkType == ELinkType.Strong) {
                            foreach (var digit in BitSet32.AllBits(p.Cell.GetOtherCondidates(), Context.Puzzle.Size)) {
                                if (digit == p.Cell.Digit || digit == p.Next.Cell.Digit) {
                                    continue;
                                }

                                if (Context.Puzzle.TrySetCandidateAt(p.Cell.Row, p.Cell.Col, digit)) {
                                    return true;
                                }
                            }
                        }
                    }
                    else if (p.LinkType == ELinkType.Weak) {
                        if (p.Prev != null) {
                            var grid1 = Context.GenerateViewGrid(p.Prev.Cell.CellIdx);
                            var grid2 = Context.GenerateViewGrid(p.Cell.CellIdx);
                            var view = grid1.Intersect(grid2).Minus(new BitSet128(p.Prev.Cell.CellIdx)).Minus(new BitSet128(p.Cell.CellIdx));
                            if (Context.EliminateSingleCandidate(view, p.Cell.Digit)) {
                                return true;
                            }
                        }
                    }
                    p = p.Next;
                } while (p != null);

                return false;
            }

            protected bool FindLoop(Cell cell, ELinkType link_type)
            {
                if (link_type == ELinkType.Strong) {
                    if (Head.Next.LinkType == ELinkType.Strong) {
                        if (cell.Digit == Head.Next.Cell.Digit) {
                            // type 2.
                            if (EliminateType2()) {
                                Debug.Log("Type 2 : " + Print());
                                return true;
                            }
                        }
                    }
                    else if (Head.Next.LinkType == ELinkType.Weak) {
                        if (cell.Digit != Head.Next.Cell.Digit) {
                            // type 3
                            if (EliminateType3()) {
                                Debug.Log("Type 3 : " + Print());
                                return true;
                            }
                        }
                        else if (EliminateNiceLoop()) {
                            Debug.Log("Nice Loop : " + Print());
                            return true;
                        }
                    }
                }
                else if (link_type == ELinkType.Weak) {
                    if (Head.Next.LinkType == ELinkType.Weak) {
                        if (cell.Digit == Head.Next.Cell.Digit) {
                            // type 1.
                            if (EliminateType1()) {
                                Debug.Log("Type 1 : " + Print());
                                return true;
                            }
                        }
                    }
                    else if (Head.Next.LinkType == ELinkType.Strong) {
                        if (cell.Digit != Head.Next.Cell.Digit) {
                            // type 3
                            if (EliminateType3()) {
                                Debug.Log("Type 3 : " + Print());
                                return true;
                            }
                        }
                        else if (EliminateNiceLoop()) {
                            Debug.Log("Nice Loop : " + Print());
                            return true;
                        }
                    }
                }

                return false;
            }

            public bool Link(Cell cell, ELinkType link_type)
            {
                if (Tail.Cell.IsAtSamePosition(cell)) {
                    return false;
                }

                if (Tail.Prev != null && Tail.Prev.Cell.IsAtSamePosition(cell)) {
                    return false;
                }

                if (FilledMask.HasBit(cell.CellIdx) && !Head.Cell.IsAtSamePosition(cell)) {
                    return false;
                }

                var node = CreateNode(cell, link_type);
                node.Prev = Tail;

                Tail.Next = node;
                Tail = node;

                FilledMask.SetBit(cell.CellIdx);

                return true;
            }

            public void UnLink()
            {
                var tail = Tail;
                FilledMask.UnSetBit(tail.Cell.CellIdx);

                Tail = tail.Prev;
                Tail.Next = null;
                tail.Prev = null;

                DestroyNode(tail);
            }

            public string Print()
            {
                var sb = new StringBuilder();
                sb.Append(Head.Cell.Print());

                var p = Head.Next;
                do {
                    sb.Append(p.Print());
                    p = p.Next;
                } while (p != null);

                return sb.ToString();
            }

            public bool FindNext(Cell cell, ELinkType link_type)
            {
                if (!Link(cell, link_type)) {
                    return false;
                }

                try {
                    // check is found nice loop
                    if (Head.Cell.IsAtSamePosition(cell)) {
                        if (FindLoop(cell, link_type)) {
                            Debug.Log(Print());
                            return true;
                        }
                    }

                    --m_BreakTime;
                    var c = cell.Digit;

                    if (Tail.LinkType == ELinkType.Strong) {
                        foreach (var cell2_idx in BitSet128.AllBits(cell.GetRelated(c))) {
                            var cell2 = CreateCell(cell2_idx, c);
                            if (FindNext(cell2, ELinkType.Weak)) {
                                return true;
                            }
                            DestroyCell(cell2);
                        }

                        foreach (var c2 in BitSet32.AllBits(cell.GetOtherCondidates(), Context.Puzzle.Size)) {
                            foreach (var cell2_idx in BitSet128.AllBits(cell.GetConjugatePairs(c2))) {
                                var cell2 = CreateCell(cell2_idx, c2);
                                if (FindNext(cell2, ELinkType.Strong)) {
                                    return true;
                                }
                                DestroyCell(cell2);
                            }
                        }
                    }
                    else if (Tail.LinkType == ELinkType.Weak) {
                        foreach (var cell2_idx in BitSet128.AllBits(cell.GetConjugatePairs(c))) {
                            var cell2 = CreateCell(cell2_idx, c);
                            if (FindNext(cell2, ELinkType.Strong)) {
                                return true;
                            }
                            DestroyCell(cell2);
                        }

                        if (cell.IsBiValue()) {
                            foreach (var c2 in BitSet32.AllBits(cell.GetOtherCondidates(), Context.Puzzle.Size)) {
                                foreach (var cell2_idx in BitSet128.AllBits(cell.GetRelated(c2))) {
                                    var cell2 = CreateCell(cell2_idx, c2);
                                    if (FindNext(cell2, ELinkType.Weak)) {
                                        return true;
                                    }
                                    DestroyCell(cell2);
                                }
                            }
                        }
                    }
                }
                finally {
                    UnLink();
                }

                return false;
            }
        }

        public bool TrySolve(Context context)
        {
            Debug.Log("TryNiceLoop");

            var loop = new Loop() {
                Context = context,
            };

            for (var r = 0; r < context.Puzzle.RowCnt; ++r) {
                for (var c = 0; c < context.Puzzle.ColCnt; ++c) {
                    var candidates = context.Puzzle.Candidates[r, c];
                    if (candidates.IsEmpty) {
                        continue;
                    }

                    foreach (var digit in BitSet32.AllBits(candidates, context.Puzzle.Size)) {
                        var cell = loop.CreateCell(context.RowCol2Index(r, c), digit);
                        loop.SetHead(cell);

                        foreach (var cell2_idx in BitSet128.AllBits(cell.GetConjugatePairs(digit))) {
                            var cell2 = loop.CreateCell(cell2_idx, digit);
                            if (loop.FindNext(cell2, ELinkType.Strong)) {
                                return true;
                            }
                            loop.DestroyCell(cell2);
                        }

                        foreach (var cell2_idx in BitSet128.AllBits(cell.GetRelated(digit))) {
                            var cell2 = loop.CreateCell(cell2_idx, digit);
                            if (loop.FindNext(cell2, ELinkType.Weak)) {
                                return true;
                            }
                            loop.DestroyCell(cell2);
                        }
                    }
                }
            }

            return false;
        }
    }
}
