using System;
using System.Runtime.Serialization;

namespace JoePitt.Cards.Exceptions
{
    /// <summary>
    /// Cards was unable to start the network listener.
    /// </summary>
    [Serializable]
    public class ListenerStartException : Exception
    {
        /// <summary>
        /// Exception Message.
        /// </summary>
        public override string Message => "Cards was unable to start the network listener.";

        /// <summary>
        /// Generate Exception.
        /// </summary>
        public ListenerStartException()
        {
        }

        /// <summary>
        /// Generate Exception.
        /// </summary>
        /// <param name="message"></param>
        public ListenerStartException(string message) : base(message)
        {
        }

        /// <summary>
        /// Generate Exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ListenerStartException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Generate Exception.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ListenerStartException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
