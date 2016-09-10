﻿using System.Linq;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static QuizBot.GameData;
using System;

namespace QuizBot
{
  /// <summary>
  /// Class representing a player in the game
  /// </summary>
	public class Player
  {
    public Player() { }

    public Player(Role _R)
    {
      role = _R;
    }

    public Player(User x)
    {
      FirstName = x.FirstName;
      LastName = x.LastName;
      Id = x.Id;
      Username = x.Username;
    }

    public Player(int Id, string username, string FirstName, string LastName)
    {
      this.Id = Id;
      if (string.IsNullOrWhiteSpace(username)) Username = string.Empty;
      else Username = username;
      this.FirstName = FirstName;
      this.LastName = LastName;
    }

    public Role role { get; set; }

    #region User Fields
    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Username
    {
      get
      {
        if (string.IsNullOrWhiteSpace(Username)) return Name;
        else return username;
      }
      set { username = value; }  
    }

    public int Id { get; private set; }
    #endregion

    private string username;

    public string Name { get { return FirstName + " " + LastName; } }

    public string Nickname { get; set; }

    #region Game Properties
    /// <summary>
    /// Boolean value indicating if the player is alive
    /// </summary>
    public bool IsAlive { get; set; } = true;

    /// <summary>
    /// Boolean value indicating if the player has been doused by the arsonist
    /// </summary>
    public bool IsDoused { get; set; }

    /// <summary>
    /// Boolean value indicating if the player has been roleblocked
    /// </summary>
    public bool IsRoleBlocked { get; set; }

    /// <summary>
    /// Boolean value indicating if the player was healed by a doctor
    /// </summary>
    public bool Healed { get; set; }

    /// <summary>
    /// The player on which the player intends to perform his action
    /// </summary>
    public Player ActionTarget { get; set; }

    private Player killedby;

    /// <summary>
    /// The player which is responsible for killing the player
    /// </summary>
    public Player WasKilledBy
    {
      get
      {
        if (IsAlive) return null;
        else return killedby;
      }
      set
      {
        killedby = value;
      }
    }

    #endregion

    #region Game Methods
    public void Kill(Player wasKilledBy)
    {
      WasKilledBy = wasKilledBy;
      IsAlive = false;
      //Tell the user they have been killed!
      Program.BotMessage(Id, wasKilledBy.role.Name + " Death");
    }

    public void Kill()
    {
      IsAlive = false;
    }

    public async void SendMessage(string message, IReplyMarkup markup = null)
    {
      await Program.Bot.SendTextMessageAsync(Id, message, replyMarkup: markup);
    }
    #endregion

    public void OnAssignRole()
    {
      Program.BotMessage(Id, "You are the " + role.Name + "!\n", role.Name + "Assign");
    }

    public static Player GetPlayer(long Id, bool dead = false)
    {
      System.Collections.Generic.List<Player> searchFrom;
      if (!dead) searchFrom = Joined;
      else searchFrom = Alive;

      try { return searchFrom.Where(x => x.Id == Id).ToArray()[0]; }
      catch(IndexOutOfRangeException) { return null; }
    }

    public static Player GetPlayer(Player test, bool dead = false)
    {
      System.Collections.Generic.List<Player> searchFrom;
      if (!dead) searchFrom = Alive;
      else searchFrom = Joined;

      if (!searchFrom.Contains(test)) return null;
      else
      {
        try { return searchFrom.Where(x => x == test).ToArray()[0]; }
        catch(IndexOutOfRangeException) { return null; }
      }
    }

    #region Operators
    public static implicit operator Player(User x)
    {
      return new Player(x);
    }

    public static bool operator ==(Player rhs, Player lhs)
    {
      return rhs.Id == lhs.Id;
    }

    public static bool operator !=(Player rhs, Player lhs)
    {
      return !(rhs == lhs);
    }

    public static bool operator ==(Player rhs, User lhs)
    {
      try { return (rhs.Id == lhs.Id); }
      catch(NullReferenceException) { return false; }
    }

    public static bool operator !=(Player rhs, User lhs)
    {
      return !(rhs == lhs);
    }

    public override bool Equals(object obj)
    {
      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    #endregion
  }
}
