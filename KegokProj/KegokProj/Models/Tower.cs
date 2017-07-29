using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace KegokProj.Models
{
    public class Tower
    {
        [DataMember]
        public string type { get; set; }

        [DataMember]
        public int id { get; set; }

        [DataMember]
        public Geometry geometry { get; set; }

        [DataMember]
        public Property properties { get; set; }

        [DataMember]
        public Option options { get; set; }
    }
}