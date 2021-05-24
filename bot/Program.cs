using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace NotificationBot
{

    class Program
    {
        public static int Index;
        //
        public static TelegramBotClient BotClient = new TelegramBotClient("1802263270:AAFfZYN-Nfdxg2Cgd7UvlkI6f62qjl3y-sQ");



        public static List<Tuple<string, string, string, string, string, string, long>> Notes = new List<Tuple<string, string, string, string, string, string, long>>();
        public static List<DateTime> NotesDate = new List<DateTime>();
        public static List<Tuple<DateTime, int>> Notifications = new List<Tuple<DateTime, int>>();
        public static List<DateTime> LocalNotifications = new List<DateTime>();

        static DateTime[] NotificationDates;
        static DateTime[] NotificationTimes;
        static int NotesCounter;
        static int Indexator = 0;

        public static List<int> Iterations = new List<int>();
        public static bool isCreatingNote, Q0, Q1, Q2, Q3, Q4, Q5, isEditNote, GetIndex, isEdit, isRemove;
        static bool isNewDate = false;
        static bool BackFromCheck = false;
        static bool Acept = false;
        static bool WaitAccept = false;


        static string Name;
        static string Freq;
        static string Cicle;
        static string Time;
        static string Type;
        static string Additional;
        static long Id;

        static int WaitingIndex;

        static void Main(string[] args)
        {
            BotClient.OnMessage += BotClient_OnMessage;
            BotClient.StartReceiving();

            Update();

            Console.ReadKey();

            BotClient.StopReceiving();
        }

        private static void BotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {

            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                if (e.Message.Text == "/start" || e.Message.Text == "/ start")
                {
                    BotClient.SendTextMessageAsync(e.Message.Chat.Id, "Створити нове нагадування?", replyMarkup: StartButtons());

                }

                if (e.Message.Text == "Створити нагадування")
                {
                    BotClient.SendTextMessageAsync(e.Message.Chat.Id, "Введіть назву лікарського засобу", replyMarkup: new ReplyKeyboardRemove());
                    isCreatingNote = true;
                    Q0 = true;

                }
                CheckNotes(e);
                CreateNew(e);
                EditNote(e);
                Edit(e);
                RemoveNote(e);
                if (e.Message.Text == "Назад" && BackFromCheck)
                {
                    BackFromCheck = false;
                    BotClient.SendTextMessageAsync(e.Message.Chat.Id, "Створити нове нагадування?", replyMarkup: StartButtons());
                }
            }

        }

        private static void CheckNotes(Telegram.Bot.Args.MessageEventArgs msg)
        {
            if (msg.Message.Text == "Список нагадувань")
            {
                string AllNotes = "";
                for (int i = 0; i < Notes.Count; i++)
                {
                    AllNotes += $"Заметка №{i + 1} | Назва лікарського засобу: {Notes[i].Item1}\n";
                }
                if (Notes.Count == 0) { BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Список нагадувань пустий", replyMarkup: StartButtons()); }
                else
                {
                    BackFromCheck = true;
                    BotClient.SendTextMessageAsync(msg.Message.Chat.Id,
                        $"Кількість нагадувань {Notes.Count}"
                        + "\n\n"
                        + AllNotes
                        , replyMarkup: BackButton());
                }
            }
        }

        private static void CreateNew(Telegram.Bot.Args.MessageEventArgs msg)
        {

            Id = msg.Message.Chat.Id;

            if (isCreatingNote && isEditNote == false)
            {
                if (Q0)
                {
                    if (msg.Message.Text != "" && msg.Message.Text != "Створити нагадування")
                    {
                        Name = msg.Message.Text;
                        Q0 = false;
                        Q1 = true;
                        Q2 = false;
                        Console.WriteLine($"Назва лікарського засобу: {Name}");
                        BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Введіть частоту вживання в форматі 2/d, де 2 - кількість прийомів в день");
                    }
                }
                if (Q1)
                {
                    if (msg.Message.Text != "" && msg.Message.Text != Name)
                    {
                        Freq = msg.Message.Text;
                        Q1 = false;
                        Q2 = true;
                        GetFreq(Freq, Q1, Q2, msg);

                    }
                }
                if (Q2)
                {
                    if (msg.Message.Text != Freq && msg.Message.Text != "Створити нагадування")
                    {
                        Q2 = false;
                        Q3 = true;
                        Cicle = msg.Message.Text;
                        GetCicle(Cicle, Q2, Q3, msg);

                    }
                }
                if (Q3)
                {
                    if (msg.Message.Text != Cicle && msg.Message.Text != "Створити нагадування")
                    {
                        Time = msg.Message.Text;
                        Q3 = false;
                        Q4 = true;
                        SetNotificationDates(Time, Q3, Q4, msg);
                    }
                }
                if (Q4)
                {
                    if (msg.Message.Text != Time && msg.Message.Text != "Створити нагадування")
                    {
                        Type = msg.Message.Text;
                        Q4 = false;
                        Q5 = true;
                        Console.WriteLine(Type);
                        BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Кількість/Особливості прийому (не може бути пустим)");
                    }
                }
                if (Q5)
                {
                    if (msg.Message.Text != Type && msg.Message.Text != "Створити нагадування")
                    {
                        Additional = msg.Message.Text;
                        Q5 = false;
                        isCreatingNote = false;
                        Q0 = true;
                        Console.WriteLine(Additional);
                        BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Нове нагадування успішно створено!", replyMarkup: StartButtons());
                        AddNote(Name, Freq, Cicle, Time, Type, Additional, Id, NotificationTimes, Indexator);
                        WriteNextStep(msg, Indexator);
                    }
                }
            }

        }

        private static void EditNote(Telegram.Bot.Args.MessageEventArgs msg)
        {
            if (msg.Message.Text == "Редагувати нагадування")
            {
                if (Notes.Count > 0)
                {
                    BotClient.SendTextMessageAsync(msg.Message.Chat.Id,
                        $"Активних нагадувань {Notes.Count}\n" +
                        "\n" +
                        "Введіть номер нагадування, щоб редагувати", replyMarkup: new ReplyKeyboardRemove());
                    GetIndex = true;
                }
                else
                {
                    GetIndex = false;
                    BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "В вас нема активних нагадувань", replyMarkup: StartButtons());
                }

            }
            if (GetIndex && msg.Message.Text != "Редагувати нагадування")
            {
                try
                {
                    Index = int.Parse(msg.Message.Text);
                    Console.WriteLine(Index);

                    if (Index > 0 && Index <= Notes.Count)
                    {
                        WriteNextStep(msg, Index - 1);
                        Console.WriteLine(Index);
                        GetIndex = false;
                        isEditNote = true;
                        BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Вкажіть назву лікарського засобу");
                    }
                    else
                    {
                        GetIndex = false;
                        isEditNote = false;
                        BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Такого нагадування не існує", replyMarkup: StartButtons());
                    }
                }
                catch
                {
                    Console.WriteLine(msg.Message.Text);
                    BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Неправильний формат вводу", replyMarkup: StartButtons());
                    GetIndex = false;
                    isEditNote = false;
                }
            }
            if (isEditNote && msg.Message.Text != Index.ToString() && GetIndex == false)
            {
                isEdit = true;
                //Edit(msg);
            }
        }

        private static void Edit(Telegram.Bot.Args.MessageEventArgs msg)
        {

            Id = msg.Message.Chat.Id;

            if (isEditNote && isEdit)
            {
                if (Q0)
                {
                    if (msg.Message.Text != "" && msg.Message.Text != "Створити нагадування" && msg.Message.Text != "/start")
                    {
                        Name = msg.Message.Text;
                        Q0 = false;
                        Q1 = true;
                        Q2 = false;
                        Console.WriteLine($"Назва лікарського засобу: {Name}");
                        BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Як часто потрібно приймати лікарський засіб?");
                    }
                }
                if (Q1)
                {
                    if (msg.Message.Text != Name && msg.Message.Text != "Створити нагадування")
                    {
                        Freq = msg.Message.Text;
                        Q1 = false;
                        Q2 = true;
                        GetFreq(Freq, Q1, Q2, msg);
                    }
                }
                if (Q2)
                {
                    if (msg.Message.Text != Freq && msg.Message.Text != "Створити нагадування")
                    {
                        Q2 = false;
                        Q3 = true;
                        Cicle = msg.Message.Text;
                        GetCicle(Cicle, Q2, Q3, msg);
                    }
                }
                if (Q3)
                {
                    if (msg.Message.Text != Cicle && msg.Message.Text != "Створити нагадування")
                    {
                        Time = msg.Message.Text;
                        Q3 = false;
                        Q4 = true;
                        SetNotificationDates(Time, Q3, Q4, msg);
                    }
                }
                if (Q4)
                {
                    if (msg.Message.Text != Time && msg.Message.Text != "Створити нагадування")
                    {
                        Type = msg.Message.Text;
                        Q4 = false;
                        Q5 = true;
                        Console.WriteLine(Type);
                        BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Кількість/Особливості прийому (не може бути пустим)");
                    }
                }
                if (Q5)
                {
                    if (msg.Message.Text != Type && msg.Message.Text != "Створити нагадування")
                    {
                        Additional = msg.Message.Text;
                        Q1 = false;
                        Q2 = false;
                        Q3 = false;
                        Q4 = false;
                        Q5 = false;
                        isEditNote = false;
                        Console.WriteLine(Additional);
                        BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Нагадування успішно відредаговано!", replyMarkup: StartButtons());
                        Notes.Remove(Notes[Index - 1]);
                        List<Tuple<string, string, string, string, string, string, long>> New = new List<Tuple<string, string, string, string, string, string, long>>();
                        New.Add(Tuple.Create(Name, Freq, Cicle, Time, Type, Additional, Id));

                        Notes.Add(New[0]);
                        New.Remove(New[0]);
                        WriteNextStep(msg, Index - 1);

                    }
                }
            }

        }

        private static void RemoveNote(Telegram.Bot.Args.MessageEventArgs msg)
        {
            if (msg.Message.Text == "Видалити нагадування")
            {
                BotClient.SendTextMessageAsync(msg.Message.Chat.Id,
                    $"Активних нагадувань {Notes.Count}"
                    + "\n\n"
                    + "Введіть номер нагадування, яке хочете видалити");
                isRemove = true;
            }
            if (msg.Message.Text != "Видалити нагадування" && isRemove)
            {
                try
                {
                    int RemoveIndex = int.Parse(msg.Message.Text);
                    if (RemoveIndex > 0 && RemoveIndex <= Notes.Count)
                    {
                        Notes.Remove(Notes[RemoveIndex - 1]);
                        for (int i = 0; i < Notifications.Count; i++)
                        {
                            if (Notifications[i].Item2 == RemoveIndex)
                            {
                                Notifications.Remove(Notifications[i]);
                            }
                        }
                        BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Нагадування було видалене");
                        isRemove = false;
                    }
                    else
                    {
                        BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Такого нагадування не уснує");
                    }
                }
                catch
                {
                    BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Неправильний формат вводу");
                }
            }
        }

        private static void AddNote(string a0, string a1, string a2, string a3, string a4, string a5, long a6, DateTime[] a7, int NotificationIndex)
        {
            Notes.Add(new Tuple<string, string, string, string, string, string, long>(a0, a1, a2, a3, a4, a5, a6));
            if (NotificationIndex < Notes.Count)
            {
                NotificationIndex = Notes.Count - 1;
            }
            for (int i = 0; i < a7.Length; i++)
            {
                Notifications.Add(Tuple.Create(a7[i], NotificationIndex));
            }
        }

        private static void CheckDate(string Date, Telegram.Bot.Args.MessageEventArgs msg)
        {
            DateTime result;
            try
            {
                string date = "";
                string[] split = Date.Split(".".ToCharArray());
                string[] spl = Date.Split(':');
                if (split[0].Length > 2) { split[0] = split[0].Remove(0, split[0].Length - 2); }
                if (split[2].Length > 2) { split[2] = split[2].Remove(2, split[2].Length - 2); }

                if (spl[0].Length > 2)
                {
                    spl[0] = spl[0].Remove(0, spl[0].Length - 2);
                }
                date = $"{split[0]}.{split[1]}.{split[2]} {spl[0]}:{spl[1]}:00";
                result = DateTime.Parse(date).AddYears(1);

                if (isNewDate)
                {
                    List<DateTime> New = new List<DateTime> { result };
                    NotesDate[Index - 1] = New[0];
                }
                else
                {
                    NotesDate.Add(result);
                }
                if (DateTime.Now > result)
                {
                    BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "не можна вводити минулу дату");
                    Q3 = true;
                    Q4 = true;
                }
                else
                {
                    Console.WriteLine(result);
                    BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Вкажіть тип лікарського засобу");
                }
            }
            catch
            {
                BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Неверный формат даты и времени,");
                Q3 = true;
                Q4 = false;
            }
        }

        private static void WriteNextStep(Telegram.Bot.Args.MessageEventArgs msg, int Index)
        {
            BotClient.SendTextMessageAsync(msg.Message.Chat.Id, $"Назва лікарського засобу {Notes[Index].Item1} \n\n"
                + $"Частота прийому: {Notes[Index].Item2}   \n" +
                  $"Період прийому: {Notes[Index].Item3}    \n"
                + $"Години нагадувань: {Notes[Index].Item4} \n"
                + $"тип лікарського засобу: {Notes[Index].Item5}    \n"
                + $"Додатково: {Notes[Index].Item6}    \n");
        }

        private static async void Update()
        {
            while (true)
            {

                if (Notifications.Count > 0)
                {
                    for (int i = 0; i < Notifications.Count; i++)
                    {

                        if (DateTime.Now > Notifications[i].Item1)
                        {
                            try
                            {
                                int Index = Notifications[i].Item2;
                                await BotClient.SendTextMessageAsync(Notes[Notifications[i].Item2].Item7, "Не забудьте випти лікарський засіб!!! \n"
                                + "\n"
                                + "Назва лікарського засобу: " + Notes[Notifications[i].Item2].Item1 + "\n"
                                + "тип лікарського засобу: " + Notes[Notifications[i].Item2].Item5 + "\n"
                                + "Опис: " + Notes[Notifications[i].Item2].Item6 + "\n"
                                + "\n\n"
                                + "Час нагадування: " + Notifications[i].Item1 + "", replyMarkup: StartButtons());

                                Notifications.Remove(Notifications[i]);
                                WaitAccept = true;
                                //NotesDate.Remove(NotesDate[i]);
                                //Notes.Remove(Notes[i]);
                            }
                            catch
                            {

                            }
                        }
                    }
                }

                await Task.Delay(1000);
            }
        }

        private static void ClickDelay(Telegram.Bot.Args.MessageEventArgs msg, int Index)
        {
            if (msg.Message.Text == "Принял" && WaitAccept)
            {
                WaitAccept = false;
                Notifications.Remove(Notifications[Index]);
                BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Принято", replyMarkup: new ReplyKeyboardRemove());
            }
            if (msg.Message.Text == "Отложить на 30 минут" && WaitAccept)
            {
                List<Tuple<DateTime, int>> New = new List<Tuple<DateTime, int>>();
                New.Add(Tuple.Create(Notifications[Index].Item1.AddMinutes(30), Notifications[Index].Item2));
                Notifications.Remove(Notifications[Index]);
                Notifications.Add(Tuple.Create(New[0].Item1, New[0].Item2));
                WaitAccept = false;

            }
        }

        private static void GetFreq(string Freq, bool _this, bool Next, Telegram.Bot.Args.MessageEventArgs msg)
        {
            try
            {
                string[] split = Freq.Split('/');
                Console.WriteLine($"Сколько раз выдавать уведомления: {split[0]}");
                int index = int.Parse(split[0]);
                BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Цикл прийому від-до");
                _this = false;
                Next = true;
                NotesCounter = index;
            }
            catch
            {
                BotClient.SendTextMessageAsync(msg.Message.Chat.Id, $"Неверно указана Частота прийому", replyMarkup: StartButtons());
                _this = false;
                Next = false;
                Q0 = false;
                Q1 = false;
                Q2 = false;
                Q3 = false;
            }
        }

        private static void GetCicle(string Cicle, bool _this, bool Next, Telegram.Bot.Args.MessageEventArgs msg)
        {
            try
            {
                string[] split = Cicle.Split('.');
                string day = "";
                if (split[0].Length > 2)
                {
                    split[0] = split[0].Remove(0, split[0].Length - 2);
                }
                if (split[2].Length > 4)
                {
                    day = split[2].Remove(0, split[2].Length - 2);
                    split[2] = split[2].Remove(4, split[2].Length - 4);
                }
                if (split[4].Length > 4)
                {
                    split[4] = split[4].Remove(4, split[4].Length - 4);
                }
                DateTime FirstDate = DateTime.Parse($"{split[0]}.{split[1]}.{split[2]}");
                DateTime LastDate = DateTime.Parse($"{day}.{split[3]}.{split[4]}");
                if ((LastDate - FirstDate).Days < 0)
                {
                    BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Різниця між датами не може бути від'ємна");
                    _this = false;                                                                                            /////ЗАМІНИВ
                    Next = true;
                }
                else
                {
                    _this = false;
                    Next = true;
                    NotificationDates = new DateTime[(LastDate - FirstDate).Days + 1];
                    for (int i = 0; i < (LastDate - FirstDate).Days + 1; i++)
                    {
                        DateTime local = FirstDate;
                        NotificationDates[i] = local.AddDays(i);
                        Console.WriteLine(NotificationDates[i]);
                    }
                    Console.WriteLine("Уведомления будут длиться: " + ((LastDate - FirstDate).Days + 1) + " дней");
                    BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Вкажіть години нагадувань в форматі год:хв через пробіл");
                }
            }
            catch
            {
                BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Неверно задан период", replyMarkup: StartButtons());
                _this = false;
                Next = false;
                Q0 = false;
                Q1 = false;
                Q2 = false;
                Q3 = false;
            }
        }

        private static void SetNotificationDates(string Times, bool _this, bool Next, Telegram.Bot.Args.MessageEventArgs msg)
        {
            try
            {
                string[] split = Times.Split(' ');
                List<Tuple<string, string>> list = new List<Tuple<string, string>>();
                for (int i = 0; i < split.Length; i++)
                {
                    list.Add(new Tuple<string, string>(split[i].Split(':')[0], split[i].Split(':')[1]));
                }

                NotificationTimes = new DateTime[NotificationDates.Length * NotesCounter];
                int c = 0;
                int Total = 0;
                for (int i = 0; i < NotificationDates.Length * NotesCounter; i++)
                {
                    if (c >= NotesCounter)
                    {
                        c = 0;
                        Total++;
                    }

                    NotificationTimes[i] = NotificationDates[Total].AddHours(int.Parse(list[c].Item1)).AddMinutes(int.Parse(list[c].Item2));
                    c++;
                }

                _this = false;
                Next = true;
                BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Вкажіть тип лікарського засобу");
            }
            catch
            {
                _this = false;
                Next = false;
                BotClient.SendTextMessageAsync(msg.Message.Chat.Id, "Ошибка! Возможно неверный ввод", replyMarkup: StartButtons());
                Q0 = false;
                Q2 = false;
                Q3 = false;
                Q4 = false;
                Q5 = false;
            }
        }

        private static IReplyMarkup StartButtons()
        {

            var Buttons = new KeyboardButton[][]
            {
                new[]{ new KeyboardButton{ Text = "Список нагадувань"}, new KeyboardButton{Text = "Створити нагадування" } },
                new[]{ new KeyboardButton{ Text = "Редагувати нагадування" }, new KeyboardButton{Text = "Видалити нагадування" } }
            };

            return new ReplyKeyboardMarkup(Buttons, true, false);
        }

        private static IReplyMarkup EditButtons()
        {
            var Buttons = new KeyboardButton[][]
            {
                new[] { new KeyboardButton {Text = "Видалити нагадування" }, new KeyboardButton { Text = "Назад"} },

            };
            return new ReplyKeyboardMarkup(Buttons, true);
        }

        private static IReplyMarkup DelayButtons()
        {
            var Buttons = new KeyboardButton[][]
            {
                new[] {new KeyboardButton { Text = "Принял"}, new KeyboardButton {Text = "Отложить на 30 минут" } },
            };

            return new ReplyKeyboardMarkup(Buttons, true);
        }

        private static IReplyMarkup BackButton()
        {
            KeyboardButton[] back = new KeyboardButton[]
            {
                new KeyboardButton { Text = "Назад"}
            };
            return new ReplyKeyboardMarkup(back, true);
        }
    }


}
