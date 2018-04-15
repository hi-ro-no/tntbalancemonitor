// Copyright (C) 2017 valiant1x contact@intensecoin.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TntBalanceMonitor
{
    public partial class frmMain : Form
    {
        BindingList<Address> Addresses = new BindingList<Address>();
        string strHttp = "http://";
        string strConfig = "/config";

        private const int nNodeTntVolume = 5000;
        private const int nRewardTntVolume = 6500;

        public frmMain()
        {
            InitializeComponent();

            System.Diagnostics.FileVersionInfo ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Text = this.Text + " Ver" + ver.FileMajorPart.ToString() + "." + ver.FileMinorPart.ToString() + ver.FileBuildPart.ToString();

            if (File.Exists("address.txt"))
            {
                try
                {
                    var lines = File.ReadAllLines("address.txt");
                    foreach(var line in lines)
                    {
                        if (string.IsNullOrEmpty(line))
                            continue;

                        if (Addresses.Where(x => x.Addr.Equals(line.Trim(), StringComparison.OrdinalIgnoreCase)).Count() == 0)
                        {
                            string[] t = line.Split('\t');
                            Addresses.Add(new Address(t[0].Trim(), t[1].Trim()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                try
                {
                    File.WriteAllText("address.txt", "");
                }
                catch { }              
            }

            AddLogMsg("API graciously powered by ethplorer.io");
            AddLogMsg("Donations greatly appreciated: 0xa81248aE54dE6521d3afd58848006676EE874bFC");
            AddLogMsg("Donations greatly appreciated (hirono): 0x463055B36E9e080cdB73eaF71744E75690c3D771");
            AddLogMsg("Loaded " + Addresses.Count + " addresses to monitor.");
            
            dgMain.DataSource = null;
            dgMain.AutoGenerateColumns = true;
            dgMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgMain.AllowDrop =
                dgMain.AllowUserToAddRows =
                dgMain.AllowUserToDeleteRows =
                false;

            dgMain.ReadOnly = true;

            dgMain.DataSource = new BindingSource { DataSource = Addresses };

            var _ = CheckTokenBalance();
        }

        async Task CheckTokenBalance()
        {
            while (true)
            {
                bool wasFresh = false;

                try
                {
                    //query server
                    string url = @"https://api.ethplorer.io/getAddressInfo/{ADDR}?apiKey=freekey";
                    var addr = Addresses.OrderBy(x => x.LastUpdated).First();
                    url = url.Replace("{ADDR}", addr.Addr);

                    wasFresh = (addr.LastUpdated == new DateTime());

                    HttpWebRequest request = HttpWebRequest.CreateHttp(url);
                    request.Timeout = 10000;
                    var response = await request.GetResponseAsync();
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string body = await reader.ReadToEndAsync();
                        dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(body);

                        addr.BalEth = jsonResponse.ETH.balance;

                        float newTntBal = 0;
                        if (jsonResponse.tokens != null)
                        {
                            newTntBal = jsonResponse.tokens[0].balance / 100000000;
                        }

                        if ((addr.BalTnt != 0) && (addr.BalTnt != newTntBal))
                        {
                            AddLogMsg("*** Value changedfor " + addr.Addr + " -- previous: " +  addr.BalTnt + " -- new: " + newTntBal);
                        }

                        if (addr.StartBalTnt == 0)
                        {
                            addr.StartBalTnt = newTntBal;
                        }
                        addr.BalTnt = newTntBal;

                        addr.LastUpdated = DateTime.Now;
                    }

                    //check node audit status
                    string auditUrl = strHttp;

                    auditUrl += addr.Ip + strConfig;

                    addr.Height = "";
                    addr.AuditPassed = false;

                    request = HttpWebRequest.CreateHttp(auditUrl);
                    request.Timeout = 10000;
                    response = await request.GetResponseAsync();
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string body = await reader.ReadToEndAsync();
                        dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(body);

                        string strTmp = jsonResponse.calendar.height.ToString();
                        addr.Height = strTmp;
                        addr.AuditPassed = true;
                    }
                }
                catch { }
                
                dgMain.AutoResizeColumns();

                await Task.Delay(wasFresh ? 10000 : 30000);
            }
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            if (Addresses.Count == 0)
            {
                MessageBox.Show("No addresses found! Put ETH addresses, IP address into address.txt file. 1 address per line.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Process.Start("address.txt");

                this.Close();
            }
        }

        void AddLogMsg(string msg)
        {
            txtLog.AppendText("[" + DateTime.Now.ToLongTimeString() + "] " + msg + Environment.NewLine);
        }

        private void dgMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgMain.Columns[e.ColumnIndex].Name == "BalTnt")
            {
                Address addr = Addresses[e.RowIndex];
                if ((addr.BalTnt >= nRewardTntVolume) && (addr.StartBalTnt != addr.BalTnt))
                {
                    e.CellStyle.BackColor = Color.Green;
                    e.CellStyle.ForeColor = Color.WhiteSmoke;
                }
                else
                {
                    if (addr.BalTnt > nNodeTntVolume)
                    {
                        e.CellStyle.ForeColor = Color.Green;
                    }
                    else if (addr.BalTnt < nNodeTntVolume)
                    {
                        e.CellStyle.ForeColor = Color.Red;
                    }
                }
            }

            if (dgMain.Columns[e.ColumnIndex].Name == "AuditPassed")
            {
                Address addr = Addresses[e.RowIndex];
                if (!addr.AuditPassed)
                {
                    e.CellStyle.BackColor = Color.OrangeRed;
                    e.CellStyle.ForeColor = Color.WhiteSmoke;
                }
            }
        }
    }
}
