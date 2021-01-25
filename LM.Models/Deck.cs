using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace LM.Models
{
    [XmlRoot]
    public class Deck
    {
        [XmlElement]
        public int Id { get; set; }

        [XmlElement]
        public string Name { get; set; }

        [XmlArray]
        public List<Card> Cards { get; set; }

        [XmlArray]
        public List<Question> Questions { get; set; }

        [XmlElement]
        public int Progress { get; set; }

        [XmlElement]
        public bool Finished { get; set; }

        public Deck() { }

        public Deck(int id, string name, List<string> cardNames, bool practiceLesson)
        {
            Id = id;
            Name = name;
            Cards = new List<Card>();
            foreach (string cardName in cardNames)
            {
                Cards.Add(CardDB.GetCardFromName(cardName));
            }
            Progress = 0;

            if (practiceLesson)
            {
                Questions = GeneratePracticeQuestions(Cards, this);
            }
            else
            {
                Questions = GenerateLessonQuestions(Cards, this);
            }
        }

        public Deck(int id, string name, List<Card> cards, bool practiceLesson)
        {
            Id = id;
            Name = name;
            Cards = cards;
            Progress = 0;

            if (practiceLesson)
            {
                Questions = GeneratePracticeQuestions(Cards, this);
            }
            else
            {
                Questions = GenerateLessonQuestions(Cards, this);
            }
        }

        public static Deck DeckFromSet(int id)
        {
            switch (id)
            {
                case 1:
                    return new Deck(0, "Unit 1: A-E", CardDB.CardsFromSet(id), false);
                case 2:
                    return new Deck(0, "Unit 2: F-J", CardDB.CardsFromSet(id), false);
                case 3:
                    return new Deck(0, "Unit 3: K-O", CardDB.CardsFromSet(id), false);
                case 4:
                    return new Deck(0, "Unit 4: P-T", CardDB.CardsFromSet(id), false);
                case 5:
                    return new Deck(0, "Unit 5: U-Z", CardDB.CardsFromSet(id), false);
                case 6:
                    return new Deck(0, "Unit 6: 0-4", CardDB.CardsFromSet(id), false);
                case 7:
                    return new Deck(0, "Unit 7: 5-9", CardDB.CardsFromSet(id), false);
                default:
                    return null;
            }
        }

        public void ShuffleDeck()
        {
            Random random = new Random();

            for (int i = Cards.Count - 1; i > 0; i--)
            {
                int j = (int) Math.Floor(random.NextDouble() * (i + 1));
                Card temp = Cards[i];
                Cards[i] = Cards[j];
                Cards[j] = temp;
            }
        }

        public List<Card> GetRandom(int amt, Card Card)
        {
            if (amt > Cards.Count)
            {
                return null;
            }

            List<Card> RandomCards = new List<Card>();
            List<Card> CardsCopy = new List<Card>(Cards);

            Random random = new Random();

            for (int i = 0; i < amt; i++)
            {
                int j = random.Next(CardsCopy.Count);
                if (j == CardsCopy.IndexOf(Card))
                {
                    j = (j + 1) % CardsCopy.Count;
                }
                RandomCards.Add(CardsCopy[j]);
                CardsCopy.RemoveAt(j);
             }

            return RandomCards;
        }

        private static List<Question> GeneratePracticeQuestions(List<Card> cards, Deck deck)
        {
            List<Question> Questions = new List<Question>();
            Array QTypeVals = Enum.GetValues(typeof(QType));
            Random random = new Random();

            foreach (Card card in cards)
            {
                for (int i = 0; i < 5; i++)
                {
                    QType QT = (QType)QTypeVals.GetValue(1 + random.Next(QTypeVals.Length - 1));
                    Questions.Add(new Question(0, QT, card, deck));
                }
            }

            return Questions;
        }

        private static List<Question> GenerateLessonQuestions(List<Card> cards, Deck deck)
        {
            List<Card> CardsCopy = new List<Card>(cards);
            Array QTypeVals = Enum.GetValues(typeof(QType));
            Random random = new Random();
            List<Question> Questions = new List<Question>();

            while (CardsCopy.Count > 0)
            {
                // setup intro session
                for (int i = 0; i < Math.Min(CardsCopy.Count, 3); i++)
                {
                    // add Intro question for a new card
                    Questions.Add(new Question(0, QType.Intro, CardsCopy[i], deck));

                    // add Practice questions for this new card
                    for (int j = 0; j < 2; j++)
                    {
                        // get value that is between 1-3 since we don't want an intro question to be in the practice pile
                        QType QT = (QType)QTypeVals.GetValue(1 + random.Next(QTypeVals.Length - 1));
                        Questions.Add(new Question(0, QT, CardsCopy[i], deck));
                    }
                }

                // setup practice session
                for (int i = 0; i < 5; i++)
                {
                    QType QT = (QType)QTypeVals.GetValue(1 + random.Next(QTypeVals.Length - 1));
                    int j = random.Next(Math.Min(CardsCopy.Count, 3));
                    Questions.Add(new Question(0, QT, CardsCopy[j], deck));
                }

                // remove the cards we just used from DeckCardsCopy
                CardsCopy.RemoveRange(0, Math.Min(CardsCopy.Count, 3));
            }

            return Questions;
        }
    }
}
