using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace sudoku.data.solve
{
    public class Solver
    {
        protected Context m_Context;
        protected List<Technique> m_Techniques = new List<Technique>();

        public Solver(Puzzle puzzle)
        {
            m_Context = new Context(puzzle);
            m_Techniques.Add(new Single());
            m_Techniques.Add(new Subsetcs());
            m_Techniques.Add(new Intersection());
            m_Techniques.Add(new SingleDigitPattern());
            m_Techniques.Add(new Wing());
            m_Techniques.Add(new Fish());
            m_Techniques.Add(new Color());
        }

        public IEnumerator Run()
        {
            m_Context.Puzzle.FillAllCandidates();

            while (!m_Context.Puzzle.IsFinished) {
                yield return new WaitForSeconds(0.2f);

                m_Context.Prepare();

                var succeed = false;
                foreach (var tech in m_Techniques) {
                    if (tech.TrySolve(m_Context)) {
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
