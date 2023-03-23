using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaiTapLon.server
{

    public class ClassSach
    {
        public int ID { get; set; }
        public string name { get; set; }
        
        public string category { get; set; }
        public string description { get; set; }
        public string imgSrc { get; set; }
        public int like { get; set; }

        public int view { get; set;}

        public ClassSach(int id, string Name, string Category, string desc, string src, int LIKE, int View)
        {
            this.ID = id;
            this.name = Name;
            this.category = Category;
            this.description = desc;
            this.imgSrc = src;
            this.like = LIKE;
            this.view = View;
        }

        public string converString()
        {
            string data = "{\"id\":\"" + this.ID + "\", " +
                "\"name\":\"" + this.name + "\"," +
                "\"category\":\"" + this.category + "\"," +
                "\"description\":\"" + this.description.Trim() + "\"," +
                "\"imgSrc\":\"" + this.imgSrc + "\"," +
                "\"like\":\"" + this.like + "\"," +
                "\"view\":\"" + this.view + "\"}";
            return data.ToString();
        }
    }
}