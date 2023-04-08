using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaiTapLon.server 
{

    public class SexFormat
    {
        public int formatted;
        public SexFormat(string text)
        {
            string textConverted = !String.IsNullOrEmpty(text) ? text.ToLower() : "";
            if(textConverted == "male" || textConverted == "nam")
            {
                this.formatted = 1;
            } else
            {
                this.formatted = 0;
            }
        }
    }
}