using Hotel.Storage;
using Hotel.Models;
using Hotel.Services;

namespace Hotel.UI;

public partial class ServiceManagementForm : Form
{
    private readonly User _currentUser;
    private readonly ServiceStore _serviceRepo = new();
    private readonly AuditStore _audit = new();

    public ServiceManagementForm(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadServices();
        ApplyPermissions();
    }

    private void LoadServices()
    {
        var data = _serviceRepo.FetchAll(txtSearch.Text);
        dgv.DataSource = null;
        dgv.DataSource = data;
        if (dgv.Columns.Contains("Id")) dgv.Columns["Id"].Visible = false;
    }

    private void ApplyPermissions()
    {
        bool canEdit = _currentUser.RoleName != "user";
        btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = canEdit;
    }

    private void OnSearch(object sender, EventArgs e) => LoadServices();
    private void OnAdd(object sender, EventArgs e) => ShowEditor(null);
    private void OnEdit(object sender, EventArgs e) => EditService();
    private void OnDelete(object sender, EventArgs e) => DeleteService();
    private void OnExport(object sender, EventArgs e) => ExportData();

    private void ShowEditor(Service? service)
    {
        var editor = new Form
        {
            Text = service == null ? "Новая услуга" : "Редактирование",
            Size = new Size(400, 280),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent
        };
        var txtName = new TextBox { Location = new Point(120, 20), Size = new Size(200, 25) };
        var nudPrice = new NumericUpDown { Location = new Point(120, 60), Size = new Size(150, 25), DecimalPlaces = 2, Minimum = 0, Maximum = 100000 };
        var txtCategory = new TextBox { Location = new Point(120, 100), Size = new Size(200, 25) };
        if (service != null)
        {
            txtName.Text = service.Name;
            nudPrice.Value = service.Price;
            txtCategory.Text = service.Category;
        }
        var saveBtn = new Button { Text = "Сохранить", Location = new Point(120, 160), Size = new Size(100, 35) };
        saveBtn.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название.");
                return;
            }
            var newService = new Service
            {
                Name = txtName.Text.Trim(),
                Price = nudPrice.Value,
                Category = txtCategory.Text.Trim()
            };
            if (service == null)
            {
                _serviceRepo.Insert(newService);
                _audit.LogAction(_currentUser.Login, "CREATE", "Service", newService.Name, $"Добавлена услуга {newService.Name}");
            }
            else
            {
                newService.Id = service.Id;
                _serviceRepo.Update(newService);
                _audit.LogAction(_currentUser.Login, "UPDATE", "Service", newService.Name, $"Изменена услуга {newService.Name}");
            }
            LoadServices();
            editor.Close();
        };
        editor.Controls.AddRange(new Control[] {
            new Label { Text = "Название:", Location = new Point(30,20), Size = new Size(80,25) }, txtName,
            new Label { Text = "Цена:", Location = new Point(30,60), Size = new Size(80,25) }, nudPrice,
            new Label { Text = "Категория:", Location = new Point(30,100), Size = new Size(80,25) }, txtCategory,
            saveBtn
        });
        editor.ShowDialog();
    }

    private void EditService()
    {
        if (dgv.CurrentRow?.DataBoundItem is Service s)
            ShowEditor(s);
        else
            MessageBox.Show("Выберите запись.");
    }

    private void DeleteService()
    {
        if (dgv.CurrentRow?.DataBoundItem is Service s)
        {
            if (MessageBox.Show($"Удалить услугу {s.Name}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _serviceRepo.Delete(s.Id);
                    _audit.LogAction(_currentUser.Login, "DELETE", "Service", s.Name, $"Удалена услуга {s.Name}");
                    LoadServices();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        else MessageBox.Show("Выберите запись.");
    }

    private void ExportData()
    {
        var data = _serviceRepo.FetchAll(txtSearch.Text);
        CsvExportHelper.Export(data, $"Services_{DateTime.Now:yyyyMMdd_HHmmss}.csv", _currentUser.Login, _audit);
    }
}