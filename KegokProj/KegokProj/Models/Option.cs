using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KegokProj.Models
{
    public class Option
    {
        public string iconLayout { get; set; }

        public string iconImageHref { get; set; }

        public int[] iconImageSize { get; set; }

        public int[] iconImageOffset { get; set; }
    }
}