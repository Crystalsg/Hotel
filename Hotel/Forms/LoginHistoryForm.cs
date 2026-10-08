using Hotel.Storage;

namespace Hotel.UI;

public partial class LoginHistoryForm : Form
{
    private readonly UserStore _userRepo = new();

    public LoginHistoryForm()
    {
        InitializeComponent();
        LoadHistory();
    }

    private void LoadHistory()
    {
        var attempts = _userRepo.GetAllAttempts();
        var display = attempts.Select(a => new { a.Id, Логин = a.UserLogin, Успех = a.IsSuccess ? "Да" : "Нет", a.Message, ДатаВремя = a.CreatedAt }).ToList();
        dgv.DataSource = null;
        dgv.DataSource = display;
        if (dgv.Columns.Contains("Id")) dgv.Columns["Id"].Visible = false;
    }

    private void OnRefresh(object sender, EventArgs e) => LoadHistory();
}