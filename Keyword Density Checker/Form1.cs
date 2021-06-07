using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace Keyword_Density_Checker
{
    public partial class Form1 : Form
    {
        List<List<string>> listOutput = new List<List<string>>();
        int countPages = 1;

        public void showOutput()
        {
            List<List<string>> listOutputFinal = new List<List<string>>();
            dgvOutput.Rows.Clear();
            dgvOutput.Refresh();

            this.dgvOutput.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            for (int i = 0; i < listOutput[0].Count; i++)
            {
                listOutputFinal.Add(new List<string>());
                for (int j = 0; j < listOutput.Count; j++)
                {
                    listOutputFinal[i].Add(listOutput[j][i]);
                }
            }

            dgvOutput.ColumnCount = listOutputFinal[0].Count;
            for (int i = 0; i < listOutputFinal[0].Count; i++)
            {
                dgvOutput.Columns[i].HeaderText = listOutputFinal[0][i];
            }

            for (int i = 1; i < listOutputFinal.Count; i++)
            {
                dgvOutput.Rows.Add();
                for (int j = 0; j < listOutputFinal[0].Count; j++) 
                {
                    if (j >= 2)
                    {
                        if (j % 2 == 0)
                        {
                            dgvOutput.Columns[j].ValueType = typeof(int);
                        }
                        else
                        {
                            dgvOutput.Columns[j].ValueType = typeof(double);
                        }
                    }

                    dgvOutput.Rows[i-1].Cells[j].Value = listOutputFinal[i][j];
                }
            }
        }

        public Form1()
        {
            listOutput.Add(new List<string>());
            listOutput[0].Add("#");
            listOutput.Add(new List<string>());
            listOutput[1].Add("Word");

            InitializeComponent();

            #region DoubleBuffered dgvOutput
            typeof(DataGridView).InvokeMember(
            "DoubleBuffered",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
            null,
            dgvOutput,
            new object[] { true });
            #endregion
        }

        private void btnSource_Click(object sender, EventArgs e)
        {
            OpenFileDialog openSource = new OpenFileDialog();
            openSource.Title = "Open source text";
            
            if (openSource.ShowDialog() == DialogResult.OK)
            {
                string filePath = openSource.FileName;
                tbSource.Text = File.ReadAllText(filePath);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            tbSearchString.Text = tbSearchString.Text.Replace('\u00a0', '\u0020');
            tbSearchString.Text = tbSearchString.Text.Replace('\u2019', '\u0027');

            tbSource.Text = tbSource.Text.Replace('\u00a0', '\u0020');
            tbSource.Text = tbSource.Text.Replace('\u2019', '\u0027');

            string[] searchWords = tbSearchString.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var countAllWords = tbSource.Text.Split(new[] { " ", "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length;
            
            int nTerm = 2 + (countPages - 1) * 2; // nth term of arithmatic sequence (a1 = 1, d = 2)

            listOutput.Add(new List<string>());
            listOutput.Add(new List<string>());


            listOutput[nTerm].Add("Page "+countPages);

            listOutput[nTerm+1].Add("");

            dgvOutput.Rows.Clear();
            dgvOutput.Columns.Clear();
            dgvOutput.Refresh();

            dgvOutput.ColumnCount = listOutput.Count;

            Parallel.For(0, listOutput.Count, i => 
            {
                for (int j = 0; j < searchWords.Length; j++)
                {
                    if (countPages <= 1)
                    {
                        listOutput[i].Add("");
                    }
                    else if (i >= nTerm)
                    {
                        listOutput[i].Add("");
                    }
                }
            });

            Parallel.For(0, searchWords.Length, i =>
            {
                if (countPages <= 1)
                {
                    listOutput[0][i+1] = i.ToString();
                    listOutput[1][i + 1] = searchWords[i];
                }

                string regex = @"(?=(\b|\W|\A))" + Regex.Escape(searchWords[i]) + @"(?<=(\b|\W|\Z))";
                int countMatches = Regex.Matches(tbSource.Text, regex, RegexOptions.IgnoreCase).Count;

                listOutput[nTerm][i + 1] = countMatches.ToString();
                listOutput[nTerm + 1][i + 1] = ((float)countMatches / (float)countAllWords).ToString();
            });

            countPages++;

            if(countPages != 1)
            {
                tbSearchString.ReadOnly = true;
            }

            showOutput();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveCSV = new SaveFileDialog();
            saveCSV.Title = "Save CSV";
            saveCSV.DefaultExt = "csv";
            
            if (saveCSV.ShowDialog() == DialogResult.OK)
            {
                string finalCSV = "";

                for (int i = 0; i < listOutput[0].Count; i++)
                {
                    for (int j = 0; j < listOutput.Count; j++)
                    {
                        finalCSV += listOutput[j][i] + ",";
                    }
                    finalCSV += "\r\n";
                }

                string filePath = saveCSV.FileName;
                File.WriteAllText(filePath, finalCSV);
            }
        }

        private void btnClearPrev_Click(object sender, EventArgs e)
        {
            DialogResult confClear = MessageBox.Show("Are you sure you want to clear the last output?", "Clear Confirmation", MessageBoxButtons.YesNo);
            if (confClear == DialogResult.Yes)
            {
                if (countPages > 1)
                {
                    listOutput.Remove(listOutput.Last());
                    listOutput.Remove(listOutput.Last());
                    countPages--;
                    showOutput();
                }
            }
            else if (confClear == DialogResult.No)
            {

            }
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            DialogResult confClear = MessageBox.Show("Are you sure you want to clear all output?", "Clear Confirmation", MessageBoxButtons.YesNo);
            if (confClear == DialogResult.Yes)
            {
                countPages = 1;

                listOutput.Clear();

                listOutput.Add(new List<string>());
                listOutput[0].Add("#");
                listOutput.Add(new List<string>());
                listOutput[1].Add("Word");

                tbSearchString.ReadOnly = false;
                dgvOutput.Rows.Clear();
                dgvOutput.Columns.Clear();
                dgvOutput.Refresh();
            }
            else if (confClear == DialogResult.No)
            {
                
            }          
        }

        private void formClose_Click(object sender, FormClosingEventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Do you really want to close the program?", "Exit Confirmation", MessageBoxButtons.YesNo);
            if (dialog == DialogResult.Yes)
            {
                Application.ExitThread();
            }
            else if (dialog == DialogResult.No)
            {

            }
        }

        private void dgvOutput_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.ValueType == typeof(double))
            {
                double a = double.Parse(e.CellValue1.ToString()), b = double.Parse(e.CellValue2.ToString());

                e.SortResult = a.CompareTo(b);

                e.Handled = true;
            }

            if (e.Column.ValueType == typeof(int))
            {
                int a = int.Parse(e.CellValue1.ToString()), b = int.Parse(e.CellValue2.ToString());

                e.SortResult = a.CompareTo(b);

                e.Handled = true;
            }
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbFilter.Text))
            {
                Parallel.ForEach(dgvOutput.Rows.Cast<DataGridViewRow>(), row => {
                    dgvOutput.BeginInvoke(new Action(() =>
                    {
                        row.Visible = true;
                    }));
                });
            }
            else if (cbExact.Checked)
            {
                Parallel.ForEach(dgvOutput.Rows.Cast<DataGridViewRow>(), row => {
                    if (row.Cells[1].Value.ToString().ToLower().Equals(tbFilter.Text.ToLower()))
                    {
                        dgvOutput.BeginInvoke(new Action(() =>
                        {
                            row.Visible = true;
                        }));
                    }
                    else
                    {
                        dgvOutput.BeginInvoke(new Action(() =>
                        {
                            row.Visible = false;
                        }));
                    }
                });
            }
            else
            {
                Parallel.ForEach(dgvOutput.Rows.Cast<DataGridViewRow>(), row => {
                    if (row.Cells[1].Value.ToString().ToLower().Contains(tbFilter.Text.ToLower()))
                    {
                        dgvOutput.BeginInvoke(new Action(() =>
                        {
                            row.Visible = true;
                        }));
                    }
                    else
                    {
                        dgvOutput.BeginInvoke(new Action(() =>
                        {
                            row.Visible = false;
                        }));
                    }
                });
            }
        }

        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnFilter.PerformClick();

                e.SuppressKeyPress = true;
            }
        }
    }
}
