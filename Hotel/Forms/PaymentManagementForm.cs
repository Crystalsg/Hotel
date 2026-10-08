using Hotel.Storage;
using Hotel.Models;
using Hotel.Services;

namespace Hotel.UI;

public partial class PaymentManagementForm : Form
{
    private readonly User _currentUser;
    private readonly PaymentStore _paymentRepo = new();
    private readonly AuditStore _audit = new();

    public PaymentManagementForm(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadPayments();
        ApplyPermissions();
    }

    private void LoadPayments()
    {
        var data = _paymentRepo.FetchAll(txtSearch.Text);
        var display = data.Select(p => new { p.Id, Бронирование = p.BookingInfo, Сумма = p.Amount, Дата = p.PaymentDate.ToString("yyyy-MM-dd HH:mm"), Способ = p.Method }).ToList();
        dgv.DataSource = null;
        dgv.DataSource = display;
        if (dgv.Columns.Contains("Id")) dgv.Columns["Id"].Visible = false;
    }

    private void ApplyPermissions()
    {
        bool canEdit = _currentUser.RoleName != "user";
        btnAdd.Enabled = btnDelete.Enabled = canEdit;
    }

    private void OnSearch(object sender, EventArgs e) => LoadPayments();
    private void OnAdd(object sender, EventArgs e) => AddPayment();
    private void OnDelete(object sender, EventArgs e) => DeletePayment();
    private void OnExport(object sender, EventArgs e) => ExportData();

    private void AddPayment()
    {
        var activeBookings = _paymentRepo.FetchActiveBookings();
        if (activeBookings.Count == 0)
        {
            MessageBox.Show("Нет активных бронирований.");
            return;
        }
        var dialog = new Form
        {
            Text = "Новая оплата",
            Size = new Size(450, 300),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent
        };
        var cmbBooking = new ComboBox
        {
            Location = new Point(140, 20),
            Size = new Size(250, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "GuestName",
            ValueMember = "Id",
            DataSource = activeBookings
        };
        var nudAmount = new NumericUpDown { Location = new Point(140, 60), Size = new Size(150, 25), DecimalPlaces = 2, Minimum = 0, Maximum = 1000000 };
        var dtpDate = new DateTimePicker
        {
            Location = new Point(140, 100),
            Size = new Size(150, 25),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "yyyy-MM-dd HH:mm:ss",
            Value = DateTime.Now
        };
        var cmbMethod = new ComboBox
        {
            Location = new Point(140, 140),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Items = { "Наличные", "Карта" }
        };
        cmbMethod.SelectedIndex = 0;
        var saveBtn = new Button { Text = "Сохранить", Location = new Point(140, 190), Size = new Size(100, 35) };
        saveBtn.Click += (s, e) =>
        {
            if (cmbBooking.SelectedItem == null || nudAmount.Value <= 0)
            {
                MessageBox.Show("Выберите бронирование и сумму.");
                return;
            }
            var payment = new Payment
            {
                BookingId = (int)cmbBooking.SelectedValue,
                Amount = nudAmount.Value,
                PaymentDate = dtpDate.Value,
                Method = cmbMethod.SelectedItem?.ToString()!
            };
            _paymentRepo.Add(payment);
            _audit.LogAction(_currentUser.Login, "CREATE", "Payment", payment.Id.ToString(), $"Оплата на сумму {payment.Amount}");
            LoadPayments();
            dialog.Close();
        };
        dialog.Controls.AddRange(new Control[] {
            new Label { Text = "Бронирование:", Location = new Point(30,20), Size = new Size(100,25) }, cmbBooking,
            new Label { Text = "Сумма:", Location = new Point(30,60), Size = new Size(100,25) }, nudAmount,
            new Label { Text = "Дата:", Location = new Point(30,100), Size = new Size(100,25) }, dtpDate,
            new Label { Text = "Способ:", Location = new Point(30,140), Size = new Size(100,25) }, cmbMethod,
            saveBtn
        });
        dialog.ShowDialog();
    }

    private void DeletePayment()
    {
        if (dgv.CurrentRow?.DataBoundItem is object item)
        {
            var idProp = item.GetType().GetProperty("Id");
            if (idProp != null)
            {
                int id = (int)idProp.GetValue(item)!;
                if (MessageBox.Show("Удалить оплату?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        _paymentRepo.Delete(id);
                        _audit.LogAction(_currentUser.Login, "DELETE", "Payment", id.ToString(), "Удалена оплата");
                        LoadPayments();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        else MessageBox.Show("Выберите запись.");
    }

    private void ExportData()
    {
        var data = _paymentRepo.FetchAll(txtSearch.Text);
        CsvExportHelper.Export(data, $"Payments_{DateTime.Now:yyyyMMdd_HHmmss}.csv", _currentUser.Login, _audit);
    }
}