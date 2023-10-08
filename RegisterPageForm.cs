using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Xml.Linq;
using System.Runtime.Remoting.Contexts;
using System.Diagnostics;

namespace USITCC_Registration
{
    public partial class RegisterPageForm : Form
    {
        public RegisterPageForm()
        {
            InitializeComponent();
        }

        // Templates

        private XmlDocument grabXML()
        {
            string xmlFilePath = @"conference_events_feed_2023_07_17.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);
            return doc;
        }

        private void resetContent()
        {
            firstNameInput.Text = string.Empty;
            lastNameInput.Text = string.Empty;
            schoolInput.SelectedIndex = -1;
            contestInput.SelectedIndex = -1;
        }
        

        private string FindCorrection(List<string> studentNames)
        {
            string concatName = firstNameInput.Text + lastNameInput.Text;
            int isGreater = 0;
            string winner = "This user does not exits";


            foreach (string student in studentNames)
            {
                string removeSpace = student.Replace(" ", string.Empty);
                
                if (concatName.Length >= removeSpace.Length)
                {
                    int probability = 0;
                    for (int i = 0; i < removeSpace.Length; i++)
                    {
                        if (concatName.Contains(removeSpace[i]))
                        {
                            probability++;
                        }
                    }
                    if (probability > isGreater)
                    {
                        isGreater = probability;
                        winner = student;
                    }
                    
                } 
                else
                {
                    int probability = 0;
                    for (int i = 0; i < concatName.Length; i++)
                    {
                        if (removeSpace.Contains(concatName[i]))
                        {
                            probability++;
                        }
                    }
                    if (probability > isGreater)
                    {
                        isGreater = probability;
                        winner = student;
                    }
                }

                
            }
            //Console.WriteLine(Convert.ToDouble(isGreater)/Convert.ToDouble(winner.Length));
            if (Convert.ToDouble(isGreater) / Convert.ToDouble(winner.Length) > 0.6)
            {
                return winner;
            } else
            {
                return "false";
            }
            
            
        }

        private string ScanXML(string firstname, string lastname, string school, string contest)
        {
            XmlDocument doc = grabXML();
            XmlNode root = doc.DocumentElement;
            XmlNodeList allTeams = root.SelectNodes("team");
            int count = 0;
            List<string> possibleStudents = new List<string>();
            foreach (XmlNode student in allTeams)
            {
                if (contest == student.SelectSingleNode("contest").InnerText && school == student.SelectSingleNode("school").InnerText)
                {
                    foreach (XmlNode contestant in student.SelectNodes("contestant"))
                    {
                        if (contestant.InnerText.ToLower() == (firstname + " " + lastname))
                        {
                            return student.SelectSingleNode("username").InnerText + " " + student.SelectSingleNode("password").InnerText;
                        }
                        possibleStudents.Add(contestant.InnerText);
                    }
                }
                count++;
            }
            //Console.WriteLine(FindCorrection(possibleStudents));
            
            if (count == allTeams.Count)
            { 
                if (FindCorrection(possibleStudents) != "false")
                {
                    DialogResult prompt = MessageBox.Show(firstNameInput.Text + " " + lastNameInput.Text + " does not exist, did you mean " + FindCorrection(possibleStudents) + " instead?", "Can't find student", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (prompt == DialogResult.Yes)
                    {
                        string[] bothNames = FindCorrection(possibleStudents).Split(' ');
                        firstNameInput.Text = bothNames[0];
                        firstNameInput.Focus();
                        firstNameInput.SelectAll();
                        System.Threading.Thread.Sleep(700);
                        lastNameInput.Text = bothNames[1];
                        lastNameInput.Focus();
                        lastNameInput.SelectAll();
                        return "true";

                    }
                    else
                    {
                        return "false";
                    }
                }
                else
                {
                    return "false";
                }
                
                
            }
            throw new NotImplementedException();
        }
        private void resetButton_Click(object sender, EventArgs e)
        {
            resetContent();
        }

        public string xmlFilePath = @"conference_events_feed_2023_07_17.xml";

        // End of Template

        private void RegisterPageForm_Load(object sender, EventArgs e)
        {
            try
            {
                XmlDocument doc = grabXML();
                XmlNode root = doc.DocumentElement;
                XmlNodeList allTeams = root.SelectNodes("team");

                if (allTeams[0].SelectSingleNode("//username") == null)
                {
                    List<string> usernameList = new List<string>();
                    List<string> passwordList = new List<string>();
                        using (StreamReader reader = new StreamReader(@"moodleUsers.csv"))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                string[] values = line.Split(',');
                                usernameList.Add(values[0]);
                                passwordList.Add(values[1]);
                            }
                        }
                    Console.Read();
                    for (int i = 0; i < allTeams.Count; i++)
                    {
                        XmlNode username = doc.CreateElement("username");
                        username.InnerText = usernameList[i];
                        XmlNode password = doc.CreateElement("password");
                        password.InnerText = passwordList[i];
                        allTeams[i].AppendChild(username);
                        allTeams[i].AppendChild(password);
                    }
                    doc.Save(xmlFilePath);
                }
                HashSet<string> schoolNames = new HashSet<string>();
                HashSet<string> contestNames = new HashSet<string>();
                for (int i = 0;i < allTeams.Count;i++)
                {
                    schoolNames.Add(allTeams[i].SelectSingleNode("school").InnerText);
                    contestNames.Add(allTeams[i].SelectSingleNode("contest").InnerText);
                }
                foreach (var givenSchool in  schoolNames)
                {
                    schoolInput.Items.Add(givenSchool);
                }
                foreach (var givenContest  in contestNames)
                {
                    contestInput.Items.Add(givenContest);
                }

            } catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
            submitButton.Enabled = true;
        }

        



        private void submitButton_Click(object sender, EventArgs e)
        {
            string firstname = firstNameInput.Text.ToLower();
            string lastname = lastNameInput.Text.ToLower();
            string school = schoolInput.Text;
            string contest = contestInput.Text;

           
            string values = ScanXML(firstname, lastname, school, contest);

            if (values != "false" && values != "true")
            {
                titleLabel.Text = "Printing...";
                resetContent();
                string[] newVal = values.Split(' ');
                var path = @"MyTest.txt";
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("Username: " + newVal[0]);
                    sw.WriteLine("Password: " + newVal[1]);
                    sw.Close();
                }
                //string strCmdText = "/c copy " + path + " LPT1";
                //Process.Start("printJob.ps1");
            } else if (values == "false")
            {
                MessageBox.Show(firstNameInput.Text + " " + lastNameInput.Text + " of " + schoolInput.Text + " is not registered to compete in " + contestInput.Text, "Inccorect Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }




        }


    }
}
