using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace LM.Models
{
    [XmlRoot]
    public enum QType { Intro, CharMC, MorseMC, MorseInput }

    [XmlRoot]
    public class Question
    {
        [XmlElement]
        public int Id { get; set; }
        [XmlArray]
        public List<Card> Answers { get; set; }
        [XmlElement]
        public Card Card { get; set; }
        [XmlElement]
        public QType Type { get; set; }
        [XmlElement]
        public int CorrectAnswerIndex { get; set; }

        public Question() { }

        public Question(int Id, QType Type, Card Card, Deck Deck)
        {
            this.Id = Id;
            this.Type = Type;
            this.Card = Card;

            if (Type == QType.CharMC || Type == QType.MorseMC)
            {
                Answers = new List<Card>();
                Answers.AddRange(Deck.GetRandom(2, Card));

                // add correct answer (Card) randomly to Answers
                int j = (int)Math.Floor(new Random().NextDouble() * (Answers.Count + 1));
                Answers.Insert(j, Card);
                CorrectAnswerIndex = j;
            }
        }

        public List<string> GetAnswers()
        {
            List<string> AnswerText = new List<string>();

            if (Type == QType.CharMC)
            {
                foreach (var answer in Answers)
                {
                    AnswerText.Add(answer.Name);
                }
            }
            else if (Type == QType.MorseMC)
            {
                foreach (var answer in Answers)
                {
                    AnswerText.Add(answer.Morse);
                }
            }

            return AnswerText;
        }

        public string GetFaceImage()
        {
            if (Type != QType.CharMC)
            {
                return Card.CharacterSide;
            }
            else
            {
                return Card.MorseSide;
            }
        }

        public string GetBackImage()
        {
            if (Type != QType.CharMC)
            {
                return Card.MorseSide;
            }
            else
            {
                return Card.CharacterSide;
            }
        }
    }
}
