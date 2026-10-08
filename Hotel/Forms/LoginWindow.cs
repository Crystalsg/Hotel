using Hotel.Storage;
using Hotel.Services;
using Hotel.Models;

namespace Hotel.UI;

public partial class LoginWindow : Form
{
    private readonly CaptchaGenerator _captcha;
    private readonly LoginService _auth;

    public LoginWindow()
    {
        InitializeComponent();
        _captcha = new CaptchaGenerator();
        _auth = new LoginService(new UserStore());
        this.Load += OnFormLoad;
    }

    private void OnFormLoad(object? sender, EventArgs e)
    {
        lblCaptcha.Text = _captcha.Generate();
    }

    private void btnLogin_Click(object sender, EventArgs e)
    {
        try
        {
            if (!_captcha.Validate(txtCaptcha.Text))
            {
                RefreshCaptcha();
                MessageBox.Show("Неверная капча.");
                return;
            }
            var user = _auth.Authenticate(txtLogin.Text.Trim(), txtPassword.Text);
            var dashboard = new DashboardForm(user);
            dashboard.Show();
            this.Hide();
            dashboard.FormClosed += (s, args) => this.Close();
        }
        catch (Exception ex)
        {
            RefreshCaptcha();
            MessageBox.Show(ex.Message);
        }
    }

    private void btnRegister_Click(object sender, EventArgs e)
    {
        new RegistrationWindow().ShowDialog();
    }

    private void RefreshCaptcha()
    {
        lblCaptcha.Text = _captcha.Generate();
        txtCaptcha.Clear();
    }

    private void txtCaptcha_TextChanged(object sender, EventArgs e)
    {

    }
}