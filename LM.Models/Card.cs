using System;
using System.Xml.Serialization;

namespace LM.Models
{
    [XmlRoot]
    public class Card
    {
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Morse { get; set; }
        [XmlElement]
        public string CharacterSide { get; set; }
        [XmlElement]
        public string MorseSide { get; set; }
        [XmlElement]
        public string Clip { get; set; }

        public Card() { }

    }
}
