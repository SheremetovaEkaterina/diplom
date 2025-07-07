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
    public partial class BugReport : Form
    {
        bool isClick = false;
        int studentId;
        public BugReport(int studentId)
        {
            InitializeComponent();
            this.studentId = studentId;
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            isClick = true;
            confirmButton.BackColor = Color.DarkGray;
            confirmButton.Enabled = false;
            nameBox.Enabled = false;
            quantityBox.Enabled = false;
        }

        private void nameBox_Click(object sender, EventArgs e)
        {
            nameBox.BackColor = Color.DarkGray;
            nameBox.Enabled = false;
            confirmButton.Enabled = false;
            quantityBox.Enabled = false;
        }

        private void quantityBox_Click(object sender, EventArgs e)
        {
            quantityBox.BackColor = Color.DarkGray;
            confirmButton.Enabled = false;
            quantityBox.Enabled = false;
            nameBox.Enabled = false;
        }
        

        private int CalculateScore()
        {

            int score = 0;
            if (isClick)
                score += 4;
            if (comboBox1.SelectedItem?.ToString() == "кнопку Подтвердить")
                score += 2;
            if (comboBox2.SelectedItem?.ToString() == "пустым")
                score += 2;
            if (comboBox3.SelectedItem?.ToString() == "принимает")
                score += 2;

            if (score >= 4)
                return score;
            else return 0;
            
        }

        private int CalculateScore2()
        {
            string input = answerBox2.Text.Trim();
            string correctSequence = "2461";

            // Проверка формата
            if (input.Length != 4 || !input.All(char.IsDigit) || input.Distinct().Count() != 4 ||
                !input.All(c => c == '1' || c == '2' || c == '4' || c == '6'))
            {
                return 0;
            }

            // Проверка точного совпадения
            if (input == correctSequence)
            {
                return 10;
            }

            // Подсчёт правильных позиций
            int correctPositions = 0;
            for (int i = 0; i < 4; i++)
            {
                if (input[i] == correctSequence[i])
                {
                    correctPositions++;
                }
            }

            // Частично правильный ответ
            if (correctPositions >= 1)
            {
                return 7; // Например, "4261" или "2146"
            }
            else
            {
                return 4; // Например, "1246"
            }
        }

        private int CalculateScore3()
        {
            double score = 0;

            // Проверка Дефект 1: Тип (UI)
            if (defect1TypeUI != null && defect1TypeUI.Checked)
                score += 2.5;
            // Проверка Дефект 1: Приоритет (Высокий)
            if (defect1PriorityHigh != null && defect1PriorityHigh.Checked)
                score += 2.5;

            // Проверка Дефект 2: Тип (Производительность)
            if (defect2TypePerformance != null && defect2TypePerformance.Checked)
                score += 2.5;
            // Проверка Дефект 2: Приоритет (Средний)
            if (defect2PriorityMedium != null && defect2PriorityMedium.Checked)
                score += 2.5;

            // Проверка, что все ответы выбраны
            bool anyDefect1TypeChecked = (defect1TypeFunctional?.Checked ?? false) ||
                                         (defect1TypeUI?.Checked ?? false) ||
                                         (defect1TypeUI?.Checked ?? false);
            bool anyDefect1PriorityChecked = (defect1PriorityLow?.Checked ?? false) ||
                                             (defect1PriorityMedium?.Checked ?? false) ||
                                             (defect1PriorityHigh?.Checked ?? false);
            bool anyDefect2TypeChecked = (defect2TypeFunctional?.Checked ?? false) ||
                                         (defect2TypeUI?.Checked ?? false) ||
                                         (defect2TypePerformance?.Checked ?? false);
            bool anyDefect2PriorityChecked = (defect2PriorityLow?.Checked ?? false) ||
                                             (defect2PriorityMedium?.Checked ?? false) ||
                                             (defect2PriorityHigh?.Checked ?? false);

            if (!anyDefect1TypeChecked || !anyDefect1PriorityChecked ||
                !anyDefect2TypeChecked || !anyDefect2PriorityChecked)
                return 0;
            
            if (score == 10)
                return 10;
            else if (score == 7.5)
                return 7;
            else if (score == 5)
                return 5;
            else
                return 0;
        }

        private void endBtn_Click(object sender, EventArgs e)
        {
            int score1 = CalculateScore();
            int score2 = CalculateScore2();
            int score3 = CalculateScore3();
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
                    cmd.Parameters.AddWithValue("test_id", 7);
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

        
    }
}
