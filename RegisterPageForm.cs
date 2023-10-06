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

namespace USITCC_Registration
{
    public partial class RegisterPageForm : Form
    {
        public RegisterPageForm()
        {
            InitializeComponent();
        }

        private void RegisterPageForm_Load(object sender, EventArgs e)
        {
            var filepath = @"conference_events_feed_2023_07_17.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);
            XmlNode root = doc.DocumentElement;
            XmlNodeList allTeams = root.SelectNodes("team");
            if (allTeams[0].SelectSingleNode("//username") == null)
            {
                List<string> usernameList = new List<string>();
                List<string> passwordList = new List<string>();
                try
                {
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
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
                Console.Read();
                for (int i = 0; i < usernameList.Count; i++)
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

            submitButton.Enabled = true;
        }
    }
}
