using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizBot
{
  //This file contains all the details with regards to actually running a game
  partial class Game
  {
    private void RunGame()
    {
      while (true)
      {
        #region Night Time
        DoNightCycle();
        Stopwatch.Start();
        while (true)
        {
          if (Stopwatch.ElapsedMilliseconds == settings.NightTime * 1000)
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
        VoteCount = GameData.Alive.ToDictionary(x => x.Id, x => 0);
        Program.BotMessage("VotingStart", settings.LynchTime);
        while (true)
        {
          if(Stopwatch.ElapsedMilliseconds == settings.LynchTime * 1000)
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
    private void ProcessHeals()
    {
      foreach(var player in GetPlayers("doctor"))
      {
        Player.GetPlayer(player.ActionTarget).Healed = true;
      }
    }

    private void ProcessEscort()
    {
      foreach (var player in GetPlayers("escort"))
      {
        if(player.role == "doctor")
        { //Reset doctor RB
          player.ActionTarget.Healed = false;
        }
        else if(player.ActionTarget.role == "serial killer")
        { //Escort visiting SK kills them
          player.Kill(player.ActionTarget);
        }
        Player.GetPlayer(player.ActionTarget).IsRoleBlocked = true;
      }

      foreach (var player in GetPlayers("consort"))
      {
        if (player.ActionTarget.role == "doctor")
        {
          player.ActionTarget.ActionTarget.Healed = false;
        }
        else if (player.ActionTarget.role == "serial killer")
        { 
          player.Kill(player.ActionTarget);
        }
        Player.GetPlayer(player.ActionTarget).IsRoleBlocked = true;
      }
    }

    private void AnnounceRB()
    {
      foreach (var player in GameData.Alive.Where(x => x.role != "serial killer"))
      { //SK cannot be rbed
        if(player.IsRoleBlocked) Program.BotMessage(player.Id, "Roleblocked");
      }
    }

    private void ProcessSK()
    {
      foreach(var player in GetPlayers("serial killer"))
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
    private void ProcessMafioso()
    {
      foreach (var player in GetPlayers("mafioso"))
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

    private void ProcessInvest()
    {
      foreach(var player in GetPlayers("investigator"))
      {
        if (player.IsRoleBlocked) continue;
        Program.BotMessage("InvestResult", GameData.InvestResults[player.ActionTarget.role.InvestResult]);
      }
    }

    private void UpdateRolesAfterNight()
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

    private void AnnounceDeaths()
    { //Announce all the deaths
      StringBuilder output = new StringBuilder("");
      foreach(var dead in GameData.Joined.Where(x => !x.IsAlive))
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

    private void DoNightCycle()
    {
      // Step 1: Send the users their options
      foreach(var player in GameData.Alive.Where(x => x.role.HasNightAction))
      {
        Program.BotMessage(player.Id, "Instruct", player.role.Instruction);
        Program.Bot.SendTextMessageAsync(player.Id, "", replyMarkup: 
          GetMarkup(player, CurrentGroup + Protocols["NightActions"], player.role.AllowSelf, player.role.AllowOthers));
      }
    }

    private void DoLynchCycle()
    {
      foreach(var player in GameData.Alive)
      {
        var message = Program.Bot.SendTextMessageAsync(player.Id, "Who would you like to lynch?", replyMarkup:
          GetMarkup(player, CurrentGroup + Protocols["Vote"], false)).Result;
        Messages.Add(Messages.Count, new Tuple<int, int>(player.Id, message.MessageId));
      }
    }

    private void CheckWinConditions()
    {
      switch(GameData.AliveCount)
      {
        case 1:
          {
            break;
          }
      }
    }

    public void ParseNightAction(Callback data)
    {
      var target = GetPlayer(int.Parse(data.Data));
      var player = GetPlayer(data.From);
      player.ActionTarget = target;
      BotMessage(data.From, "ChosenTarget", player.ActionTarget.Name);
    }
    #endregion

    #region Lynch Stuff
    public void ParseVoteChoice(Callback data)
    {
      BotMessage(data.From, "VoteReceived", Player.GetPlayer(int.Parse(data.Data)).Username);
      VoteCount[int.Parse(data.Data)]++;
    }

    private bool GetLynch(out int output)
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

    private Dictionary<int, int> VoteCount;

    private Dictionary<int, Tuple<int, int>> Messages;
    #endregion

    private System.Diagnostics.Stopwatch Stopwatch;

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
        foreach (var player in GameData.Alive)
        {
          if (!allowSelf && player == self) continue;
          markup[i] = new[] { new InlineKeyboardButton(player.Name, new Callback(self.Id, protocol, player.Id.ToString())) };
        }
      }
      return new InlineKeyboardMarkup(markup);
    }

    private Player GetPlayer(long Id)
    {
      return Alive.Where(x => x.Id == Id).ToArray()[0];
    }

    public Dictionary<string, Action<Callback>> Parsers { get; private set; }
  }
}
