using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace diplom
{
    public partial class TestDesign : Form
    {
        private readonly Label[,] tableCells = new Label[4, 3];
        int studentId;
        public TestDesign(int studentId)
        {
            InitializeComponent();
            SetupDragAndDrop();
            this.studentId = studentId;

        }

        private void TestDesign_Load(object sender, EventArgs e)
        {
            
          
        }

        private int CalculateScoreEquivalencePartitioning(string input)
        {
            try
            {
                var classes = input.Split(';')
                    .Select(x => x.Trim().ToLower())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                if (classes.Count != 3) return 0;

                var expectedClasses = new HashSet<string> { "<1", "1-10", ">10" };
                var normalizedClasses = classes.Select(c =>
                    c.Contains("меньше 1") ? "<1" :
                    c.Contains("1-10") || c.Contains("1 до 10") ? "1-10" :
                    c.Contains("больше 10") ? ">10" : c).ToList();

                int correctCount = normalizedClasses.Count(c => expectedClasses.Contains(c));
                return correctCount == 3 ? 10 : correctCount == 2 ? 6 : 0;
            }
            catch
            {
                return 0;
            }
        }

        private int CalculateScorePairwise(string input)
        {
            try
            {
                var pairs = input.Split(';')
                    .Select(x => x.Trim().ToLower())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => x.Split(','))
                    .Where(x => x.Length == 2)
                    .Select(x => (x[0].Trim(), x[1].Trim()))
                    .ToList();

                if (pairs.Count != 4) return 0;

                var expectedPairs = new HashSet<(string, string)>
        {
            ("взрослый", "карта"), ("взрослый", "наличные"),
            ("детский", "карта"), ("детский", "наличные")
        };

                var inputPairs = new HashSet<(string, string)>(pairs);
                int coveredPairs = expectedPairs.Intersect(inputPairs).Count();
                return coveredPairs == 4 ? 10 : coveredPairs == 3 ? 7 : 0;
            }
            catch
            {
                return 0;
            }
        }

        private void SetupDragAndDrop()
        {
            // Настройка перетаскиваемых элементов
            string[] itemNames = { "item1", "item2", "item3", "item4", "item5", "item6" };
            foreach (string name in itemNames)
            {
                Label item = dragPanel.Controls.Find(name, true).FirstOrDefault() as Label;
                if (item != null)
                {
                    item.Tag = item.Text; // Храним текст для перетаскивания
                    item.MouseDown += (s, e) =>
                    {
                        item.DoDragDrop(item.Tag.ToString(), DragDropEffects.Move);
                    };
                }
            }

            // Настройка ячеек таблицы
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    string cellName = $"cell{row + 1}_{col + 1}";
                    Label cell = desicionTable.Controls.Find(cellName, true).FirstOrDefault() as Label;
                    if (cell != null)
                    {
                        cell.AllowDrop = true; // На случай, если не задано в дизайнере
                        cell.DragEnter += (s, e) =>
                        {
                            if (e.Data.GetDataPresent(DataFormats.StringFormat))
                                e.Effect = DragDropEffects.Move;
                        };
                        cell.DragDrop += (s, e) =>
                        {
                            cell.Text = e.Data.GetData(DataFormats.StringFormat).ToString();
                        };
                        tableCells[row, col] = cell;
                    }
                }
            }
        }

        private int CalculateScore()
        {
            var expectedTable = new[,]
            {
            { "Студент: Да", "Билетов > 5: Да", "Скидка: Да" },
            { "Студент: Да", "Билетов > 5: Нет", "Скидка: Нет" },
            { "Студент: Нет", "Билетов > 5: Да", "Скидка: Нет" },
            { "Студент: Нет", "Билетов > 5: Нет", "Скидка: Нет" }
        };

            int correctRows = 0;
            for (int row = 0; row < 4; row++)
            {
                bool rowCorrect = true;
                for (int col = 0; col < 3; col++)
                {
                    if (tableCells[row, col].Text != expectedTable[row, col])
                    {
                        rowCorrect = false;
                        break;
                    }
                }
                if (rowCorrect)
                    correctRows++;
            }

            if (correctRows == 4)
                return 10;
            else if (correctRows == 3)
                return 7;
            else if (correctRows == 2)
                return 4;
            else
                return 0;
        }

        private void endBtn_Click(object sender, EventArgs e)
        {
            int score1 = CalculateScoreEquivalencePartitioning(answerBox1.Text);
            int score2 = CalculateScorePairwise(answerBox2.Text);
            int score3 = CalculateScore();
            int result = score1 + score2 + score3;
            try
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = Connection.conn;
                    cmd.CommandText = @"
                    INSERT INTO test_results (student_id, test_id, score)
                    VALUES (@student_id, @test_id, @score)";
                    cmd.Parameters.AddWithValue("student_id", studentId);
                    cmd.Parameters.AddWithValue("test_id", 6);
                    cmd.Parameters.AddWithValue("score", result);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show($"Твое количество баллов: {result} из 30", "Отправка ответов успешна", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
