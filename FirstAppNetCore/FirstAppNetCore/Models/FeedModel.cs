using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FirstAppNetCore.Models
{
    public class FeedModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
        public string Fonte { get; set; }
        public string Img { get; set; }
        public bool Ratio { get; set; }
        public DateTime PublishDate { get; set; }

        public FeedModel()
        {
            Title = "";
            Description = "";
            Content = "";
            Link = "";
            Img = "";
            PublishDate = DateTime.Today;
        }
    }
}
