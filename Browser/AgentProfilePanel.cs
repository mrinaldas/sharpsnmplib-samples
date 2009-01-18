﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

namespace Lextm.SharpSnmpLib.Browser
{
    internal partial class AgentProfilePanel : DockContent
    {
        private ProfileRegistry _profiles = new ProfileRegistry();
        
        public ProfileRegistry Profiles
        {
            get { return _profiles; }
        }
        
        public AgentProfilePanel()
        {
            InitializeComponent();
            _profiles.LoadProfiles();
            UpdateView(this, EventArgs.Empty);
            _profiles.OnChanged += UpdateView;            
        }

        private void AgentProfilePanel_Load(object sender, System.EventArgs e)
        {
            UpdateView(this, e);
        }

        private void UpdateView(object sender, System.EventArgs e)
        {
            string display = "";

            listView1.Items.Clear();
            foreach (AgentProfile profile in _profiles.Profiles)
            {
                if (profile.Name.Length != 0)
                {
                    display = profile.Name;
                }
                else
                {
                    display = profile.Agent.ToString();
                }

                ListViewItem item = new ListViewItem(new string[] { display, profile.Agent.ToString() });
                listView1.Items.Add(item);
                item.Tag = profile;

                switch (profile.VersionCode)
                {
                    case VersionCode.V1:
                        {
                            item.Group = listView1.Groups["lvgV1"];
                            break;
                        }
                    case VersionCode.V2:
                        {
                            item.Group = listView1.Groups["lvgV2"];
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                //
                // Lets make the default Agent bold
                //
                if (profile == _profiles.DefaultProfile)
                {
                    item.Font = new Font(listView1.Font, FontStyle.Bold);
                }

                item.ToolTipText = profile.Agent.ToString();
            }
        }

        private void actDelete_Update(object sender, System.EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1 && _profiles.DefaultProfile == listView1.SelectedItems[0].Tag as AgentProfile)
            {
                actDelete.Enabled = false;
            }
            else
            {
                actDelete.Enabled = listView1.SelectedItems.Count == 1;
            }
        }

        private void actEdit_Update(object sender, System.EventArgs e)
        {
            actEdit.Enabled = listView1.SelectedItems.Count == 1;
        }

        private void actDefault_Update(object sender, System.EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1 && _profiles.DefaultProfile == listView1.SelectedItems[0].Tag as AgentProfile)
            {
                actDefault.Enabled = false;
            }
            else
            {
                actDefault.Enabled = listView1.SelectedItems.Count == 1;
            }
        }

        private void actDefault_Execute(object sender, System.EventArgs e)
        {
            _profiles.DefaultProfile = listView1.SelectedItems[0].Tag as AgentProfile;
            _profiles.SaveProfiles();

            //
            // Update view for new default agent
            //
            UpdateView(null, null);
        }

        private void actionList1_Update(object sender, System.EventArgs e)
        {
            tslblDefault.Text = "Default agent is " + _profiles.DefaultProfile.Name;
        }

        private void actDelete_Execute(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Do you want to remove this item", "Confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _profiles.DeleteProfile((listView1.SelectedItems[0].Tag as AgentProfile).Agent);
                _profiles.SaveProfiles();
            }
            catch (BrowserException ex)
            {
                TraceSource source = new TraceSource("Browser");
                source.TraceInformation(ex.Message);
                source.Flush();
                source.Close();
            }
        }

        private void actAdd_Execute(object sender, System.EventArgs e)
        {
            using (FormProfile editor = new FormProfile(null))
            {
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _profiles.AddProfile(new AgentProfile(editor.VersionCode, new IPEndPoint(editor.IP, editor.Port), editor.GetCommunity, editor.SetCommunity, editor.AgentName));
                        _profiles.SaveProfiles();
                    }
                    catch (BrowserException ex)
                    {
                        TraceSource source = new TraceSource("Browser");
                        source.TraceInformation(ex.Message);
                        source.Flush();
                        source.Close();
                    }
                }
            }
        }

        private void actEdit_Execute(object sender, System.EventArgs e)
        {
            AgentProfile profile = listView1.SelectedItems[0].Tag as AgentProfile;
            using (FormProfile editor = new FormProfile(profile))
            {
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    _profiles.ReplaceProfile(new AgentProfile(editor.VersionCode, new IPEndPoint(editor.IP, editor.Port), editor.GetCommunity, editor.SetCommunity, editor.AgentName));
                    _profiles.SaveProfiles();
                }
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextAgentMenu.Show(listView1, e.Location);
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && listView1.SelectedItems.Count == 1)
            {
                actEdit.DoExecute();
            }
        }
    }
}