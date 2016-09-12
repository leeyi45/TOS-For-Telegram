using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json.Linq;

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
    public Alignment() { Name = "Any"; }

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

    public override string Name
    {
      get
      {
        if (name != "Any") return team.ToString() + " " + name;
        else return "Any";
      }
      set { name = value; }
    }

    public override string ToString()
    {
      return Name;
    }

    #region Operators
    public static bool operator ==(Alignment rhs, Alignment lhs)
    {
      return (rhs.name == lhs.name && rhs.team == lhs.team);
    }

    public static bool operator !=(Alignment Rhs, Alignment lhs)
    {
      return !(Rhs == lhs);
    }

    public static implicit operator Alignment(string test)
    {
      return Parse(test);
    }

    public override bool Equals(object obj)
    {
      if (obj is Alignment) return this == (Alignment)obj;
      else return base.Equals(obj);
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
    [Obsolete("Use an array constructor instead", false)]
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
    /// <summary>
    /// Brief description of the role
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The instruction that users who have this role are given
    /// </summary>
    public string Instruction { get; set; }

    /// <summary>
    /// The alignment of the role
    /// </summary>
    public Alignment Alignment { get; set; }

    /// <summary>
    /// Boolean value indicating if it has a night action
    /// </summary>
    public bool HasNightAction { get; set; }

    /// <summary>
    /// Boolean value indicating if it has a day action
    /// </summary>
    public bool HasDayAction { get; set; }

    /// <summary>
    /// Boolean value indicating if the role is night immune
    /// </summary>
    public bool NightImmune { get; set;}

    /// <summary>
    /// Integer value indicating which investigator result to display
    /// </summary>
    public int InvestResult { get; set; }

    /// <summary>
    /// Boolean value indicating if it is suspicious
    /// </summary>
    public bool Suspicious { get; set; }

    /// <summary>
    /// Boolean value indicating if the role is unique
    /// </summary>
    public bool Unique { get; set; }

    // The following two fields are for sending the callback data
    public bool AllowOthers { get; set; }

    public bool AllowSelf { get; set; }
    #endregion

    public override string ToString()
    {
      return Name + ": " + Description;
    }

    public string ToString(bool thing)
    {
      return base.ToString();
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

    public static bool operator ==(Role lhs, string rhs)
    {
      return rhs == lhs.Name.ToLower();
    }

    public static bool operator !=(Role lhs, string rhs)
    {
      return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
      if (obj is Role) return this == (Role)obj;
      else return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    #endregion
  }
}
