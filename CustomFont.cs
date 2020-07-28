using System;
using System.Collections.Generic;
using System.IO;

namespace CustomFont
{
    public class CustomFont : DoubleBytesEncoding
    {
        public CustomFont(string fontpath) : base(fontpath)
        {
            if (!File.Exists(fontpath))
                return;
            FilePath = fontpath + ".cfont";
            LoadCustomFont();
        }

        public string FilePath { get; set; }

        public void LoadCustomFont()
        {
            if (!File.Exists(FilePath))
                return;
            using (StreamReader sr = new StreamReader(FilePath))
            {
                while (sr.Peek() >= 0)
                {
                    string ss = sr.ReadLine().Trim();
                    if (ss.Length == 0)
                        continue;
                    if (database.TryGetKey(ss[0], out ushort code))
                        custom.Add(ss[0], code);
                }
            }
        }

        public void SaveCustomFont()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
            using (StreamWriter sr = new StreamWriter(FilePath))
            {
                foreach (char c in custom.Keys)
                    sr.WriteLine(c);
            }
        }

        public void Clear()
        {
            custom.Clear();
        }

        public UInt16 StartCode { get; set; }
        /// <summary>
        /// 不认识的字符如果数据库中存在就自动加入字库
        /// </summary>
        public bool AutoAddToCustomFont { get; set; }

        private string _inputstring;
        public string InputString
        {
            get
            {
                return _inputstring;
            }
            set
            {
                _inputstring = value;
                InputBytes = this.GetBytes(_inputstring);
                OutputBytes = EvaluateString(_inputstring);
            }
        }

        private byte[] _inputbytes;
        public byte[] InputBytes
        {
            get
            {
                return _inputbytes;
            }
            set
            {
                _inputbytes = value;
            }
        }

        private byte[] _outputbytes;
        public byte[] OutputBytes
        {
            get
            {
                return _outputbytes;
            }
            set
            {
                _outputbytes = value;
            }
        }

        public bool TryGetCustomCode(char c, out UInt16 code)
        {
            int i = custom.IndexOfKey(c);
            if (i < 0)
            {
                code = default;
                return false; 
            }
            code = System.Convert.ToUInt16(StartCode + i);
            return true;
        }

        public byte[] EvaluateString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            List<byte> ret = new List<byte>();
            for (int i = 0; i < s.Length; ++i)
            {
                char c = s[i];
                if (TryGetCustomCode(c, out ushort value))
                    ret.AddRange(this.GetBytes(value));
                else if (AutoAddToCustomFont)
                {
                    if (database.TryGetKey(c, out ushort ov))
                    {
                        custom.Add(c, ov);
                        if (TryGetCustomCode(c, out ushort v2))
                            ret.AddRange(this.GetBytes(v2));
                    }
                }
            }
            return ret.ToArray();
        }

        public string FormatCustomFont(UInt32 cPerRow)
        {
            if (cPerRow == 0)
                return string.Empty;
            string ret = "";
            int count = custom.Count;
            for (int i = 0; i < count; )
            {
                for (int j = 0; j < cPerRow; ++j)
                { 
                    ret += custom.GetKey(i);
                    if (++i >= count)
                        return ret;
                }
                ret += Environment.NewLine;
            }
            return ret;
        }

        public string FormatBytes(byte[] bytes, string seperator)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;
            string ret = string.Empty;
            for (int i = 0; i < bytes.Length; ++i)
            {
                if (i > 0)
                    ret += seperator;
                ret += System.Convert.ToString(bytes[i], 16).ToUpper().PadLeft(2, '0');
            }
            return ret;
        }

        /// <summary>
        /// 自定义字库
        /// </summary>
        protected ListDictionary<char, UInt16> custom = new ListDictionary<char, UInt16>();
    }


    /// <summary>
    /// 双字节编码类
    /// </summary>
    public class DoubleBytesEncoding : CustomEncoding<UInt16>
    {
        public DoubleBytesEncoding(string txtpath, UInt16 mincode = 0, UInt16 maxcode = 0xFFFF) : base(2)
        {
            if (!File.Exists(txtpath))
                return;
            using (StreamReader sr = new StreamReader(txtpath))
            {
                while (sr.Peek() >= 0)
                {
                    string[] ss = sr.ReadLine().Split('\t');
                    if (ss.Length > 3)
                    {
                        try
                        {
                            UInt16 code = System.Convert.ToUInt16(ss[1]);
                            if (code < mincode || code > maxcode)
                                continue;
                            char c = ss[3][0];
                            database.Add(code, c);
                        }
                        catch { }
                    }
                }
            }
        }

        protected override bool Write(ushort value, byte[] bytes, int index)
        {
            if (index < bytes.Length && index + 1 < bytes.Length)
            {
                bytes[index] = System.Convert.ToByte(value >> 8);
                bytes[index + 1] = System.Convert.ToByte(value & 0x00FF);
                return true;
            }
            return false;
        }

        protected byte[] GetBytes(ushort value)
        {
            return new byte[2]
            {
                System.Convert.ToByte(value >> 8),
                System.Convert.ToByte(value & 0x00FF)
            };
        }

        protected override bool Read(byte[] bytes, int index, out ushort value)
        {
            if (index < bytes.Length && index + 1 < bytes.Length)
            {
                value = System.Convert.ToUInt16((bytes[index] << 8) + bytes[index + 1]);
                return true;
            }
            value = 0;
            return false;
        }
    }
}
