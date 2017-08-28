using JoePitt.Cards.Net;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;

namespace JoePitt.Cards.UI
{
    /// <summary>
    /// The Debug UI Control.
    /// </summary>
    public partial class Debug : Form
    {
        /// <summary>
        /// Initalise the UI.
        /// </summary>
        public Debug()
        {
            InitializeComponent();
            KeyDown += Debug_KeyDown;
            treDebug.KeyDown += Debug_KeyDown;
            treDebug.AfterCheck += TreDebug_AfterCheck;
        }

        /// <summary>
        /// Cascade (un)check to child nodes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreDebug_AfterCheck(object sender, TreeViewEventArgs e)
        {
            CheckChildren(e.Node, e.Node.Checked);
        }

        /// <summary>
        /// Handles saving of BugReports, when CTRL+S is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Debug_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.S)
            {
                Export();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CardSet")]
        private void Debug_Load(object sender, EventArgs e)
        {
            Game game = Program.CurrentGame;

            treDebug.Nodes[0].Nodes["Playable"].Text = "Playable: " + game.Playable;
            treDebug.Nodes[0].Nodes["GameType"].Text = "Game Type: " + game.GameType;

            if (game.GameType != 'J')
            {
                treDebug.Nodes[0].Nodes["ServerNetworking"].Nodes.Add("Connection Strings");
                foreach (Net.ConnectionDetails ConnectionString in game.HostNetwork.ConnectionStrings)
                {
                    treDebug.Nodes[0].Nodes["ServerNetworking"].Nodes[0].Nodes.Add(ConnectionString.IP.ToString() + ":" + ConnectionString.Port);
                }
                treDebug.Nodes[0].Nodes["ServerNetworking"].Nodes.Add("Port", "Port: " + game.HostNetwork.Port);
            }
            else
            {
                treDebug.Nodes[0].Nodes["ServerNetworking"].Text = "Server Networking: N/A";
            }
            // Client Networking
            int i = 0;
            foreach (ClientNetworking client in game.LocalPlayers)
            {
                treDebug.Nodes[0].Nodes["ClientNetworking"].Nodes.Add("Client_" + i, "Client_" + i);
                treDebug.Nodes[0].Nodes["ClientNetworking"].Nodes[i].Nodes.Add("Owner", "Owner: " + client.Owner.Name);
                string command = client.NextCommand;
                if (client.NewCommand)
                {
                    command = "(PENDING) " + command;
                }
                else
                {
                    command = "(SENT) " + command;
                }
                treDebug.Nodes[0].Nodes["ClientNetworking"].Nodes[i].Nodes.Add("Command", command);

                string response = client.LastResponse;
                if (client.NewResponse)
                {
                    response = "(New) " + response;
                }
                else
                {
                    response = "(Old) " + response;
                }
                treDebug.Nodes[0].Nodes["ClientNetworking"].Nodes[i].Nodes.Add("Response", response);
                i++;
            }

            treDebug.Nodes[0].Nodes["NetworkUp"].Text = "Network Up: " + game.NetworkUp;
            treDebug.Nodes[0].Nodes["CardsPerUser"].Text = "Cards Per User: " + game.CardsPerUser;
            treDebug.Nodes[0].Nodes["Rounds"].Text = "Rounds: " + game.Rounds;
            treDebug.Nodes[0].Nodes["NeverHaveI"].Text = "Never Have I: " + game.NeverHaveI;
            treDebug.Nodes[0].Nodes["RebootingTheUniverse"].Text = "Rebooting The Universe: " + game.RebootingTheUniverse;
            treDebug.Nodes[0].Nodes["PlayerCount"].Text = "Player Count: " + game.PlayerCount;
            treDebug.Nodes[0].Nodes["BotCount"].Text = "Bot Count: " + game.BotCount;

            i = 0;
            foreach (Player player in game.Players)
            {
                treDebug.Nodes[0].Nodes["Players"].Nodes.Add("Player_" + i, "Player_" + i);
                treDebug.Nodes[0].Nodes["Players"].Nodes[i].Nodes.Add("Name", "Name: " + player.Name);
                treDebug.Nodes[0].Nodes["Players"].Nodes[i].Nodes.Add("IsBot", "Is Bot: " + player.IsBot);
                treDebug.Nodes[0].Nodes["Players"].Nodes[i].Nodes.Add("Score", "Score: " + player.Score);
                treDebug.Nodes[0].Nodes["Players"].Nodes[i].Nodes.Add("WhiteCards", "White Cards");
                int j = 0;
                foreach (Card card in player.WhiteCards)
                {
                    treDebug.Nodes[0].Nodes["Players"].Nodes[i].Nodes[3].Nodes.Add("Cards_" + j, "Cards_" + j);
                    treDebug.Nodes[0].Nodes["Players"].Nodes[i].Nodes[3].Nodes[j].Nodes.Add("ID", "ID: " + card.Id);
                    treDebug.Nodes[0].Nodes["Players"].Nodes[i].Nodes[3].Nodes[j].Nodes.Add("Text", "Text: " + card.ToString);
                    j++;
                }
                treDebug.Nodes[0].Nodes["Players"].Nodes[i].Nodes["WhiteCards"].Collapse();
                i++;
            }

            i = 0;
            foreach (CardSet cardSet in game.CardSets)
            {
                treDebug.Nodes[0].Nodes["CardSets"].Nodes.Add("CardSet_" + i, "CardSet_" + i);
                treDebug.Nodes[0].Nodes["CardSets"].Nodes[i].Nodes.Add("GUID", "GUID: " + cardSet.CardSetGuid);
                treDebug.Nodes[0].Nodes["CardSets"].Nodes[i].Nodes.Add("Name", "Name: " + cardSet.Name);
                treDebug.Nodes[0].Nodes["CardSets"].Nodes[i].Nodes.Add("Version", "Version " + cardSet.Version);
                treDebug.Nodes[0].Nodes["CardSets"].Nodes[i].Nodes.Add("BlackCardCount", "Black Card Count: " + cardSet.BlackCardCount);
                treDebug.Nodes[0].Nodes["CardSets"].Nodes[i].Nodes.Add("WhiteCardCount", "White Card Count: " + cardSet.WhiteCardCount);
                i++;
            }

            treDebug.Nodes[0].Nodes["Stage"].Text = "Stage: " + game.Stage;

            i = 0;
            foreach (Card card in game.GameSet.BlackCards.Values)
            {
                treDebug.Nodes[0].Nodes["GameSet"].Nodes["GameBlackCards"].Nodes.Add("Card_" + i, "Card_" + i);
                treDebug.Nodes[0].Nodes["GameSet"].Nodes["GameBlackCards"].Nodes[i].Nodes.Add("ID", "ID: " + card.Id);
                treDebug.Nodes[0].Nodes["GameSet"].Nodes["GameBlackCards"].Nodes[i].Nodes.Add("Text", "Text: " + card.ToString);
                treDebug.Nodes[0].Nodes["GameSet"].Nodes["GameBlackCards"].Nodes[i].Nodes.Add("Needs", "Needs: " + card.Needs);
                i++;
            }
            i = 0;
            foreach (Card card in game.GameSet.WhiteCards.Values)
            {
                treDebug.Nodes[0].Nodes["GameSet"].Nodes["GameWhiteCards"].Nodes.Add("Card_" + i, "Card_" + i);
                treDebug.Nodes[0].Nodes["GameSet"].Nodes["GameWhiteCards"].Nodes[i].Nodes.Add("ID", "ID: " + card.Id);
                treDebug.Nodes[0].Nodes["GameSet"].Nodes["GameWhiteCards"].Nodes[i].Nodes.Add("Text", "Text: " + card.ToString);
                i++;
            }

            treDebug.Nodes[0].Nodes["CurrentBlackCard"].Text = "Current Black Card: " + game.CurrentBlackCard;
            treDebug.Nodes[0].Nodes["CurrentWhiteCard"].Text = "Current White Card: " + game.CurrentWhiteCard;
            treDebug.Nodes[0].Nodes["Round"].Text = "Round: " + game.Round;

            i = 0;
            foreach (Answer answer in game.Answers)
            {
                treDebug.Nodes[0].Nodes["Answers"].Nodes.Add("Answer_" + i, "Answer_" + i);
                treDebug.Nodes[0].Nodes["Answers"].Nodes[i].Nodes.Add("Submitter", "Submitter: " + answer.Submitter.Name);
                treDebug.Nodes[0].Nodes["Answers"].Nodes[i].Nodes.Add("Text", "Text: " + answer.ToString);
                i++;
            }

            i = 0;
            foreach (Vote vote in game.Votes)
            {
                treDebug.Nodes[0].Nodes["Votes"].Nodes.Add("Vote_" + i, vote.Voter.Name);
                treDebug.Nodes[0].Nodes["Votes"].Nodes[i].Nodes.Add("Voter", "Voter: " + vote.Voter.Name);
                treDebug.Nodes[0].Nodes["Votes"].Nodes[i].Nodes.Add("Text", "Text: " + vote.Choice.ToString);
                i++;
            }

            i = 0;
            foreach (Answer winner in game.Winners)
            {
                treDebug.Nodes[0].Nodes["Winners"].Nodes.Add("Winner_" + i, winner.Submitter.Name);
            }

            treDebug.ExpandAll();
            treDebug.Nodes[0].Nodes["GameSet"].Nodes["GameBlackCards"].Collapse();
            treDebug.Nodes[0].Nodes["GameSet"].Nodes["GameWhiteCards"].Collapse();
            foreach (TreeNode node in treDebug.Nodes[0].Nodes["Players"].Nodes)
            {
                node.Nodes["WhiteCards"].Collapse();
            }
            treDebug.SelectedNode = treDebug.Nodes[0];
            CheckAllNodes(treDebug.Nodes, true);
            CheckChildren(treDebug.Nodes[0].Nodes["ServerNetworking"].Nodes["Connection Strings"], false);
        }

        /// <summary>
        /// (Un)checks all nodes under the specified node.
        /// </summary>
        /// <param name="nodes">The Nodes to be (un)checked.</param>
        /// <param name="Check">Whether to Check or Uncheck.</param>
        private void CheckAllNodes(TreeNodeCollection nodes, bool Check)
        {
            foreach (TreeNode node in nodes)
            {
                node.Checked = Check;
                CheckChildren(node, Check);
            }
        }

        /// <summary>
        /// Iterates through the tiers to (un)check all child nodes.
        /// </summary>
        /// <param name="rootNode">The Topmost Node.</param>
        /// <param name="Check">Whether to Check or Uncheck.</param>
        private void CheckChildren(TreeNode rootNode, bool Check)
        {
            if (rootNode != null)
            {
                foreach (TreeNode node in rootNode.Nodes)
                {
                    CheckChildren(node, Check);
                    node.Checked = Check;
                }
            }
        }

        /// <summary>
        /// Save the BugReport.
        /// </summary>
        /// <returns>If the bug report was saved.</returns>
        private bool Export()
        {
            XElement rootElement = new XElement("Game", CreateXmlElement(treDebug.Nodes[0].Nodes));
            XDocument document = new XDocument(rootElement);
            SaveFileDialog xmlSave = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = ".xml",
                FileName = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + "_CardsDebug.xml",
                Filter = "XML File (*.xml)|*.xml",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Title = "Save Debug Report"
            };
            if (xmlSave.ShowDialog() == DialogResult.OK)
            {
                document.Save(xmlSave.FileName);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Generate the BugReport XML.
        /// </summary>
        /// <param name="treeViewNodes">The Nodes to put into the BugReport XML.</param>
        /// <returns>The XML Elements to put in the BugReport XML.</returns>
        private static List<XElement> CreateXmlElement(TreeNodeCollection treeViewNodes)
        {
            List<XElement> elements = new List<XElement>();
            foreach (TreeNode treeViewNode in treeViewNodes)
            {
                if (treeViewNode.Checked)
                {
                    XElement element = new XElement(treeViewNode.Name.Replace('/', '_'));
                    if (treeViewNode.GetNodeCount(true) == 0)
                    {
                        element.Value = treeViewNode.Text;
                        if (element.Value.Contains(": "))
                        {
                            element.Value = element.Value.Substring(element.Value.IndexOf(":") + 2);
                        }
                    }
                    else
                    {
                        element.Add(CreateXmlElement(treeViewNode.Nodes));
                    }
                    elements.Add(element);
                }
                else
                {
                    XElement element = new XElement(treeViewNode.Name.Replace('/', '_'))
                    {
                        Value = "REMOVED"
                    };
                    elements.Add(element);
                }
            }
            return elements;
        }
    }
}