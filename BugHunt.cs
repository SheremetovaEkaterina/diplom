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
    public partial class BugHunt : Form
    {
        private HashSet<string> foundBugs = new HashSet<string>();
        private int score = 0;
        int studentId;
        public BugHunt(int studentId)
        {
            InitializeComponent();
            this.studentId = studentId;
        }

        private void BugHunt_Load(object sender, EventArgs e)
        {

        }

        private void RegisterBug(string bugId)
        {
            if (!foundBugs.Contains(bugId))
            {
                foundBugs.Add(bugId);
                score += 2;
                bugProgressBar.Value = foundBugs.Count; // Обновляем ProgressBar
            }
        }

        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            string text = nameTextBox.Text;
            if (!string.IsNullOrEmpty(text))
            {
                if (text.All(char.IsDigit) && !foundBugs.Contains("Только цифры"))
                {
                    RegisterBug("Только цифры");
                }
                
            }
            UpdateConfirmButtonState();

        }

        private void quantityTextBox_TextChanged(object sender, EventArgs e)
        {
            string text = quantityTextBox.Text;
            if (!string.IsNullOrEmpty(text))
            {
                if (text.Contains("-") && !foundBugs.Contains("Отрицательное количество"))
                {
                    RegisterBug("Отрицательное количество");
                }
                else if (text.Contains(".") && !foundBugs.Contains("Дробное количество"))
                {
                    RegisterBug("Дробное количество");
                }
                else if (int.TryParse(text, out int quantity) && quantity > 10 && !foundBugs.Contains("Слишком большое количество"))
                {
                    RegisterBug("Слишком большое количество");
                }
                else if (!string.IsNullOrEmpty(text) && text.Any(char.IsLetter) && !foundBugs.Contains("Буквы в количестве"))
                {
                    RegisterBug("Буквы в количестве");
                }
            }
            UpdateConfirmButtonState();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePicker1.Value.Date < DateTime.Today && !foundBugs.Contains("Прошедшая дата"))
            {
                score += 2;
                RegisterBug("Прошедшая дата");
            }
            UpdateConfirmButtonState();
        }

        private void phoneTextBox_TextChanged(object sender, EventArgs e)
        {
            string text = phoneTextBox.Text;
            if (!string.IsNullOrEmpty(text) && text.Any(char.IsLetter) && !foundBugs.Contains("Буквы в номере"))
            {
                RegisterBug("Буквы в номере");
            }
            
            else if (!string.IsNullOrEmpty(text) && text.Length > 11 && !foundBugs.Contains("Номер больше 11"))
            {
                RegisterBug("Номер больше 11");
            }
            UpdateConfirmButtonState();
        }

        private void phoneLabel_Click(object sender, EventArgs e)
        {
            if (!foundBugs.Contains("Перекрытие метки"))
            {
                RegisterBug("Перекрытие метки");
            }
        }
        bool isValid;
        private void UpdateConfirmButtonState()
        {
            bool isEmpty = string.IsNullOrEmpty(nameTextBox.Text) && string.IsNullOrEmpty(quantityTextBox.Text) &&
                           string.IsNullOrEmpty(phoneTextBox.Text);
            isValid = !string.IsNullOrEmpty(nameTextBox.Text) && nameTextBox.Text.Length >= 2 &&
                           int.TryParse(quantityTextBox.Text, out int quantity) && quantity >= 1 && quantity <= 10 &&
                           DateTime.TryParse(dateTimePicker1.Text, out DateTime date) && date >= DateTime.Today &&
                           !string.IsNullOrEmpty(phoneTextBox.Text) && phoneTextBox.Text.All(char.IsDigit) && 
                           phoneTextBox.Text.Length == 11;

            if (isEmpty)
            {
                confirmButton.Enabled = true; // Баг: активна при пустых полях
            }
            else if (isValid)
            {
                confirmButton.Enabled = false; // Баг: неактивна при валидных данных
                if (!foundBugs.Contains("Кнопка неактивна"))
                {
                    RegisterBug("Кнопка неактивна");
                }

            }
            else
            {
                confirmButton.Enabled = true; // Для остальных случаев, включая баг "Пустой номер"
            }
        }
        private void confirmButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(phoneTextBox.Text) && !foundBugs.Contains("Пустой номер"))
            {
                RegisterBug("Пустой номер");
            }
            else if (string.IsNullOrEmpty(nameTextBox.Text) && string.IsNullOrEmpty(quantityTextBox.Text) &&
                     string.IsNullOrEmpty(phoneTextBox.Text) && !foundBugs.Contains("Активна при пустых полях"))
            {
                RegisterBug("Активна при пустых полях");
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (!foundBugs.Contains("Поля не очищены"))
            {
                RegisterBug("Поля не очищены");
            }
        }

        private void quantityLabel_Click(object sender, EventArgs e)
        {
            if (!foundBugs.Contains("Ошибка в слове"))
            {
                RegisterBug("Ошибка в слове");
            }
        }

        private void endBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = Connection.conn;
                    cmd.CommandText = @"
                    INSERT INTO test_results (student_id, test_id, score)
                    VALUES (@student_id, @test_id, @score)";
                    cmd.Parameters.AddWithValue("student_id", studentId);
                    cmd.Parameters.AddWithValue("test_id", 9);
                    cmd.Parameters.AddWithValue("score", score);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show($"Твое количество баллов: {score} из 30", "Отправка ответов успешна", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
