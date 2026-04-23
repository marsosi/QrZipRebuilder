namespace QrZipRebuilder;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.ThreadException += (_, e) =>
        {
            MessageBox.Show(
                e.Exception?.Message ?? "Erro desconhecido",
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        };
        Application.Run(new MainForm());
    }
}
