using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConf.Examples
{
    public class Config
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public string ConnectionString { get; set; }

        public Config()
        {
            Name = "Timothy";
            ConnectionString = "localhost";
        }

        public override string ToString()
        {
            return String.Format(
                @"Hi, my name is {0}. I am {1} years old and I am a web server at ""{2}"".",
                Name, Age, ConnectionString);
        }
    }

    class Program
    {
        public static void ManageMyConfigFile(Config config)
        {
            Console.WriteLine(config);
            Console.WriteLine(
                "I'll track changes even in methods/classes that don't know about " +
                "the ConfigurationManager, as long as the instance stays the same.");
            config.Age = 20;
            Console.WriteLine(config);
        }

        public static void PrintChangedProperties(ConfigurationManager<Config> conf)
        {
            var props = conf.GetPropertyChangesByName();
            if (props.Any())
            {
                Console.WriteLine("Changed Properties:");
                foreach (var prop in props)
                {
                    Console.WriteLine(String.Format("  {0}: {1}", prop.Key, prop.Value));
                }
            }
            else
            {
                Console.WriteLine("No changed properties.");
            }
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            var config = new ConfigurationManager<Config>("settings.xml", "local.xml");
            PrintChangedProperties(config);

            ManageMyConfigFile(config.Out);
            PrintChangedProperties(config);

            Console.WriteLine("Check out.xml in the bin directory for the 'new' local config file.");
            config.SaveChanges("out.xml");

            var savedConf = new ConfigurationManager<Config>("settings.xml", "out.xml");
            Console.WriteLine("After loading and merging 'out.xml':");
            PrintChangedProperties(config);
        }
    }
}
