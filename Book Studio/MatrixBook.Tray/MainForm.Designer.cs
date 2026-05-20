namespace MatrixBook.Tray;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        notifyIcon = new NotifyIcon(components);
        contextMenuStrip = new ContextMenuStrip(components);
        statusStrip = new StatusStrip();
        listViewHistory = new ListView();
        lblHistory = new Label();
        SuspendLayout();
        // 
        // notifyIcon
        // 
        notifyIcon.Text = "notifyIcon";
        notifyIcon.Visible = true;
        // 
        // contextMenuStrip
        // 
        contextMenuStrip.ImageScalingSize = new Size(24, 24);
        contextMenuStrip.Name = "contextMenuStrip";
        contextMenuStrip.Size = new Size(61, 4);
        // 
        // statusStrip
        // 
        statusStrip.ImageScalingSize = new Size(24, 24);
        statusStrip.Location = new Point(0, 947);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(878, 28);
        statusStrip.TabIndex = 1;
        statusStrip.Text = "statusStrip1";
        // 
        // listViewHistory
        // 
        listViewHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listViewHistory.Location = new Point(12, 37);
        listViewHistory.Name = "listViewHistory";
        listViewHistory.Size = new Size(855, 905);
        listViewHistory.TabIndex = 2;
        listViewHistory.UseCompatibleStateImageBehavior = false;
        listViewHistory.View = View.Details;
        listViewHistory.VirtualListSize = 100;
        // 
        // lblHistory
        // 
        lblHistory.AutoSize = true;
        lblHistory.Location = new Point(12, 9);
        lblHistory.Name = "lblHistory";
        lblHistory.Size = new Size(158, 25);
        lblHistory.TabIndex = 3;
        lblHistory.Text = "Command History";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(878, 975);
        Controls.Add(lblHistory);
        Controls.Add(listViewHistory);
        Controls.Add(statusStrip);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "MainForm";
        Text = "Matrix Book";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private NotifyIcon notifyIcon;
    private ContextMenuStrip contextMenuStrip;
    private StatusStrip statusStrip;
    private ListView listViewHistory;
    private Label lblHistory;
}
