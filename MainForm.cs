namespace Shotcut;

public partial class MainForm : Form
{
    private TabControl _tabControl = null!;
    private Button _btnAdd = null!;
    private Button _btnEdit = null!;
    private Button _btnRemove = null!;
    private Button _btnToggle = null!;
    private Button _btnAddGroup = null!;
    private Button _btnRemoveGroup = null!;
    private Button _btnRenameGroup = null!;
    private Label _lblStatus = null!;
    private NotifyIcon _notifyIcon = null!;
    private List<ShortcutGroup> _groups = new();
    private bool _active;
    private int _nextId = 1;

    public MainForm()
    {
        InitializeComponent();
        BuildUI();
        LoadConfig();
    }

    private ShortcutGroup? CurrentGroup =>
        _tabControl.SelectedIndex >= 0 && _tabControl.SelectedIndex < _groups.Count
            ? _groups[_tabControl.SelectedIndex]
            : null;

    private ListView? CurrentListView =>
        _tabControl.SelectedTab?.Controls.Count > 0
            ? _tabControl.SelectedTab.Controls[0] as ListView
            : null;

    private void BuildUI()
    {
        Text = "Shotcut - Mover Imagens";
        Size = new Size(660, 460);
        MinimumSize = new Size(560, 400);
        StartPosition = FormStartPosition.CenterScreen;

        _tabControl = new TabControl
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Location = new Point(12, 12),
            Size = new Size(520, 350)
        };
        _tabControl.SelectedIndexChanged += (_, _) =>
        {
            if (_active)
            {
                UnregisterAll();
                RegisterAll();
            }
        };

        var btnX = 544;
        _btnAdd = new Button { Text = "Adicionar", Location = new Point(btnX, 12), Width = 96 };
        _btnEdit = new Button { Text = "Editar", Location = new Point(btnX, 44), Width = 96 };
        _btnRemove = new Button { Text = "Remover", Location = new Point(btnX, 76), Width = 96 };

        _btnToggle = new Button
        {
            Text = "Ativar",
            Location = new Point(btnX, 126),
            Width = 96,
            Height = 40,
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };

        _lblStatus = new Label
        {
            Text = "Inativo",
            Location = new Point(btnX, 174),
            Width = 96,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(Font.FontFamily, 9, FontStyle.Bold),
            ForeColor = Color.Gray
        };

        _btnAddGroup = new Button { Text = "+ Grupo", Location = new Point(btnX, 220), Width = 96 };
        _btnRenameGroup = new Button { Text = "Renomear", Location = new Point(btnX, 252), Width = 96 };
        _btnRemoveGroup = new Button { Text = "- Grupo", Location = new Point(btnX, 284), Width = 96 };

        _btnAdd.Click += BtnAdd_Click;
        _btnEdit.Click += BtnEdit_Click;
        _btnRemove.Click += BtnRemove_Click;
        _btnToggle.Click += BtnToggle_Click;
        _btnAddGroup.Click += BtnAddGroup_Click;
        _btnRemoveGroup.Click += BtnRemoveGroup_Click;
        _btnRenameGroup.Click += BtnRenameGroup_Click;

