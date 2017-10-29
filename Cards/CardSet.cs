using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace JoePitt.Cards
{
    /// <summary>
    /// A collection of Cards which make up a Set.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{Name} ({Version})")]
    internal class CardSet
    {
        /// <summary>
        /// The Globally Unique Identifier of the Card Set.
        /// </summary>
        public string CardSetGuid { get; private set; }
        /// <summary>
        /// The Display Name of the Card Set.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The version of the Card Set, in x.y format.
        /// </summary>
        public string Version { get; private set; }
        /// <summary>
        /// The Number of White Cards.
        /// </summary>
        public int WhiteCardCount { get; private set; }
        /// <summary>
        /// All the White Cards in the Set.
        /// </summary>
        public Dictionary<string, Card> WhiteCards { get; private set; }
        /// <summary>
        /// The current order of the White Cards.
        /// </summary>
        public Dictionary<int, string> WhiteCardIndex { get; private set; }
        /// <summary>
        /// The number of Black Cards in the Set.
        /// </summary>
        public int BlackCardCount { get; private set; }
        /// <summary>
        /// All the Black Cards in the Set.
        /// </summary>
        public Dictionary<string, Card> BlackCards { get; private set; }
        /// <summary>
        /// The current order of the Black Cards.
        /// </summary>
        public Dictionary<int, string> BlackCardIndex { get; private set; }
        private string Hash;

        /// <summary>
        /// Loads a Card Set from its XML File.
        /// </summary>
        /// <param name="cardSetGuid">The GUID of the Card Set to load.</param>
        /// <exception cref="ArgumentNullException">Thrown if no Guid is provided.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the card set cannot be found.</exception>
        /// <exception cref="XmlException">Thrown if the card set XML is corrupted.</exception>
        /// <exception cref="ApplicationException">Thrown if the card set is corrupted.</exception>
        public CardSet(string cardSetGuid)
        {
            if (string.IsNullOrEmpty(cardSetGuid)) { throw new ArgumentNullException("cardSetGuid"); }
            if (!Dealer.TestCardSetPath()) throw new IOException("Card Set Path does not exist, and could not be created.");
            string[] files = Directory.GetFiles(Program.CardSetPath, "*" + cardSetGuid + ".cardset");
            if (files == null)
            {
                throw new FileNotFoundException("Cannot find card set", Program.CardSetPath + "\\*" + cardSetGuid + ".cardset");
            }
            XmlDocument cardSetDoc = new XmlDocument();
            cardSetDoc.Load(files[0]);
            XmlElement cardSetInfo = (XmlElement)cardSetDoc.GetElementsByTagName("CardSet")[0];
            string[] setInfo = new string[5];
            CardSetGuid = cardSetInfo.GetAttribute("GUID");
            Name = setInfo[1] = cardSetInfo.GetAttribute("Name");
            Version = setInfo[2] = cardSetInfo.GetAttribute("Version");

            XmlElement xmlBlackCards = (XmlElement)cardSetDoc.GetElementsByTagName("BlackCards")[0];
            XmlElement xmlWhiteCards = (XmlElement)cardSetDoc.GetElementsByTagName("WhiteCards")[0];
            SHA256CryptoServiceProvider hasher = new SHA256CryptoServiceProvider();
            byte[] allCards = Encoding.Default.GetBytes(xmlBlackCards.InnerXml + xmlWhiteCards.InnerXml);
            byte[] hash = hasher.ComputeHash(allCards);
            Hash = Convert.ToBase64String(hash);

            if (cardSetInfo.GetAttribute("Hash") == Hash)
            {
            }
            else
            {
                throw new FormatException("Card Set " + Name + " Corrupt.");
            }

            BlackCardCount = 0;
            BlackCards = new Dictionary<string, Card>();
            BlackCardIndex = new Dictionary<int, string>();
            XmlNodeList CardBlock = cardSetDoc.GetElementsByTagName("CardPack");
            CardBlock = cardSetDoc.GetElementsByTagName("BlackCards");
            XmlNodeList Cards = CardBlock[0].ChildNodes;
            foreach (XmlElement Card in Cards)
            {
                string cardID = CardSetGuid.ToString() + "/" + Card.Attributes["ID"].Value;
                BlackCards.Add(cardID, new Card(cardID, Card.InnerText, Convert.ToInt32(Card.Attributes["Needs"].Value)));
                BlackCardCount++;
                BlackCardIndex.Add(BlackCardCount, cardID);
            }

            WhiteCardCount = 0;
            WhiteCards = new Dictionary<string, Card>();
            WhiteCardIndex = new Dictionary<int, string>();
            CardBlock = cardSetDoc.GetElementsByTagName("WhiteCards");
            Cards = CardBlock[0].ChildNodes;
            foreach (XmlElement Card in Cards)
            {
                string cardID = cardSetGuid + "/" + Card.Attributes["ID"].Value;
                WhiteCards.Add(cardID, new Card(cardID, Card.InnerText));
                WhiteCardCount++;
                WhiteCardIndex.Add(WhiteCardCount, cardID);
            }
            Dealer.ShuffleCards(BlackCardIndex);
            Dealer.ShuffleCards(WhiteCardIndex);
        }

        /// <summary>
        /// Merges 1 or more Card Set(s) into this temporary Game Set.
        /// </summary>
        /// <param name="cardSetGuids">GUIDs for all the Card Sets to be added.</param>
        /// <exception cref="ArgumentNullException">Thrown if no Guid is provided.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the card set cannot be found.</exception>
        /// <exception cref="XmlException">Thrown if the card set XML is corrupted.</exception>
        /// <exception cref="ApplicationException">Thrown if the card set is corrupted.</exception>
        public void Merge(List<string> cardSetGuids)
        {
            if (cardSetGuids == null) { throw new ArgumentNullException("cardSetGuids"); }
            CardSetGuid = new Guid().ToString();
            Name = "GameSet";
            Version = "0.0";
            foreach (string guid in cardSetGuids)
            {

                string[] files = Directory.GetFiles(Program.CardSetPath, "*" + guid + ".cardset");
                if (files == null)
                {
                    throw new FileNotFoundException("Cannot find card set", Program.CardSetPath + guid + ".cardset");
                }
                XmlDocument cardSetDoc = new XmlDocument();
                cardSetDoc.Load(files[0]);
                XmlElement cardSetInfo = (XmlElement)cardSetDoc.GetElementsByTagName("CardSet")[0];
                XmlElement xmlBlackCards = (XmlElement)cardSetDoc.GetElementsByTagName("BlackCards")[0];
                XmlElement xmlWhiteCards = (XmlElement)cardSetDoc.GetElementsByTagName("WhiteCards")[0];
                SHA256CryptoServiceProvider hasher = new SHA256CryptoServiceProvider();
                byte[] allCards = Encoding.Default.GetBytes(xmlBlackCards.InnerXml + xmlWhiteCards.InnerXml);
                byte[] hash = hasher.ComputeHash(allCards);
                Hash = Convert.ToBase64String(hash);

                if (cardSetInfo.GetAttribute("Hash") == Hash)
                {
                }
                else
                {
                    throw new FormatException("Card Set " + Name + " Corrupt.");
                }

                XmlNodeList CardBlock = cardSetDoc.GetElementsByTagName("CardPack");
                CardBlock = cardSetDoc.GetElementsByTagName("BlackCards");
                XmlNodeList Cards = CardBlock[0].ChildNodes;
                foreach (XmlElement Card in Cards)
                {
                    string cardID = guid + "/" + Card.Attributes["ID"].Value;
                    BlackCards.Add(cardID, new Card(cardID, Card.InnerText, Convert.ToInt32(Card.Attributes["Needs"].Value)));
                    BlackCardCount++;
                    BlackCardIndex.Add(BlackCardCount, cardID);
                }

                CardBlock = cardSetDoc.GetElementsByTagName("WhiteCards");
                Cards = CardBlock[0].ChildNodes;
                foreach (XmlElement Card in Cards)
                {
                    string cardID = guid + "/" + Card.Attributes["ID"].Value;
                    WhiteCards.Add(cardID, new Card(cardID, Card.InnerText));
                    WhiteCardCount++;
                    WhiteCardIndex.Add(WhiteCardCount, cardID);
                }
            }
            Dealer.ShuffleCards(BlackCardIndex);
            Dealer.ShuffleCards(WhiteCardIndex);
        }

        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                Program.Formatter.Serialize(stream, this);
                stream.Position = 0;
                bytes = stream.ToArray();
            }
            return bytes;
        }
    }
}