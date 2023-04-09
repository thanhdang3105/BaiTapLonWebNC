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
        public string author { get; set; }
        public CategoryClass category { get; set; }
        public string content { get; set; }
        public string imgSrc { get; set; }
        public int like { get; set; }

        public int view { get; set;}

        public ClassSach(int id, string Name, string Author, string CategoryName, string CategoryKey, string Content, string src, int LIKE, int View)
        {
            this.id = id;
            this.name = Name;
            this.author = Author;
            this.category = new CategoryClass(CategoryName,CategoryKey);
            this.content = Content;
            this.imgSrc = src;
            this.like = LIKE;
            this.view = View;
        }
    }

    public class CategoryClass
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