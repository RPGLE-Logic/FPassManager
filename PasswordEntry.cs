using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPassManager
{
    public class PasswordEntry
    {
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();

        public void SetValue(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                Data[key] = value;
        }

        public string GetValue(string key)
        {
            return Data.TryGetValue(key, out string value) ? value : string.Empty;
        }
    }

}
