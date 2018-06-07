using System;
using System.Collections.Generic;
using System.IO;

namespace Utils
{
    public class ConfigurationReader
    {
        public Configuration Read(string filePath)
        {
            string line;
            string[] aux;
            Dictionary<string, string> configValues = new Dictionary<string, string>();

            if (!File.Exists(filePath))
            {
                return null;
            }
            else
            {
                // Read the file and display it line by line.
                var file = new StreamReader(filePath);
                while ((line = file.ReadLine()) != null)
                {
                    if (line[0] != '#')
                    {
                        aux = line.Split('=');
                        configValues.Add(aux[0], aux[1]);
                    }
                }
                file.Close();

                var configuration = new Configuration();

                configuration.Host = configValues["host"];
                configuration.Password = configValues["password"];
                configuration.Port = Convert.ToInt32(configValues["port"]);
                configuration.Sid = configValues["sid"];
                configuration.User = configValues["user"];
                configuration.Empresa = configValues["empresa"];
                return configuration;
            }
        }
    }
}
