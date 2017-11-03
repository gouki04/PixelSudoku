using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace sudoku.data.solve
{
    public class Solver
    {
        protected Context mContext;
        protected List<Technique> mTechniques = new List<Technique>();

        public Solver(Puzzle puzzle)
        {
            mContext = new Context(puzzle);
            mTechniques.Add(new Single());
            mTechniques.Add(new Subsetcs());
            mTechniques.Add(new Intersection());
            mTechniques.Add(new SingleDigitPattern());
            mTechniques.Add(new Wing());
            mTechniques.Add(new Fish());
            mTechniques.Add(new Color());
        }

        public IEnumerator Run()
        {
            mContext.Puzzle.FillAllCandidates();

            while (!mContext.Puzzle.IsFinished) {
                yield return new WaitForSeconds(0.2f);

                mContext.Prepare();

                var succeed = false;
                foreach (var tech in mTechniques) {
                    if (tech.TrySolve(mContext)) {
                        succeed = true;
                        break;
                    }
                }

                if (!succeed) {
                    break;
                }
            }
        }
    }
}
