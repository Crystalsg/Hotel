using Hotel.Storage;

namespace Hotel.UI;

public partial class RegistrationWindow : Form
{
    private readonly UserStore _userRepo = new();

    public RegistrationWindow()
    {
        InitializeComponent();
    }

    private void btnRegister_Click(object sender, EventArgs e)
    {
        var fullName = txtFullName.Text.Trim();
        var login = txtLogin.Text.Trim();
        var password = txtPassword.Text;
        var confirm = txtConfirmPassword.Text;

        if (string.IsNullOrWhiteSpace(fullName))
        {
            MessageBox.Show("Введите ФИО.");
            return;
        }
        if (login.Length < 4)
        {
            MessageBox.Show("Логин не менее 4 символов.");
            return;
        }
        if (password.Length < 6)
        {
            MessageBox.Show("Пароль не менее 6 символов.");
            return;
        }
        if (password != confirm)
        {
            MessageBox.Show("Пароли не совпадают.");
            return;
        }
        if (_userRepo.IsLoginTaken(login))
        {
            MessageBox.Show("Логин уже существует.");
            return;
        }

        _userRepo.RegisterNewUser(login, password, fullName, 3); 
        MessageBox.Show("Регистрация успешна!");
        Close();
    }
}