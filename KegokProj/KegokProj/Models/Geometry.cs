using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KegokProj.Models
{
    public class Geometry
    {
        [DataMember]
        public string type { get; set; }

        [DataMember]
        public double[] coordinates { get; set; }
    }
}