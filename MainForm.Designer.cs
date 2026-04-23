using System.Drawing;
using System.Windows.Forms;

namespace QrZipRebuilder;

public sealed partial class MainForm : Form
{
    private TableLayoutPanel _tableLayoutTop = null!;
    private Label _labelFolder = null!;
    private TextBox _folderPath = null!;
    private Button _browse = null!;
    private Button _rebuild = null!;
    private ProgressBar _progress = null!;
    private Label _labelGrid = null!;
    private SplitContainer _split = null!;
    private DataGridView _grid = null!;
    private TextBox _log = null!;

    private void InitializeComponent()
    {
        _tableLayoutTop = new TableLayoutPanel();
        _labelFolder = new Label();
        _folderPath = new TextBox();
        _browse = new Button();
        _rebuild = new Button();
        _progress = new ProgressBar();
        _labelGrid = new Label();
        _split = new SplitContainer();
        _grid = new DataGridView();
        _log = new TextBox();
        _tableLayoutTop.SuspendLayout();
        _split.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
        SuspendLayout();

        _tableLayoutTop.ColumnCount = 2;
        _tableLayoutTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tableLayoutTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _tableLayoutTop.Dock = DockStyle.Top;
        _tableLayoutTop.Padding = new Padding(16, 12, 16, 8);
        _tableLayoutTop.RowCount = 4;
        _tableLayoutTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _tableLayoutTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _tableLayoutTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _tableLayoutTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _tableLayoutTop.Size = new Size(900, 128);
        _tableLayoutTop.TabIndex = 0;

        _labelFolder.AutoSize = true;
        _labelFolder.Dock = DockStyle.Fill;
        _labelFolder.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        _labelFolder.ForeColor = Color.FromArgb(32, 32, 32);
        _labelFolder.Margin = new Padding(0, 0, 0, 4);
        _labelFolder.Text = "Folder with QR code photos:";

        _folderPath.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        _folderPath.Font = new Font("Segoe UI", 9.25F);
        _folderPath.Margin = new Padding(0, 0, 8, 0);
        _folderPath.PlaceholderText = "Select a folder (e.g. qrcode_001.jpg, qrcode_010.jpg …)";
        _folderPath.TabIndex = 0;

        _browse.AutoSize = true;
        _browse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _browse.Margin = new Padding(0);
        _browse.MinimumSize = new Size(100, 30);
        _browse.TabIndex = 1;
        _browse.Text = "Browse…";
        _browse.UseVisualStyleBackColor = true;
        _browse.Click += Browse_Click;

        _rebuild.AutoSize = true;
        _rebuild.Font = new Font("Segoe UI", 9F);
        _rebuild.Margin = new Padding(0, 8, 0, 0);
        _rebuild.MinimumSize = new Size(260, 32);
        _rebuild.TabIndex = 2;
        _rebuild.Text = "Rebuild codigo_recuperado.zip";
        _rebuild.UseVisualStyleBackColor = true;
        _rebuild.Click += Rebuild_Click;

        _progress.Anchor = AnchorStyles.Right;
        _progress.Margin = new Padding(0, 8, 0, 0);
        _progress.MarqueeAnimationSpeed = 0;
        _progress.Size = new Size(220, 18);
        _progress.Style = ProgressBarStyle.Marquee;
        _progress.TabIndex = 3;
        _progress.Visible = false;

        _labelGrid.AutoSize = true;
        _labelGrid.Dock = DockStyle.Fill;
        _labelGrid.Font = new Font("Segoe UI", 9F);
        _labelGrid.ForeColor = Color.FromArgb(32, 32, 32);
        _labelGrid.Margin = new Padding(0, 4, 0, 0);
        _labelGrid.Text = "Images (processing order):";

        _tableLayoutTop.SetColumnSpan(_labelFolder, 2);
        _tableLayoutTop.Controls.Add(_labelFolder, 0, 0);
        _tableLayoutTop.Controls.Add(_folderPath, 0, 1);
        _tableLayoutTop.Controls.Add(_browse, 1, 1);
        _tableLayoutTop.Controls.Add(_rebuild, 0, 2);
        _tableLayoutTop.Controls.Add(_progress, 1, 2);
        _tableLayoutTop.SetColumnSpan(_labelGrid, 2);
        _tableLayoutTop.Controls.Add(_labelGrid, 0, 3);

        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(32, 32, 32);
        _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 10, 10, 10);
        _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 240, 240);
        _grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _grid.ColumnHeadersHeight = 42;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        DataGridViewTextBoxColumn colOrdem = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Ordem",
            FillWeight = 8F,
            HeaderText = "No.",
            MinimumWidth = 50,
            Name = "colOrdem"
        };
        DataGridViewTextBoxColumn colNome = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "NomeFicheiro",
            FillWeight = 45F,
            HeaderText = "File name",
            MinimumWidth = 120,
            Name = "colNome"
        };
        DataGridViewTextBoxColumn colTamanho = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Tamanho",
            FillWeight = 15F,
            HeaderText = "Size",
            MinimumWidth = 80,
            Name = "colTamanho"
        };
        DataGridViewTextBoxColumn colChave = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ChaveOrdenacao",
            FillWeight = 32F,
            HeaderText = "Name digits (sort key)",
            MinimumWidth = 120,
            Name = "colChave"
        };
        _grid.Columns.AddRange(colOrdem, colNome, colTamanho, colChave);
        _grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.25F);
        _grid.DefaultCellStyle.ForeColor = Color.FromArgb(32, 32, 32);
        _grid.DefaultCellStyle.Padding = new Padding(10, 8, 10, 8);
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
        _grid.DefaultCellStyle.SelectionForeColor = Color.White;
        _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _grid.Dock = DockStyle.Fill;
        _grid.EnableHeadersVisualStyles = false;
        _grid.GridColor = Color.FromArgb(220, 220, 220);
        _grid.MultiSelect = false;
        _grid.ReadOnly = true;
        _grid.RowHeadersVisible = false;
        _grid.RowTemplate.MinimumHeight = 28;
        _grid.RowTemplate.Height = 32;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.TabIndex = 0;
        _grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

        _log.BackColor = Color.FromArgb(250, 250, 250);
        _log.BorderStyle = BorderStyle.FixedSingle;
        _log.Dock = DockStyle.Fill;
        _log.Font = new Font("Consolas", 9.25F);
        _log.Multiline = true;
        _log.ReadOnly = true;
        _log.ScrollBars = ScrollBars.Both;
        _log.TabIndex = 1;
        _log.TabStop = false;
        _log.WordWrap = false;

        _split.BackColor = Color.FromArgb(230, 230, 230);
        _split.Dock = DockStyle.Fill;
        _split.Orientation = Orientation.Horizontal;
        _split.Panel1.BackColor = SystemColors.Window;
        _split.Panel1.Controls.Add(_grid);
        _split.Panel1MinSize = 100;
        _split.Panel2.BackColor = Color.FromArgb(245, 245, 245);
        _split.Panel2.Controls.Add(_log);
        _split.Panel2.Padding = new Padding(12, 0, 12, 12);
        _split.Panel2MinSize = 100;
        _split.Size = new Size(900, 430);
        _split.SplitterWidth = 5;
        _split.TabIndex = 1;
        _split.TabStop = false;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 245, 245);
        ClientSize = new Size(900, 580);
        Controls.Add(_split);
        Controls.Add(_tableLayoutTop);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(720, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "QR to ZIP Rebuilder";
        _tableLayoutTop.ResumeLayout(false);
        _tableLayoutTop.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
        _split.ResumeLayout(true);
        ResumeLayout(true);
    }
}
