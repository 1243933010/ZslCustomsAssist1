using Newtonsoft.Json;
using ZslCustomsAssist.Jobs;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Runtime.Config;
using ZslCustomsAssist.Service;
using ZslCustomsAssist.Service.Rest;
using ZslCustomsAssist.SPSecure;
using ZslCustomsAssist.User;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist
{
    public partial class FormLogin : AbstractFormLog
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string str1 = !ServerCore.IsWithOutCard ? ServerCore.userData.szCardID : this.txtCardNo.Text;
            if (ServerCore.IsBindingCard && str1 != ResourceSetting.GetValue("RestrictedCardNo"))
            {
                int num = (int)MessageBox.Show("卡号异常，客户端即将退出！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                new ExitProgramJob().QuitApplication();
            }
            else
            {
                labMsg.Text = "登录中...";
                btnLogin.Enabled = false;
                if (!ServerCore.IsWithOutCard)
                {
                    string text = SPSecureAPI.SpcVerifyPIN(txtUkeyPasswd.Text);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        int num = (int)MessageBox.Show(text, "提示", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        btnLogin.Enabled = true;
                        labMsg.Text = "";
                        labUkeyPasswd.Text = "";
                        return;
                    }
                    UserData.GetCardMsg();
                }
                else
                {
                    ServerCore.userData.szCardID = this.txtCardNo.Text;
                    FormLogin.logger.Info((object)("当前卡号：" + this.txtCardNo.Text));
                }

                //登录API
                ApiTokenData apiToken = null;
                try
                {
                    apiToken = new ApiService().Login(txtAppId.Text, txtAppSecret.Text, out string loginMsg);
                    if (apiToken == null)
                    {
                        int num = (int)MessageBox.Show(loginMsg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    int num = (int)MessageBox.Show("登录中商旅系统接口发生异常，客户端即将退出！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    new ExitProgramJob().QuitApplication();
                    return;
                }

                if (ServerCore.clientConfig == null)
                    ServerCore.clientConfig = new ClientConfig();
                ServerCore.clientConfig.TypistPassword = txtUkeyPasswd.Text;
                ServerCore.clientConfig.ApiAppId = txtAppId.Text;
                ServerCore.clientConfig.ApiAppSecret = txtAppSecret.Text;
                ServerCore.clientConfig.ApiToken = apiToken;
                ClientConfig clientConfig = ClientConfig.LoadConfig();
                ServerCore.clientConfig.IcNo = this.txtCardNo.Text;
                clientConfig.IcNo = this.txtCardNo.Text;
                clientConfig.TypistPassword = txtUkeyPasswd.Text;
                clientConfig.ApiAppId = txtAppId.Text;
                clientConfig.ApiAppSecret = txtAppSecret.Text;
                clientConfig.ApiToken = apiToken;

                string str2 = IOHelper.InputConfigFile(clientConfig);
                if (string.IsNullOrWhiteSpace(str2))
                {
                    ServerCore.isLogin = true;
                    this.Hide();
                }
                else
                {
                    int num = (int)MessageBox.Show(str2 + "导致配置失败！\n并请检查本地“" + ClientConfig.GetConfigFilePath() + "”目录读写权限后重启客户端！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    new ExitProgramJob().QuitApplication();
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            new ExitProgramJob().QuitApplication();
            this.Close();
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            ServerCore.isLogin = false;
            if (ServerCore.userData == null)
                ServerCore.userData = new UserData();
            if (ServerCore.IsWithOutCard)
            {
                FormLogin.logger.Info((object)"无卡模式登录页启动！");
                txtCardNo.ReadOnly = false;
                //this.txtCardNo.Visible = true;
                //this.lblCardNo.Visible = true;
                //this.lblCardNoRequired.Visible = true;
                this.Text = "登录中商旅报关辅助程序(无卡模式)";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ServerCore.userData.szCardID))
                {
                    int num = (int)MessageBox.Show("请确认插卡设备正常运作后再打开客户端！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    new ExitProgramJob().QuitApplication();
                    this.Close();
                }
                txtCardNo.Text = ServerCore.userData.szCardID;
                FormLogin.logger.Info((object)("当前IC卡号：" + ServerCore.userData.szCardID));
            }
        }

        private bool SaveValidator(bool skipLengthValidator = false)
        {
            bool valid = true;
            if (ServerCore.IsWithOutCard && string.IsNullOrWhiteSpace(this.txtCardNo.Text))
            {
                labCardNo.Text = "卡号不能为空";
                btnLogin.Enabled = false;
                valid = false;
            }
            string text = txtUkeyPasswd.Text;
            int length = text.Length;
            if (string.IsNullOrWhiteSpace(text))
            {
                labUkeyPasswd.Text = "密码不能为空";
                btnLogin.Enabled = false;
                valid = false;
            }
            else if (length != 8)
            {
                labUkeyPasswd.Text = "密码只能为八位字符";
                btnLogin.Enabled = false;
                valid = false;
            } 
            else
            {
                labUkeyPasswd.Text = "";
            }
            

            string appId = txtAppId.Text;
            if (string.IsNullOrWhiteSpace(appId))
            {
                labAppId.Text = "请输入中商旅AppId";
                btnLogin.Enabled = false;
                valid = false;
            } 
            else
            {
                labAppId.Text = "";
            }

            string secret = txtAppSecret.Text;
            if (string.IsNullOrWhiteSpace(secret))
            {
                labAppSecret.Text = "请输入中商旅AppSecret";
                btnLogin.Enabled = false;
                valid = false;
            }
            else
            {
                labAppSecret.Text = "";
            }

            labMsg.Text = "";
            if (valid)
            {
                btnLogin.Enabled = true;
            }
            return valid;
        }

        private void txtBox_TextChanged(object sender, EventArgs e) => this.SaveValidator(true);
    }
}
