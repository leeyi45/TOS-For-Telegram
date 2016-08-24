using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot
{
  //This class is here so I can store both attributes and roles in the same dictionary
  /// <summary>
  /// Wrapper class for roles and attributes
  /// </summary>
  public class Wrapper
  {
    public virtual Team team { get; set; }

    public virtual string Name { get; set; }
  }

  /// <summary>
  /// Class to represent a team
  /// </summary>
  public class TeamWrapper : Wrapper
  {
    public TeamWrapper(Team team)
    {
      this.team = team;
    }

    public override string Name
    {
      get { return "Random " + team.ToString(); }
    }
  }

  /// <summary>
  /// Class representing the type of role
  /// </summary>
  public class Alignment : Wrapper
  {
    /// <summary>
    /// Initializes an "Any" attribute
    /// </summary>
    public Alignment()
    {
      Name = "Any";
    }

    public Alignment(string name, Team team)
    {
      this.team = team;
      this.name = name;
    }

    public static Alignment Parse(string input)
    {
      string[] args = input.Split(' ');
      if (args.Length > 2 || string.IsNullOrWhiteSpace(input)) throw new FormatException();
      return new Alignment(args[1], (Team)Enum.Parse(typeof(Team), args[0]));
    }

    private string name;

    #region Operators
    public override string Name
    {
      get
      {
        if (name != "Any") return team.ToString() + " " + name;
        else return "Any";
      }
      set { name = value; }
    }

    public static bool operator ==(Alignment rhs, Alignment lhs)
    {
      return (rhs.name == lhs.name && rhs.team == lhs.team);
    }

    public static bool operator !=(Alignment Rhs, Alignment lhs)
    {
      return !(Rhs == lhs);
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

  /// <summary>
  /// Class to represent a role
  /// </summary>
	public class Role : Wrapper
  {
    public Role(string _name, Team _team, string d, Alignment a, bool n, bool day)
    {
      Name = _name;
      team = _team;
      Alignment = a;
      Description = d;
      HasNightAction = n;
      HasDayAction = day;
    }

    public Role() { }

    #region Properties
    public string Description { get; set; }

    public Alignment Alignment { get; set; }

    public bool HasNightAction { get; set; }

    public bool HasDayAction { get; set; }

    public bool NightImmune { get; set;}

    public int InvestResult { get; set; }

    public bool Suspicious { get; set; }
    #endregion

    public virtual void DoDayAction()
    {
      if (!HasDayAction) return;
      //Game.DayRoleActions[Name]();
    }

    public virtual void DoNightAction()
    {
      if (!HasNightAction) return;
      //Game.NightRoleActions[Name]();
    }

    #region Operators
    public static bool operator ==(Role rhs, Role lhs)
    {
      return (rhs.Name == lhs.Name);
    }

    public static bool operator !=(Role rhs, Role lhs)
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
