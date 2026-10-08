using Hotel.Models;
using Hotel.Services;
using Hotel.Storage;
using Microsoft.Data.Sqlite;

namespace Hotel.UI;

public partial class BookingManagementForm : Form
{
    private readonly User _currentUser;
    private readonly BookingStore _bookingRepo = new();
    private readonly GuestStore _guestRepo = new();
    private readonly RoomStore _roomRepo = new();
    private readonly AuditStore _audit = new();

    public BookingManagementForm(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadBookings();
        ApplyPermissions();
    }

    private void LoadBookings()
    {
        var data = _bookingRepo.FetchAll(txtSearch.Text);
        var display = data.Select(b => new { b.Id, Гость = b.GuestName, Номер = b.RoomNumber, Заезд = b.CheckInDate.ToShortDateString(), Выезд = b.CheckOutDate.ToShortDateString(), Статус = b.Status, Сумма = b.TotalAmount }).ToList();
        dgv.DataSource = null;
        dgv.DataSource = display;
        if (dgv.Columns.Contains("Id")) dgv.Columns["Id"].Visible = false;
    }

    private void ApplyPermissions()
    {
        bool canEdit = _currentUser.RoleName != "user";
        btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = canEdit;
    }

    private void OnSearch(object sender, EventArgs e) => LoadBookings();
    private void OnAdd(object sender, EventArgs e) => ShowEditor(null);
    private void OnEdit(object sender, EventArgs e) => EditBooking();
    private void OnDelete(object sender, EventArgs e) => DeleteBooking();
    private void OnExport(object sender, EventArgs e) => ExportData();

    private void ShowEditor(Booking? booking)
    {
        var editor = new Form
        {
            Text = booking == null ? "Новое бронирование" : "Редактирование",
            Size = new Size(480, 400),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent
        };
        var guests = _guestRepo.FetchAll("");
        var cmbGuest = new ComboBox
        {
            Location = new Point(120, 20),
            Size = new Size(280, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "FullName",
            ValueMember = "Id",
            DataSource = guests
        };
        var rooms = _roomRepo.GetAll("");
        var cmbRoom = new ComboBox
        {
            Location = new Point(120, 60),
            Size = new Size(280, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "Number",
            ValueMember = "Id",
            DataSource = rooms
        };
        var dtpIn = new DateTimePicker { Location = new Point(120, 100), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };
        var dtpOut = new DateTimePicker { Location = new Point(120, 140), Size = new Size(150, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddDays(1) };
        var cbStatus = new ComboBox
        {
            Location = new Point(120, 180),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Items = { "Активно", "Завершено", "Отменено" }
        };
        cbStatus.SelectedIndex = 0;
        var nudTotal = new NumericUpDown { Location = new Point(120, 220), Size = new Size(150, 25), DecimalPlaces = 2, Minimum = 0, Maximum = 1000000 };

        if (booking != null)
        {
            cmbGuest.SelectedValue = booking.GuestId;
            cmbRoom.SelectedValue = booking.RoomId;
            dtpIn.Value = booking.CheckInDate;
            dtpOut.Value = booking.CheckOutDate;
            cbStatus.SelectedItem = booking.Status;
            nudTotal.Value = booking.TotalAmount;
        }
        else
        {
            void UpdateTotal()
            {
                if (cmbRoom.SelectedItem is Room selectedRoom)
                {
                    int days = (dtpOut.Value - dtpIn.Value).Days;
                    if (days < 1) days = 1;
                    nudTotal.Value = selectedRoom.PricePerNight * days;
                }
            }
            dtpIn.ValueChanged += (s, e) => UpdateTotal();
            dtpOut.ValueChanged += (s, e) => UpdateTotal();
            cmbRoom.SelectedIndexChanged += (s, e) => UpdateTotal();
            UpdateTotal();
        }

        var saveBtn = new Button { Text = "Сохранить", Location = new Point(120, 270), Size = new Size(100, 35) };
        saveBtn.Click += (s, e) =>
        {
            if (cmbGuest.SelectedItem == null || cmbRoom.SelectedItem == null)
            {
                MessageBox.Show("Выберите гостя и номер.");
                return;
            }
            var newBooking = new Booking
            {
                GuestId = (int)cmbGuest.SelectedValue,
                RoomId = (int)cmbRoom.SelectedValue,
                CheckInDate = dtpIn.Value,
                CheckOutDate = dtpOut.Value,
                Status = cbStatus.SelectedItem?.ToString()!,
                TotalAmount = nudTotal.Value
            };
            if (booking == null)
            {
                _bookingRepo.Insert(newBooking);
                _audit.LogAction(_currentUser.Login, "CREATE", "Booking", newBooking.Id.ToString(), "Добавлено бронирование");
            }
            else
            {
                newBooking.Id = booking.Id;
                _bookingRepo.Update(newBooking);
                _audit.LogAction(_currentUser.Login, "UPDATE", "Booking", newBooking.Id.ToString(), "Изменено бронирование");
            }
            LoadBookings();
            editor.Close();
        };

        int y = 20;
        editor.Controls.AddRange(new Control[] {
            new Label { Text = "Гость:", Location = new Point(30, y), Size = new Size(80,25) }, cmbGuest,
            new Label { Text = "Номер:", Location = new Point(30, y+40), Size = new Size(80,25) }, cmbRoom,
            new Label { Text = "Заезд:", Location = new Point(30, y+80), Size = new Size(80,25) }, dtpIn,
            new Label { Text = "Выезд:", Location = new Point(30, y+120), Size = new Size(80,25) }, dtpOut,
            new Label { Text = "Статус:", Location = new Point(30, y+160), Size = new Size(80,25) }, cbStatus,
            new Label { Text = "Сумма:", Location = new Point(30, y+200), Size = new Size(80,25) }, nudTotal,
            saveBtn
        });
        editor.ShowDialog();
    }

    private void EditBooking()
    {
        if (dgv.CurrentRow?.DataBoundItem is object item)
        {
            var idProp = item.GetType().GetProperty("Id");
            if (idProp != null)
            {
                int id = (int)idProp.GetValue(item)!;
                var booking = _bookingRepo.FindById(id);
                if (booking != null) ShowEditor(booking);
            }
        }
        else MessageBox.Show("Выберите запись.");
    }

    private void DeleteBooking()
    {
        if (dgv.CurrentRow?.DataBoundItem is object item)
        {
            var idProp = item.GetType().GetProperty("Id");
            if (idProp != null)
            {
                int id = (int)idProp.GetValue(item)!;
                if (MessageBox.Show("Удалить бронирование?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        _bookingRepo.Delete(id);
                        _audit.LogAction(_currentUser.Login, "DELETE", "Booking", id.ToString(), "Удалено бронирование");
                        LoadBookings();
                    }
                    catch (InvalidOperationException ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        MessageBox.Show("Невозможно удалить бронирование: по нему уже проведены оплаты. Сначала удалите оплаты.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        else MessageBox.Show("Выберите запись.");
    }

    private void ExportData()
    {
        var data = _bookingRepo.FetchAll(txtSearch.Text);
        CsvExportHelper.Export(data, $"Bookings_{DateTime.Now:yyyyMMdd_HHmmss}.csv", _currentUser.Login, _audit);
    }
}