using System;
using System.Collections.Generic;
using System.Text;

namespace CustomFont
{
    public class HashDictionary<TKey, TValue>
    {
        public HashDictionary()
        {
            d1 = new Dictionary<TKey, TValue>();
            d2 = new Dictionary<TValue, TKey>();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool ret = d1.TryGetValue(key, out TValue v);
            value = v;
            return ret;
        }

        public bool TryGetKey(TValue value, out TKey key)
        {
            bool ret = d2.TryGetValue(value, out TKey k);
            key = k;
            return ret;
        }

        public void Add(TKey key, TValue value)
        {
            d1.Add(key, value);
            d2.Add(value, key);
        }

        public void Clear()
        {
            d1.Clear();
            d2.Clear();
        }

        protected Dictionary<TKey, TValue> d1;
        protected Dictionary<TValue, TKey> d2;
    }

    public class ListDictionary<TKey, TValue>
    {
        public ListDictionary()
        {
            l1 = new List<TKey>();
            l2 = new List<TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            l1.Add(key);
            l2.Add(value);
        }

        public void AddRange(IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            l1.AddRange(keys);
            l2.AddRange(values);
        }

        public int IndexOfKey(TKey key)
        {
            return l1.IndexOf(key);
        }

        public int IndexOfValue(TValue value)
        {
            return l2.IndexOf(value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int i = l1.IndexOf(key);
            if (i < 0)
            {
                value = default;
                return false;
            }
            value = l2[i];
            return true;
        }

        public bool TryGetKey(TValue value, out TKey key)
        {
            int i = l2.IndexOf(value);
            if (i < 0)
            {
                key = default;
                return false;
            }
            key = l1[i];
            return true;
        }

        public TKey GetKey(int index)
        {
            if (index < 0 || index >= l1.Count)
                return default;
            return l1[index];
        }

        public TValue GetValue(int index)
        {
            if (index < 0 || index >= l2.Count)
                return default;
            return l2[index];
        }

        public bool Remove(TKey key)
        {
            int i = l1.IndexOf(key);
            if (i < 0)
                return false;
            RemoveAt(i);
            return true;
        }

        public bool Remove(TValue value)
        {
            int i = l2.IndexOf(value);
            if (i < 0)
                return false;
            RemoveAt(i);
            return true;
        }

        public void RemoveAt(int index)
        {
            l1.RemoveAt(index);
            l2.RemoveAt(index);
        }

        public void Insert(int index, TKey key, TValue value)
        {
            l1.Insert(index, key);
            l2.Insert(index, value);
        }

        public void Clear()
        {
            l1.Clear();
            l2.Clear();
        }

        public int Count { get { return l1.Count; } }

        public ICollection<TKey> Keys { get { return l1.AsReadOnly(); } }
        public ICollection<TValue> Values { get { return l2.AsReadOnly(); } }

        protected List<TKey> l1;
        protected List<TValue> l2;
    }

    /// <summary>
    /// 等字节编码类
    /// </summary>
    /// <typeparam name="T">编码</typeparam>
    public abstract class CustomEncoding<T> : Encoding
    {
        /// <summary>
        /// 等字节编码
        /// </summary>
        /// <param name="bytecount">每个字符编码字节长度</param>
        public CustomEncoding(int bytecount)
        {
            database = new HashDictionary<T, char>();
            ByteCount = bytecount;
        }

        protected HashDictionary<T, char> database;

        public readonly int ByteCount;

        public bool TryGetEncode(T code, out char c)
        {
            bool ret = database.TryGetValue(code, out char ch);
            c = ch;
            return ret;
        }

        public bool TryGetEncode(char c, out T code)
        {
            bool ret = database.TryGetKey(c, out T c1);
            code = c1;
            return ret;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (bytes.Length <= byteIndex)
                return 0;
            if (charCount > chars.Length - charIndex)
                charCount = chars.Length - charIndex;
            int ret = 0;
            for (int i = 0; i < charCount; ++i)
            {
                if (database.TryGetKey(chars[i + charIndex], out T v))
                {
                    if (Write(v, bytes, byteIndex))
                    {
                        ret += ByteCount;
                        byteIndex += ByteCount;
                    }
                    else
                        break;
                }
                else
                    break;
            }
            return ret;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (chars.Length <= charIndex)
                return 0;
            if (byteCount > bytes.Length - byteIndex)
                byteCount = bytes.Length - byteIndex;
            int ret = 0;
            for (int i = 0; i < byteCount; )
            {
                if (Read(bytes, byteIndex + i, out T value))
                {
                    i += ByteCount;
                    if (database.TryGetValue(value, out char c))
                    {
                        if (charIndex < chars.Length)
                        { 
                            chars[charIndex++] = c;
                            ret++;
                        }
                        else
                            break;
                    }
                    else
                        break;
                }
                else
                    break;
            }
            return ret;
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return ByteCount * Math.Min(count, chars.Length - index);
        }

        public override int GetByteCount(string s)
        {
            return ByteCount * s.Length;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return Math.Min(count, bytes.Length - index) / ByteCount;
        }

        public override int GetCharCount(byte[] bytes)
        {
            return GetCharCount(bytes, 0, bytes.Length);
        }

        public override byte[] GetBytes(string s)
        {
            byte[] ret = new byte[s.Length * ByteCount];
            GetBytes(s, 0, s.Length, ret, 0);
            return ret;
        }

        public override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (bytes.Length <= byteIndex)
                return 0;
            if (charCount > s.Length - charIndex)
                charCount = s.Length - charIndex;
            int ret = 0;
            for (int i = 0; i < charCount; ++i)
            {
                if (database.TryGetKey(s[i + charIndex], out T v))
                {
                    if (Write(v, bytes, byteIndex))
                    {
                        ret += ByteCount;
                        byteIndex += ByteCount;
                    }
                    else
                        break;
                }
                else
                    break;
            }
            return ret;
        }

        public override string GetString(byte[] bytes)
        {
            return GetString(bytes, 0, bytes.Length);
        }

        public override string GetString(byte[] bytes, int index, int count)
        {
            if (count <= 0)
                return string.Empty;
            if (count > bytes.Length - index)
                count = bytes.Length - index;
            StringBuilder sb = new StringBuilder(count / ByteCount);
            for (int i = 0; i < count;)
            {
                if (Read(bytes, index + i, out T value))
                {
                    i += ByteCount;
                    if (database.TryGetValue(value, out char c))
                        sb.Append(c);
                    else
                        break;
                }
                else
                    break;
            }
            return sb.ToString();
        }

        public override int GetMaxByteCount(int charCount)
        {
            return ByteCount * charCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount / ByteCount;
        }

        /// <summary>
        /// 将值写入字节数组中
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="bytes">字节数组</param>
        /// <param name="index">起始</param>
        /// <returns>成功写入返回True</returns>
        protected abstract bool Write(T value, byte[] bytes, int index);
        /// <summary>
        /// 从字节数组中读取值
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="index">起始</param>
        /// <param name="value">读出的值</param>
        /// <returns>成功读取返回True</returns>
        protected abstract bool Read(byte[] bytes, int index, out T value);
    }
}
