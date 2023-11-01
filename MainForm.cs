using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPassManager
{
    public partial class MainForm : Form
    {
        PasswordManager passwordManager;  // Class-level variable
        private Button selectedPasswordButton = null;

        public MainForm()
        {
            InitializeComponent();
            passwordManager = new PasswordManager();

            // Load the vault when the application starts
            string loadResult = passwordManager.LoadVault("password_vault.enc", "YourEncryptionKey");
            if (!loadResult.Contains("loaded"))
            {
                MessageBox.Show(loadResult, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Assuming you have initialized passwordsPanel and detailsTextBox in the designer
            LoadPasswordButtons();
        }

        private void LoadCategories()
        {
            comboCategories.Items.Clear();
            var categories = passwordManager.GetAllCategories();
            foreach (var category in categories)
            {
                comboCategories.Items.Add(category);
            }
        }

        private void btnAddCategory_Click(object sender, EventArgs e)
        {
            string newCategory = txtCategory.Text.Trim();
            if (!string.IsNullOrEmpty(newCategory))
            {
                passwordManager.AddCategory(newCategory);
                LoadCategories();
                txtCategory.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a category name.");
            }
        }

        private void btnEditCategory_Click(object sender, EventArgs e)
        {
            string oldCategory = comboCategories.SelectedItem?.ToString();
            string newCategory = txtCategory.Text.Trim();

            if (!string.IsNullOrEmpty(oldCategory) && !string.IsNullOrEmpty(newCategory))
            {
                passwordManager.EditCategory(oldCategory, newCategory);
                LoadCategories();
                txtCategory.Clear();
            }
            else
            {
                MessageBox.Show("Please select a category and enter a new name.");
            }
        }

        private void btnDeleteCategory_Click(object sender, EventArgs e)
        {
            string categoryToDelete = comboCategories.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(categoryToDelete))
            {
                string result = passwordManager.DeleteCategory(categoryToDelete);
                if (result.Contains("deleted"))
                {
                    LoadCategories();
                }
                MessageBox.Show(result);
            }
            else
            {
                MessageBox.Show("Please select a category to delete.");
            }
        }



        private void LoadPasswordButtons(string category = null)
        {
            // Clear existing buttons
            passwordsPanel.Controls.Clear();

            IEnumerable<PasswordEntry> entries;

            if (string.IsNullOrEmpty(category))
            {
                entries = passwordManager.GetAllEntries();
            }
            else
            {
                entries = passwordManager.GetPasswordsByCategory(category);
            }

            foreach (var entry in entries)
            {
                Button btn = new Button();
                btn.Text = entry.GetValue("Description");
                btn.Tag = entry;  // Store the PasswordEntry object for later use
                btn.Paint += CustomButtonPaint;  // Custom paint event
                btn.Click += PasswordButton_Click;  // Show details when clicked
                btn.Width = 172;
                btn.Height = 50;
                passwordsPanel.Controls.Add(btn);
            }
        }

        private void CustomButtonPaint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            PasswordEntry entry = btn.Tag as PasswordEntry;  // Retrieve the PasswordEntry object

            const int padding = 5;  // Left padding

            // Draw the description in bold, left-aligned
            e.Graphics.DrawString(entry.GetValue("Description"), new Font(btn.Font, FontStyle.Bold), Brushes.Black, padding, 5);  // Adjust the 5 for desired vertical spacing

            // Draw the email in regular font, left-aligned and positioned below the description
            SizeF descriptionSize = e.Graphics.MeasureString(entry.GetValue("Description"), new Font(btn.Font, FontStyle.Bold));
            e.Graphics.DrawString($"({entry.GetValue("Email")})", btn.Font, Brushes.Black, padding, descriptionSize.Height + 10);  // Adjust the 10 for desired vertical spacing
        }


        private void PasswordButton_Click(object sender, EventArgs e)
        {
            selectedPasswordButton = sender as Button;
            PasswordEntry entry = selectedPasswordButton.Tag as PasswordEntry;  // Retrieve the PasswordEntry object

            // Populate the labels and RichTextBox with the details of the selected password
            txtDescription.Text = $"{entry.GetValue("Description")}";
            txtEmail.Text = $"{entry.GetValue("Email")}";
            txtUsername.Text = $"{entry.GetValue("Username")}";
            txtPassword.Text = $"{entry.GetValue("Password")}";
            rtbNotes.Text = $"{entry.GetValue("Notes")}";
            txtCategory.Text = $"{entry.GetValue("Category")}";

            // Disable the "Add New Password" button and enable the "Update" button
            btnAdd.Enabled = false;
            btnUpdatePassword.Enabled = true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string description = txtDescription.Text;
            string email = txtEmail.Text;
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string notes = rtbNotes.Text;
            string category = cbCategory.SelectedItem?.ToString() ?? string.Empty;

            string result = passwordManager.AddPassword(description, email, username, password, notes, category);
            MessageBox.Show(result);

            // Clear the text boxes and RichTextBox
            txtDescription.Clear();
            txtEmail.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            rtbNotes.Clear();

            LoadPasswordButtons();  // Refresh the displayed buttons to reflect updates.
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            string description = txtDescription.Text;

            string result = passwordManager.RemovePassword(description);
            MessageBox.Show(result);

            // Clear the description text box
            txtDescription.Clear();
        }

        private void comboCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCategory = comboCategories.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedCategory))
            {
                LoadPasswordButtons(selectedCategory);
            }
        }

        private void btnUpdatePassword_Click(object sender, EventArgs e)
        {
            if (selectedPasswordButton == null)
            {
                MessageBox.Show("No password entry selected.");
                return;
            }

            PasswordEntry currentEntry = selectedPasswordButton.Tag as PasswordEntry;

            // Assuming you have TextBox controls for editing details:
            string newDescription = txtDescription.Text;
            string newEmail = txtEmail.Text;
            string newUsername = txtUsername.Text;
            string newPassword = txtPassword.Text;
            string newNotes = rtbNotes.Text;
            string newCategory = cbCategory.SelectedItem?.ToString() ?? string.Empty;

            string result = passwordManager.UpdatePassword(currentEntry, newDescription, newEmail, newUsername, newPassword, newNotes, newCategory);
            MessageBox.Show(result);

            LoadPasswordButtons();  // Refresh the displayed buttons to reflect updates.
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear the entry boxes
            txtDescription.Clear();
            txtEmail.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            rtbNotes.Clear();
            if (cbCategory.Items.Count > 0)
            {
                cbCategory.SelectedIndex = 0;
            }

            // Disable the "Update" button and enable the "Add New Password" button
            btnUpdatePassword.Enabled = false;
            btnAdd.Enabled = true;

            // Clear the selected password button reference
            selectedPasswordButton = null;
        }
     }
}
