using System.Collections.Generic;

namespace sudoku.data
{
    public struct GridFlag
    {
        long low_flag;
        long high_flag;

        public GridFlag(long _low_flag, long _high_flag)
        {
            low_flag = _low_flag;
            high_flag = _high_flag;
        }

        public void AddNumber(int number)
        {
            if (number >= 64) {
                high_flag |= (1L << (number - 64));
            }
            else {
                low_flag |= (1L << number);
            }
        }

        public void RemoveNumber(int number)
        {
            if (number >= 64) {
                high_flag ^= (1L << (number - 64));
            }
            else {
                low_flag ^= (1L << number);
            }
        }

        public bool HasNumber(int number)
        {
            if (number >= 64) {
                return (high_flag & (1L << (number - 64))) != 0;
            }
            else {
                return (low_flag & (1L << number)) != 0;
            }
        }

        /// <summary>
        /// 算交集
        /// </summary>
        /// <param name="gf"></param>
        /// <returns></returns>
        public GridFlag Intersect(GridFlag gf)
        {
            return new GridFlag(gf.low_flag & low_flag, gf.high_flag & high_flag);
        }

        /// <summary>
        /// 算并集
        /// </summary>
        /// <param name="gf"></param>
        /// <returns></returns>
        public GridFlag Union(GridFlag gf)
        {
            return new GridFlag(gf.low_flag | low_flag, gf.high_flag | high_flag);
        }

        /// <summary>
        /// 算差集
        /// </summary>
        /// <param name="gf"></param>
        /// <returns></returns>
        public GridFlag Exclusive(GridFlag gf)
        {
            return new GridFlag(gf.low_flag ^ low_flag, gf.high_flag ^ high_flag);
        }

        /// <summary>
        /// 去掉另一个集合的元素
        /// </summary>
        /// <param name="gf"></param>
        /// <returns></returns>
        public GridFlag Minus(GridFlag gf)
        {
            return new GridFlag((low_flag | gf.low_flag) ^ gf.low_flag, (high_flag | gf.high_flag) ^ gf.high_flag);
        }

        public int NumberCount
        {
            get {
                // Brian Kernighan's Algorithm
                // @see http://cs-fundamentals.com/tech-interview/c/c-program-to-count-number-of-ones-in-unsigned-integer.php
                int count = 0;
                var n = low_flag;
                while (n != 0) {
                    n = n & (n - 1);
                    ++count;
                }

                n = high_flag;
                while (n != 0) {
                    n = n & (n - 1);
                    ++count;
                }

                return count;
            }
        }

        public void Reset()
        {
            low_flag = 0;
            high_flag = 0;
        }

        public bool IsEmpty
        {
            get {
                return low_flag == 0 && high_flag == 0;
            }
        }

        public static GridFlag zero = new GridFlag();

        #region operator
        public static bool operator ==(GridFlag lhs, GridFlag rhs)
        {
            if (lhs.low_flag == rhs.low_flag && lhs.high_flag == rhs.high_flag) {
                return true;
            }
            else {
                return false;
            }
        }

        public static bool operator !=(GridFlag lhs, GridFlag rhs)
        {
            return !(lhs == rhs);
        }

        public static GridFlag operator +(GridFlag lhs, GridFlag rhs)
        {
            return new GridFlag(
                lhs.low_flag | rhs.low_flag,
                lhs.high_flag | rhs.high_flag);
        }

        public static GridFlag operator -(GridFlag lhs, GridFlag rhs)
        {
            return new GridFlag(
                (lhs.low_flag | rhs.low_flag) ^ rhs.low_flag,
                (lhs.high_flag | rhs.high_flag) ^ rhs.high_flag);
        }

        public static bool operator true(GridFlag gf)
        {
            return gf.low_flag != 0 || gf.high_flag != 0;
        }

        public static bool operator false(GridFlag gf)
        {
            return gf.low_flag == 0 && gf.high_flag == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is GridFlag) {
                return (GridFlag)obj == this;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return high_flag.ToString() + low_flag.ToString();
        }

        #endregion

        #region IEnumerable
        public static IEnumerable<int> EnumFrom(int begin_idx, int end_idx, long flag, int offset = 0)
        {
            for (var i = begin_idx; i < end_idx; ++i) {
                if ((flag & (1L << i)) != 0) {
                    yield return i + offset;
                }
            }
        }

