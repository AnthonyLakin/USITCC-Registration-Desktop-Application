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

        public string filepath = @"conference_events_feed_2023_07_17.xml";
        private void RegisterPageForm_Load(object sender, EventArgs e)
        {
            try
            {
                
                XmlDocument doc = new XmlDocument();
                doc.Load(filepath);
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
                    doc.Save(filepath);
                }
                else
                {
                    Console.WriteLine("This file already has usernames");
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
            string firtname = firstNameInput.Text.ToLower();
            string lastname = lastNameInput.Text.ToLower();
            string school = schoolInput.Text;
            string contest = contestInput.Text;

            //Console.WriteLine(ScanXML(firtname, lastname, school, contest));
            string values = ScanXML(firtname, lastname, school, contest);
            string[] newVal = values.Split(' ');

            var path = @"MyTest.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("Username: " + newVal[0]);
                sw.WriteLine("Password: " + newVal[1]);
                sw.Close();
            }
            //string strCmdText = "/c copy " + path + " LPT1";
            Process.Start("printJob.ps1");
        }

        private string ScanXML(string firtname, string lastname, string school, string contest)
        {
            
            var filepath = @"conference_events_feed_2023_07_17.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);
            XmlNode root = doc.DocumentElement;
            XmlNodeList allTeams = root.SelectNodes("team");
            int count = 0;
            foreach (XmlNode student in allTeams)
            {
                if (contest == student.SelectSingleNode("contest").InnerText && school == student.SelectSingleNode("school").InnerText)
                {
                    foreach (XmlNode contestant in student.SelectNodes("contestant"))
                    {

                        if (contestant.InnerText.ToLower() == (firtname + " " + lastname))
                        {
                            return student.SelectSingleNode("username").InnerText + " " + student.SelectSingleNode("password").InnerText;
                        }
                    }
                } count++;

            }
            if (count == allTeams.Count)
            {
                return "false";
            }
            throw new NotImplementedException();
        }
    }
}
