using System;

namespace FilmCS
{
    public class Logger
    {
        private readonly string _name;
        public Logger(string name)
        {
            _name = name;
        }

        public void Debug(string msg)
        {
            Console.WriteLine($"[DEBUG] [{_name}] {msg}");
        }

        public void Info(string msg)
        {
            Console.WriteLine($"[INFO] [{_name}] {msg}");
        }

        public void Warning(string msg)
        {
            Console.WriteLine($"[WARNING] [{_name}] {msg}");
        }

        public void Error(string msg)
        {
            Console.WriteLine($"[ERROR] [{_name}] {msg}");
        }

        public void Critical(string msg)
        {
            Console.WriteLine($"[CRITICAL] [{_name}] {msg}");
        }
    }
}
