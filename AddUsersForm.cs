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
    public partial class AddUsersForm : Form
    {
        public AddUsersForm()
        {
            InitializeComponent();
        }
        string xmlFilePath;

        public XmlDocument grabXML()
        {
            xmlFilePath = @"crucial_Files/xml_Data/conference_events_feed_2023_07_17.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);
            return doc;
        }

        List<string> usernameList = new List<string>();
        List<string> passwordList = new List<string>();
        private void RegisterPageForm_Load(object sender, EventArgs e)
        {
            try
            {
                XmlDocument doc = grabXML();
                XmlNode root = doc.DocumentElement;
                XmlNodeList allTeams = root.SelectNodes("team");
                using (StreamReader reader = new StreamReader(@"crucial_Files/moodleUsersCSV/moodleUsers.csv"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] values = line.Split(',');
                        usernameList.Add(values[0]);
                        passwordList.Add(values[1]);
                    }
                }
                if (allTeams[0].SelectSingleNode("username") == null)
                {
                    
                    
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
                for (int i = 0; i < allTeams.Count; i++)
                {
                    schoolNames.Add(allTeams[i].SelectSingleNode("school").InnerText);
                    contestNames.Add(allTeams[i].SelectSingleNode("contest").InnerText);
                }
                foreach (var givenSchool in schoolNames)
                {
                    schoolInput.Items.Add(givenSchool);
                }
                foreach (var givenContest in contestNames)
                {
                    contestInput.Items.Add(givenContest);
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
            submitButton.Enabled = true;
        }

        // Templates

       

        private void resetContent()
        {
            submitButton.Enabled = false;
            resetButton.Enabled = false;
            int timer = 300;
            firstNameInput.Focus();
            firstNameInput.SelectAll();
            System.Threading.Thread.Sleep(timer);
            firstNameInput.Text = string.Empty;
            lastNameInput.Focus();
            lastNameInput.SelectAll();
            System.Threading.Thread.Sleep(timer);
            lastNameInput.Text = string.Empty;
            schoolInput.Focus();
            schoolInput.SelectAll();
            System.Threading.Thread.Sleep(timer);
            schoolInput.SelectedIndex = -1;
            contestInput.Focus();
            contestInput.SelectAll();
            System.Threading.Thread.Sleep(timer);
            contestInput.SelectedIndex = -1;
            System.Threading.Thread.Sleep(400);
            titleLabel.Text = "Please Register";
            titleLabel.ForeColor = Color.White;
            lineBox.BackColor = Color.White;
            overwriteButton.Visible = false;
            overwriteButton.Enabled = false;
            submitButton.Visible = true;
            submitButton.Enabled = true;
            resetButton.Enabled = true;

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
            if (Convert.ToDouble(isGreater) / Convert.ToDouble(winner.Length) > 0.5)
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
                        if (contestant.InnerText.ToLower() == (firstname.ToLower() + " " + lastname.ToLower()))
                        {
                            return student.SelectSingleNode("username").InnerText + " " + student.SelectSingleNode("password").InnerText;
                        }
                        possibleStudents.Add(contestant.InnerText);
                    }
                }
                count++;
            }
            if (count == allTeams.Count)
            { 
                if (FindCorrection(possibleStudents) != "false")
                {
                    DialogResult prompt = MessageBox.Show('\u0022' + firstNameInput.Text + " " + lastNameInput.Text + '\u0022' + " does not exist, did you mean " + '\u0022' + FindCorrection(possibleStudents) + '\u0022' + " instead?", "Can't find student", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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


        // End of Template

                
        private async void submitButton_Click(object sender, EventArgs e)
        {
            string firstname = firstNameInput.Text;
            string lastname = lastNameInput.Text;
            string school = schoolInput.Text;
            string contest = contestInput.Text;

            if (firstname == string.Empty || lastname == string.Empty || school == string.Empty || contest == string.Empty)
            {
                MessageBox.Show("Please fill out all boxes!", "Empty box detected", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string values = ScanXML(firstname, lastname, school, contest);
                string[] newVal;
                if (values != "false" && values != "true")
                {
                    titleLabel.Text = "Printing...";
                    newVal = values.Split(' ');
                    SendToPrinter.RichTextParser(firstNameInput.Text, lastNameInput.Text, schoolInput.Text, contestInput.Text, newVal[0], newVal[1]);
                    submitButton.Enabled = false;
                    resetButton.Enabled = false;
                    await Task.Delay(400);
                    resetContent();
                    submitButton.Enabled = true;
                    resetButton.Enabled = true;
                }
                else if (values == "false")
                {
                    titleLabel.Text = "User Not Registered";
                    titleLabel.ForeColor = Color.Olive;
                    lineBox.BackColor = Color.Olive;
                    overwriteButton.Location = new Point(474, 557);
                    overwriteButton.Visible = true;
                    overwriteButton.Enabled = true;
                    submitButton.Visible = false;
                    submitButton.Enabled = false;
                    MessageBox.Show(firstNameInput.Text + " " + lastNameInput.Text + " of " + schoolInput.Text + " is not registered to compete in " + contestInput.Text + "\n\nYou can still compete but must click OVERWRITE button.", "User Not Registered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void RegisterPageForm_Resize(object sender, EventArgs e)
        {
           formWrapper.Left = (this.ClientSize.Width - formWrapper.Width) / 2;
           formWrapper.Top = (this.ClientSize.Height - formWrapper.Height) / 2;
        }

        private void formInputChange(object sender, EventArgs e)
        {
            titleLabel.Text = "Please Register";
            titleLabel.ForeColor = Color.White;
            lineBox.BackColor = Color.White;
            overwriteButton.Visible = false;
            overwriteButton.Enabled = false;
            submitButton.Visible = true;
            submitButton.Enabled = true;
            resetButton.Enabled = true;
        }

        private void overwriteButton_Click(object sender, EventArgs e)
        {
            XmlDocument doc = grabXML();
            XmlNode root = doc.DocumentElement;
            XmlNodeList allTeams = root.SelectNodes("team");
            XmlNode team = doc.CreateElement("team");
            XmlNode school = doc.CreateElement("school");
            school.InnerText = schoolInput.Text;
            XmlNode contestant = doc.CreateElement("contestant");
            contestant.InnerText = firstNameInput.Text + " " + lastNameInput.Text;
            XmlNode contest = doc.CreateElement("contest");
            contest.InnerText = contestInput.Text;
            XmlNode username = doc.CreateElement("username");
            username.InnerText = usernameList[allTeams.Count];
            XmlNode password = doc.CreateElement("password");
            password.InnerText = passwordList[allTeams.Count];
            root.AppendChild(team);
            team.AppendChild(school);
            team.AppendChild(contestant);
            team.AppendChild(contest);
            team.AppendChild(username);
            team.AppendChild(password);
            doc.Save(xmlFilePath);
            SendToPrinter.RichTextParser(firstNameInput.Text, lastNameInput.Text, schoolInput.Text, contestInput.Text, usernameList[allTeams.Count], passwordList[allTeams.Count]);
            resetContent();
            

        }
    }
}