        Controls.AddRange(new Control[]
        {
            _tabControl, _btnAdd, _btnEdit, _btnRemove,
            _btnToggle, _lblStatus,
            _btnAddGroup, _btnRenameGroup, _btnRemoveGroup
        });

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Shotcut",
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) =>
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Abrir", null, (_, _) => { Show(); WindowState = FormWindowState.Normal; });
        contextMenu.Items.Add("Sair", null, (_, _) => { _notifyIcon.Visible = false; Application.Exit(); });
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void LoadConfig()
    {
        _groups = ConfigStore.Load();
        _nextId = _groups.SelectMany(g => g.Shortcuts).Select(s => s.Id).DefaultIfEmpty(0).Max() + 1;
        RebuildTabs();
    }

    private void SaveConfig() => ConfigStore.Save(_groups);

    private ListView CreateListView()
    {
        var lv = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Dock = DockStyle.Fill,
            CheckBoxes = true
        };
        lv.Columns.Add("Nome", 120);
        lv.Columns.Add("Atalho", 120);
        lv.Columns.Add("Pasta Destino", 260);
        lv.ItemChecked += ListView_ItemChecked;
        return lv;
    }

    private void RebuildTabs()
    {
        _tabControl.TabPages.Clear();
        foreach (var group in _groups)
        {
            var tab = new TabPage(group.Name);
            var lv = CreateListView();
            tab.Controls.Add(lv);
            _tabControl.TabPages.Add(tab);
            RefreshList(lv, group);
        }
    }

    private void RefreshList(ListView lv, ShortcutGroup group)
    {
        lv.ItemChecked -= ListView_ItemChecked;
        lv.Items.Clear();
        foreach (var sc in group.Shortcuts)
        {
            var item = new ListViewItem(sc.Label) { Tag = sc, Checked = sc.Enabled };
            item.SubItems.Add(sc.HotkeyDisplay);
            item.SubItems.Add(sc.DestinationPath);
            lv.Items.Add(item);
        }
        lv.ItemChecked += ListView_ItemChecked;
    }

    private void RefreshCurrentTab()
    {
        var group = CurrentGroup;
        var lv = CurrentListView;
        if (group != null && lv != null)
            RefreshList(lv, group);
    }

    private void ListView_ItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        if (e.Item?.Tag is ShortcutConfig sc)
        {
            sc.Enabled = e.Item.Checked;
            SaveConfig();
            if (_active)
            {
                UnregisterAll();
                RegisterAll();
            }
        }
    }

    private void BtnAddGroup_Click(object? sender, EventArgs e)
    {
        var name = PromptText("Novo Grupo", "Nome do grupo:");
        if (name == null) return;

        _groups.Add(new ShortcutGroup { Name = name });
        SaveConfig();
        RebuildTabs();
        _tabControl.SelectedIndex = _tabControl.TabCount - 1;
    }

    private void BtnRenameGroup_Click(object? sender, EventArgs e)
    {
        var group = CurrentGroup;
        if (group == null) return;

        var name = PromptText("Renomear Grupo", "Novo nome:", group.Name);
        if (name == null) return;

        group.Name = name;
        SaveConfig();
        _tabControl.SelectedTab!.Text = name;
    }

    private void BtnRemoveGroup_Click(object? sender, EventArgs e)
    {
        var group = CurrentGroup;
        if (group == null) return;

        if (MessageBox.Show($"Remover grupo '{group.Name}' e todos os seus atalhos?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        if (_active)
        {
            foreach (var sc in group.Shortcuts)
                UnregisterHotkey(sc);
        }

        _groups.Remove(group);
        SaveConfig();
        RebuildTabs();
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var group = CurrentGroup;
        if (group == null)
        {
            MessageBox.Show("Crie um grupo primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_active) UnregisterAll();
        using var form = new EditShortcutForm();
        if (form.ShowDialog() == DialogResult.OK && form.Result != null)
        {
            form.Result.Id = _nextId++;
            group.Shortcuts.Add(form.Result);
            SaveConfig();
            RefreshCurrentTab();
        }
        if (_active) RegisterAll();
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        var lv = CurrentListView;
        var group = CurrentGroup;
        if (lv == null || group == null || lv.SelectedItems.Count == 0) return;
        var selected = (ShortcutConfig)lv.SelectedItems[0].Tag!;

        if (_active) UnregisterAll();
        using var form = new EditShortcutForm(selected);
        if (form.ShowDialog() == DialogResult.OK && form.Result != null)
        {
            var idx = group.Shortcuts.IndexOf(selected);
            group.Shortcuts[idx] = form.Result;
            SaveConfig();
            RefreshCurrentTab();
        }
        if (_active) RegisterAll();
    }

    private void BtnRemove_Click(object? sender, EventArgs e)
    {
        var lv = CurrentListView;
        var group = CurrentGroup;
        if (lv == null || group == null || lv.SelectedItems.Count == 0) return;
        var selected = (ShortcutConfig)lv.SelectedItems[0].Tag!;

        if (MessageBox.Show($"Remover '{selected.Label}'?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            if (_active) UnregisterHotkey(selected);
            group.Shortcuts.Remove(selected);
            SaveConfig();
            RefreshCurrentTab();
        }
    }

    private void BtnToggle_Click(object? sender, EventArgs e)
    {
        _active = !_active;
        if (_active)
        {
            RegisterAll();
            _btnToggle.Text = "Desativar";
            _btnToggle.BackColor = Color.FromArgb(231, 76, 60);
            _lblStatus.Text = "Ativo";
            _lblStatus.ForeColor = Color.Green;
        }
        else
        {
            UnregisterAll();
            _btnToggle.Text = "Ativar";
            _btnToggle.BackColor = Color.FromArgb(46, 204, 113);
            _lblStatus.Text = "Inativo";
            _lblStatus.ForeColor = Color.Gray;
        }
    }

    private void RegisterAll()
    {
        var group = CurrentGroup;
        if (group == null) return;
        foreach (var sc in group.Shortcuts.Where(s => s.Enabled))
            RegisterHotkey(sc);
    }

    private void UnregisterAll()
    {
        foreach (var group in _groups)
            foreach (var sc in group.Shortcuts)
                GlobalHotkey.Unregister(Handle, sc.Id);
    }

    private void RegisterHotkey(ShortcutConfig sc)
    {
        if (!GlobalHotkey.Register(Handle, sc.Id, sc.Modifiers, sc.Key))
        {
            MessageBox.Show($"Não foi possível registrar o atalho '{sc.HotkeyDisplay}' ({sc.Label}). Pode estar em uso por outro programa.",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UnregisterHotkey(ShortcutConfig sc)
    {
        GlobalHotkey.Unregister(Handle, sc.Id);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == GlobalHotkey.WM_HOTKEY)
        {
            int id = m.WParam.ToInt32();
            var group = CurrentGroup;
            var sc = group?.Shortcuts.FirstOrDefault(s => s.Id == id);
            if (sc != null)
                HandleHotkey(sc);
        }
        base.WndProc(ref m);
    }

    private void HandleHotkey(ShortcutConfig sc)
    {
        var files = ExplorerHelper.GetSelectedFiles();
        var images = ExplorerHelper.FilterImages(files);

        if (images.Count == 0)
        {
            _notifyIcon.BalloonTipTitle = "Shotcut";
            _notifyIcon.BalloonTipText = "Nenhuma imagem selecionada no Explorer.";
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(2000);
            return;
        }

        int moved = 0;
        foreach (var img in images)
        {
            try
            {
                var fileName = Path.GetFileName(img);
                var dest = Path.Combine(sc.DestinationPath, fileName);

                if (File.Exists(dest))
                {
                    var nameNoExt = Path.GetFileNameWithoutExtension(fileName);
                    var ext = Path.GetExtension(fileName);
                    dest = Path.Combine(sc.DestinationPath, $"{nameNoExt}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}");
                }

                File.Move(img, dest);
                moved++;
            }
            catch
            {
            }
        }

        _notifyIcon.BalloonTipTitle = "Shotcut";
        _notifyIcon.BalloonTipText = $"{moved} imagem(ns) movida(s) para '{sc.Label}'.";
        _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        _notifyIcon.ShowBalloonTip(2000);
    }

    private static string? PromptText(string title, string label, string defaultValue = "")
    {
        var form = new Form
        {
            Text = title,
            Size = new Size(360, 150),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterParent
        };

        var lbl = new Label { Text = label, Location = new Point(12, 18), AutoSize = true };
        var txt = new TextBox { Text = defaultValue, Location = new Point(12, 42), Width = 320 };
        var btnOk = new Button { Text = "OK", Location = new Point(170, 75), Width = 75, DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Cancelar", Location = new Point(255, 75), Width = 75, DialogResult = DialogResult.Cancel };

        form.AcceptButton = btnOk;
        form.CancelButton = btnCancel;
        form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });

        return form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txt.Text)
            ? txt.Text.Trim()
            : null;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            _notifyIcon.BalloonTipTitle = "Shotcut";
            _notifyIcon.BalloonTipText = "Aplicação minimizada. Atalhos continuam ativos.";
            _notifyIcon.ShowBalloonTip(2000);
            return;
        }
        UnregisterAll();
        _notifyIcon.Visible = false;
        base.OnFormClosing(e);
    }
}
