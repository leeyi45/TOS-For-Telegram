using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizBot
{
  class Game
  {
    public static Dictionary<string, RoleAction> DayRoleActions;

    public static Dictionary<string, RoleAction> NightRoleActions;

    public delegate void RoleAction();

    public static void LoadDayRoles()
    {
      DayRoleActions = new Dictionary<string, RoleAction>();
      foreach(var method in typeof(DayRoles).GetMethods()
        .Where(x => x.GetCustomAttribute(typeof(RoleAttribute)) != null))
      {
        DayRoleActions.Add((method.GetCustomAttribute(typeof(RoleAttribute)) as RoleAttribute).role,
          (RoleAction)Delegate.CreateDelegate(typeof(RoleAction), method));
      }
    }

    public static void LoadNightRoles()
    {
      NightRoleActions = new Dictionary<string, RoleAction>();
      foreach (var method in typeof(NightRoles).GetMethods()
        .Where(x => x.GetCustomAttribute(typeof(RoleAttribute)) != null))
      {
        NightRoleActions.Add((method.GetCustomAttribute(typeof(RoleAttribute)) as RoleAttribute).role,
          (RoleAction)Delegate.CreateDelegate(typeof(RoleAction), method));
      }
    }

    private class DayRoles
    {
      [RoleAttribute(role="Escort")]
      public static void Escort()
      {
        
      }
    }

    private class NightRoles
    {

    }

    private class RoleAttribute : Attribute
    {
      public string role { get; set; }
    }

    /*
    private ActionObject GetKeyboardMarkup(Player from)
    {
      InlineKeyboardButton[][] buttons = new InlineKeyboardButton[GameData.AliveCount][];
      int i = 0;
      foreach(var each in GameData.Joined.Where(x => x.Value.IsAlive))
      {
        var player = each.Value;
        buttons[i] = new InlineKeyboardButton[] { new InlineKeyboardButton(player.Name, string.Join(" ", player.Name, from.Name)) };
      }
      var test = new InlineKeyboardMarkup(buttons);
      return new ActionObject(from, test);
    }*/
  }
}
