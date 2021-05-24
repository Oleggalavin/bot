using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;
//using System.Type;
namespace bot
{
    internal class TelegramBotHelper
    {
        private const string TEXT_1 = "1";
        private const string TEXT_2 = "2";
        private const string TEXT_3 = "3";
        private const string TEXT_4 = "4";

        private string _token;
        Telegram.Bot.TelegramBotClient _client;

        public TelegramBotHelper(string token)
        {
            this._token = token;
        }

        internal void GetUpdates()
        {
            _client = new Telegram.Bot.TelegramBotClient(_token);
            var me = _client.GetMeAsync().Result;
            if (me != null && !string.IsNullOrEmpty(me.Username))
            {
                int offset = 0;
                while (true)
                {
                    try
                    {
                        var updates = _client.GetUpdatesAsync(offset).Result;
                        if (updates != null && updates.Count() > 0) ;
                        {
                            foreach (var update in updates)
                            {
                                processUpdate(update);
                                offset = update.Id + 1;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }

                    Thread.Sleep(1000);
                }
            }
        }

        private void processUpdate(Telegram.Bot.Types.Update update)
        {
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                    var text = update.Message.Text;
                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Receive text" + text, replyMarkup: GetButtons());
                    break;
                default:
                    Console.WriteLine(update.Type + "Not implemented");
                    break;

            }
        }

        private IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton { Text = TEXT_1 }, new KeyboardButton { Text = TEXT_2 }, },
                    new List<KeyboardButton> { new KeyboardButton { Text = TEXT_3 }, new KeyboardButton { Text = TEXT_4 } }
                }
            };


        }
    }
}