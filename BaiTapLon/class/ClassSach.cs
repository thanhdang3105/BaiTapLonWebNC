using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaiTapLon.server
{

    public class ClassSach
    {
        public int id { get; set; }
        public string name { get; set; }
        
        public string category { get; set; }
        public string desc { get; set; }
        public string imgSrc { get; set; }
        public int like { get; set; }

        public int view { get; set;}

        public ClassSach(int id, string Name, string Category, string desc, string src, int LIKE, int View)
        {
            this.id = id;
            this.name = Name;
            this.category = Category;
            this.desc = desc;
            this.imgSrc = src;
            this.like = LIKE;
            this.view = View;
        }

        public string converString()
        {

            if(!this.desc.StartsWith("\"") && !this.desc.EndsWith("\""))
            {
                this.desc = "\"" + this.desc + "\"";
            }


            string data = "{\"id\":\"" + this.id + "\", " +
                "\"name\":\"" + this.name.Replace("\"","\\\"") + "\"," +
                "\"category\":\"" + this.category.Replace("\"", "\\\"") + "\"," +
                "\"desc\":" + this.desc + "," +
                "\"imgSrc\":\"" + this.imgSrc + "\"," +
                "\"like\":\"" + this.like + "\"," +
                "\"view\":\"" + this.view + "\"}";
            return data.ToString();
        }
    }

    class CategoryClass
    {
        public string key { get; set; }
        public string value { get; set; }
        public CategoryClass(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
}