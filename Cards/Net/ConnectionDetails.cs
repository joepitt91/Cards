using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace JoePitt.Cards.Net
{
    /// <summary>
    /// Details of an available connection for hosted games.
    /// </summary>
    public class ConnectionDetails
    {
        /// <summary>
        /// The IP Address associated with the connection.
        /// </summary>
        public IPAddress IP { get; private set; }
        /// <summary>
        /// The TCP port number associated with the connection.
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// If the IP Address associated with the connection is Internet routable.
        /// </summary>
        public bool InternetRoutable { get; private set; }
        /// <summary>
        /// If the PortCheck test on this connection passed.
        /// </summary>
        public bool TestPassed { get; private set; }

        /// <summary>
        /// Create a new Connection Details object.
        /// </summary>
        /// <param name="address">The IP Address for the connection.</param>
        /// <param name="port">The Port for the connection.</param>
        /// <param name="internet">If the connection is internet routable.</param>
        /// <param name="testPassed">If the PortCheck test passed.</param>
        public ConnectionDetails(IPAddress address, int port, bool internet, bool testPassed)
        {
            IP = address;
            Port = port;
            InternetRoutable = internet;
            TestPassed = testPassed;
        }

        /// <summary>
        /// Provide an updated test result after a retest.
        /// </summary>
        /// <param name="result">If the PortCheck test passed.</param>
        public void UpdateTestResult(bool result)
        {
            TestPassed = result;
        }
    }
}
