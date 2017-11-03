using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace sudoku.view
{
    public class Cell : MonoBehaviour
    {
        public asset.Skin Skin;
        public Image BgImg;
        public Image GivenBgImg;
        public Image NumberImg;
        public Image HighlightImg;

        public GameObject CandidatesRoot;
        public List<Image> CandidateImgList;

        public Button Button;

        public int Row = 0;
        public int Col = 0;

        public void Error(bool v)
        {
            if (v) {
                NumberImg.color = Color.red;
            }
            else {
                NumberImg.color = Color.black;
            }
        }

        public void Highlight(bool v)
        {
            if (v) {
                HighlightImg.gameObject.SetActive(true);
                HighlightImg.sprite = Skin.Highlight;
            }
            else {
                HighlightImg.gameObject.SetActive(false);
            }
        }
    }
}
