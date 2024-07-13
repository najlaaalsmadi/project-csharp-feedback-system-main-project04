using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace final_project
{
    public partial class ShowResult : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCoursesDropdown();
            }
        }

        private void LoadCoursesDropdown()
        {
            string coursesPath = Server.MapPath("~/files/Feedback/");
            if (!Directory.Exists(coursesPath))
            {
                Response.Write("<script>alert('Feedback directory not found');</script>");
                return;
            }

            string[] courseFiles = Directory.GetFiles(coursesPath, "*.txt");
            if (courseFiles.Length == 0)
            {
                Response.Write("<script>alert('No feedback files found');</script>");
                return;
            }

            HashSet<string> courseNames = new HashSet<string>();

            foreach (string file in courseFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string courseName = fileName.Split('_').Last(); // Extract the course name after the last underscore
                courseNames.Add(courseName);
            }

            DropDownList1.Items.Clear();
            DropDownList1.Items.Add(new ListItem("Select Course", ""));
            foreach (string courseName in courseNames)
            {
                DropDownList1.Items.Add(new ListItem(courseName, courseName));
            }
        }

        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCourse = DropDownList1.SelectedValue;

            if (string.IsNullOrEmpty(selectedCourse))
            {
                ResetResults();
            }
            else
            {
                CalculateAndDisplayAverage(selectedCourse);
            }
        }

        private void ResetResults()
        {
            Label2.Text = "";
            ResultLabel.Text = "";
            ResultPanel.BackColor = System.Drawing.Color.Empty;
        }

        private void CalculateAndDisplayAverage(string courseName)
        {
            string feedbackPath = Server.MapPath("~/files/Feedback/");
            string[] feedbackFiles = Directory.GetFiles(feedbackPath, "*.txt");
            int totalScore = 0;
            int questionCount = 0;
            int fileCount = 0;

            foreach (string file in feedbackFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                if (fileNameWithoutExtension.EndsWith(courseName, StringComparison.OrdinalIgnoreCase))
                {
                    string[] lines = File.ReadAllLines(file);
                    foreach (string line in lines)
                    {
                        if (line.Contains(':'))
                        {
                            string[] parts = line.Split(new char[] { ':', '?' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 3)
                            {
                                string answer = parts[2].Trim();
                                totalScore += ConvertAnswerToScore(answer);
                                questionCount++;
                            }
                        }
                    }

                    fileCount++;
                    if (fileCount >= 5)
                        break;
                }
            }

            if (questionCount > 0)
            {
                int maxScore = questionCount * 4;
                double percentageScoreb = ((double)totalScore / maxScore) * 100;
                int percentageScore = (int)percentageScoreb;

                DisplayResult(percentageScore);
            }
            else
            {
                Response.Write("<script>alert('No questions found for the selected course');</script>");
                ResetResults();
            }
        }

        private int ConvertAnswerToScore(string answer)
        {
            switch (answer)
            {
                case "Excellent":
                    return 4;
                case "Very Good":
                    return 3;
                case "Good":
                    return 2;
                case "Poor":
                    return 1;
                default:
                    return 0;
            }
        }

        private void DisplayResult(int percentageScore)
        {
            string message;
            string backgroundColor;

            if (percentageScore > 85)
            {
                message = "Excellent performance!";
                backgroundColor = "#3AFFA0";
            }
            else if (percentageScore > 60)
            {
                message = "Good job!";
                backgroundColor = "yellow";
            }
            else if (percentageScore > 35)
            {
                message = "Needs improvement.";
                backgroundColor = "orange";
            }
            else
            {
                message = "Poor performance.";
                backgroundColor = "red";
            }

            ResultLabel.Text = $"{percentageScore}%";
            Label2.Text = message;
            ResultPanel.BackColor = System.Drawing.ColorTranslator.FromHtml(backgroundColor);
        }

        protected void ContinueButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("HomePage.aspx");
        }
    }
}
