using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace FPassManager
{
    public class PasswordManager
    {
        private Dictionary<string, PasswordEntry> passwordVault;

        public PasswordManager()
        {
            passwordVault = new Dictionary<string, PasswordEntry>();
        }

        public string ViewPasswords()
        {
            if (passwordVault.Count == 0)
            {
                return "No passwords found.";
            }

            string passwordsStr = "";
            foreach (var entry in passwordVault.Values)
            {
                passwordsStr += $"Username: {entry.GetValue("Username")}\nPassword: {entry.GetValue("Password")}\nDescription: {entry.GetValue("Description")}\nNotes: {entry.GetValue("Notes")}\n---\n";
            }

            return passwordsStr;
        }

        public string AddPassword(string description, string email, string username, string password, string notes, string category)
        {
            if (string.IsNullOrEmpty(description))
                return "Description cannot be empty.";

            if (passwordVault.ContainsKey(description))
                return $"An entry with the description '{description}' already exists.";

            PasswordEntry newEntry = new PasswordEntry();
            newEntry.SetValue("Description", description);
            newEntry.SetValue("Email", email);
            newEntry.SetValue("Username", username);
            newEntry.SetValue("Password", password);
            newEntry.SetValue("Notes", notes);
            newEntry.SetValue("Category", category);

            passwordVault.Add(description, newEntry);
            SaveVault("password_vault.enc", "YourEncryptionKey");

            return $"Password entry for the description '{description}' added successfully.";
        }



        public string SearchPassword(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return "Description cannot be empty.";
            }

            if (!passwordVault.ContainsKey(description))
            {
                return $"No password found for the description '{description}'.";
            }

            PasswordEntry entry = passwordVault[description];
            return $"Username: {entry.GetValue("Username")}\nPassword: {entry.GetValue("Password")}\nDescription: {entry.GetValue("Description")}\nNotes: {entry.GetValue("Notes")}";
        }

        public string UpdatePassword(PasswordEntry currentEntry, string newDescription, string newEmail, string newUsername, string newPassword, string newNotes, string newCategory)
        {
            // Check for mandatory fields.
            if (string.IsNullOrEmpty(newDescription))
                return "Description cannot be empty.";

            // If the description is being changed, check for duplicates.
            if (newDescription != currentEntry.GetValue("Description") && passwordVault.ContainsKey(newDescription))
                return $"An entry with the description '{newDescription}' already exists.";

            // Update the entry details.
            currentEntry.SetValue("Description", newDescription);
            currentEntry.SetValue("Email", newEmail);
            currentEntry.SetValue("Username", newUsername);
            currentEntry.SetValue("Password", newPassword);
            currentEntry.SetValue("Notes", newNotes);
            currentEntry.SetValue("Category", newCategory);

            // If the description has changed, update the key in the dictionary.
            if (newDescription != currentEntry.GetValue("Description"))
            {
                passwordVault.Remove(currentEntry.GetValue("Description"));
                passwordVault[newDescription] = currentEntry;
            }

            SaveVault("password_vault.enc", "YourEncryptionKey");  // Save the updated vault.

            return $"Password entry for '{newDescription}' updated successfully.";
        }

        public string RemovePassword(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return "Description cannot be empty.";
            }

            if (!passwordVault.ContainsKey(description))
            {
                return $"No password found for the description '{description}'.";
            }

            passwordVault.Remove(description);

            // Save the vault after removing the password
            SaveVault("password_vault.enc", "YourEncryptionKey");

            return $"Password for the description '{description}' removed successfully.";
        }

        private string EncryptString(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(key, Encoding.UTF8.GetBytes("SomeSaltValue"));
                aesAlg.Key = keyGenerator.GetBytes(32);
                aesAlg.IV = keyGenerator.GetBytes(16);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
                
            }
        }

        private string DecryptString(string cipherText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(key, Encoding.UTF8.GetBytes("SomeSaltValue"));
                aesAlg.Key = keyGenerator.GetBytes(32);
                aesAlg.IV = keyGenerator.GetBytes(16);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        public string SaveVault(string filename, string key)
        {
            try
            {
                string json = JsonConvert.SerializeObject(passwordVault);
                string encryptedData = EncryptString(json, key);

                File.WriteAllText(filename, encryptedData);
                return $"Encrypted password vault saved to {filename}.";
            }
            catch (Exception e)
            {
                return $"An error occurred while saving the vault: {e.Message}";
            }
        }

        public string LoadVault(string filename, string key)
        {
            try
            {
                string encryptedData = File.ReadAllText(filename);
                string decryptedData = DecryptString(encryptedData, key);

                passwordVault = JsonConvert.DeserializeObject<Dictionary<string, PasswordEntry>>(decryptedData);
                return $"Encrypted password vault loaded from {filename}.";
            }
            catch (FileNotFoundException)
            {
                // If the file doesn't exist, initialize a new, empty password vault
                passwordVault = new Dictionary<string, PasswordEntry>();
                return $"{filename} not found. Initialized a new password vault.";
            }
            catch (Exception e)
            {
                return $"An error occurred while loading the vault: {e.Message}";
            }
        }

        public IEnumerable<PasswordEntry> GetAllEntries()
        {
            return passwordVault.Values;
        }

        public List<PasswordEntry> GetPasswordsByCategory(string category)
        {
            return passwordVault.Values.Where(entry => entry.GetValue("Category") == category).ToList();
        }

        public IEnumerable<string> GetAllCategories()
        {
            return passwordVault.Values
                .Select(entry => entry.GetValue("Category"))
                .Where(category => !string.IsNullOrEmpty(category))
                .Distinct();
        }


        public string GetDetailsByDescription(string description)
        {
            if (passwordVault.TryGetValue(description, out PasswordEntry entry))
            {
                return $"Description: {entry.GetValue("Description")}\nEmail: {entry.GetValue("Email")}\nUsername: {entry.GetValue("Username")}\nPassword: {entry.GetValue("Password")}\nNotes: {entry.GetValue("Notes")}";
            }
            else
            {
                return $"No details found for description: {description}";
            }
        }

        public void AddCategory(string category)
        {
            // Simply create a new password entry with just a category.
            // Note: This assumes that categories are unique, similar to descriptions.
            if (!passwordVault.ContainsKey(category))
            {
                PasswordEntry newEntry = new PasswordEntry();
                newEntry.SetValue("Category", category);
                passwordVault.Add(category, newEntry);
            }
        }

        public void EditCategory(string oldCategory, string newCategory)
        {
            if (passwordVault.TryGetValue(oldCategory, out PasswordEntry entry))
            {
                passwordVault.Remove(oldCategory);
                entry.SetValue("Category", newCategory);
                passwordVault.Add(newCategory, entry);
            }
        }

        public string DeleteCategory(string category)
        {
            if (IsCategoryUsed(category))
            {
                return $"The category '{category}' is associated with existing password entries and cannot be deleted.";
            }
            else
            {
                passwordVault.Remove(category);
                return $"Category '{category}' deleted successfully.";
            }
        }


        public bool IsCategoryUsed(string category)
        {
            return passwordVault.Values.Any(entry => entry.GetValue("Category") == category);
        }


    }

}
