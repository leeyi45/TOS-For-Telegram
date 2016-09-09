using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizBot
{
	static class Config
	{
    private static InlineKeyboardButton GetButton(string text, int Id, string protocol, string data)
    {
      return new InlineKeyboardButton(text, new Callback(Id, protocol, data));
    }

		public static void SendInline(User lol, string title)
		{
      var id = Program.Bot.SendTextMessageAsync(lol.Id, "Configuration Options for group: " + title,
        replyMarkup: GetMarkup(lol.Id)).Result.MessageId;
      try { MessageIds.Add(lol.Id, new Tuple<string, int>(title, id)); }
      catch(ArgumentException)
      {
        MessageIds[lol.Id] = new Tuple<string, int>(title, id);
      }
      Program.ConsoleLog("Sent config markup to " + lol.Username);
    }

    private static InlineKeyboardMarkup GetMarkup(int Id)
    {
      var keys = Settings.SetPropertyValue.Keys.ToArray();
      var values = Settings.SetPropertyValue.Values.ToArray();
      var buttons = new InlineKeyboardButton[(int)Math.Floor((decimal)(Settings.SettingCount / 2))][];
      for (int i = 0; i < Settings.SettingCount; i += 2)
      {
        buttons[i / 2] = new[] { GetButton(values[i].DisplayName, Id, GetProtocol("ConfigOption"), keys[i]),
          GetButton(values[i+1].DisplayName, Id, GetProtocol("ConfigOption"), keys[i+1]) };
      }
      return new InlineKeyboardMarkup(buttons);
    }

    private static InlineKeyboardMarkup GetCancel(string protocol, int Id)
    {
      return new InlineKeyboardMarkup(new[]
          {
            new[] { GetButton("Cancel", Id, protocol, "cancel") }
          });
    }

    private static Dictionary<int, Tuple<string, int>> MessageIds;

    private static Dictionary<string, Action> Functions;

    private static string GetProtocol(string key)
    {
      return GameData.Protocols[key];
    }

    public static void Load()
    {
      MessageIds = new Dictionary<int, Tuple<string, int>>();
      Functions = new Dictionary<string, Action>();
      CommandVars.ReceivingVals = new Dictionary<int, Tuple<bool, string>>();
    }

    public async static void Parse(Callback data)
    {
      var markup = new InlineKeyboardMarkup(new[]
      { new [] 
        {
          GetButton("Change Value", data.From, GetProtocol("SelectedConfigOption"), "change " + data.Data),
          GetButton("Cancel", data.From, GetProtocol("SelectedConfigOption"), "cancel")
        }
      });
      var option = Settings.SetPropertyValue[data.Data];
      try
      {
        await Program.Bot.EditMessageTextAsync(data.From, MessageIds[data.From].Item2,
          "Current value: " + option.Info.GetValue(null), replyMarkup: markup);
      }
      catch(KeyNotFoundException) { }
    }

    public async static void ChangeParse(Callback data)
    {
      var stuff = data.Data.Split(' ');
      if(stuff[0] == "cancel")
      {
        await Program.Bot.EditMessageTextAsync(data.From, MessageIds[data.From].Item2,
          "Configuration Options for group: " + MessageIds[data.From].Item1,
          replyMarkup: GetMarkup(data.From));
      }
      else if(stuff[0] == "change")
      {
        await Program.Bot.EditMessageTextAsync(data.From, MessageIds[data.From].Item2,
          "Please send the new value", replyMarkup: 
          GetCancel(GetProtocol("SelectedConfigOption"), data.From));
        try { CommandVars.ReceivingVals[data.From] = new Tuple<bool, string>(true, stuff[1]); }
        catch(KeyNotFoundException)
        {
          CommandVars.ReceivingVals.Add(data.From, new Tuple<bool, string>(true, stuff[1]));
        }
      }
    }

    public async static void ParseValue(Message data)
    {
      var receiving = CommandVars.ReceivingVals[data.From.Id];
      if (receiving.Item1)
      {
        try
        {
          Settings.SetPropertyValue[receiving.Item2].SetValue(data.Text);
          Program.EditBotMessage(data.From.Id, MessageIds[data.From.Id].Item2, 
            "ValueChanged", GetCancel(GetProtocol("ChangingConfigOption")
            , data.From.Id), receiving.Item2, data.Text);
          CommandVars.ReceivingVals[data.From.Id] = 
            new Tuple<bool, string>(false, string.Empty);
        }
        catch(ArgumentException)
        {
          Program.EditBotMessage(data.From.Id, MessageIds[data.From.Id].Item2,
            "InvalidValue", GetCancel(GetProtocol("ChangingConfigOption")
            , data.From.Id), data.Text, receiving.Item2);
        }
      }
    }
	}
}
