using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace USITCC_Registration
{

        [XmlRoot(ElementName = "team")]
        public class Team
        {

            [XmlElement(ElementName = "contest")]
            public string Contest { get; set; }

            [XmlElement(ElementName = "school")]
            public string School { get; set; }

            [XmlElement(ElementName = "city")]
            public string City { get; set; }

            [XmlElement(ElementName = "state")]
            public string State { get; set; }

            [XmlElement(ElementName = "name")]
            public string Name { get; set; }

            [XmlElement(ElementName = "contestant")]
            public List<string> Contestant { get; set; }

            [XmlElement(ElementName = "email")]
            public List<string> Email { get; set; }

            [XmlElement(ElementName = "username")]
            public List<string> Username { get; set; }

            [XmlElement(ElementName = "username")]
            public List<string> Password { get; set; }
    }

        [XmlRoot(ElementName = "teams")]
        public class Teams
        {

            [XmlElement(ElementName = "team")]
            public List<Team> Team { get; set; }
        }


    
}
