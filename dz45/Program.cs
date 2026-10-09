using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static string login = "";

  

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        MakeSampleQuiz();

        while (true)
        {
            Console.WriteLine("\n=== ВХІД ===");
            Console.Write("логін (0 - вихід): ");
            string l = Console.ReadLine();
            if (l == "0") return;
            Console.Write("пароль: ");
            string p = Console.ReadLine();

            string[] user = FindUser(l);
            if (user == null)
            {
                Console.Write("такого користувача немає. Зареєструватись? (y/n): ");
                if (Console.ReadLine() == "y")
                    Register(l, p);
                continue;
            }
            if (user[1] != p)
            {
                Console.WriteLine("неправильний пароль!");
                continue;
            }
            login = l;
            UserMenu();
        }
    }

    static string[] FindUser(string l)
    {
        if (!File.Exists("users.txt")) return null;
        foreach (string line in File.ReadAllLines("users.txt"))
        {
            string[] parts = line.Split('|');
            if (parts[0] == l) return parts;
        }
        return null;
    }

    static void Register(string l, string p)
    {
     
        Console.Write("Дата народження (дд.мм.рррр): ");
        DateTime d;
        while (!DateTime.TryParse(Console.ReadLine(), out d))
            Console.Write("неправильна дата, ще раз: ");

        File.AppendAllText("users.txt", l + "|" + p + "|" + d.ToString("dd.MM.yyyy") + "\n");
        Console.WriteLine("реєстрація успішна!");
    }

    static void UserMenu()
    {
        while (true)
        {
            Console.WriteLine("\n=== МЕНЮ (" + login + ") ===");
            Console.WriteLine("1. нова вікторина");
            Console.WriteLine("2. мої минулі результати");
            Console.WriteLine("3. топ-20 вікторини");
            Console.WriteLine("4. налаштування");
            Console.WriteLine("0. вихід");
            string c = Console.ReadLine();

            if (c == "1") StartQuiz();
            else if (c == "2") MyResults();
            else if (c == "3") Top20();
            else if (c == "4") Settings();
            else if (c == "0") return;
        }
    }

    static string ChooseTopic(bool withMixed)
    {
        string[] files = Directory.GetFiles("quizzes", "*.txt");
        Console.WriteLine("оберіть розділ:");
        for (int i = 0; i < files.Length; i++)
            Console.WriteLine((i + 1) + ". " + Path.GetFileNameWithoutExtension(files[i]));
        if (withMixed)
            Console.WriteLine("0. змішана вікторина");

        string c = Console.ReadLine();
        int n;
        if (!int.TryParse(c, out n)) return null;
        if (n == 0 && withMixed) return "змішана";
        if (n < 1 || n > files.Length) return null;
        return Path.GetFileNameWithoutExtension(files[n - 1]);
    }

    static void StartQuiz()
    {
        string topic = ChooseTopic(true);
        if (topic == null) { Console.WriteLine("невірний вибір!"); return; }

       
        List<string> questions = new List<string>();
        if (topic == "змішана")
        {
            foreach (string f in Directory.GetFiles("quizzes", "*.txt"))
                questions.AddRange(File.ReadAllLines(f));
        }
        else
        {
            questions.AddRange(File.ReadAllLines("quizzes/" + topic + ".txt"));
        }

        
        questions = questions.Where(q => q.Trim() != "").OrderBy(q => Guid.NewGuid()).Take(20).ToList();

        int score = 0;
        int num = 1;
        foreach (string q in questions)
        {
            string[] parts = q.Split('|');
            string[] answers = parts[1].Split(';');

            Console.WriteLine("\nпитання " + num + " з " + questions.Count + ": " + parts[0]);
            for (int i = 0; i < answers.Length; i++)
                Console.WriteLine((i + 1) + ") " + answers[i]);
            Console.Write("ваша відповідь (номери через кому): ");

            string userAnswer = NormalizeAnswer(Console.ReadLine());
            string rightAnswer = NormalizeAnswer(parts[2]);

            if (userAnswer == rightAnswer)
            {
                Console.WriteLine("правильно!");
                score++;
            }
            else
            {
                Console.WriteLine("неправильно.");
            }
            num++;
        }

        Console.WriteLine("\nвікторина закінчена! Правильних відповідей: " + score + " з " + questions.Count);
        File.AppendAllText("results.txt", login + "|" + topic + "|" + score + "\n");

     
        List<string[]> all = ReadResults(topic);
        int place = 1;
        foreach (string[] r in all)
        {
            if (int.Parse(r[2]) > score) place++;
        }
        Console.WriteLine("ваше місце в таблиці результатів: " + place);
    }

   
    static string NormalizeAnswer(string s)
    {
        List<int> nums = new List<int>();
        foreach (string part in s.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            int n;
            if (int.TryParse(part, out n) && !nums.Contains(n))
                nums.Add(n);
        }
        nums.Sort();
        return string.Join(",", nums);
    }

    static List<string[]> ReadResults(string topic)
    {
        List<string[]> list = new List<string[]>();
        if (!File.Exists("results.txt")) return list;
        foreach (string line in File.ReadAllLines("results.txt"))
        {
            string[] parts = line.Split('|');
            if (parts[1] == topic) list.Add(parts);
        }
        return list;
    }

    static void MyResults()
    {
        if (!File.Exists("results.txt"))
        {
            Console.WriteLine("результатів ще немає.");
            return;
        }
        Console.WriteLine("\nВаші результати:");
        foreach (string line in File.ReadAllLines("results.txt"))
        {
            string[] parts = line.Split('|');
            if (parts[0] == login)
                Console.WriteLine(parts[1] + ": " + parts[2] + " правильних");
        }
    }

    static void Top20()
    {
        string topic = ChooseTopic(true);
        if (topic == null) { Console.WriteLine("невірний вибір!"); return; }

        var top = ReadResults(topic).OrderByDescending(r => int.Parse(r[2])).Take(20).ToList();
        Console.WriteLine("\nТоп-20 (" + topic + "):");
        int place = 1;
        foreach (string[] r in top)
        {
            Console.WriteLine(place + ". " + r[0] + " - " + r[2]);
            place++;
        }
    }

    static void Settings()
    {
        Console.WriteLine("1. змінити пароль");
        Console.WriteLine("2. змінити дату народження");
        string c = Console.ReadLine();

        string[] lines = File.ReadAllLines("users.txt");
        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split('|');
            if (parts[0] != login) continue;

            if (c == "1")
            {
                Console.Write("новий пароль: ");
                parts[1] = Console.ReadLine();
            }
            else if (c == "2")
            {
                Console.Write("нова дата (дд.мм.рррр): ");
                DateTime d;
                while (!DateTime.TryParse(Console.ReadLine(), out d))
                    Console.Write("Неправильна дата, ще раз: ");
                parts[2] = d.ToString("dd.MM.yyyy");
            }
            lines[i] = string.Join("|", parts);
        }
        File.WriteAllLines("users.txt", lines);
        Console.WriteLine("збережено!");
    }

    
    static void MakeSampleQuiz()
    {
        if (!Directory.Exists("quizzes"))
            Directory.CreateDirectory("quizzes");

        if (Directory.GetFiles("quizzes").Length == 0)
        {
            string[] sample = {
                "столиця України?|Київ;Львів;Одеса|1",
                "які з цих міст в Україні?|Київ;Берлін;Рівне;Париж|1,3",
                "скільки буде 2+2?|3;4;5|2"
            };
            File.WriteAllLines("quizzes/Географія.txt", sample);
        }
    }
}