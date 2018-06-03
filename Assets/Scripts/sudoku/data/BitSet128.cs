using System.Collections.Generic;

namespace sudoku.data
{
    public struct BitSet128
    {
        private long m_LowBit;
        private long m_HighBit;

        public BitSet128(long low_bit, long high_bit)
        {
            m_LowBit = low_bit;
            m_HighBit = high_bit;
        }

        public BitSet128(int idx)
        {
            m_LowBit = 0L;
            m_HighBit = 0L;

            SetBit(idx);
        }

        public void SetBit(int idx)
        {
            if (idx >= 64) {
                m_HighBit |= (1L << (idx - 64));
            }
            else {
                m_LowBit |= (1L << idx);
            }
        }

        public void UnSetBit(int idx)
        {
            if (idx >= 64) {
                m_HighBit &= ~(1L << (idx - 64));
            }
            else {
                m_LowBit &= ~(1L << idx);
            }
        }

        public bool HasBit(int idx)
        {
            if (idx >= 64) {
                return (m_HighBit & (1L << (idx - 64))) != 0;
            }
            else {
                return (m_LowBit & (1L << idx)) != 0;
            }
        }

        /// <summary>
        /// 算交集
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitSet128 Intersect(BitSet128 other)
        {
            return new BitSet128(other.m_LowBit & m_LowBit, other.m_HighBit & m_HighBit);
        }

        /// <summary>
        /// 算并集
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitSet128 Union(BitSet128 other)
        {
            return new BitSet128(other.m_LowBit | m_LowBit, other.m_HighBit | m_HighBit);
        }

        /// <summary>
        /// 算差集
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitSet128 Exclusive(BitSet128 other)
        {
            return new BitSet128(other.m_LowBit ^ m_LowBit, other.m_HighBit ^ m_HighBit);
        }

        /// <summary>
        /// 去掉另一个集合的元素
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitSet128 Minus(BitSet128 other)
        {
            return new BitSet128((m_LowBit | other.m_LowBit) ^ other.m_LowBit, (m_HighBit | other.m_HighBit) ^ other.m_HighBit);
        }

        public int BitCount
        {
            get {
                // Brian Kernighan's Algorithm
                // @see http://cs-fundamentals.com/tech-interview/c/c-program-to-count-number-of-ones-in-unsigned-integer.php
                int count = 0;
                var n = m_LowBit;
                while (n != 0) {
                    n = n & (n - 1);
                    ++count;
                }

                n = m_HighBit;
                while (n != 0) {
                    n = n & (n - 1);
                    ++count;
                }

                return count;
            }
        }

#if UNITY_EDITOR_WIN
        public string DebugLowBitString
        {
            get {
                return System.Convert.ToString(m_LowBit, 2);
            }
        }

        public string DebugHighBitString
        {
            get {
                return System.Convert.ToString(m_HighBit, 2);
            }
        }

        public string DebugContentString
        {
            get {
                var sb = new System.Text.StringBuilder();
                for (var r = 0; r < 9; ++r) {
                    for (var c = 0; c < 9; ++c) {
                        var idx = r * 9 + c;
                        if (HasBit(idx)) {
                            sb.Append(1);
                        }
                        else {
                            sb.Append(0);
                        }
                    }
                    sb.Append('\n');
                }

                return sb.ToString();
            }
        }
#endif

        public void Reset()
        {
            m_LowBit = 0;
            m_HighBit = 0;
        }

        public bool IsEmpty
        {
            get {
                return m_LowBit == 0 && m_HighBit == 0;
            }
        }

        public static BitSet128 zero = new BitSet128();

        #region operator
        public static bool operator ==(BitSet128 lhs, BitSet128 rhs)
        {
            if (lhs.m_LowBit == rhs.m_LowBit && lhs.m_HighBit == rhs.m_HighBit) {
                return true;
            }
            else {
                return false;
            }
        }

        public static bool operator !=(BitSet128 lhs, BitSet128 rhs)
        {
            return !(lhs == rhs);
        }

        public static BitSet128 operator +(BitSet128 lhs, BitSet128 rhs)
        {
            return new BitSet128(
                lhs.m_LowBit | rhs.m_LowBit,
                lhs.m_HighBit | rhs.m_HighBit);
        }

        public static BitSet128 operator -(BitSet128 lhs, BitSet128 rhs)
        {
            return new BitSet128(
                (lhs.m_LowBit | rhs.m_LowBit) ^ rhs.m_LowBit,
                (lhs.m_HighBit | rhs.m_HighBit) ^ rhs.m_HighBit);
        }

        public static bool operator true(BitSet128 bit_set_128)
        {
            return bit_set_128.m_LowBit != 0 || bit_set_128.m_HighBit != 0;
        }

