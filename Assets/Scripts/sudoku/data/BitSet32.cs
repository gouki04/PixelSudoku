using System.Collections.Generic;

namespace sudoku.data
{
    public struct BitSet32
    {
        private int m_Bit;

        public BitSet32(int bit)
        {
            m_Bit = bit;
        }

        public void SetBit(int idx)
        {
            m_Bit |= (1 << idx);
        }

        /// <summary>
        /// 外部要先用HasBit判断，再调用本函数来删
        /// </summary>
        /// <param name="idx"></param>
        public void UnSetBit(int idx)
        {
            m_Bit &= ~(1 << idx);
        }

        public bool HasBit(int idx)
        {
            return (m_Bit & (1 << idx)) != 0;
        }

        /// <summary>
        /// 算交集
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitSet32 Intersect(BitSet32 other)
        {
            return new BitSet32(other.m_Bit & m_Bit);
        }

        /// <summary>
        /// 算并集
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitSet32 Union(BitSet32 other)
        {
            return new BitSet32(other.m_Bit | m_Bit);
        }

        /// <summary>
        /// 算差集
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitSet32 Exclusive(BitSet32 other)
        {
            return new BitSet32(other.m_Bit ^ m_Bit);
        }

        /// <summary>
        /// 去掉另一个集合的元素
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public BitSet32 Minus(BitSet32 other)
        {
            return new BitSet32((m_Bit | other.m_Bit) ^ other.m_Bit);
        }

        public int BitCount
        {
            get {
                // Brian Kernighan's Algorithm
                // @see http://cs-fundamentals.com/tech-interview/c/c-program-to-count-number-of-ones-in-unsigned-integer.php
                int count = 0;
                int n = m_Bit;
                while (n != 0) {
                    n = n & (n - 1);
                    ++count;
                }

                return count;
            }
        }

#if UNITY_EDITOR_WIN
        public string DebugBitString
        {
            get {
                return System.Convert.ToString(m_Bit, 2);
            }
        }

        public string DebugContentString
        {
            get {
                var sb = new System.Text.StringBuilder();
                foreach (var bit in AllBits(this, 9)) {
                    sb.Append(bit.ToString());
                    sb.Append(',');
                }

                return sb.ToString();
            }
        }
#endif

        public void Reset()
        {
            m_Bit = 0;
        }

        public bool IsEmpty
        {
            get {
                return m_Bit == 0;
            }
        }

        public static BitSet32 zero = new BitSet32(0);

        #region operator
        public static implicit operator BitSet32(int bit)
        {
            return new BitSet32(bit);
        }

        public static bool operator ==(BitSet32 lhs, BitSet32 rhs)
        {
            if (lhs.m_Bit == rhs.m_Bit) {
                return true;
            } else {
                return false;
            }
        }

        public static bool operator !=(BitSet32 lhs, BitSet32 rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator ==(BitSet32 lhs, int rhs)
        {
            if (lhs.m_Bit == rhs) {
                return true;
            } else {
                return false;
            }
        }

        public static bool operator !=(BitSet32 lhs, int rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator ==(int lhs, BitSet32 rhs)
        {
            if (lhs == rhs.m_Bit) {
                return true;
            } else {
                return false;
            }
        }

        public static bool operator !=(int lhs, BitSet32 rhs)
        {
            return !(lhs == rhs);
        }

        public static BitSet32 operator +(BitSet32 lhs, BitSet32 rhs)
        {
            return new BitSet32(lhs.m_Bit | rhs.m_Bit);
        }

        public static BitSet32 operator -(BitSet32 lhs, BitSet32 rhs)
        {
            return new BitSet32((lhs.m_Bit | rhs.m_Bit) ^ rhs.m_Bit);
        }

        public static bool operator true(BitSet32 nf)
        {
            return nf.m_Bit != 0;
        }

        public static bool operator false(BitSet32 nf)
        {
            return nf.m_Bit == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is BitSet32) {
                return (BitSet32)obj == this;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_Bit;
        }

        public override string ToString()
        {
            return m_Bit.ToString();
        }

        #endregion

        #region IEnumerable
        public static IEnumerable<int> AllBits(BitSet32 bit_set_32, int max)
        {
            if (bit_set_32.m_Bit != 0) {
                for (var i = 0; i <= max; ++i) {
                    if (bit_set_32.HasBit(i)) {
                        yield return i;
                    }
                }
            }
        }

        public static bool GetBits(BitSet32 bit_set_32, out int a)
        {
            a = 0;
            if (bit_set_32.m_Bit != 0) {
                for (var i = 0; i < 32; ++i) {
                    if (bit_set_32.HasBit(i)) {
                        a = i;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool GetBits(BitSet32 bit_set_32, out int a, out int b)
        {
            a = 0;
            b = 0;
            if (bit_set_32.m_Bit != 0) {
                var i = 0;
                for (i = 0; i < 32; ++i) {
                    if (bit_set_32.HasBit(i)) {
                        a = i;
                        break;
                    }
                }

                for (i = i + 1; i < 32; ++i) {
                    if (bit_set_32.HasBit(i)) {
                        b = i;
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion
    }
}
