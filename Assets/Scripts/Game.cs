using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    //public SudokuBoard CurrentBoard;
    //public GameObject UIBoard;
    //public GameObject CellPrefab;
    //public sudoku.view.Skin Skin;

    //protected List<List<GameObject>> mCells = new List<List<GameObject>>();

    //public Vector2 CellSize
    //{
    //    get {
    //        var rect_transform = CellPrefab.transform as RectTransform;
    //        return rect_transform.sizeDelta;
    //    }
    //}

    //public void Start()
    //{
    //    if (CurrentBoard != null && CurrentBoard.OriginBoard != null) {
    //        var board = CurrentBoard.OriginBoard;
    //        for (var r = 0; r < CurrentBoard.RowCnt; ++r) {
    //            for (var c = 0; c < CurrentBoard.ColCnt; ++c) {
    //                var cell = GameObject.Instantiate(CellPrefab);
    //                var rect_transform = cell.transform as RectTransform;
    //                rect_transform.SetParent(UIBoard.transform);
    //                rect_transform.anchoredPosition = new Vector2(CellSize.x * c + CellSize.x / 2, -(CellSize.y * r + CellSize.y / 2));
    //                rect_transform.localRotation = Quaternion.identity;
    //                rect_transform.localScale = Vector3.one;
    //            }
    //        }
    //    }
    //}

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
    ///             if cell2 == cell then
    ///                 continue
    ///             else
    ///                 loop.link(cell2, c, link_type.weak)
    ///                 next(loop)
    ///             end
    ///         end
    ///         
    ///         // 不同数字的强链（如果这个单元格是y） - 再次，你需要找到这个单元格的共轭对
    ///         foreach c2 in cell.other_candidates do
    ///             foreach cell2 in cell.conjugate_pairs[c2] do
    ///                 if cell2 == cell then
    ///                     continue
    ///                 else
    ///                     loop.link(cell2, c2, link_type.strong)
    ///                     next(loop)
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
}
