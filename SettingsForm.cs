using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.IO;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        public class SettingsForm : Form
        {
            private ListBox lbAvailable;
            private ListBox lbSelected;
            private Button btnAdd;
            private Button btnRemove;
            private Button btnMoveUp;
            private Button btnMoveDown;
            private Button btnOK;
            private Button btnCancel;
            private Label lblInfo;
            private Label lblScanning;

            private MusicBeeApiInterface mbApi;
            private Dictionary<MetaDataType, string> fieldNames;
            private Dictionary<MetaDataType, int> fieldDataCounts;
            private List<MetaDataType> selectedFields;
            private string settingsPath;

            public List<MetaDataType> SelectedFields
            {
                get { return selectedFields; }
            }

            public SettingsForm(MusicBeeApiInterface api, string storagePath, List<MetaDataType> currentFields)
            {
                mbApi = api;
                settingsPath = Path.Combine(storagePath, "MusicBeeQuickTag_Fields.txt");
                selectedFields = new List<MetaDataType>(currentFields);
                fieldNames = new Dictionary<MetaDataType, string>();
                fieldDataCounts = new Dictionary<MetaDataType, int>();
                
                InitializeComponent();
                ScanLibraryFields();
                PopulateFieldLists();
            }

            private void InitializeComponent()
            {
                this.Text = Localization.Get("SettingsTitle");
                this.Size = new Size(800, 600);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;

                // Main Layout
                TableLayoutPanel mainLayout = new TableLayoutPanel();
                mainLayout.Dock = DockStyle.Fill;
                mainLayout.ColumnCount = 3;
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
                mainLayout.RowCount = 3;
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // Top Info
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Lists
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Bottom Buttons
                mainLayout.Padding = new Padding(10);
                this.Controls.Add(mainLayout);

                // Info label
                lblInfo = new Label();
                lblInfo.Text = Localization.Get("SettingsInfo");
                lblInfo.Dock = DockStyle.Fill;
                lblInfo.TextAlign = ContentAlignment.MiddleLeft;
                mainLayout.Controls.Add(lblInfo, 0, 0);
                mainLayout.SetColumnSpan(lblInfo, 3);

                // Left Panel (Available)
                Panel pnlLeft = new Panel();
                pnlLeft.Dock = DockStyle.Fill;
                
                Label lblLeft = new Label();
                lblLeft.Text = Localization.Get("AvailableFields");
                lblLeft.Dock = DockStyle.Top;
                lblLeft.Height = 25;
                pnlLeft.Controls.Add(lblLeft);

                lbAvailable = new ListBox();
                lbAvailable.Dock = DockStyle.Fill;
                lbAvailable.SelectionMode = SelectionMode.MultiExtended;
                lbAvailable.Font = new Font(this.Font.FontFamily, 10);
                lbAvailable.IntegralHeight = false; // Prevent resizing issues
                pnlLeft.Controls.Add(lbAvailable);
                lbAvailable.BringToFront(); // Ensure it's not covered

                mainLayout.Controls.Add(pnlLeft, 0, 1);

                // Center Panel (Buttons)
                Panel pnlCenter = new Panel();
                pnlCenter.Dock = DockStyle.Fill;

                // Spacer to match label height of other panels
                Panel pnlCenterTopSpacer = new Panel();
                pnlCenterTopSpacer.Dock = DockStyle.Top;
                pnlCenterTopSpacer.Height = 25;
                pnlCenter.Controls.Add(pnlCenterTopSpacer);

                TableLayoutPanel tlpCenterButtons = new TableLayoutPanel();
                tlpCenterButtons.Dock = DockStyle.Fill;
                tlpCenterButtons.RowCount = 4;
                tlpCenterButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                tlpCenterButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpCenterButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpCenterButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                pnlCenter.Controls.Add(tlpCenterButtons);
                tlpCenterButtons.BringToFront();
                
                // Add/Remove Buttons
                btnAdd = new Button();
                btnAdd.Text = ">>"; // Add to Selected
                btnAdd.Size = new Size(60, 30);
                btnAdd.Anchor = AnchorStyles.None;
                btnAdd.Margin = new Padding(0, 0, 0, 5);
                btnAdd.Click += BtnAdd_Click;
                tlpCenterButtons.Controls.Add(btnAdd, 0, 1);

                btnRemove = new Button();
                btnRemove.Text = "<<"; // Remove from Selected
                btnRemove.Size = new Size(60, 30);
                btnRemove.Anchor = AnchorStyles.None;
                btnRemove.Margin = new Padding(0, 5, 0, 0);
                btnRemove.Click += BtnRemove_Click;
                tlpCenterButtons.Controls.Add(btnRemove, 0, 2);

                mainLayout.Controls.Add(pnlCenter, 1, 1);

                // Right Panel (Selected + Sort)
                Panel pnlRight = new Panel();
                pnlRight.Dock = DockStyle.Fill;

                Label lblRight = new Label();
                lblRight.Text = Localization.Get("SelectedFields");
                lblRight.Dock = DockStyle.Top;
                lblRight.Height = 25;
                pnlRight.Controls.Add(lblRight);

                // Container for List and Sort Buttons
                TableLayoutPanel rightInnerLayout = new TableLayoutPanel();
                rightInnerLayout.Dock = DockStyle.Fill;
                rightInnerLayout.ColumnCount = 2;
                rightInnerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                rightInnerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
                rightInnerLayout.RowCount = 1;
                rightInnerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                pnlRight.Controls.Add(rightInnerLayout);
                rightInnerLayout.BringToFront();

                lbSelected = new ListBox();
                lbSelected.Dock = DockStyle.Fill;
                lbSelected.SelectionMode = SelectionMode.MultiExtended;
                lbSelected.Font = new Font(this.Font.FontFamily, 10);
                lbSelected.IntegralHeight = false;
                rightInnerLayout.Controls.Add(lbSelected, 0, 0);

                // Sort Buttons Panel
                TableLayoutPanel pnlSort = new TableLayoutPanel();
                pnlSort.Dock = DockStyle.Fill;
                pnlSort.RowCount = 4;
                pnlSort.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                pnlSort.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                pnlSort.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                pnlSort.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                
                btnMoveUp = new Button();
                btnMoveUp.Text = Localization.Get("MoveUp");
                btnMoveUp.Size = new Size(50, 30);
                btnMoveUp.Anchor = AnchorStyles.None;
                btnMoveUp.Margin = new Padding(0, 0, 0, 5);
                btnMoveUp.Click += BtnMoveUp_Click;
                pnlSort.Controls.Add(btnMoveUp, 0, 1);

                btnMoveDown = new Button();
                btnMoveDown.Text = Localization.Get("MoveDown");
                btnMoveDown.Size = new Size(50, 30);
                btnMoveDown.Anchor = AnchorStyles.None;
                btnMoveDown.Margin = new Padding(0, 5, 0, 0);
                btnMoveDown.Click += BtnMoveDown_Click;
                pnlSort.Controls.Add(btnMoveDown, 0, 2);

                rightInnerLayout.Controls.Add(pnlSort, 1, 0);

                mainLayout.Controls.Add(pnlRight, 2, 1);

                // Bottom Panel (OK/Cancel)
                FlowLayoutPanel bottomPanel = new FlowLayoutPanel();
                bottomPanel.Dock = DockStyle.Fill;
                bottomPanel.FlowDirection = FlowDirection.RightToLeft;
                bottomPanel.Padding = new Padding(0, 10, 10, 0);
                
                btnCancel = new Button();
                btnCancel.Text = Localization.Get("Cancel");
                btnCancel.Size = new Size(90, 30);
                btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
                bottomPanel.Controls.Add(btnCancel);

                btnOK = new Button();
                btnOK.Text = Localization.Get("OK");
                btnOK.Size = new Size(90, 30);
                btnOK.Click += BtnOK_Click;
                bottomPanel.Controls.Add(btnOK);

                mainLayout.Controls.Add(bottomPanel, 0, 2);
                mainLayout.SetColumnSpan(bottomPanel, 3);

                // Scanning label (Overlay)
                lblScanning = new Label();
                lblScanning.Text = Localization.Get("Scanning");
                lblScanning.Dock = DockStyle.Fill;
                lblScanning.TextAlign = ContentAlignment.MiddleCenter;
                lblScanning.Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold);
                lblScanning.ForeColor = Color.DarkBlue;
                lblScanning.BackColor = SystemColors.Control;
                this.Controls.Add(lblScanning);
                lblScanning.BringToFront();
            }

            private void ScanLibraryFields()
            {
                Application.DoEvents(); // Update UI

                // 1. Get all possible fields dynamically
                List<MetaDataType> fieldsToScan = new List<MetaDataType>();
                foreach (MetaDataType field in Enum.GetValues(typeof(MetaDataType)))
                {
                    // Exclude specific fields based on user request and performance
                    if (field == MetaDataType.Artist || 
                        field == MetaDataType.TrackTitle || 
                        field == MetaDataType.Year ||
                        field == MetaDataType.Artwork ||
                        field == MetaDataType.Lyrics)
                    {
                        continue;
                    }
                    
                    fieldsToScan.Add(field);
                }

                MetaDataType[] fieldsArray = fieldsToScan.ToArray();

                // 2. Scan library using batch retrieval
                string[] allFiles;
                if (mbApi.Library_QueryFilesEx("", out allFiles))
                {
                    // Prepare storage for unique values
                    Dictionary<MetaDataType, HashSet<string>> fieldUniqueValues = new Dictionary<MetaDataType, HashSet<string>>();
                    foreach(var field in fieldsToScan)
                    {
                        fieldUniqueValues[field] = new HashSet<string>();
                    }

                    // Single pass scan
                    foreach (string file in allFiles)
                    {
                        string[] tagValues;
                        if (mbApi.Library_GetFileTags(file, fieldsArray, out tagValues))
                        {
                            for (int i = 0; i < fieldsArray.Length; i++)
                            {
                                string value = tagValues[i];
                                if (!string.IsNullOrWhiteSpace(value))
                                {
                                    fieldUniqueValues[fieldsArray[i]].Add(value);
                                }
                            }
                        }
                    }

                    // 3. Populate results
                    foreach (var kvp in fieldUniqueValues)
                    {
                        if (kvp.Value.Count > 0)
                        {
                            MetaDataType field = kvp.Key;
                            string fieldName = mbApi.Setting_GetFieldName(field);
                            if (string.IsNullOrEmpty(fieldName))
                            {
                                fieldName = field.ToString();
                            }
                            
                            fieldNames[field] = fieldName;
                            fieldDataCounts[field] = kvp.Value.Count;
                        }
                    }
                }
            }

            private void PopulateFieldLists()
            {
                lblScanning.Visible = false;

                lbAvailable.Items.Clear();
                lbSelected.Items.Clear();

                // 1. Add selected fields in order
                foreach (var field in selectedFields)
                {
                    if (fieldNames.ContainsKey(field))
                    {
                        string displayText = string.Format("{0} ({1})", fieldNames[field], fieldDataCounts[field]);
                        lbSelected.Items.Add(new FieldItem { Field = field, DisplayText = displayText });
                    }
                }

                // 2. Add remaining fields to available
                var sortedFields = fieldNames.OrderBy(kvp => kvp.Value).ToList();
                foreach (var kvp in sortedFields)
                {
                    if (!selectedFields.Contains(kvp.Key))
                    {
                         string displayText = string.Format("{0} ({1})", kvp.Value, fieldDataCounts[kvp.Key]);
                         lbAvailable.Items.Add(new FieldItem { Field = kvp.Key, DisplayText = displayText });
                    }
                }
            }

            private void BtnAdd_Click(object sender, EventArgs e)
            {
                MoveItems(lbAvailable, lbSelected);
            }

            private void BtnRemove_Click(object sender, EventArgs e)
            {
                MoveItems(lbSelected, lbAvailable);
            }

            private void MoveItems(ListBox source, ListBox dest)
            {
                List<object> itemsToRemove = new List<object>();
                foreach (var item in source.SelectedItems)
                {
                    dest.Items.Add(item);
                    itemsToRemove.Add(item);
                }
                
                foreach (var item in itemsToRemove)
                {
                    source.Items.Remove(item);
                }
            }

            private void BtnMoveUp_Click(object sender, EventArgs e)
            {
                MoveItem(-1);
            }

            private void BtnMoveDown_Click(object sender, EventArgs e)
            {
                MoveItem(1);
            }

            private void MoveItem(int direction)
            {
                if (lbSelected.SelectedItem == null || lbSelected.SelectedItems.Count > 1) return;
                
                int newIndex = lbSelected.SelectedIndex + direction;
                if (newIndex < 0 || newIndex >= lbSelected.Items.Count) return;

                object selected = lbSelected.SelectedItem;
                lbSelected.Items.Remove(selected);
                lbSelected.Items.Insert(newIndex, selected);
                lbSelected.SelectedIndex = newIndex;
            }

            private void BtnOK_Click(object sender, EventArgs e)
            {
                selectedFields.Clear();
                foreach (FieldItem item in lbSelected.Items)
                {
                    selectedFields.Add(item.Field);
                }

                if (selectedFields.Count == 0)
                {
                    MessageBox.Show(Localization.Get("SelectAtLeastOne"), Localization.Get("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save settings
                try
                {
                    List<string> lines = new List<string>();
                    foreach (var field in selectedFields)
                    {
                        lines.Add(((int)field).ToString());
                    }
                    File.WriteAllLines(settingsPath, lines);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Localization.Get("SaveFailed") + ex.Message, Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            private class FieldItem
            {
                public MetaDataType Field { get; set; }
                public string DisplayText { get; set; }

                public override string ToString()
                {
                    return DisplayText;
                }
            }

            public static List<MetaDataType> LoadSettings(string storagePath)
            {
                string settingsPath = Path.Combine(storagePath, "MusicBeeQuickTag_Fields.txt");
                List<MetaDataType> fields = new List<MetaDataType>();

                if (File.Exists(settingsPath))
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(settingsPath);
                        foreach (string line in lines)
                        {
                            int fieldValue;
                            if (int.TryParse(line.Trim(), out fieldValue))
                            {
                                fields.Add((MetaDataType)fieldValue);
                            }
                        }
                    }
                    catch { }
                }

                // Default fields if no settings found
                if (fields.Count == 0)
                {
                    fields.Add(MetaDataType.Grouping);
                    fields.Add(MetaDataType.Genre);
                    fields.Add(MetaDataType.Mood);
                }

                return fields;
            }
        }
    }
}
