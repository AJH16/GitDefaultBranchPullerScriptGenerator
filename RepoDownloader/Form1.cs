using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RepoDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            VssBasicCredential credential = new VssBasicCredential(string.Empty, textBox1.Text);
            GitHttpClient gitClient = new GitHttpClient(new Uri(textBox2.Text), credential);
            List<GitRepository> repositories = gitClient.GetRepositoriesAsync((bool?)null, null).GetAwaiter().GetResult();
            List<KeyValuePair<GitRepository, Task<List<GitBranchStats>>>> branchStatsTasks = new List<KeyValuePair<GitRepository, Task<List<GitBranchStats>>>>();
            List<KeyValuePair<GitRepository, GitBranchStats>> branches = new List<KeyValuePair<GitRepository, GitBranchStats>>();
            StringBuilder builder = new StringBuilder();
            StringBuilder fetchBuilder = new StringBuilder();
            foreach(GitRepository repo in repositories)
            {
                builder.AppendLine("git clone " + repo.Url + " " + repo.Name);
                branchStatsTasks.Add(new KeyValuePair<GitRepository, Task<List<GitBranchStats>>>(repo, gitClient.GetBranchesAsync(repo.Id)));
            }
            textBox3.Text = builder.ToString();

            foreach(KeyValuePair<GitRepository, Task<List<GitBranchStats>>> task in branchStatsTasks)
            {
                branches.AddRange(task.Value.GetAwaiter().GetResult().Where(o => o.Name.ToLowerInvariant() == "developer" || o.Name == task.Key.Name).Select(o => new KeyValuePair<GitRepository, GitBranchStats>(task.Key, o)));
            }

            foreach(KeyValuePair<GitRepository, GitBranchStats> branch in branches)
            {
                fetchBuilder.AppendLine("git -C ./" + branch.Key.Name + " fetch origin " + branch.Value.Name + ":" + branch.Value.Name);
            }
            textBox4.Text = fetchBuilder.ToString();
        }
    }
}
