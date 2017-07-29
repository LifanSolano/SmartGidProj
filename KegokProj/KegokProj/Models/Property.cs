using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace KegokProj.Models
{
    public class Property
    {
        [DataMember]
        public string balloonContentHeader { get; set; }

        [DataMember]
        public string balloonContentBody { get; set; }

        [DataMember]
        public string balloonContentFooter { get; set; }

        [DataMember]
        public string clusterCaption { get; set; }

        [DataMember]
        public string hintContent { get; set; }
    }
}