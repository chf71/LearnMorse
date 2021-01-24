using System;
using System.Collections.Generic;
using System.Text;


namespace LM.Models
{
    public class CardDB
    {
        public static List<string> AllNames { get; set; } =
            new List<string> {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k",
                                "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v",
                                "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6",
                                "7", "8", "9"};

        public static List<string> AllCharNames { get; set; } = AllNames.GetRange(0, 26);

        public static List<string> AllNumNames { get; set; } = AllNames.GetRange(26, 10);

        public static List<string> AllMorse { get; set; } =
            new List<string> {". _ ", "_ . . . ", "_ . _ . ", "_ . . ", ". ", ". . _ . ", "_ _ . ",
                                ". . . . ", ". . ", ". _ _ _ ", "_ . _ ", ". _ .  .", "_ _ ", "_ . ",
                                "_ _ _ ", ". _ . _ ", "_ _ . _ ", ". _ . ", ". . . ", "_ ", ". . _ ",
                                ". . . _ ", ". _ _ ", "_ . . _ ", "_ . _ _ ", "_ _ . . ", "_ _ _ _ _ ",
                                ". _ _ _ _ ", ". . _ _ _ ", ". . . _ _ ", ". . . . _ ", ". . . . . ",
                                "_ . . . . ", "_ _ . . . ", "_ _ _ . . ", "_ _ _ _ . "};

        public static List<string> AllCharMorse { get; set; } = AllMorse.GetRange(0, 26);

        public static List<string> AllNumMorse { get; set; } = AllMorse.GetRange(26, 10);

        public static List<Card> AllCards { get; set; } = LoadCards();

        public static List<Card> AllCharCards { get; set; } = LoadAllCharCards();

        public static List<Card> AllNumCards { get; set; } = LoadAllNumCards();

        public static List<Card> LoadAllCharCards()
        {
            List<Card> cards = new List<Card>();

            int i = 0;
            foreach (string name in AllCharNames)
            {
                Card card = new Card();
                card.Name = name;
                card.Morse = AllCharMorse[i];
                card.CharacterSide = "images/" + card.Name + "_back.png";
                card.MorseSide = "images/" + card.Morse + "_front.png";
                card.Clip = "clips/" + card.Name + ".mp3";
                i++;

                cards.Add(card);
            }

            return cards;
        }

        public static List<Card> LoadAllNumCards()
        {
            List<Card> cards = new List<Card>();

            int i = 0;
            foreach (string name in AllNumNames)
            {
                Card card = new Card();
                card.Name = name;
                card.Morse = AllNumMorse[i];
                card.CharacterSide = "images/" + card.Name + "_back.png";
                card.MorseSide = "images/" + card.Morse + "_front.png";
                card.Clip = "clips/" + card.Name + ".mp3";
                i++;

                cards.Add(card);
            }

            return cards;
        }


        [Obsolete]
        public CardDB()
        {
            LoadCards();
        }

        public static List<Card> LoadCards()
        {
            List<Card> cards = new List<Card>();

            int i = 0;
            foreach (string name in AllNames)
            {
                Card card = new Card();
                card.Name = name;
                card.Morse = AllMorse[i];
                card.CharacterSide = "images/" + card.Name + "_back.png";
                card.MorseSide = "images/" + card.Name + "_front.png";
                card.Clip = "clips/" + card.Name + ".mp3";
                i++;

                cards.Add(card);
            }

            return cards;
        }

        public static List<Card> CardsFromSet(int set)
        {
            switch (set)
            {
                case 1: return AllCards.GetRange(0, 5);
                case 2: return AllCards.GetRange(5, 5);
                case 3: return AllCards.GetRange(10, 5);
                case 4: return AllCards.GetRange(15, 5);
                case 5: return AllCards.GetRange(20, 6);
                case 6: return AllCards.GetRange(26, 5);
                case 7: return AllCards.GetRange(31, 5);
                default: return null;
            }
        }

        public static Card GetCardFromName(string name)
        {
            foreach (Card card in AllCards)
            {
                if (card.Name.Equals(name)) return card;
            }

            return null;
        }
    }
}
