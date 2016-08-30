using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizBot
{
	class Game
	{
    /// <summary>
    /// Return players with the role specified
    /// </summary>
    /// <param name="condition">The role to search for</param>
    /// <returns>An array containing the players that fit the definition</returns>
    public static Player[] ReturnPlayers(Role condition)
    {
      return GameData.Alive.Values.Where(x => x.role == condition).ToArray();
    }

    public static void ProcessHeals()
    {
      foreach(var player in ReturnPlayers(GameData.Roles["Doctor"]))
      {
        GameData.GetPlayer(player.ActionTarget).Healed = true;
      }
    }

    public static void ProcessEscort()
    {
      foreach (var player in ReturnPlayers(GameData.Roles["Escort"]))
      {
        if(player.role == GameData.Roles["Doctor"])
        { //Reset doctor RB
          player.ActionTarget.Healed = false;
        }
        else if(player.ActionTarget.role == GameData.Roles["Serial Killer"])
        { //Escort visiting SK kills them
          player.Kill(player.ActionTarget);
        }
        GameData.GetPlayer(player.ActionTarget).IsRoleBlocked = true;
      }

      foreach (var player in ReturnPlayers(GameData.Roles["Consort"]))
      {
        if (player.role == GameData.Roles["Doctor"])
        {
          player.ActionTarget.Healed = false;
        }
        else if (player.ActionTarget.role == GameData.Roles["Serial Killer"])
        { 
          player.Kill(player.ActionTarget);
        }
        GameData.GetPlayer(player.ActionTarget).IsRoleBlocked = true;
      }
    }

    public static void AnnounceRB()
    {
      foreach (var player in GameData.Alive.Where(x => x.Value.role != GameData.Roles["Serial Killer"]))
      { //SK cannot be rbed
        if(player.Value.IsRoleBlocked) Program.BotMessage(player.Value.Id, "Roleblocked");
      }
    }

    public static void ProcessSK()
    {
      foreach(var player in ReturnPlayers(GameData.Roles["Serial Killer"]))
      {
        if(player.role.NightImmune)
        { //Inform the player
          
        }
        else
        {
          player.ActionTarget.Kill(player);
        }
      }
    }
    //Perhaps combine the two functions idk
    public static void ProcessMafioso()
    {
      foreach (var player in ReturnPlayers(GameData.Roles["Mafioso"]))
      {
        if (player.IsRoleBlocked) continue;
        if (player.role.NightImmune)
        { //Inform the player

        }
        else
        {
          player.ActionTarget.Kill(player);
        }
      }
    }

    public static void ProcessInvest()
    {
      foreach(var player in ReturnPlayers(GameData.Roles["Investigator"]))
      {
        if (player.IsRoleBlocked) continue;
        Program.BotMessage("InvestResult", GameData.InvestResults[player.ActionTarget.role.InvestResult]);
      }
    }

    public static void UpdateRolesAfterNight()
    {
      //Process the doctors' first
      ProcessHeals();

      //Process the roleblocks next
      ProcessEscort();

      //Process the SKs next
      ProcessSK();

      //Process the mafioso
      ProcessMafioso();

      //Announce to all those who have roleblocked
      AnnounceRB();
    }

    public static void AnnounceDeaths()
    { //Announce all the deaths
      StringBuilder output = new StringBuilder("");
      foreach(var dead in GameData.Joined.Values.Where(x => !x.IsAlive))
      {
        if (dead.WasKilledBy != null)
        {
          output.Append(string.Format(GameData.Messages[dead.WasKilledBy.role.Name + " PublicDeath"] +
            dead.Username));
        }
        else output.Append(dead.Username + " apparently committed suicide.");
        output.AppendLine(" He/she was the " + dead.role.Name + "\n");
      }
      Program.BotMessage(output.ToString());
    }

    public static void DoNightCycle()
    {
      foreach(var player in GameData.Joined.Values.Where(x => x.role.HasNightAction))
      {
        Program.BotMessage(player.Id, "Instruct", player.role.Instruction);
        Program.Bot.SendTextMessageAsync(player.Id, "", replyMarkup: GetMarkup(player));
      }
    }

    public static InlineKeyboardMarkup GetMarkup(Player self, bool allowSelf = true, string protocol = "nightAction")
    {
      var markup = new InlineKeyboardButton[GameData.AliveCount][];
      int i = 0;
      foreach(var player in GameData.Alive.Values)
      {
        if (!allowSelf && player == self) continue;
        markup[i] = new[] { new InlineKeyboardButton(player.Name, new Callback(self, protocol, player.Id.ToString())) };
      }
      return new InlineKeyboardMarkup(markup);
    }

    public static void ParseNightAction(Callback data)
    {
      data.From.ActionTarget = GameData.Alive.Values.Where(x => x.Id == int.Parse(data.Data)).ToArray()[0];
      Program.BotMessage(data.From.Id, "ChosenTarget", data.From.ActionTarget.Name);
    }
  }
}
