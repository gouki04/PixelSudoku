using System.Collections.Generic;

namespace sudoku.data
{
    public struct NumberFlag
    {
        int flag;

        public NumberFlag(int _flag)
        {
            flag = _flag;
        }

        public void AddNumber(int number)
        {
            flag |= (1 << number);
        }

        /// <summary>
        /// 外部要先用HasNumber判断，再调用本函数来删
        /// </summary>
        /// <param name="number"></param>
        public void RemoveNumber(int number)
        {
            flag ^= (1 << number);
        }

        public bool HasNumber(int number)
        {
            return (flag & (1 << number)) != 0;
        }

        public int NumberCount
        {
            get {
                // Brian Kernighan's Algorithm
                // @see http://cs-fundamentals.com/tech-interview/c/c-program-to-count-number-of-ones-in-unsigned-integer.php
                int count = 0;
                int n = flag;
                while (n != 0) {
                    n = n & (n - 1);
                    ++count;
                }

                return count;
            }
        }

        public void Reset()
        {
            flag = 0;
        }

        public bool IsEmpty
        {
            get {
                return flag == 0;
            }
        }

        public static NumberFlag zero = new NumberFlag(0);

        #region operator
        public static implicit operator NumberFlag(int flag)
        {
            return new NumberFlag(flag);
        }

        public static bool operator ==(NumberFlag lhs, NumberFlag rhs)
        {
            if (lhs.flag == rhs.flag) {
                return true;
            } else {
                return false;
            }
        }

        public static bool operator !=(NumberFlag lhs, NumberFlag rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator ==(NumberFlag lhs, int rhs)
        {
            if (lhs.flag == rhs) {
                return true;
            } else {
                return false;
            }
        }

        public static bool operator !=(NumberFlag lhs, int rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator ==(int lhs, NumberFlag rhs)
        {
            if (lhs == rhs.flag) {
                return true;
            } else {
                return false;
            }
        }

        public static bool operator !=(int lhs, NumberFlag rhs)
        {
            return !(lhs == rhs);
        }

        public static NumberFlag operator +(NumberFlag lhs, NumberFlag rhs)
        {
            return new NumberFlag(lhs.flag | rhs.flag);
        }

        public static NumberFlag operator -(NumberFlag lhs, NumberFlag rhs)
        {
            return new NumberFlag((lhs.flag | rhs.flag) ^ rhs.flag);
        }

        public static bool operator true(NumberFlag nf)
        {
            return nf.flag != 0;
        }

        public static bool operator false(NumberFlag nf)
        {
            return nf.flag == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is NumberFlag) {
                return (NumberFlag)obj == this;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return flag;
        }

        public override string ToString()
        {
            return flag.ToString();
        }

        #endregion

        #region IEnumerable
        public static IEnumerable<int> AllNumbers(NumberFlag nf, int max)
        {
            if (nf.flag != 0) {
                for (var i = 1; i <= max; ++i) {
                    if (nf.HasNumber(i)) {
                        yield return i;
                    }
                }
            }
        }
        #endregion
    }
}
