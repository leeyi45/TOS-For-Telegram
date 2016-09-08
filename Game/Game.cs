using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizBot
{
	static class Game
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

    public static void RunGame()
    {
      Stopwatch = new System.Diagnostics.Stopwatch();
      Messages = new Dictionary<int, Tuple<int, int>>();
      while (true)
      {
        #region Night Time
        DoNightCycle();
        Stopwatch.Start();
        while (true)
        {
          if (Stopwatch.ElapsedMilliseconds == Settings.NightTime * 1000)
          {
            Stopwatch.Stop();
            UpdateRolesAfterNight();
            break;
          }
        }
        #endregion
        AnnounceDeaths();
        #region Voting time
        Stopwatch.Reset();
        VoteCount = GameData.Alive.ToDictionary(x => x.Value.Id, x => 0);
        Program.BotMessage("VotingStart", Settings.LynchTime);
        while (true)
        {
          if(Stopwatch.ElapsedMilliseconds == Settings.LynchTime * 1000)
          {
            foreach(var message in Messages.Values)
            {
              Program.Bot.EditMessageTextAsync(message.Item1, message.Item2, "Time's up!");
            }
            Stopwatch.Stop();
            break;
          }
        }

        #endregion
        CheckWinConditions();
      }
    }

    #region Night Stuff
    private static void ProcessHeals()
    {
      foreach(var player in ReturnPlayers(GameData.Roles["doctor"]))
      {
        Player.GetPlayer(player.ActionTarget).Healed = true;
      }
    }

    private static void ProcessEscort()
    {
      foreach (var player in ReturnPlayers(GameData.Roles["escort"]))
      {
        if(player.role == GameData.Roles["doctor"])
        { //Reset doctor RB
          player.ActionTarget.Healed = false;
        }
        else if(player.ActionTarget.role == GameData.Roles["serial killer"])
        { //Escort visiting SK kills them
          player.Kill(player.ActionTarget);
        }
        Player.GetPlayer(player.ActionTarget).IsRoleBlocked = true;
      }

      foreach (var player in ReturnPlayers(GameData.Roles["consort"]))
      {
        if (player.ActionTarget.role == GameData.Roles["doctor"])
        {
          player.ActionTarget.ActionTarget.Healed = false;
        }
        else if (player.ActionTarget.role == GameData.Roles["serial killer"])
        { 
          player.Kill(player.ActionTarget);
        }
        Player.GetPlayer(player.ActionTarget).IsRoleBlocked = true;
      }
    }

    private static void AnnounceRB()
    {
      foreach (var player in GameData.Alive.Where(x => x.Value.role != GameData.Roles["serial killer"]))
      { //SK cannot be rbed
        if(player.Value.IsRoleBlocked) Program.BotMessage(player.Value.Id, "Roleblocked");
      }
    }

    private static void ProcessSK()
    {
      foreach(var player in ReturnPlayers(GameData.Roles["serial killer"]))
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
    private static void ProcessMafioso()
    {
      foreach (var player in ReturnPlayers(GameData.Roles["mafioso"]))
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

    private static void ProcessInvest()
    {
      foreach(var player in ReturnPlayers(GameData.Roles["investigator"]))
      {
        if (player.IsRoleBlocked) continue;
        Program.BotMessage("InvestResult", GameData.InvestResults[player.ActionTarget.role.InvestResult]);
      }
    }

    private static void UpdateRolesAfterNight()
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

    private static void AnnounceDeaths()
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

    private static void DoNightCycle()
    {
      // Step 1: Send the users their options
      foreach(var player in GameData.Alive.Values.Where(x => x.role.HasNightAction))
      {
        Program.BotMessage(player.Id, "Instruct", player.role.Instruction);
        Program.Bot.SendTextMessageAsync(player.Id, "", replyMarkup: 
          GetMarkup(player, "NightAction", player.role.AllowSelf, player.role.AllowOthers));
      }
    }

    private static void DoLynchCycle()
    {
      foreach(var player in GameData.Alive.Values)
      {
        var message = Program.Bot.SendTextMessageAsync(player.Id, "Who would you like to lynch?", replyMarkup:
          GetMarkup(player, "VoteAction", false)).Result;
        Messages.Add(Messages.Count, new Tuple<int, int>(player.Id, message.MessageId));
      }
    }

    private static void CheckWinConditions()
    {
      switch(GameData.AliveCount)
      {
        case 1:
          {
            break;
          }
      }
    }

    public static void ParseNightAction(Callback data)
    {
      var target = Player.GetPlayer(int.Parse(data.Data));
      var player = Player.GetPlayer(data.From);
      player.ActionTarget = target;
      Program.BotMessage(data.From, "ChosenTarget", player.ActionTarget.Name);
    }
    #endregion

    #region Lynch Stuff
    public static void ParseVoteChoice(Callback data)
    {
      Program.BotMessage(data.From, "VoteReceived", Player.GetPlayer(int.Parse(data.Data)).Username);
      VoteCount[int.Parse(data.Data)]++;
    }

    private static bool GetLynch(out int output)
    {
      int value = VoteCount.Values.Max();
      var values = VoteCount.Where(x => x.Value == value).ToArray();
      if (values.Length != 1)
      {
        output = 0;
        return false;
      }
      else
      {
        output = values[0].Key;
        return true;
      }
    }

    private static Dictionary<int, int> VoteCount;

    private static Dictionary<int, Tuple<int, int>> Messages;
    #endregion

    private static System.Diagnostics.Stopwatch Stopwatch;

    public static InlineKeyboardMarkup GetMarkup(Player self, string protocol, bool allowSelf = true, 
      bool allowOthers = true)
    {
      var markup = new InlineKeyboardButton[GameData.AliveCount][];
      int i = 0;
      if (!allowOthers)
      {
        markup[0] = new[] { new InlineKeyboardButton(self.Name, new Callback(self.Id, protocol, self.Id.ToString())) };
      }
      else
      {
        foreach (var player in GameData.Alive.Values)
        {
          if (!allowSelf && player == self) continue;
          markup[i] = new[] { new InlineKeyboardButton(player.Name, new Callback(self.Id, protocol, player.Id.ToString())) };
        }
      }
      return new InlineKeyboardMarkup(markup);
    }
  }
}
