using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QuizBot
{
  /* The following classes are not my own work, but taken from the 
  * werewolf for telegram bot
  * https://github.com/parabola949/Werewolf/
  */
  internal static class UpdateHelper
  {
    internal static bool IsGroupAdmin(Update update)
    {
      return IsGroupAdmin(update.Message.From.Id, update.Message.Chat.Id);
    }

    internal static bool IsGroupAdmin(int user, long group)
    {
      //fire off admin request
      try
      {
        var admin = Program.Bot.GetChatMemberAsync(group, user).Result;
        return admin.Status == ChatMemberStatus.Administrator || admin.Status == ChatMemberStatus.Creator;
      }
      catch
      {
        return false;
      }
    }

    public static bool HasJoined(Player x)
    {
      foreach (var each in GameData.Joined)
      {
        if (x == each.Value) return true;
      }
      return false;
    }
  }

  //Class originally defined in the werewolf for telegram
  public class Command : Attribute
  {
    /// <summary>
    /// The string to trigger the command
    /// </summary>
    public string Trigger { get; set; }

    /// <summary>
    /// Is this command limited to bot admins only
    /// </summary>
    public bool GlobalAdminOnly { get; set; } = false;

    /// <summary>
    /// Is this command limited to group admins only
    /// </summary>
    public bool GroupAdminOnly { get; set; } = false;

    /// <summary>
    /// Developer only command
    /// </summary>
    public bool DevOnly { get; set; } = false;

    /// <summary>
    /// Marks the command as something to block (for example, in support chat)
    /// </summary>
    public bool Blockable { get; set; } = false;

    /// <summary>
    /// Marks the command as to be used within a group only
    /// </summary>
    public bool InGroupOnly { get; set; } = false;

    /// <summary>
    /// Marks the command as to be used within a private chat only
    /// </summary>
    public bool InPrivateOnly { get; set; } = false;
  }
}
