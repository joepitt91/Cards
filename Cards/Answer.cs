using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace JoePitt.Cards
{
    /// <summary>
    /// A player's answer, consisting of a Black Card and one or two White Cards.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{Submitter.Name}: {ToString}")]
    internal class Answer
    {
        /// <summary>
        /// The player who submitted the answer.
        /// </summary>
        public Player Submitter { get; private set; }
        /// <summary>
        /// The Black Card the answer is for.
        /// </summary>
        public Card BlackCard { get; private set; }
        /// <summary>
        /// The Submitted White Card.
        /// </summary>
        public Card WhiteCard { get; private set; }
        /// <summary>
        /// The Second White card, where required, that was submitted.
        /// </summary>
        public Card WhiteCard2 { get; private set; }
        /// <summary>
        /// The final text of the answer, after filling the blanks.
        /// </summary>
        public new string ToString { get; private set; }
        private string blankRegEx = "_{3,}";

        /// <summary>
        /// Create an answer for a black card which requires 1 White Card.
        /// </summary>
        /// <param name="submitter">The Player submitting the answer.</param>
        /// <param name="blackCard">The black card the answer is for.</param>
        /// <param name="whiteCard">The white card being submitted.</param>
        /// <exception cref="ArgumentNullException">A required parameter is missing.</exception>
        public Answer(Player submitter, Card blackCard, Card whiteCard)
        {
            Submitter = submitter ?? throw new ArgumentNullException("submitter");
            BlackCard = blackCard ?? throw new ArgumentNullException("blackCard");
            WhiteCard = whiteCard ?? throw new ArgumentNullException("whiteCard");
            ToString = BlackCard.ToString;
            Regex regexr = new Regex(blankRegEx);
            if (regexr.IsMatch(ToString))
            {
                ToString = regexr.Replace(ToString, WhiteCard.ToString, 1);
            }
            else
            {
                ToString = ToString + " " + WhiteCard.ToString;
            }
        }

        /// <summary>
        /// Create an answer for a black card which requires 2 White Card.
        /// </summary>
        /// <param name="submitter">The Player submitting the answer.</param>
        /// <param name="blackCard">The black card the answer is for.</param>
        /// <param name="whiteCard">The first white card being submitted.</param>
        /// <param name="whiteCard2">The second white card being submitted.</param>
        /// <exception cref="ArgumentNullException">A required parameter is missing.</exception>
        public Answer(Player submitter, Card blackCard, Card whiteCard, Card whiteCard2)
        {
            Submitter = submitter ?? throw new ArgumentNullException("submitter");
            BlackCard = blackCard ?? throw new ArgumentNullException("blackCard");
            WhiteCard = whiteCard ?? throw new ArgumentNullException("whiteCard");
            WhiteCard2 = whiteCard2 ?? throw new ArgumentNullException("whiteCard2");
            ToString = BlackCard.ToString;
            string blankRegEx = "_{3,}";
            Regex regexr = new Regex(blankRegEx);
            if (regexr.IsMatch(ToString))
            {
                ToString = regexr.Replace(ToString, whiteCard.ToString, 1);
                if (regexr.IsMatch(ToString))
                {
                    ToString = regexr.Replace(ToString, whiteCard2.ToString, 1);
                }
                else
                {
                    ToString = ToString + " " + WhiteCard2.ToString;
                }
            }
            else
            {
                ToString = ToString + " " + WhiteCard.ToString + " -- " + WhiteCard2.ToString;
            }
        }

        /// <summary>
        /// Deserialise Answer.
        /// </summary>
        /// <param name="SerialisedAnswer">The Serialised Answer.</param>
        public Answer(byte[] SerialisedAnswer)
        {
            using (MemoryStream stream = new MemoryStream(SerialisedAnswer)
            {
                Position = 0
            })
            {
                Answer imported = (Answer)Program.Formatter.Deserialize(stream);
                Submitter = imported.Submitter;
                BlackCard = imported.BlackCard;
                WhiteCard = imported.WhiteCard;
                WhiteCard2 = imported.WhiteCard2;
                ToString = imported.ToString;
            }
        }

        /// <summary>
        /// Convert the Answer to a Byte Array.
        /// </summary>
        /// <returns>The Serialised Byte Array.</returns>
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
