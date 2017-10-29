using System;
using System.IO;
using System.Diagnostics;

namespace JoePitt.Cards
{
    /// <summary>
    /// A playing card.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{ToString}")]
    internal class Card
    {
        /// <summary>
        /// The Unique Identified of the card, [GUID]/[Type][Number]
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// The text of the card.
        /// </summary>
        public new string ToString { get; private set; }
        /// <summary>
        /// For black cards, the number of white cards required by this card.
        /// </summary>
        public int Needs { get; private set; }

        /// <summary>
        /// Create a Black Card.
        /// </summary>
        /// <param name="id">The Unique Identifier of the card.</param>
        /// <param name="text">The Text of the card.</param>
        /// <param name="needs">How many white cards, this black card requires.</param>
        public Card(string id, string text, int needs)
        {
            Id = id;
            ToString = text;
            Needs = needs;
        }

        /// <summary>
        /// Create a White Card.
        /// </summary>
        /// <param name="id">The Unique Identifier of the card.</param>
        /// <param name="text">The Text of the card.</param>
        public Card(string id, string text)
        {
            Id = id;
            ToString = text;
        }

        /// <summary>
        /// Exports the card as a byte array.
        /// </summary>
        /// <returns>A byte array of the Card.</returns>
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                Program.Formatter.Serialize(stream, this);
                bytes = stream.ToArray();
            }
            return bytes;
        }
    }
}
