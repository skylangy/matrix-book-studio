
using MatrixBook.Tray.Common;
using MatrixBook.Tray.Models;
using MatrixBook.Tray.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Reflection;

namespace MatrixBook.Tray;

public partial class MainForm : Form
{
    private readonly ICommandListener _commandListener;
    private readonly IMessageMediator _messageMediator;
    private readonly Configuration _configuration;
    private readonly ICommandHistory _commandHistory;
    private readonly IBackgroundService _backgroundService;
    private readonly ILogger _logger;

    public MainForm(
        IOptions<Configuration> configuration,
        ICommandListener commandListener,
        IMessageMediator messageMediator,
        ICommandHistory commandHistory,
        IBackgroundService backgroundService,
        ILogger<MainForm> logger)
    {
        InitializeComponent();
        InitializeListView();

        _commandListener = commandListener;
        _messageMediator = messageMediator;
        _configuration = configuration.Value;
        _commandHistory = commandHistory;
        _backgroundService = backgroundService;
        _logger = logger;
    }

    protected override async void OnLoad(EventArgs e)
    {
        try
        {
            _logger.LogInformation("Starting Matrix Book Studio Tray application...");
            Visible = false;
            ShowInTaskbar = false;
            InitializeStatusbar();
            base.OnLoad(e);

            InitializeContextMenu();

            var connectedIcon = GetIconFromResources("MatrixBook.Tray.Resources.book.ico");
            var disconnectedIcon = ConvertToGrayscaleIcon(connectedIcon);

            notifyIcon = new NotifyIcon
            {
                Text = "Matrix Book Studio",
                Icon = disconnectedIcon,
                ContextMenuStrip = contextMenuStrip,
                Visible = true
            };

            _messageMediator.RegisterHandler<ConnectionMessage>(message =>
            {
                if (message.IsConnected)
                {
                    notifyIcon.Icon = connectedIcon;
                    notifyIcon.ShowBalloonTip(1000, "Connected", $"Successfully connected to the server '{message.ServerName}'.", ToolTipIcon.Info);
                }
                else
                {
                    notifyIcon.Icon = disconnectedIcon;
                    notifyIcon.ShowBalloonTip(1000, "Disconnected", $"Disconnected from the server '{message.ServerName}'.", ToolTipIcon.Warning);
                }
            });
            _commandHistory.AddItemHandler("MainForm", item =>
            {
                Invoke(() =>
                {
                    listViewHistory.BeginUpdate();
                    var listViewItem = ToListViewItem(item);
                    listViewHistory.Items.Insert(0, listViewItem);
                    listViewHistory.EndUpdate();
                });
            });
            var cancellToken = new CancellationTokenSource().Token;
            _backgroundService.Start(cancellToken);
            await _commandListener.Start();
            _logger.LogInformation("Matrix Book Studio Tray application started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while starting the application.");
            notifyIcon.ShowBalloonTip(1000, "Error", "An error occurred while starting the application.", ToolTipIcon.Error);
        }
    }

    public void SetAutoStart(bool enable)
    {
        string appName = "MatrixBookStudioTray";
        string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath ?? throw new InvalidOperationException("Unable to determine the executable path.");

        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
        if (key != null)
        {
            _logger.LogInformation("Setting auto start for {appName} to {path}.", appName, exePath);
            if (enable)
            {
                key.SetValue(appName, exePath);
            }
            else
            {
                key.DeleteValue(appName, false);
            }
            notifyIcon.ShowBalloonTip(1000, "Succeeded", "Successfully set auto start.", ToolTipIcon.Info);
        }
        else
        {
            notifyIcon.ShowBalloonTip(1000, "Failed", "Failed to set auto start.", ToolTipIcon.Error);
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        e.Cancel = true;
        Visible = false;
        ShowInTaskbar = false;
    }

    private void InitializeListView()
    {
        listViewHistory.SetDoubleBuffered();
        // Enable details view
        listViewHistory.View = View.Details;
        listViewHistory.FullRowSelect = true;
        listViewHistory.GridLines = true;

        // Define columns
        listViewHistory.Columns.Add("Create Time", 200);
        listViewHistory.Columns.Add("Command", 180);
        listViewHistory.Columns.Add("Arguments", 400);
    }

    private void InitializeContextMenu()
    {
        contextMenuStrip.Items.Add("Reconnect", null, async (sender, args) => await _commandListener.Start());
        contextMenuStrip.Items.Add("-");
        contextMenuStrip.Items.Add("Set Auto Start", null, (sender, args) => { SetAutoStart(true); });
        contextMenuStrip.Items.Add("-");

        if (_configuration.Servers.Count > 0)
        {
            var serverMenu = new ToolStripMenuItem("Servers");
            foreach (var server in _configuration.Servers)
            {
                serverMenu.DropDownItems.Add($"{server.Name} ({server.Url})", null, (sender, args) => { });
            }
            contextMenuStrip.Items.Add(serverMenu);
        }

        contextMenuStrip.Items.Add("Show", null, (sender, args) => { ShowWindow(); });
        contextMenuStrip.Items.Add("Exit", null, (sender, args) => Application.Exit());
    }

    private void ShowWindow()
    {
        Visible = true;
        WindowState = FormWindowState.Normal;
        ShowInTaskbar = true;
        Activate();
    }

    private void InitializeStatusbar()
    {
        foreach (var server in _configuration.Servers)
        {
            statusStrip.Items.Add(new ToolStripStatusLabel
            {
                Text = $"{server.Name}: {server.Url}"
            });
        }
    }

    private static ListViewItem ToListViewItem(CommandHistoryItem item)
    {
        var listViewItem = new ListViewItem(item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        listViewItem.SubItems.Add(item.Command);
        listViewItem.SubItems.Add(item.Arguments);
        return listViewItem;
    }

    private static Icon GetIconFromResources(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);

        return stream == null ? throw new ArgumentException($"Resource not found: {resourceName}") : new Icon(stream);
    }

    private static Icon ConvertToGrayscaleIcon(Icon originalIcon)
    {
        // Convert Icon to Bitmap
        using var bitmap = originalIcon.ToBitmap();

        // Create a new bitmap for the grayscale image
        using var grayscaleBitmap = new Bitmap(bitmap.Width, bitmap.Height);

        // Create a graphics object to draw on the new bitmap
        using (var graphics = Graphics.FromImage(grayscaleBitmap))
        {
            // Create a grayscale color matrix
            float[][] matrixElements = [
            [0.299f, 0.299f, 0.299f, 0, 0], // Red
            [0.587f, 0.587f, 0.587f, 0, 0], // Green
            [0.114f, 0.114f, 0.114f, 0, 0], // Blue
            [0, 0, 0, 1, 0],               // Alpha
            [0, 0, 0, 0, 1]                // Translation
        ];

            var colorMatrix = new ColorMatrix(matrixElements);
            using var attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            // Draw the bitmap with the grayscale color matrix
            graphics.DrawImage(
                bitmap,
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                0, 0, bitmap.Width, bitmap.Height,
                GraphicsUnit.Pixel,
                attributes);
        }

        // Convert the grayscale bitmap back to an icon
        return Icon.FromHandle(grayscaleBitmap.GetHicon());
    }
}