        public static IEnumerable<int> AllNumbers(GridFlag gf)
        {
            if (gf.low_flag != 0) {
                if ((gf.low_flag & unchecked(0x00000000FFFFFFFF)) != 0) {
                    if ((gf.low_flag & unchecked(0x000000000000FFFF)) != 0) {
                        if ((gf.low_flag & unchecked(0x00000000000000FF)) != 0) {
                            for (var i = 0; i < 8; ++i) {
                                if ((gf.low_flag & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }

                        if ((gf.low_flag & unchecked(0x000000000000FF00)) != 0) {
                            for (var i = 8; i < 16; ++i) {
                                if ((gf.low_flag & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }
                    }

                    if ((gf.low_flag & unchecked(0x00000000FFFF0000)) != 0) {
                        if ((gf.low_flag & unchecked(0x0000000000FF0000)) != 0) {
                            for (var i = 16; i < 24; ++i) {
                                if ((gf.low_flag & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }

                        if ((gf.low_flag & unchecked(0x00000000FF000000)) != 0) {
                            for (var i = 24; i < 32; ++i) {
                                if ((gf.low_flag & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }
                    }
                }

                if ((gf.low_flag & unchecked((long)0xFFFFFFFF00000000)) != 0) {
                    if ((gf.low_flag & unchecked((long)0x0000FFFF00000000)) != 0) {
                        if ((gf.low_flag & unchecked((long)0x000000FF00000000)) != 0) {
                            for (var i = 32; i < 40; ++i) {
                                if ((gf.low_flag & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }

                        if ((gf.low_flag & unchecked((long)0x0000FF0000000000)) != 0) {
                            for (var i = 40; i < 48; ++i) {
                                if ((gf.low_flag & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }
                    }

                    if ((gf.low_flag & unchecked((long)0xFFFF000000000000)) != 0) {
                        if ((gf.low_flag & unchecked((long)0x00FF000000000000)) != 0) {
                            for (var i = 48; i < 56; ++i) {
                                if ((gf.low_flag & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }

                        if ((gf.low_flag & unchecked((long)0xFF00000000000000)) != 0) {
                            for (var i = 56; i < 64; ++i) {
                                if ((gf.low_flag & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }
                    }
                }
            }

            if (gf.high_flag != 0) {
                if ((gf.high_flag & unchecked(0x00000000FFFFFFFF)) != 0) {
                    if ((gf.high_flag & unchecked(0x000000000000FFFF)) != 0) {
                        if ((gf.high_flag & unchecked(0x00000000000000FF)) != 0) {
                            for (var i = 0; i < 8; ++i) {
                                if ((gf.high_flag & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }

                        if ((gf.high_flag & unchecked(0x000000000000FF00)) != 0) {
                            for (var i = 8; i < 16; ++i) {
                                if ((gf.high_flag & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }
                    }

                    if ((gf.high_flag & unchecked(0x00000000FFFF0000)) != 0) {
                        if ((gf.high_flag & unchecked(0x0000000000FF0000)) != 0) {
                            for (var i = 16; i < 24; ++i) {
                                if ((gf.high_flag & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }

                        if ((gf.high_flag & unchecked(0x00000000FF000000)) != 0) {
                            for (var i = 24; i < 32; ++i) {
                                if ((gf.high_flag & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }
                    }
                }

                if ((gf.high_flag & unchecked((long)0xFFFFFFFF00000000)) != 0) {
                    if ((gf.high_flag & unchecked((long)0x0000FFFF00000000)) != 0) {
                        if ((gf.high_flag & unchecked((long)0x000000FF00000000)) != 0) {
                            for (var i = 32; i < 40; ++i) {
                                if ((gf.high_flag & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }

                        if ((gf.high_flag & unchecked((long)0x0000FF0000000000)) != 0) {
                            for (var i = 40; i < 48; ++i) {
                                if ((gf.high_flag & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }
                    }

                    if ((gf.high_flag & unchecked((long)0xFFFF000000000000)) != 0) {
                        if ((gf.high_flag & unchecked((long)0x00FF000000000000)) != 0) {
                            for (var i = 48; i < 56; ++i) {
                                if ((gf.high_flag & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }

                        if ((gf.high_flag & unchecked((long)0xFF00000000000000)) != 0) {
                            for (var i = 56; i < 64; ++i) {
                                if ((gf.high_flag & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
