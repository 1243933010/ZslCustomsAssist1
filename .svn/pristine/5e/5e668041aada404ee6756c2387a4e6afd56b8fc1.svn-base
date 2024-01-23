using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.SPSecure;
using ZslCustomsAssist.Utils;

namespace ZslCustomsAssist
{
    public partial class FormSign : Form
    {
        public FormSign()
        {
            InitializeComponent();
        }

        private void btnSign_Click(object sender, EventArgs e)
        {
            string str = txtSource.Text;
            txtResult.Text = Encrypter.DefaultDecodeAES(str);
            return;


            if (ServerCore.clientConfig.TypistPassword == "")
            {
                //SignForm.logger.Debug((object)"IC密码不允许为空,不能验证key!");
                int num = (int)MessageBox.Show("IC密码不允许为空！");
            }
            else
            {
                string str1 = "" + "卡号:" + SPSecureAPI.SpcGetCardID() + "\r\n" + "证书号:" + SPSecureAPI.SpcGetCertNo() + "\r\n";
                string str2 = "";
                try
                {
                    str2 = SPSecureAPI.SpcSignData(txtSource.Text);
                }
                catch (Exception ex)
                {
                    //SignForm.logger.Error((object)ex);
                }
                txtResult.Text = str1 + "加签结果:\r\n" + "\t" + str2;
            }
        }

        private void FormSign_Load(object sender, EventArgs e)
        {
            
        }

        private void FormSign_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
