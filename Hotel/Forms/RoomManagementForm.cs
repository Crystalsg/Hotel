using Hotel.Models;
using Hotel.Services;
using Hotel.Storage;
using Microsoft.Data.Sqlite;

namespace Hotel.UI;

public partial class RoomManagementForm : Form
{
    private readonly User _currentUser;
    private readonly RoomStore _roomRepo = new();
    private readonly AuditStore _audit = new();

    public RoomManagementForm(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadRooms();
        SetPermissions();
    }

    private void LoadRooms()
    {
        var data = _roomRepo.GetAll(txtSearch.Text);
        dgv.DataSource = null;
        dgv.DataSource = data;
        if (dgv.Columns.Contains("Id")) dgv.Columns["Id"].Visible = false;
    }

    private void SetPermissions()
    {
        bool canEdit = _currentUser.RoleName != "user";
        btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = canEdit;
    }

    private void OnSearch(object sender, EventArgs e) => LoadRooms();
    private void OnAdd(object sender, EventArgs e) => ShowEditor(null);
    private void OnEdit(object sender, EventArgs e) => EditRoom();
    private void OnDelete(object sender, EventArgs e) => DeleteRoom();
    private void OnExport(object sender, EventArgs e) => ExportData();

    private void ShowEditor(Room? room)
    {
        var editor = new Form
        {
            Text = room == null ? "Новый номер" : "Редактирование",
            Size = new Size(400, 300),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent
        };
        var txtNumber = new TextBox { Location = new Point(120, 20), Size = new Size(200, 25) };
        var cbType = new ComboBox { Location = new Point(120, 60), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
        cbType.Items.AddRange(new[] { "Стандарт", "Люкс", "Студия" });
        var nudCapacity = new NumericUpDown { Location = new Point(120, 100), Size = new Size(100, 25), Minimum = 1, Maximum = 10 };
        var nudPrice = new NumericUpDown { Location = new Point(120, 140), Size = new Size(150, 25), DecimalPlaces = 2, Minimum = 0, Maximum = 100000 };
        var cbStatus = new ComboBox { Location = new Point(120, 180), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
        cbStatus.Items.AddRange(new[] { "Свободен", "Занят", "Ремонт" });

        if (room != null)
        {
            txtNumber.Text = room.Number;
            cbType.SelectedItem = room.Type;
            nudCapacity.Value = room.Capacity;
            nudPrice.Value = room.PricePerNight;
            cbStatus.SelectedItem = room.Status;
        }
        else cbType.SelectedIndex = 0;

        var saveBtn = new Button { Text = "Сохранить", Location = new Point(120, 220), Size = new Size(100, 30) };
        saveBtn.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtNumber.Text))
            {
                MessageBox.Show("Введите номер.");
                return;
            }
            var newRoom = new Room
            {
                Number = txtNumber.Text,
                Type = cbType.SelectedItem?.ToString()!,
                Capacity = (int)nudCapacity.Value,
                PricePerNight = nudPrice.Value,
                Status = cbStatus.SelectedItem?.ToString()!
            };
            if (room == null)
            {
                _roomRepo.Add(newRoom);
                _audit.LogAction(_currentUser.Login, "CREATE", "Room", newRoom.Number, $"Добавлен номер {newRoom.Number}");
            }
            else
            {
                newRoom.Id = room.Id;
                _roomRepo.Update(newRoom);
                _audit.LogAction(_currentUser.Login, "UPDATE", "Room", newRoom.Number, $"Изменён номер {newRoom.Number}");
            }
            LoadRooms();
            editor.Close();
        };

        editor.Controls.AddRange(new Control[] {
            new Label { Text = "Номер:", Location = new Point(30,20), Size = new Size(80,25) }, txtNumber,
            new Label { Text = "Тип:", Location = new Point(30,60), Size = new Size(80,25) }, cbType,
            new Label { Text = "Вместимость:", Location = new Point(30,100), Size = new Size(80,25) }, nudCapacity,
            new Label { Text = "Цена за ночь:", Location = new Point(30,140), Size = new Size(80,25) }, nudPrice,
            new Label { Text = "Статус:", Location = new Point(30,180), Size = new Size(80,25) }, cbStatus,
            saveBtn
        });
        editor.ShowDialog();
    }

    private void EditRoom()
    {
        if (dgv.CurrentRow?.DataBoundItem is Room r)
            ShowEditor(r);
        else
            MessageBox.Show("Выберите запись.");
    }

    private void DeleteRoom()
    {
        if (dgv.CurrentRow?.DataBoundItem is Room r)
        {
            if (MessageBox.Show($"Удалить номер {r.Number}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _roomRepo.Delete(r.Id);
                    _audit.LogAction(_currentUser.Login, "DELETE", "Room", r.Number, $"Удалён номер {r.Number}");
                    LoadRooms();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    MessageBox.Show("Невозможно удалить номер: на него есть бронирования. Сначала удалите бронирования.",
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
        var data = _roomRepo.GetAll(txtSearch.Text);
        CsvExportHelper.Export(data, $"Rooms_{DateTime.Now:yyyyMMdd_HHmmss}.csv", _currentUser.Login, _audit);
    }
}