using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

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

    public Role role { get; set; }

    #region User Fields
    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Username { get; private set; }

    public int Id { get; private set; }
    #endregion

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
    /// The player which is responsible the player
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
      Program.BotMessage(Id, wasKilledBy.role.Name + " Death"); //Tell the user they have been killed!
    }
    #endregion

    public void OnAssignRole()
    {
      Program.BotMessage(Id, "You are the " + role.Name + "!\n", role.Name + "Assign");
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
