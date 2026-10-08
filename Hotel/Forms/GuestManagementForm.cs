using Hotel.Models;
using Hotel.Services;
using Hotel.Storage;
using Microsoft.Data.Sqlite;

namespace Hotel.UI;

public partial class GuestManagementForm : Form
{
    private readonly User _currentUser;
    private readonly GuestStore _guestRepo = new();
    private readonly AuditStore _audit = new();

    public GuestManagementForm(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadGuests();
        ApplyPermissions();
    }

    private void LoadGuests()
    {
        var data = _guestRepo.FetchAll(txtSearch.Text);
        dgv.DataSource = null;
        dgv.DataSource = data;
        if (dgv.Columns.Contains("Id")) dgv.Columns["Id"].Visible = false;
    }

    private void ApplyPermissions()
    {
        bool canEdit = _currentUser.RoleName != "user";
        btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = canEdit;
    }

    private void OnSearch(object sender, EventArgs e) => LoadGuests();
    private void OnAdd(object sender, EventArgs e) => ShowEditor(null);
    private void OnEdit(object sender, EventArgs e) => EditGuest();
    private void OnDelete(object sender, EventArgs e) => DeleteGuest();
    private void OnExport(object sender, EventArgs e) => ExportData();

    private void ShowEditor(Guest? guest)
    {
        var editor = new Form
        {
            Text = guest == null ? "Новый гость" : "Редактирование",
            Size = new Size(450, 350),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent
        };
        var txtFullName = new TextBox { Location = new Point(120, 20), Size = new Size(250, 25) };
        var txtPassport = new TextBox { Location = new Point(120, 60), Size = new Size(250, 25) };
        var txtPhone = new TextBox { Location = new Point(120, 100), Size = new Size(250, 25) };
        var txtEmail = new TextBox { Location = new Point(120, 140), Size = new Size(250, 25) };
        var chkActive = new CheckBox { Text = "Активен", Location = new Point(120, 180), Size = new Size(100, 25), Checked = true };

        if (guest != null)
        {
            txtFullName.Text = guest.FullName;
            txtPassport.Text = guest.Passport;
            txtPhone.Text = guest.Phone;
            txtEmail.Text = guest.Email;
            chkActive.Checked = guest.IsActive;
        }

        var saveBtn = new Button { Text = "Сохранить", Location = new Point(120, 230), Size = new Size(100, 35) };
        saveBtn.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtPassport.Text))
            {
                MessageBox.Show("ФИО и паспорт обязательны.");
                return;
            }
            var newGuest = new Guest
            {
                FullName = txtFullName.Text.Trim(),
                Passport = txtPassport.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                IsActive = chkActive.Checked
            };
            if (guest == null)
            {
                _guestRepo.Insert(newGuest);
                _audit.LogAction(_currentUser.Login, "CREATE", "Guest", newGuest.FullName, $"Добавлен гость {newGuest.FullName}");
            }
            else
            {
                newGuest.Id = guest.Id;
                _guestRepo.Update(newGuest);
                _audit.LogAction(_currentUser.Login, "UPDATE", "Guest", newGuest.FullName, $"Изменён гость {newGuest.FullName}");
            }
            LoadGuests();
            editor.Close();
        };

        editor.Controls.AddRange(new Control[] {
            new Label { Text = "ФИО:", Location = new Point(30,20), Size = new Size(80,25) }, txtFullName,
            new Label { Text = "Паспорт:", Location = new Point(30,60), Size = new Size(80,25) }, txtPassport,
            new Label { Text = "Телефон:", Location = new Point(30,100), Size = new Size(80,25) }, txtPhone,
            new Label { Text = "Email:", Location = new Point(30,140), Size = new Size(80,25) }, txtEmail,
            chkActive, saveBtn
        });
        editor.ShowDialog();
    }

    private void EditGuest()
    {
        if (dgv.CurrentRow?.DataBoundItem is Guest g)
            ShowEditor(g);
        else
            MessageBox.Show("Выберите запись.");
    }

    private void DeleteGuest()
    {
        if (dgv.CurrentRow?.DataBoundItem is Guest g)
        {
            if (MessageBox.Show($"Удалить гостя {g.FullName}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _guestRepo.Delete(g.Id);
                    _audit.LogAction(_currentUser.Login, "DELETE", "Guest", g.FullName, $"Удалён гость {g.FullName}");
                    LoadGuests();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    MessageBox.Show("Невозможно удалить гостя: у него есть бронирования. Сначала удалите бронирования.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        else MessageBox.Show("Выберите запись.");
    }

    private void ExportData()
    {
        var data = _guestRepo.FetchAll(txtSearch.Text);
        CsvExportHelper.Export(data, $"Guests_{DateTime.Now:yyyyMMdd_HHmmss}.csv", _currentUser.Login, _audit);
    }
}