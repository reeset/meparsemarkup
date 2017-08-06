using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace meparsemarkup
{
    public static class helpers
    {
        public static bool IsNumeric(string s)
        {
            if (s.Length == 0) return false;
            for (int x = 0; x < s.Length; x++)
            {
                if (Char.IsNumber(s[x]) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /*==============================================
		 * Function/Sub: IsMARC
		 * Description: Checks to see if the record is 
		 * MARC or not.
		 * =============================================*/
        public static bool IsMARC(string sSource)
        {
            char[] buffer = new char[1024];

            System.IO.Stream objSource = new System.IO.FileStream(sSource, System.IO.FileMode.Open);
            System.IO.StreamReader w = new System.IO.StreamReader(objSource, System.Text.Encoding.GetEncoding(1252));
            w.Read(buffer, 0, 1024);

            w.Close();
            objSource.Close();
            for (int x = 0; x < 5; x++)
            {
                if (Char.IsNumber(buffer[x]) == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
