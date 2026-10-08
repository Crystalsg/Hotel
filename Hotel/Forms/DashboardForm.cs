using Hotel.Models;
using Hotel.Services;
using Hotel.Storage;

namespace Hotel.UI; 

public partial class DashboardForm : Form
{
    private readonly User _currentSession;
    private readonly RoomStore _roomRepo = new();
    private readonly BookingStore _bookingRepo = new();

    public DashboardForm(User user)
    {
        InitializeComponent();
        _currentSession = user;
        Text = $"Гостиница Космос — {_currentSession.FullName} ({_currentSession.RoleName})";
        RefreshDashboard();
        LoadRecent();
        ApplyRights();
    }

    private void RefreshDashboard()
    {
        lblFreeRooms.Text = $"Свободных номеров: {_roomRepo.CountFree()}";
        lblCheckIns.Text = $"Заездов сегодня: {_bookingRepo.CountTodayArrivals()}";
        lblCheckOuts.Text = $"Выездов сегодня: {_bookingRepo.CountTodayDepartures()}";
    }

    private void LoadRecent()
    {
        var data = _bookingRepo.FetchAll(txtSearchBooking.Text);
        var gridData = data.Select(b => new
        {
            b.Id,
            Гость = b.GuestName,
            Номер = b.RoomNumber,
            Заезд = b.CheckInDate.ToShortDateString(),
            Выезд = b.CheckOutDate.ToShortDateString(),
            Статус = b.Status,
            Сумма = b.TotalAmount
        }).ToList();
        dgvRecentBookings.DataSource = null;
        dgvRecentBookings.DataSource = gridData;
        if (dgvRecentBookings.Columns.Contains("Id"))
            dgvRecentBookings.Columns["Id"].Visible = false;
    }

    private void ApplyRights()
    {
        bool isAdmin = _currentSession.RoleName == "admin";
        logsMenuItem.Visible = isAdmin;
        actionLogsMenuItem.Visible = isAdmin;
       
    }

   
    private void OnRoomsClick(object sender, EventArgs e)
    {
        using var form = new RoomManagementForm(_currentSession);
        form.ShowDialog();
        RefreshDashboard(); 
        LoadRecent();      
    }
    private void OnGuestsClick(object sender, EventArgs e) => new GuestManagementForm(_currentSession).ShowDialog();
    private void OnBookingsClick(object sender, EventArgs e)
    {
        using var form = new BookingManagementForm(_currentSession);
        form.ShowDialog();

       
        RefreshDashboard();  
        LoadRecent();        
    }
    private void OnServicesClick(object sender, EventArgs e) => new ServiceManagementForm(_currentSession).ShowDialog();
    private void OnPaymentsClick(object sender, EventArgs e) => new PaymentManagementForm(_currentSession).ShowDialog();
    private void OnLoginLogsClick(object sender, EventArgs e) => new LoginHistoryForm().ShowDialog();
    private void OnActionLogsClick(object sender, EventArgs e) => ShowActionLogs();
    private void OnSearchClick(object sender, EventArgs e) => LoadRecent();
    private void OnExitClick(object sender, EventArgs e) => Application.Exit();


    private void ShowActionLogs()
    {
        var audit = new AuditStore();
        var logs = audit.FetchAllLogs();
        var viewer = new Form { Text = "Журнал действий", Size = new Size(900, 400) };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true };
        grid.DataSource = logs.Select(l => new { l.UserLogin, l.ActionType, l.EntityName, l.EntityId, l.Details, l.CreatedAt }).ToList();
        viewer.Controls.Add(grid);
        viewer.ShowDialog();
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.CloseReason == CloseReason.UserClosing)
        {
            Application.Exit();
        }
    }
}