        public static bool operator false(BitSet128 bit_set_128)
        {
            return bit_set_128.m_LowBit == 0 && bit_set_128.m_HighBit == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is BitSet128) {
                return (BitSet128)obj == this;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return m_HighBit.ToString() + m_LowBit.ToString();
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

        public static IEnumerable<int> AllBits(BitSet128 bit_set_128)
        {
            if (bit_set_128.m_LowBit != 0) {
                if ((bit_set_128.m_LowBit & unchecked(0x00000000FFFFFFFF)) != 0) {
                    if ((bit_set_128.m_LowBit & unchecked(0x000000000000FFFF)) != 0) {
                        if ((bit_set_128.m_LowBit & unchecked(0x00000000000000FF)) != 0) {
                            for (var i = 0; i < 8; ++i) {
                                if ((bit_set_128.m_LowBit & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }

                        if ((bit_set_128.m_LowBit & unchecked(0x000000000000FF00)) != 0) {
                            for (var i = 8; i < 16; ++i) {
                                if ((bit_set_128.m_LowBit & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }
                    }

                    if ((bit_set_128.m_LowBit & unchecked(0x00000000FFFF0000)) != 0) {
                        if ((bit_set_128.m_LowBit & unchecked(0x0000000000FF0000)) != 0) {
                            for (var i = 16; i < 24; ++i) {
                                if ((bit_set_128.m_LowBit & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }

                        if ((bit_set_128.m_LowBit & unchecked(0x00000000FF000000)) != 0) {
                            for (var i = 24; i < 32; ++i) {
                                if ((bit_set_128.m_LowBit & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }
                    }
                }

                if ((bit_set_128.m_LowBit & unchecked((long)0xFFFFFFFF00000000)) != 0) {
                    if ((bit_set_128.m_LowBit & unchecked((long)0x0000FFFF00000000)) != 0) {
                        if ((bit_set_128.m_LowBit & unchecked((long)0x000000FF00000000)) != 0) {
                            for (var i = 32; i < 40; ++i) {
                                if ((bit_set_128.m_LowBit & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }

                        if ((bit_set_128.m_LowBit & unchecked((long)0x0000FF0000000000)) != 0) {
                            for (var i = 40; i < 48; ++i) {
                                if ((bit_set_128.m_LowBit & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }
                    }

                    if ((bit_set_128.m_LowBit & unchecked((long)0xFFFF000000000000)) != 0) {
                        if ((bit_set_128.m_LowBit & unchecked((long)0x00FF000000000000)) != 0) {
                            for (var i = 48; i < 56; ++i) {
                                if ((bit_set_128.m_LowBit & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }

                        if ((bit_set_128.m_LowBit & unchecked((long)0xFF00000000000000)) != 0) {
                            for (var i = 56; i < 64; ++i) {
                                if ((bit_set_128.m_LowBit & (1L << i)) != 0) {
                                    yield return i;
                                }
                            }
                        }
                    }
                }
            }

            if (bit_set_128.m_HighBit != 0) {
                if ((bit_set_128.m_HighBit & unchecked(0x00000000FFFFFFFF)) != 0) {
                    if ((bit_set_128.m_HighBit & unchecked(0x000000000000FFFF)) != 0) {
                        if ((bit_set_128.m_HighBit & unchecked(0x00000000000000FF)) != 0) {
                            for (var i = 0; i < 8; ++i) {
                                if ((bit_set_128.m_HighBit & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }

                        if ((bit_set_128.m_HighBit & unchecked(0x000000000000FF00)) != 0) {
                            for (var i = 8; i < 16; ++i) {
                                if ((bit_set_128.m_HighBit & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }
                    }

                    if ((bit_set_128.m_HighBit & unchecked(0x00000000FFFF0000)) != 0) {
                        if ((bit_set_128.m_HighBit & unchecked(0x0000000000FF0000)) != 0) {
                            for (var i = 16; i < 24; ++i) {
                                if ((bit_set_128.m_HighBit & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }

                        if ((bit_set_128.m_HighBit & unchecked(0x00000000FF000000)) != 0) {
                            for (var i = 24; i < 32; ++i) {
                                if ((bit_set_128.m_HighBit & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }
                    }
                }

                if ((bit_set_128.m_HighBit & unchecked((long)0xFFFFFFFF00000000)) != 0) {
                    if ((bit_set_128.m_HighBit & unchecked((long)0x0000FFFF00000000)) != 0) {
                        if ((bit_set_128.m_HighBit & unchecked((long)0x000000FF00000000)) != 0) {
                            for (var i = 32; i < 40; ++i) {
                                if ((bit_set_128.m_HighBit & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }

                        if ((bit_set_128.m_HighBit & unchecked((long)0x0000FF0000000000)) != 0) {
                            for (var i = 40; i < 48; ++i) {
                                if ((bit_set_128.m_HighBit & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }
                    }

                    if ((bit_set_128.m_HighBit & unchecked((long)0xFFFF000000000000)) != 0) {
                        if ((bit_set_128.m_HighBit & unchecked((long)0x00FF000000000000)) != 0) {
                            for (var i = 48; i < 56; ++i) {
                                if ((bit_set_128.m_HighBit & (1L << i)) != 0) {
                                    yield return i + 64;
                                }
                            }
                        }

                        if ((bit_set_128.m_HighBit & unchecked((long)0xFF00000000000000)) != 0) {
                            for (var i = 56; i < 64; ++i) {
                                if ((bit_set_128.m_HighBit & (1L << i)) != 0) {
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
