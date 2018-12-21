using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace G_H_Transplantation_For_AMAT.Resources
{
    /// <summary>
    /// 数値及び文字の各種変換
    /// </summary>
    class transform
    {
        /// <summary>
        /// 16進数値からASCIIのアルファベット or 数値に変換する関数
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string NunToChar(byte number)
        {
            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
            byte[] btNumber = new byte[] { (byte)number };
            return asciiEncoding.GetString(btNumber);
        }

        /// <summary>
        /// 半角に変換する関数
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns> 
        public static String ToDBC(String input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new String(c);
        }

    }
}
