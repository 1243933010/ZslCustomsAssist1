using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Service;

namespace ZslCustomsAssist
{
    public partial class FormSend : Form
    {
        public FormSend()
        {
            InitializeComponent();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string fileName = txtFileName.Text.Trim();
            string content = txtContent.Text.Trim();
            if (fileName.Length == 0) {
                MessageBox.Show("请输入报文名称");
                return;
            }
            if (content.Length == 0)
            {
                MessageBox.Show("请输入报文内容");
                return;
            }

            try
            {
                txtResult.Text = new SendHelper().SendReport(fileName, content).ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = ex.Message;
            }
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            string withoutExtension = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
            string[] strArray = withoutExtension.Split('_');
            if (strArray.Length < 2)
                return;
            string messageType = strArray[0];
            string receiverID = strArray[1];
            string str1 = strArray.Length <= 2 ? DateTime.Now.ToString("yyyyMMddHHmmssff") : withoutExtension.Replace(messageType + "_" + receiverID + "_", "");
            string xmlContent = XmlHelp.GetXmlContent(openFileDialog.FileName, (Encoding)null);
            txtContent.Text = xmlContent.Replace("\r\n", Environment.NewLine).Replace("\n", Environment.NewLine);
            txtFileName.Text = withoutExtension;
        }

        private void FormSend_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void txtContent_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFileName.Text.Trim()) || string.IsNullOrEmpty(txtContent.Text.Trim()))
            {
                btnSend.Enabled = false;
            } 
            else
            {
                btnSend.Enabled = true;
            }
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFileName.Text.Trim()) || string.IsNullOrEmpty(txtContent.Text.Trim()))
            {
                btnSend.Enabled = false;
            }
            else
            {
                btnSend.Enabled = true;
            }
        }

        private void FormSend_Load(object sender, EventArgs e)
        {

        }
    }
}
