using System;
using System.IO;

namespace JoePitt.Cards
{
    /// <summary>
    /// A Player's Vote of the best answer.
    /// </summary>
    [Serializable]
    internal class Vote
    {
        /// <summary>
        /// Who submitted the vote.
        /// </summary>
        public Player Voter { get; set; }
        /// <summary>
        /// The Answer that was voted for.
        /// </summary>
        public Answer Choice { get; set; }

        /// <summary>
        /// Creates a vote.
        /// </summary>
        /// <param name="voteFrom">Who's submitting the vote.</param>
        /// <param name="voteFor">Which Answer did they vote for.</param>
        public Vote(Player voteFrom, Answer voteFor)
        {
            Voter = voteFrom;
            Choice = voteFor;
        }

        /// <summary>
        /// Exports the Vote to a byte array.
        /// </summary>
        /// <returns>The byte array of the Vote.</returns>
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
