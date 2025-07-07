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
    public partial class API : Form
    {
        private string selectedMethod, selectedEndpoint, selectedBody, selectedCode;
        int studentId;
        public API(int studentId)
        {
            InitializeComponent();
            this.studentId = studentId;
        }

        private void API_Load(object sender, EventArgs e)
        {
            ContextMenuStrip methodMenu = new ContextMenuStrip();
            foreach (string method in new[] { "GET", "POST", "PUT" })
            {
                methodMenu.Items.Add(method, null, (s, ev) =>
                {
                    selectedMethod = method;
                    methodLabel.Text = method;
                    methodLabel.BackColor = Color.Yellow;
                });
            }
            methodLabel.ContextMenuStrip = methodMenu;

            ContextMenuStrip endpointMenu = new ContextMenuStrip();
            foreach (string endpoint in new[] { "/orders", "/tickets", "/users" })
            {
                endpointMenu.Items.Add(endpoint, null, (s, ev) =>
                {
                    selectedEndpoint = endpoint;
                    endpointLabel.Text = endpoint;
                    endpointLabel.BackColor = Color.Yellow;
                });
            }
            endpointLabel.ContextMenuStrip = endpointMenu;

            ContextMenuStrip bodyMenu = new ContextMenuStrip();
            foreach (string body in new[] {
            "{ \"name\": \"John Doe\", \"quantity\": 2 }",
            "{ \"quantity\": -1 }",
            "{ \"name\": \"\" }"
        })
            {
                bodyMenu.Items.Add(body, null, (s, ev) =>
                {
                    selectedBody = body;
                    bodyLabel.Text = body;
                    bodyLabel.BackColor = Color.Yellow;
                });
            }
            bodyLabel.ContextMenuStrip = bodyMenu;

            ContextMenuStrip codeMenu = new ContextMenuStrip();
            foreach (string code in new[] { "400", "200", "500" })
            {
                codeMenu.Items.Add(code, null, (s, ev) =>
                {
                    selectedCode = code;
                    codeLabel.Text = code;
                    codeLabel.BackColor = Color.Yellow;
                });
            }
            codeLabel.ContextMenuStrip = codeMenu;
        }

        private void endBtn_Click(object sender, EventArgs e)
        {
            int result = CalculateScore1() + CalculateScore2(answer2Box.Text) + CalculateScore3();
            try
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = Connection.conn;
                    cmd.CommandText = @"
                    INSERT INTO test_results (student_id, test_id, score)
                    VALUES (@student_id, @test_id, @score)";
                    cmd.Parameters.AddWithValue("student_id", studentId);
                    cmd.Parameters.AddWithValue("test_id", 8);
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

        private int CalculateScore1()
        {
            if (selectedMethod == null || selectedEndpoint == null || selectedBody == null || selectedCode == null)
                return 0;

            double score = 0;
            if (selectedMethod == "POST")
                score += 2.5;
            if (selectedEndpoint == "/orders")
                score += 2.5;
            if (selectedBody == "{ \"name\": \"John Doe\", \"quantity\": 2 }")
                score += 2.5;
            if (selectedCode == "200")
                score += 2.5;


            if (score == 10)
                return 10;
            else if (score > 7)
                return 7;
            else if (score == 5)
                return 4;
            else return 0;
        }

        private int CalculateScore2(string input)
        {
            try
            {
                var lines = input.Split('\n')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                if (lines.Count < 2) return 0;

                var response = new Dictionary<string, string>();
                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                        response[parts[0].Trim().ToLower()] = parts[1].Trim();
                }

                int score = 0;

                // Проверка "Корректен"
                if (response.ContainsKey("корректен") && response["корректен"].ToLower() == "нет")
                    score += 4;

                // Проверка "Проблема"
                if (response.ContainsKey("проблема"))
                {
                    var problem = response["проблема"].ToLower();
                    if (problem.Contains("201") && problem.Contains("200"))
                        score += 3;
                    if (problem.Contains("orderid") && problem.Contains("id"))
                        score += 3;
                }


                if (score >= 4)
                    return score;
                else return 0;
                

            }
            catch
            {
                return 0;
            }
        }

        private int CalculateScore3()
        {
            int score = 0;

            
            if (status400 != null && status400.Checked)
                score += 5;
            
            if (quantityError != null && quantityError.Checked)
                score += 5;


            // Проверка, что все ответы выбраны
            bool anyStatusChecked = (status400?.Checked ?? false) ||
                                         (status200?.Checked ?? false) ||
                                         (status404?.Checked ?? false) || (status500?.Checked ?? false);
            bool anyErrorChecked = (quantityError?.Checked ?? false) ||
                                             (serverError?.Checked ?? false) ||
                                             (notFoundError?.Checked ?? false) || (success?.Checked ?? false);

            if (!anyErrorChecked || !anyStatusChecked)
                return 0;

            return score;
        }
    }
}
