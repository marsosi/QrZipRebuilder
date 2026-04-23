namespace QrZipRebuilder;

public sealed partial class MainForm
{
    private CancellationTokenSource? _runCts;

    public MainForm()
    {
        InitializeComponent();
        DoubleBuffered = true;
        _split.Resize += Split_Resize;
    }

    private void Split_Resize(object? sender, EventArgs e) => AjustarSplitterDentroDosLimites();

    private void AplicarPosicaoInicialDoSplitter()
    {
        if (_split.Height <= 0) return;
        var max = _split.Height - _split.SplitterWidth - _split.Panel2MinSize;
        var min = _split.Panel1MinSize;
        if (max < min) return;
        var ideal = (int)(_split.ClientSize.Height * 0.38);
        _split.SplitterDistance = Math.Clamp(ideal, min, max);
    }

    private void AjustarSplitterDentroDosLimites()
    {
        if (_split.IsDisposed) return;
        if (_split.Height < _split.Panel1MinSize + _split.SplitterWidth + _split.Panel2MinSize) return;
        var max = _split.Height - _split.SplitterWidth - _split.Panel2MinSize;
        var min = _split.Panel1MinSize;
        if (max < min) return;
        var d = _split.SplitterDistance;
        if (d < min || d > max)
            _split.SplitterDistance = Math.Clamp(d, min, max);
    }

    private void AtualizarGrelha()
    {
        _grid.DataSource = null;
        var path = _folderPath.Text.Trim();
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            return;
        var rows = QrRebuilder.ListarImagensOrdenadas(path);
        _grid.DataSource = new BindingSource(rows, null);
    }

    private void Browse_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            UseDescriptionForTitle = true,
            Description = "Select the folder with photos of the QR codes (file names with numbers, e.g. qrcode_001.jpg).",
            ShowNewFolderButton = true
        };

        if (Directory.Exists(_folderPath.Text.Trim()))
            dlg.InitialDirectory = _folderPath.Text.Trim();

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _folderPath.Text = dlg.SelectedPath;
            AtualizarGrelha();
        }
    }

    private void FolderPath_FinalizarEdicao()
    {
        if (!IsDisposed)
            AtualizarGrelha();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _folderPath.Leave += (_, _) => FolderPath_FinalizarEdicao();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        AplicarPosicaoInicialDoSplitter();
    }

    private void AppendLogLinha(string line)
    {
        if (IsDisposed) return;
        if (InvokeRequired)
        {
            try
            {
                if (!IsHandleCreated) return;
                BeginInvoke(new MethodInvoker(() => AppendLogLinha(line)));
            }
            catch (ObjectDisposedException)
            {
            }

            return;
        }

        _log.AppendText(line + Environment.NewLine);
        _log.SelectionStart = _log.TextLength;
        _log.ScrollToCaret();
    }

    private async void Rebuild_Click(object? sender, EventArgs e)
    {
        var path = _folderPath.Text.Trim();
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            MessageBox.Show(this, "Choose a folder that exists and contains images.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _rebuild.Enabled = false;
        _browse.Enabled = false;
        _log.Clear();
        _progress.MarqueeAnimationSpeed = 30;
        _progress.Visible = true;

        var cts = new CancellationTokenSource();
        _runCts = cts;
        IProgress<string> progress = new Progress<string>(AppendLogLinha);

        try
        {
            var result = await Task.Run(
                () => QrRebuilder.RebuildAsync(path, progress, cts.Token),
                cts.Token);

            if (result.Success)
            {
                MessageBox.Show(
                    this,
                    $"File created successfully:\n{result.OutputPath}",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    AppendLogLinha("Error: " + result.ErrorMessage);
                MessageBox.Show(this, result.ErrorMessage ?? "Reconstruction failed.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (OperationCanceledException)
        {
            AppendLogLinha("Cancelled.");
        }
        catch (Exception ex)
        {
            AppendLogLinha("Exception: " + ex);
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _progress.MarqueeAnimationSpeed = 0;
            _progress.Visible = false;
            _rebuild.Enabled = true;
            _browse.Enabled = true;
            cts.Dispose();
        }
    }
}
