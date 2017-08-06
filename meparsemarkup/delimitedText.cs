using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace meparsemarkup
{

    public delegate void ProcessedRecord(object sender, EventArgs e);

    public class delimitedText
    {
        public struct T_Fields
        {
            public int lng_Tab;
            public string str_MARC_Field;
            public string str_Indicators;
            public string str_Punc;
            public int lng_Constant;
            public int lng_Repeatable;
            public int lng_group;

            public T_Fields(int plng_Tab,
                string pstr_MARC_Field,
                string pstr_Indicators,
                string pstr_Punct,
                int plng_Constant,
                int plng_Repeatable,
                int plng_group)
            {
                lng_Tab = plng_Tab;
                str_MARC_Field = pstr_MARC_Field;
                str_Indicators = pstr_Indicators;
                str_Punc = pstr_Punct;
                lng_Constant = plng_Constant;
                lng_Repeatable = plng_Repeatable;
                lng_group = plng_group;
            }
        }
        public event ProcessedRecord UpdateStatus;

        private string pLastError = "";
        private string pField_008 = "s9999\\\\\\\\xx\\\\\\\\\\\\\\\\\\\\\\\\000\\0\\und\\d";
        private string pField_LDR = "LDR  00000nam  2200000Ia 45e0";
        private string pFieldDelimiter = "\t";
        private string pTextQualifier = "";
        private bool pisUNIMARC = false;
        private System.Text.Encoding pEncoding = System.Text.Encoding.GetEncoding(1252);

        public void EventPump()
        {
            SendEvent(new System.EventArgs());
        }

        protected virtual void SendEvent(EventArgs e)
        {
            UpdateStatus(new delimitedText(), new EventArgs());
        }

        public string FieldDelimiter
        {
            set { pFieldDelimiter = value; }
            get { return pFieldDelimiter; }
        }

        public string TextQualifier
        {
            set { pTextQualifier = value; }
            get { return pTextQualifier; }
        }

        public System.Text.Encoding FileEncoding
        {
            set { pEncoding = value; }
            get { return pEncoding; }
        }

        public string LastError
        {
            get { return pLastError; }
            set { pLastError = value; }
        }

        public bool IsUNIMARC
        {
            get { return pisUNIMARC; }
            set { pisUNIMARC = value; }
        }

        public string Field_008
        {
            set { pField_008 = value; }
            get { return pField_008; }
        }

        public string Field_LDR
        {
            set { pField_LDR = value; }
            get { return pField_LDR; }
        }

        
        public bool ProcessProfile(string sSource, 
            string sDest, 
            string sProfile, 
            string filing_list)
        {

            
            
            bool bSort = false;
            bool bfiling_characters = false;
            string record_node = "";

            
            int x = 0;
            string t_string;
            string[] t_arr;
            System.IO.FileStream objSource = new System.IO.FileStream(sProfile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.StreamReader reader = new System.IO.StreamReader(objSource, System.Text.Encoding.UTF8);
            T_Fields args = new T_Fields();
            System.Collections.ArrayList tmpFields = new System.Collections.ArrayList();

            while (reader.Peek() > -1)
            {
                t_string = reader.ReadLine();
                if (t_string.Trim().Length != 0)
                {
                    if (t_string == "***Repeatable")
                    {
                        //ck_Repeatable.Checked = true;
                    }
                    else if (t_string == "#SORT") { bSort = true; }
                    else if (t_string == "#CALC_NONFILING") { bfiling_characters = true; }
                    else if (t_string.StartsWith("#RECORD_NODE="))
                    {
                        record_node = t_string.Substring("#RECORD_NODE=".Length);
                    }
                    else
                    {

                        if (t_string.IndexOf((char)30) > -1)
                        {
                            t_arr = t_string.Split(((char)30).ToString().ToCharArray());
                        }
                        else if (t_string.IndexOf((char)9) > -1)
                        {
                            t_arr = t_string.Split(((char)30).ToString().ToCharArray());
                        }
                        else
                        {
                            t_arr = t_string.Split(((char)30).ToString().ToCharArray());
                        }
                        
                        args.lng_Tab = System.Convert.ToInt32(t_arr[0]);
                        args.str_Indicators = t_arr[1];
                        args.str_MARC_Field = t_arr[2];
                        args.str_Punc = t_arr[3];
                        if (t_arr.Length > 3)
                        {
                            args.lng_Constant = System.Convert.ToInt32(t_arr[4]);
                        }
                        else
                        {
                            args.lng_Constant = 0;
                        }

                        if (t_arr.Length > 5)
                        {
                            args.lng_Repeatable = System.Convert.ToInt32(t_arr[5]);
                        }
                        else
                        {
                            args.lng_Repeatable = 0;
                        }
                        tmpFields.Add(args);                    
                    }
                    x++;
                    
                }
            }


            reader.Close();
            objSource.Close();

            meparsemarkup.delimitedText.T_Fields[] targs = new meparsemarkup.delimitedText.T_Fields[tmpFields.Count];
            tmpFields.CopyTo(targs);         

            if (bfiling_characters == false) { filing_list = null; }

            return parseXMLAsDelimited(sSource, sDest, record_node, targs, filing_list, bSort);
        }

        public string[] parseXMLAsDelimitedPreview(string sSource, 
            string record_node, 
            int iPreview = 3) {
            System.IO.FileStream xmlStream = new System.IO.FileStream(sSource, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.Xml.XmlReader rd = new System.Xml.XmlTextReader(xmlStream, System.Xml.XmlNodeType.Document, null);
            string sxml = "";
            string sbody = "";
            string sheader = "";
            int iCount = 0;
            try
            {
                while (rd.Read())
                {
                    if (rd.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        if (rd.LocalName == record_node ||
                            rd.Name == record_node)
                        {
                            sxml = rd.ReadOuterXml();
                            if (sheader.Length == 0)
                            {
                                sheader = parseNode(sxml, true);
                            }

                            if (iCount < iPreview)
                            {
                                sbody += parseNode(sxml) + "\n";
                                iCount++;
                            }
                            else
                            {
                                break;
                            }                                                        
                        }
                    }
                }

                sbody = sbody.TrimEnd("\n".ToCharArray());
                System.Collections.ArrayList tList = new System.Collections.ArrayList();
                tList.Add(sheader);
                foreach (string s in sbody.Split("\n".ToCharArray()))
                {
                    tList.Add(s);
                }

                if (rd != null)
                    rd.Close();

                string[] arr_return = new string[tList.Count];
                tList.CopyTo(arr_return);
                return arr_return;
            }
            catch (System.Exception pp)
            {
                LastError = "Unable to process XML File.\n\nFull Exception:\n" + pp.ToString();
                if (rd != null)
                    rd.Close();
                return null;
            }
        }

    

        public bool parseXMLAsDelimited(string sSource,
            string sDest, 
            string record_node,
            T_Fields[] arr_args,
            string filing_file = null,
            bool bSort = false)
        {

            System.Collections.ArrayList filing_values = null;
            string tmpDest = System.IO.Path.GetTempFileName();

            System.IO.FileStream xmlStream = new System.IO.FileStream(sSource, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(tmpDest, false, new System.Text.UTF8Encoding(false));
            System.Xml.XmlReader rd = new System.Xml.XmlTextReader(xmlStream, System.Xml.XmlNodeType.Document, null);
            string sreturn = "";
            try
            {
                while (rd.Read())
                {
                    if (rd.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        if (rd.LocalName == record_node ||
                            rd.Name == record_node)
                        {
                            sreturn = rd.ReadOuterXml();

                            writer.WriteLine(parseNode(sreturn));
                        }
                    }
                }
                if (rd != null)
                    rd.Close();
                if (writer != null)
                    writer.Close();

                return processFile2MARC(tmpDest, sDest, arr_args, filing_file, bSort);
            }
            catch (System.Exception pp)
            {
                LastError = "Unable to process XML File.\n\nFull Exception:\n" + pp.ToString();
                if (rd != null)
                    rd.Close();
                if (writer != null)
                    writer.Close();
                return false;
            }
        }

        public bool processFile2XMLMap(string sSource, 
            string sDest, 
            string sXMLMap)
        {
            if (!System.IO.File.Exists(sSource))
            {
                LastError = "Source file not found";
                return false;
            }

            if (!System.IO.File.Exists(sXMLMap))
            {
                LastError = "XML Map file not located.";
                return false;
            }


            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(sSource, FileEncoding, false);
                System.IO.StreamWriter writer = new System.IO.StreamWriter(sDest, false, new System.Text.UTF8Encoding(false));

                //load xml map
                string xml = System.IO.File.ReadAllText(sXMLMap, System.Text.Encoding.UTF8);

                string xml_header = xml.Substring(0, xml.IndexOf("END_HEADER-->") + "END_HEADER-->".Length);
                string xml_record = xml.Substring(xml.IndexOf("END_HEADER-->") + "END_HEADER-->".Length, xml.IndexOf("END_RECORD-->") + "END_RECORD-->".Length);
                string xml_footer = xml.Substring(xml.IndexOf("END_RECORD-->") + "END_RECORD-->".Length);

                string sline = "";
                string[] arr_fields = null;
                string tmp_record = "";
                int column_index = 0;

                writer.WriteLine(xml_header);
                while (reader.Peek() > -1)
                {
                    sline = reader.ReadLine();
                    if (sline.Trim().Length > 0)
                    {
                        arr_fields = ParseDelimited(sline, FieldDelimiter, TextQualifier);
                        column_index = 0;
                        foreach (string column in arr_fields)
                        {
                            tmp_record = tmp_record.Replace("{column_" + column_index.ToString() + "}", System.Security.SecurityElement.Escape(column));
                        }
                        writer.WriteLine(tmp_record);
                        EventPump();
                    }
                }

                writer.WriteLine(xml_footer);
                reader.Close();
                writer.Close();

                return true;

            } catch (System.Exception unknownError)
            {
                LastError = "Error processing xml map.\n\nFull Exception:\n" + unknownError.ToString();
                return false;
            }
        }


        public bool processFile2MARC(string sSource,
            string sDest,
            T_Fields[] arr_args,
            string filing_list = null, 
            bool bSort = false
            )
        {

            System.Collections.ArrayList filing_values = null;

            if (filing_list != null &&
                System.IO.File.Exists(filing_list))
            {
                filing_values = new System.Collections.ArrayList();
                System.IO.StreamReader filing_stream = new System.IO.StreamReader(filing_list, System.Text.Encoding.UTF8, false);
                while (filing_stream.Peek() > -1)
                {
                    string tfiling = filing_stream.ReadLine();
                    if (!string.IsNullOrEmpty(tfiling))
                    {
                        filing_values.Add(tfiling);
                    }
                }
                filing_stream.Close();
            }

            T_Fields args = new T_Fields();
            System.Collections.ArrayList arr_MARC = new System.Collections.ArrayList();
            int x;
            int Y;
            int i;
            int lret;
            string str_Filter;
            string str_SaveFile;
            string t_string;
            bool bool_found;
            string str_008;
            string tmp_string = "";

            string str_Data;
            string[] tmp_array;
            string str_MARC;
            int loc_index;



            string sLDR = "";
            string match_string = "";
            string tmp_match_string = "";
            string arg_match_indicators = "";
            string tmp_join = "";

            System.IO.FileStream objDest;
            System.IO.FileStream objSource;
            System.IO.StreamWriter writer;
            System.IO.StreamReader reader;


            //setup the filing digits
            if (filing_values == null ||
                filing_values.Count == 0)
            {
                filing_values = new System.Collections.ArrayList();
                filing_values.Add("a");
                filing_values.Add("an");
                filing_values.Add("and");
                filing_values.Add("the");
            }

            bool_found = false;
            loc_index = 0;


            if (sDest.Length != 0)
            {
                if (System.IO.File.Exists(sDest))
                {
                    try
                    {
                        System.IO.File.Delete(sDest);
                    }
                    catch (System.Exception fileException)
                    {
                        LastError = "Cannot create destination file.\n\nFull Exception:\n" + fileException.ToString();
                        return false;
                    }
                }
            }


            tmp_string = System.DateTime.Now.Year.ToString().Substring(2);
            //			if (tmp_string.Length > 2) 
            //			{
            //				tmp_string = tmp_string.Substring(tmp_string.Length-2, 2);
            //			}
            string str_master_008 = "";

            if (IsUNIMARC == false)
            {
                str_master_008 = "008  " + tmp_string + System.DateTime.Now.Month.ToString("00") + System.DateTime.Now.Day.ToString("00") + Field_008;
            }
            else
            {
                str_master_008 = @"100  \\$a" + System.DateTime.Now.Year.ToString("0000") + System.DateTime.Now.Month.ToString("00") + System.DateTime.Now.Day.ToString("00") + Field_008;
            }

            tmp_string = "";
            //arr_MARC.Add(str_008);
            loc_index++;

            Normalize_File(sSource);


            objSource = new System.IO.FileStream(sSource, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            reader = new System.IO.StreamReader(objSource, FileEncoding);

            objDest = new System.IO.FileStream(sDest, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            writer = new System.IO.StreamWriter(objDest, FileEncoding);

            bool is_008 = false;
            bool combine_field = false;
            char[] ctrims = new char[] { (char)0x0, ' ', (char)0x0D, (char)0x0A, (char)0x09 };

            int watch;
            while (reader.Peek() > -1)
            {
                str_Data = reader.ReadLine();

                if (str_Data.Trim(ctrims).Length != 0)
                {
                    str_Data = str_Data.Replace("\r", "");
                    str_Data = str_Data.Replace("\n", "");
                    tmp_array = ParseDelimited(str_Data, FieldDelimiter, TextQualifier);


                    //System.Windows.Forms.MessageBox.Show(string.Join("\t", tmp_array));
                    //tmp_array = str_Data.Split(STATIC_DELIMITER.ToCharArray());
                    watch = tmp_array.Length;
                    int length = 0;


                    if (tmp_array.Length > arr_args.Count())
                    {
                        length = arr_args.Count();
                    }
                    else
                    {
                        length = tmp_array.Length;
                    }

                    str_008 = str_master_008;
                    //System.Windows.Forms.MessageBox.Show(arr_args.Count.ToString());
                    for (Y = 0; Y < arr_args.Count(); Y++)
                    {
                        args = arr_args[Y];

                        if (args.str_MARC_Field.StartsWith("*"))
                        {
                            combine_field = true;
                            tmp_join = System.Text.RegularExpressions.Regex.Match(args.str_MARC_Field, @"\**[0-9a-zA-Z]{3}").Groups[0].Value; //args.str_MARC_Field.Substring(1);
                            match_string = args.str_MARC_Field.Replace("*", "").Substring(0, 3);
                            if (args.str_MARC_Field.Length < tmp_join.Length + 2)
                            {
                                arg_match_indicators = @"\\";
                            }
                            else
                            {
                                arg_match_indicators = args.str_MARC_Field.Substring(tmp_join.Length, 2);
                            }
                        }
                        else
                        {
                            if (args.str_MARC_Field.Trim().Length != 0)
                            {
                                match_string = args.str_MARC_Field.Substring(0, 3);
                            }
                            combine_field = false;
                            tmp_join = "";
                        }

                        if (args.lng_Tab < tmp_array.Length)
                        {
                            if (args.str_MARC_Field.Trim().Length != 0)
                            {


                                //System.Windows.Forms.MessageBox.Show(arr_MARC.Count.ToString());
                                //int sindex = FindLast(arr_MARC, args.str_MARC_Field.Substring(0, 3));
                                int sindex = FindLast(arr_MARC, tmp_join);
                                //System.Windows.Forms.MessageBox.Show(sindex.ToString());
                                if (sindex < 0)
                                {
                                    sindex = 0;
                                }

                                for (i = sindex; i < arr_MARC.Count; i++)
                                {
                                    if (args.lng_Tab < 0)
                                    {
                                        continue;
                                    }
                                    //System.Windows.Forms.MessageBox.Show(args.str_MARC_Field + "\n" + bool_found.ToString());    
                                    bool_found = false;

                                    tmp_match_string = System.Text.RegularExpressions.Regex.Match((string)arr_MARC[i], @"\**[0-9a-zA-Z]{3}").Groups[0].Value;
                                    tmp_string = (string)arr_MARC[i];

                                    //System.Windows.Forms.MessageBox.Show(args.str_MARC_Field + "\n" + tmp_string);		
                                    //System.Windows.Forms.MessageBox.Show(tmp_string.Substring(0, 3) + "\n" + args.str_MARC_Field.Substring(0, 3));
                                    //if (tmp_string.Substring(0, 3) == args.str_MARC_Field.Substring(0, 3))
                                    //System.Windows.Forms.MessageBox.Show(tmp_match_string + "\n" + match_string);
                                    if (tmp_match_string == tmp_join)
                                    {
                                        if (args.lng_Constant != 1)
                                        {
                                            if (args.str_MARC_Field == "LDR")
                                            {

                                                sLDR = args.str_MARC_Field + "  " + tmp_array[args.lng_Tab];

                                            }
                                            else if (args.str_MARC_Field.Length == 3)
                                            {

                                                if (args.str_MARC_Field == "008")
                                                {
                                                    is_008 = true;
                                                }
                                                arr_MARC[i] = tmp_string + tmp_array[args.lng_Tab];
                                                bool_found = true;
                                                break;
                                            }
                                            //else if (tmp_string.IndexOf(args.str_MARC_Field.Substring(3, 2)) == -1 || combine_field == true || args.lng_Repeatable==1)
                                            else if (tmp_string.IndexOf(arg_match_indicators) == -1 || combine_field == true || args.lng_Repeatable == 1)
                                            {
                                                if (tmp_array[args.lng_Tab].Trim().Length != 0)
                                                {
                                                    if (combine_field == true)
                                                    {
                                                        if (tmp_array[args.lng_Tab].EndsWith(args.str_Punc))
                                                        {

                                                            if (((string)arr_MARC[i]).IndexOf(args.str_MARC_Field.Substring(tmp_join.Length)) < 0)
                                                            {
                                                                //System.Windows.Forms.MessageBox.Show("1: \ntmp_string: " + tmp_string + "\nstr_marc_field: " + args.str_MARC_Field.Substring(tmp_join.Length) + "\nargs_lng_tab: " + tmp_array[args.lng_Tab]); 
                                                                arr_MARC[i] = tmp_string + args.str_MARC_Field.Substring(tmp_join.Length) + tmp_array[args.lng_Tab];
                                                            }
                                                            else
                                                            {
                                                                //System.Windows.Forms.MessageBox.Show("2: \ntmp_string: " + tmp_string + "\n" + tmp_array[args.lng_Tab]);
                                                                if (args.lng_Repeatable == 1)
                                                                {
                                                                    //this isn't just a combined field, it's a repeated subfield on the joined element
                                                                    string subfield = args.str_MARC_Field.Substring(tmp_join.Length);
                                                                    if (subfield.IndexOf("$") > -1)
                                                                    {
                                                                        subfield = subfield.Substring(subfield.IndexOf("$"));
                                                                    }
                                                                    arr_MARC[i] = tmp_string + subfield + tmp_array[args.lng_Tab];
                                                                }
                                                                else
                                                                {
                                                                    arr_MARC[i] = tmp_string + tmp_array[args.lng_Tab];
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (((string)arr_MARC[i]).IndexOf(args.str_MARC_Field.Substring(tmp_join.Length)) < 0)
                                                            {
                                                                //System.Windows.Forms.MessageBox.Show("3: \ntmp_string: " + tmp_string + "\nstr_marc_field: " + args.str_MARC_Field.Substring(tmp_join.Length) + "\nargs_lng_tab: " + tmp_array[args.lng_Tab]); 
                                                                arr_MARC[i] = tmp_string + args.str_MARC_Field.Substring(tmp_join.Length) + tmp_array[args.lng_Tab] + args.str_Punc;
                                                            }
                                                            else
                                                            {
                                                                //System.Windows.Forms.MessageBox.Show("4: \ntmp_string: " + tmp_string + "\n" + tmp_array[args.lng_Tab]);
                                                                if (args.lng_Repeatable == 1)
                                                                {
                                                                    //this isn't just a combined field, it's a repeated subfield on the joined element
                                                                    string subfield = args.str_MARC_Field.Substring(tmp_join.Length);
                                                                    if (subfield.IndexOf("$") > -1)
                                                                    {
                                                                        subfield = subfield.Substring(subfield.IndexOf("$"));
                                                                    }
                                                                    arr_MARC[i] = tmp_string + subfield + tmp_array[args.lng_Tab] + args.str_Punc;

                                                                }
                                                                else
                                                                {
                                                                    arr_MARC[i] = tmp_string + tmp_array[args.lng_Tab] + args.str_Punc;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        arr_MARC[i] = tmp_string + args.str_MARC_Field.Substring(3) + tmp_array[args.lng_Tab] + args.str_Punc;
                                                    }
                                                    bool_found = true;
                                                    break;

                                                }
                                            }
                                            else
                                            {

                                                if (tmp_array[args.lng_Tab].Trim().Length != 0)
                                                {
                                                    //arr_MARC(loc_index) = VB.Left(arr_args(Y).str_MARC_Field, 3) + "  " + arr_args(Y).str_Indicators + Mid(arr_args(Y).str_MARC_Field, 4) + tmp_array(arr_args(Y).lng_Tab) + arr_args(Y).str_Punc;
                                                    arr_MARC.Add(args.str_MARC_Field.Substring(0, 3) + "  " + args.str_Indicators + args.str_MARC_Field.Substring(3) + tmp_array[args.lng_Tab] + args.str_Punc);
                                                    tmp_string = "";
                                                    loc_index++;
                                                    bool_found = true;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {

                                            if ((tmp_array[args.lng_Tab].Trim().Length != 0) && (tmp_string.Trim().Length != 0))
                                            {
                                                if (args.str_MARC_Field == "LDR")
                                                {
                                                    sLDR = args.str_MARC_Field + "  " + tmp_array[args.lng_Tab];
                                                }
                                                else if (args.str_MARC_Field.Length == 3)
                                                {
                                                    if (args.str_MARC_Field == "008")
                                                    {
                                                        is_008 = true;
                                                    }
                                                    arr_MARC[i] = tmp_string + tmp_array[args.lng_Tab];
                                                    bool_found = true;
                                                    break;
                                                }
                                                else if (tmp_string.IndexOf(args.str_MARC_Field.Substring(3, 2)) == -1 || combine_field == true || args.lng_Repeatable == 1)
                                                {
                                                    //if (combine_field == true)
                                                    //{
                                                    //    arr_MARC[i] = tmp_string +  args.str_Punc;
                                                    //}
                                                    //else
                                                    //{



                                                    //System.Windows.Forms.MessageBox.Show(args.str_MARC_Field);
                                                    int del_index = args.str_MARC_Field.IndexOf("$");
                                                    if (del_index == -1)
                                                    {
                                                        del_index = 3;
                                                    }

                                                    arr_MARC[i] = tmp_string + args.str_MARC_Field.Substring(del_index) + args.str_Punc;
                                                    //}
                                                    bool_found = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                }

                                if (bool_found == false)
                                {
                                    if (args.lng_Tab < 0)
                                    {

                                        arr_MARC.Add(args.str_MARC_Field.Substring(0, 3) + "  " + args.str_Indicators + args.str_MARC_Field.Substring(3) + args.str_Punc);
                                        bool_found = true;
                                        loc_index++;
                                    }
                                    else
                                    {

                                        if (args.lng_Constant != 1)
                                        {
                                            if ((tmp_array.Length - 1) >= args.lng_Tab)
                                            {
                                                if (tmp_array[args.lng_Tab].Trim().Length != 0)
                                                {
                                                    if (args.str_MARC_Field == "LDR")
                                                    {
                                                        sLDR = args.str_MARC_Field + "  " + tmp_array[args.lng_Tab];
                                                    }

                                                    else
                                                    {
                                                        if (args.str_MARC_Field == "008")
                                                        {
                                                            is_008 = true;
                                                        }


                                                        if (args.str_MARC_Field.StartsWith("*"))
                                                        {
                                                            arr_MARC.Add(args.str_MARC_Field.Substring(0, tmp_join.Length) + "  " + args.str_Indicators + args.str_MARC_Field.Substring(tmp_join.Length) + tmp_array[args.lng_Tab] + args.str_Punc);
                                                        }
                                                        else
                                                        {
                                                            arr_MARC.Add(args.str_MARC_Field.Substring(0, 3) + "  " + args.str_Indicators + args.str_MARC_Field.Substring(3) + tmp_array[args.lng_Tab] + args.str_Punc);
                                                        }
                                                        bool_found = true;
                                                        loc_index++;
                                                    }

                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (args.str_MARC_Field.StartsWith("*"))
                                            {
                                                arr_MARC.Add(args.str_MARC_Field.Substring(0, tmp_join.Length) + "  " + args.str_Indicators + args.str_MARC_Field.Substring(tmp_join.Length) + args.str_Punc);
                                            }
                                            else
                                            {
                                                arr_MARC.Add(args.str_MARC_Field.Substring(0, 3) + "  " + args.str_Indicators + args.str_MARC_Field.Substring(3) + args.str_Punc);
                                            }
                                            bool_found = true;
                                            loc_index++;
                                        }
                                    }
                                }
                                else
                                {
                                    bool_found = false;
                                }
                            }
                        }
                        EventPump();
                    }
                   
                    if (is_008 == false)
                    {
                        //arr_MARC.Insert(2,str_008);
                        if (str_008.IndexOf("{260$c}") > -1)
                        {
                            string t260c = "9999";
                            //loop through the array for the field
                            foreach (string f in arr_MARC)
                            {
                                //System.Windows.Forms.MessageBox.Show(f);
                                if (f.IndexOf("260  ") > -1)
                                {
                                    string[] ta = f.Split("$".ToCharArray());
                                    foreach (string sf in ta)
                                    {
                                        if (sf.StartsWith("c"))
                                        {
                                            t260c = sf.Substring(1);
                                        }
                                    }
                                }
                            }

                            System.Text.RegularExpressions.Regex objr = new System.Text.RegularExpressions.Regex("[^0-9]");
                            t260c = objr.Replace(t260c, "");
                            
                            if (t260c.Length != 4)
                            {
                                t260c = "9999";
                            }

                            str_008 = str_008.Replace("{260$c}", t260c);
                        }
                        else if (str_008.IndexOf("{264$c}") > -1)
                        {
                            //arr_MARC.Add(str_008);
                            string t260c = "9999";
                            //loop through the array for the field
                            foreach (string f in arr_MARC)
                            {
                                //System.Windows.Forms.MessageBox.Show(f);
                                if (f.IndexOf("264  ") > -1)
                                {
                                    string[] ta = f.Split("$".ToCharArray());
                                    foreach (string sf in ta)
                                    {
                                        if (sf.StartsWith("c"))
                                        {
                                            t260c = sf.Substring(1);
                                        }
                                    }
                                }
                            }
                            //Normalize the t260c variable 
                            //if ?, - in the field, drop it and use the 9999 constant
                            //System.Windows.Forms.MessageBox.Show(t260c);
                            System.Text.RegularExpressions.Regex objr = new System.Text.RegularExpressions.Regex("[^0-9]");
                            t260c = objr.Replace(t260c, "");
                            //System.Windows.Forms.MessageBox.Show(t260c);
                            if (t260c.Length != 4)
                            {
                                t260c = "9999";
                            }

                            str_008 = str_008.Replace("{264$c}", t260c);
                        }
                        arr_MARC.Add(str_008);
                    }

                    for (int tmp_i = 0; tmp_i < arr_MARC.Count; tmp_i++)
                    {
                        if (((string)arr_MARC[tmp_i]).IndexOf("*") > -1)
                        {
                            arr_MARC[tmp_i] = System.Text.RegularExpressions.Regex.Replace((string)arr_MARC[tmp_i], @"^\**([0-9a-zA-Z]{3})", "$1");                            
                        }
                    }

                    if (bSort == true)
                    {
                        arr_MARC.Sort(); //Hopefully this does what I'm looking for.
                    }
                    if (sLDR.Length > 0)
                    {
                        if (FileEncoding == System.Text.Encoding.UTF8)
                        {
                            if (sLDR.Length > 15)
                            {
                                sLDR = sLDR.Substring(0, 14) + "a" + sLDR.Substring(15);
                            }
                        }
                        str_MARC = "=" + sLDR + System.Environment.NewLine;
                    }
                    else
                    {
                        string tmpLDR = Field_LDR;
                        if (FileEncoding == System.Text.Encoding.UTF8)
                        {
                            if (tmpLDR.Length > 15)
                            {
                                tmpLDR = tmpLDR.Substring(0, 14) + "a" + tmpLDR.Substring(15);
                            }
                        }
                        str_MARC = "=" + tmpLDR + System.Environment.NewLine;
                    }

                    
                    for (x = 0; x < arr_MARC.Count; x++)
                    {
                        tmp_string = (string)arr_MARC[x];
                        if (tmp_string.StartsWith("245"))
                        {
                            tmp_string = ProcessFiling(tmp_string, filing_values);
                        }
                        str_MARC += "=" + tmp_string + System.Environment.NewLine;
                    }
                    writer.WriteLine(str_MARC);
                    bool_found = false;
                    str_MARC = "";
                    arr_MARC.Clear();
                    loc_index = 1;
                    is_008 = false;
                    sLDR = "";
                }
            }
            reader.Close();
            writer.Close();
            objSource.Close();
            objDest.Close();

            
            return true;

        }


    /*******************************************************************
     * Private Functions
     * ****************************************************************/

    private string parseNode(string xml, bool bheader = false)
    {
        string csvSeparator = "\t";
        Func<string, string> escapeValue = val => val;
        
        XElement doc = XElement.Parse(xml);
        var headers = doc.Elements().Select(el => el.Name.LocalName);
        string headerRow = string.Join(csvSeparator, headers);
        
        if (bheader == true) { return headerRow; }
        
        var rows = from el in doc.Elements() 
                   select escapeValue(Normalize(el.Value));

        return string.Join(csvSeparator, rows);
    }

        private string Normalize(string value)
        {
            return value.Replace("\t", "; ").Replace("\r\n", " ").Replace("\r", "").Replace("\n", " ");
        }

        private string ProcessFiling(string tmp_string, 
            System.Collections.ArrayList filing_values)
        {
            string tmp_field = "";
            string tmp_ind1 = "";
            string tmp_ind2 = "";
            string tmp = "";
            string tmp_test = "";
            int indicator_holder = 0;

            tmp_field = tmp_string.Substring(0, 3);
            tmp_ind1 = tmp_string.Substring(5, 1);
            tmp_ind2 = tmp_string.Substring(6, 1);

            if (helpers.IsNumeric(tmp_ind2) == true)
            {
                indicator_holder = System.Convert.ToInt32(tmp_ind2);
            }
            tmp = tmp_string.Substring(7);
            tmp_test = tmp.Substring(2);

            do
            {
                Startfilingloop:
                foreach (string f in filing_values)
                {
                    if (tmp_test.ToLower().StartsWith(f + " "))
                    {
                        indicator_holder += (f + " ").Length;
                        tmp_test = tmp_test.Substring(f.Length + 1);
                        goto Startfilingloop;
                    }
                }
                break;
            } while (tmp_test.Length > 0);

            return tmp_field + "  " + tmp_ind1 + indicator_holder.ToString() + tmp;
        }


        private int FindLast(System.Collections.ArrayList a, string s)
        {
            int last = -1;
            for (int x = 0; x < a.Count; x++)
            {
                string tmp = (string)a[x];
                if (tmp.IndexOf(s) > -1)
                {
                    last = x;
                }
            }
            return last;
        }

        private void Normalize_File(string sFile)
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            string tbuffer = "";
            int iread = 0;
            System.IO.File.Copy(sFile, tmpfile, true);

            System.IO.StreamReader reader = new System.IO.StreamReader(tmpfile, FileEncoding);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(sFile, false, FileEncoding);

            while (reader.Peek() > -1)
            {
                char[] tmp = new char[8000];
                iread = reader.ReadBlock(tmp, 0, tmp.Length);
                string s = new string(tmp, 0, iread);
                if (tbuffer != "")
                {
                    s = tbuffer + s;
                    tbuffer = "";
                }

                if (s.EndsWith("\r") == true)
                {
                    tbuffer = "\r";
                    s = s.Substring(0, s.Length - 1);
                }
                s = s.Replace("\r\n", "{mynewline}");
                if (s.IndexOf("{mynewline}") == -1)
                {
                    s = s.Replace("\n", "{mynewline}");
                }
                else
                {
                    s = s.Replace("\n", " ");
                }
                s = s.Replace("{mynewline}", System.Environment.NewLine);
                writer.Write(s);

            }

            writer.Close();
            reader.Close();

            try
            {
                System.IO.File.Delete(tmpfile);
            }
            catch { }
        }

        private string[] ParseDelimited(string str1, string del, string qual)
        {
            string[] tmp;

            if (qual != "")
            {
                string pattern = del + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(pattern);

                tmp = r.Split(str1);

                string trimstring = " \n\r" + qual;

                if (qual != "")
                {
                    for (int x = 0; x < tmp.Length; x++)
                    {
                        tmp[x] = tmp[x].Trim(trimstring.ToCharArray());
                        tmp[x] = tmp[x].Replace(qual + qual, qual);
                    }
                }
            }
            else
            {
                tmp = str1.Split(del.ToCharArray());
            }
            return tmp;
        }

        private string CTrim(string str1, string str2)
        {
            string tmp = "";
            for (int x = 0; x <= str1.Length; x++)
            {
                if (str1.Substring(x, 1) != str2)
                {
                    tmp = str1.Substring(x);
                    break;
                }
            }
            str1 = tmp;
            for (int x = (str1.Length - 1); x >= 0; x--)
            {
                if (str1.Substring(x, 1) != str2)
                {
                    tmp = str1.Substring(1, x);
                    break;
                }
            }
            return str1;
        }

    }
}
