namespace Shotcut;

public class EditShortcutForm : Form
{
    private readonly TextBox _txtLabel;
    private readonly HotkeyTextBox _txtHotkey;
    private readonly TextBox _txtPath;
    private readonly Button _btnBrowse;
    private readonly Button _btnOk;
    private readonly Button _btnCancel;

    public ShortcutConfig? Result { get; private set; }

    public EditShortcutForm(ShortcutConfig? existing = null)
    {
        Text = existing == null ? "Novo Atalho" : "Editar Atalho";
        Size = new Size(500, 220);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var lblLabel = new Label { Text = "Nome:", Location = new Point(12, 18), AutoSize = true };
        _txtLabel = new TextBox { Location = new Point(120, 15), Width = 340 };

        var lblHotkey = new Label { Text = "Atalho:", Location = new Point(12, 52), AutoSize = true };
        _txtHotkey = new HotkeyTextBox { Location = new Point(120, 49), Width = 340 };

        var lblPath = new Label { Text = "Pasta destino:", Location = new Point(12, 86), AutoSize = true };
        _txtPath = new TextBox { Location = new Point(120, 83), Width = 290 };
        _btnBrowse = new Button { Text = "...", Location = new Point(416, 82), Width = 44 };

        _btnOk = new Button { Text = "OK", Location = new Point(300, 130), Width = 80, DialogResult = DialogResult.OK };
        _btnCancel = new Button { Text = "Cancelar", Location = new Point(386, 130), Width = 80, DialogResult = DialogResult.Cancel };

        AcceptButton = _btnOk;
        CancelButton = _btnCancel;

        _btnBrowse.Click += (_, _) =>
        {
            using var dlg = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(_txtPath.Text))
                dlg.InitialDirectory = _txtPath.Text;
            if (dlg.ShowDialog() == DialogResult.OK)
                _txtPath.Text = dlg.SelectedPath;
        };

        _btnOk.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtLabel.Text))
            {
                MessageBox.Show("Informe um nome.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            if (_txtHotkey.HotKey == Keys.None)
            {
                MessageBox.Show("Defina um atalho.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            if (string.IsNullOrWhiteSpace(_txtPath.Text) || !Directory.Exists(_txtPath.Text))
            {
                MessageBox.Show("Informe uma pasta de destino válida.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            Result = new ShortcutConfig
            {
                Id = existing?.Id ?? 0,
                Label = _txtLabel.Text.Trim(),
                Key = _txtHotkey.HotKey,
                Modifiers = _txtHotkey.HotKeyModifiers,
                DestinationPath = _txtPath.Text.Trim(),
                Enabled = existing?.Enabled ?? true
            };
        };

        Controls.AddRange(new Control[] { lblLabel, _txtLabel, lblHotkey, _txtHotkey, lblPath, _txtPath, _btnBrowse, _btnOk, _btnCancel });

        if (existing != null)
        {
            _txtLabel.Text = existing.Label;
            _txtHotkey.SetHotkey(existing.Modifiers, existing.Key);
            _txtPath.Text = existing.DestinationPath;
        }
    }
}
