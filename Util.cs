using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CustomFont
{
    static class Util
    {
        public static Dictionary<UInt16, string> ReadFontFromTxt(string fontpath, UInt16 mincode = 0, UInt16 maxcode = 0xFFFF)
        {
            if (!File.Exists(fontpath))
                return null;
            Dictionary<UInt16, string> fonts = new Dictionary<UInt16, string>();
            using (StreamReader sr = new StreamReader(fontpath))
            {
                while (sr.Peek() >= 0)
                {
                    string[] ss = sr.ReadLine().Split('\t');
                    if (ss.Length > 3)
                    {
                        try
                        {
                            UInt16 code = Convert.ToUInt16(ss[1]);
                            if (code < mincode || code > maxcode)
                                continue;
                            fonts.Add(code, ss[3]);
                        }
                        catch { }
                    }
                }
            }
            return fonts;
        }

        public static Dictionary<string, UInt16> ReadFontFromTxtR(string fontpath, UInt16 mincode = 0, UInt16 maxcode = 0xFFFF)
        {
            if (!File.Exists(fontpath))
                return null;
            Dictionary<string, UInt16> fonts = new Dictionary<string, UInt16>();
            using (StreamReader sr = new StreamReader(fontpath))
            {
                while (sr.Peek() >= 0)
                {
                    string[] ss = sr.ReadLine().Split('\t');
                    if (ss.Length > 3)
                    {
                        try
                        {
                            UInt16 code = Convert.ToUInt16(ss[1]);
                            if (code < mincode || code > maxcode)
                                continue;
                            fonts.Add(ss[3], code);
                        }
                        catch { }
                    }
                }
            }
            return fonts;
        }
    }
}
