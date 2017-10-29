using JoePitt.Cards.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace JoePitt.Cards
{
    /// <summary>
    /// Carries out Card Set Management and Dealer Functions.
    /// </summary>
    static internal class Dealer
    {
        static private RNGCryptoServiceProvider seeder = new RNGCryptoServiceProvider();
        static private SHA256CryptoServiceProvider hasher = new SHA256CryptoServiceProvider();

        static public bool TestCardSetPath()
        {
            try
            {
                if (!Directory.Exists(Program.CardSetPath))
                {
                    string joePittAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JoePitt";
                    if (!Directory.Exists(joePittAppData))
                    {
                        Directory.CreateDirectory(joePittAppData);
                    }
                    Directory.CreateDirectory(Program.CardSetPath);
                    //Copy in Card Sets...
                    foreach (string CardSet in Directory.GetFiles("Resources\\CardSets\\", "*.cardset"))
                    {
                        string filename = CardSet.Substring(CardSet.LastIndexOf("\\") + 1);
                        File.Copy(CardSet, Program.CardSetPath + filename);
                    }
                    return true;
                }
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Display Name of the Windows/AD User running the game.
        /// </summary>
        /// <returns>The Current User's Display Name.</returns>
        static public string GetPlayerName()
        {
            string Username = "FAIL";
            string FullUsername = WindowsIdentity.GetCurrent().Name;
            if (FullUsername.Contains("\\"))
            {
                Username = FullUsername.Substring(FullUsername.LastIndexOf("\\") + 1);
            }
            else
            {
                Username = FullUsername;
            }
            string Query = "SELECT * FROM Win32_UserAccount WHERE Name = '" + Username + "'";
            ManagementObjectSearcher usersSearcher = new ManagementObjectSearcher(Query);
            ManagementObjectCollection users = usersSearcher.Get();

            foreach (ManagementObject user in users)
            {
                return user["FullName"].ToString();
            }
            return null;
        }

        /// <summary>
        /// Get a list of installed Card Sets.
        /// </summary>
        /// <returns>Details of Installed Card Sets</returns>
        static public List<string[]> GetCardSets()
        {
            List<string[]> cardSets = new List<string[]>();
            // Find Card Sets
            if (!TestCardSetPath()) return new List<string[]>();
            string[] cardSetFiles = Directory.GetFiles(Program.CardSetPath, "*.cardset");
            foreach (string cardSet in cardSetFiles)
            {
                // Try to Open Card Set
                XmlDocument cardSetDoc = new XmlDocument();
                try
                {
                    cardSetDoc.Load(cardSet);
                }
                catch (XmlException ex)
                {
                    MessageBox.Show(cardSet + " is not a valid Card Set (" + ex.Message + ")", "Bad Card Set", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }
                catch (IOException ex)
                {
                    MessageBox.Show(cardSet + " is not a valid Card Set (" + ex.Message + ")", "Bad Card Set", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }
                // Load Card Set Details
                XmlElement cardSetDetail = (XmlElement)cardSetDoc.GetElementsByTagName("CardSet")[0];
                // GUID, Name, Version, Path, Hash Status
                string[] packInfo = new string[5];
                packInfo[0] = cardSetDetail.GetAttribute("GUID");
                packInfo[1] = cardSetDetail.GetAttribute("Name");
                packInfo[2] = cardSetDetail.GetAttribute("Version");
                packInfo[3] = cardSet;
                // Verify Hash
                XmlElement xmlBlackCards = (XmlElement)cardSetDoc.GetElementsByTagName("BlackCards")[0];
                XmlElement xmlWhiteCards = (XmlElement)cardSetDoc.GetElementsByTagName("WhiteCards")[0];
                byte[] allCards = Encoding.Default.GetBytes(xmlBlackCards.InnerXml + xmlWhiteCards.InnerXml);
                byte[] hash = hasher.ComputeHash(allCards);
                string computedHash = Convert.ToBase64String(hash);

                if (cardSetDetail.GetAttribute("Hash") == computedHash)
                {
                    packInfo[4] = "OK";
                    cardSets.Add(packInfo);
                }
                else
                {
                    packInfo[4] = "Corrupt";
                    MessageBox.Show(cardSetDetail.GetAttribute("Name") + " (" + cardSetDetail.GetAttribute("Version") + ") has failed it's integrity check and not been loaded, Please reinstall it.", "Card Set Corrupted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return cardSets;
        }

        /// <summary>
        /// Install a Card Set.
        /// </summary>
        /// <param name="cardSetFile">The path to the Card Set to be installed.</param>
        /// <returns>If the Card Set was installed.</returns>
        static public bool InstallCardSet(string cardSetFile)
        {
            bool installed = false;
            bool skipped = false;
            List<string[]> installedSets = GetCardSets();

            XmlDocument cardSetDoc = new XmlDocument();
            try
            {
                cardSetDoc.Load(cardSetFile);
            }
            catch (XmlException ex)
            {
                MessageBox.Show(cardSetFile + " is not a valid Card Set (" + ex.Message + ")", "Bad Card Set", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return installed;
            }
            catch (IOException ex)
            {
                MessageBox.Show(cardSetFile + " is not a valid Card Set (" + ex.Message + ")", "Bad Card Set", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return installed;
            }
            XmlElement cardSetInfo = (XmlElement)cardSetDoc.GetElementsByTagName("CardSet")[0];
            /*
             * [0] = GUID
             * [1] = Name
             * [2] = Version
             * [3] = file
             * [4] = OK/Corrupt
             */
            foreach (string[] installedSet in installedSets)
            {
                // If GUID found.
                if (installedSet[0] == cardSetInfo.GetAttribute("GUID"))
                {
                    // If GUID found and version matches.
                    if (installedSet[2] == cardSetInfo.GetAttribute("Version"))
                    {
                        if (MessageBox.Show(cardSetInfo.GetAttribute("Name") + " (" + cardSetInfo.GetAttribute("Version") + ") is already installed. Reinstall?", "Reinstall Pack?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            File.Delete(installedSet[3]);
                            File.Copy(cardSetFile, installedSet[3]);
                            installed = true;
                            break;
                        }
                        else
                        {
                            skipped = true;
                            break;
                        }
                    }
                    // If GUID found and version doesn't match.
                    string[] oldVersion = installedSet[2].Split('.');
                    int oldMajVersion = Convert.ToInt32(oldVersion[0]);
                    int oldMinVersion = Convert.ToInt32(oldVersion[1]);
                    string[] newVersion = cardSetInfo.GetAttribute("Version").Split('.');
                    int newMajVersion = Convert.ToInt32(newVersion[0]);
                    int newMinVersion = Convert.ToInt32(newVersion[1]);

                    if (newMajVersion > oldMajVersion)
                    {
                        if (MessageBox.Show(installedSet[1] + " (" + installedSet[2] + ") will be upgraded to " + cardSetInfo.GetAttribute("Name") + " (" + cardSetInfo.GetAttribute("Version") + "). Continue?", "Upgrade Card Pack?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            File.Delete(installedSet[3]);
                            File.Copy(cardSetFile, installedSet[3]);
                            installed = true;
                            break;
                        }
                        else
                        {
                            skipped = true;
                            break;
                        }
                    }
                    else if (newMajVersion == oldMajVersion)
                    {
                        if (newMinVersion > oldMinVersion)
                        {
                            if (MessageBox.Show(installedSet[1] + " (" + installedSet[2] + ") will be upgraded to " + cardSetInfo.GetAttribute("Name") + " (" + cardSetInfo.GetAttribute("Version") + "). Continue?", "Upgrade Card Pack?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                File.Delete(installedSet[3]);
                                File.Copy(cardSetFile, installedSet[3]);
                                installed = true;
                                break;
                            }
                            else
                            {
                                skipped = true;
                                break;
                            }
                        }
                        else
                        {
                            if (MessageBox.Show(installedSet[1] + " (" + installedSet[2] + ") will be DOWNGRADED to " + cardSetInfo.GetAttribute("Name") + " (" + cardSetInfo.GetAttribute("Version") + "). Continue?", "Downgrade Card Pack?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                File.Delete(installedSet[3]);
                                File.Copy(cardSetFile, installedSet[3]);
                                installed = true;
                                break;
                            }
                            else
                            {
                                skipped = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (MessageBox.Show(installedSet[1] + " (" + installedSet[2] + ") will be DOWNGRADED to " + cardSetInfo.GetAttribute("Name") + " (" + cardSetInfo.GetAttribute("Version") + "). Continue?", "Downgrade Card Pack?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            File.Delete(installedSet[3]);
                            File.Copy(cardSetFile, installedSet[3]);
                            installed = true;
                            break;
                        }
                        else
                        {
                            skipped = true;
                            break;
                        }
                    }
                }
            }
            if (!installed && !skipped)
            {
                if (MessageBox.Show(cardSetInfo.GetAttribute("Name") + " (" + cardSetInfo.GetAttribute("Version") + ") will be installed. Continue?", "Install Card Pack?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (!TestCardSetPath()) return installed;
                    if (File.Exists(Program.CardSetPath + "\\" + cardSetInfo.GetAttribute("Name").Replace(' ', '-') + "_" + cardSetInfo.GetAttribute("Version") + "_" + cardSetInfo.GetAttribute("GUID") + ".cardset"))
                    { File.Delete(Program.CardSetPath + "\\" + cardSetInfo.GetAttribute("Name").Replace(' ', '-') + "_" + cardSetInfo.GetAttribute("Version") + "_" + cardSetInfo.GetAttribute("GUID") + ".cardset"); }
                    File.Copy(cardSetFile, Program.CardSetPath + "\\" + cardSetInfo.GetAttribute("Name").Replace(' ', '-') + "_" + cardSetInfo.GetAttribute("Version") + "_" + cardSetInfo.GetAttribute("GUID") + ".cardset");
                    installed = true;
                }
            }

            if (installed)
            { MessageBox.Show(cardSetInfo.GetAttribute("Name") + " (" + cardSetInfo.GetAttribute("Version") + ") has been installed.", "Card Set Installed", MessageBoxButtons.OK, MessageBoxIcon.Information); }

            return installed;
        }

        /// <summary>
        /// Shuffle a Deck of Cards.
        /// </summary>
        /// <param name="cards">The card index to be shuffled.</param>
        /// <returns>A Shuffled Card Deck.</returns>
        static public Dictionary<int, string> ShuffleCards(Dictionary<int, string> cards)
        {
            if (cards == null) { throw new ArgumentNullException("cards"); }
            int moveFrom = 1;
            byte[] seed = new byte[4];
            seeder.GetBytes(seed);
            Random shuffler = new Random(BitConverter.ToInt32(seed, 0));
            int shuffleCount = 0;
            int shuffles = shuffler.Next(10, 20);
            while (shuffleCount < shuffles)
            {
                while (moveFrom < cards.Count)
                {
                    int moveTo = shuffler.Next(1, cards.Count + 1);
                    string x = cards[moveFrom];
                    string y = cards[moveTo];
                    cards[moveFrom] = y;
                    cards[moveTo] = x;
                    moveFrom++;
                }
                moveFrom = 1;
                shuffleCount++;
            }
            return cards;
        }

        /// <summary>
        /// Shuffles Answers into a random order.
        /// </summary>
        /// <param name="answers">The Answers to be shuffled.</param>
        /// <returns></returns>
        static public List<Answer> ShuffleAnswers(List<Answer> answers)
        {
            if (answers == null) { throw new ArgumentNullException("answers"); }
            int moveFrom = 0;
            byte[] seed = new byte[4];
            seeder.GetBytes(seed);
            Random shuffler = new Random(BitConverter.ToInt32(seed, 0));
            int shuffleCount = 0;
            int shuffles = shuffler.Next(10, 20);
            while (shuffleCount < shuffles)
            {
                while (moveFrom < answers.Count)
                {
                    int moveTo = shuffler.Next(0, answers.Count - 1);
                    Answer x = answers[moveFrom];
                    Answer y = answers[moveTo];
                    answers[moveFrom] = y;
                    answers[moveTo] = x;
                    moveFrom++;
                }
                moveFrom = 1;
                shuffleCount++;
            }
            return answers;
        }

        /// <summary>
        /// Deal the White Cards for players.
        /// </summary>
        /// <param name="cardSet">Pre-shuffled CardSet.</param>
        /// <param name="playerCount">The total number of Players.</param>
        /// <param name="cardsPerPlayer">The number of Cards to deal to each Player.</param>
        /// <returns>The Dealt Hands</returns>
        /// <exception cref="ApplicationException">If no card are left</exception>
        static public List<Tuple<int, Card>> Deal(CardSet cardSet, int playerCount, int cardsPerPlayer)
        {
            if (cardSet == null) { throw new ArgumentNullException("cardSet"); }
            List<Tuple<int, Card>> hands = new List<Tuple<int, Card>>();
            int card = 1;
            int player = 1;
            int dealtEach = 0;
            while (dealtEach < cardsPerPlayer)
            {
                while (player <= playerCount)
                {
                    try
                    {
                        hands.Add(new Tuple<int, Card>(player, cardSet.WhiteCards[cardSet.WhiteCardIndex[card]]));
                        card++;
                        player++;
                    }
                    catch
                    {
                        throw new OutOfCardsException();
                    }
                }
                player = 1;
                dealtEach++;
            }
            hands.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            return hands;
        }
    }
}