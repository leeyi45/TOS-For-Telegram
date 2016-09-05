using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using static QuizBot.Program;

namespace QuizBot
{
  class GameData
  {
    #region Intialization
    public const string xmlFile = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\TOS-For-Telegram\Game\Roles.xml";

		public const string messageFile = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\TOS-For-Telegram\Game\Messages.xml";

		private static void InitialErr(string message, InitException e)
		{
      MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      ConsoleLog(e.Message);
    }

    #region Role creation functions
    public static void Error(string message, XElement each)
    {
      throw new InitException("Failed to get " + message, each);
    }

    private static bool[] GetHasActionValues(XElement each)
    {
      bool[] values = new bool[4];
      string[] data = { "HasDayAction", "HasNightAction", "AllowSelf", "AllowOthers" };
      string parse;
      for (int i = 0; i < 4; i++)
      {
        parse = each.TryGetStringElement(data[i], true);
        bool temp;
        if (!bool.TryParse(parse, out temp)) temp = true;
        values[i] = temp;
      }
      return values;
    }

    private static Team GetTeam(XElement each)
    {
      Team team;
      try { team = (Team)Enum.Parse(typeof(Team), each.TryGetStringElement("Team")); }
      catch (ArgumentException) { throw new InitException("Only \"Town\", \"Mafia\" and \"Neutral\" are acceptable values for \"Team\"", each); }
      return team;
    }

    private static Alignment GetAlignment(XElement each, Team team)
    {
      string alignStr;
      if (!each.TryGetElement("Alignment", out alignStr)) Error("alignment", each);
      return new Alignment(each.TryGetStringElement("Name"), team);
    }

    private static bool GetNightImmune(XElement each)
    {
      bool nightImmune = false;
      string boolParse;
      if (each.TryGetElement("NightImmune", out boolParse))
      {
        try { nightImmune = bool.Parse(boolParse); }
        catch (FormatException)
        {
          Error("night immunity value", each);
        }
      }
      return nightImmune;
    }

    private static int GetInvestResult(XElement each)
    {
      string parse = each.TryGetStringElement("Invest");
      int output;
      if (!int.TryParse(parse, out output)) Error("invest result", each);
      return output;
    }
    #endregion
    public static void Log(string text, bool logtoconsole)
    {
      if (logtoconsole) ConsoleLog(text);
      else startup.SetExtraInfo(text);
    }

    public static void InitializeRoles() { InitializeRoles(false); }

    public static void InitializeRoles(bool logtoconsole)
    {
      Roles = new Dictionary<string, Role>();
      RoleLists = new Triptionary<string, Wrapper, int>();
      Alignments = new Dictionary<int, Alignment>();
      InvestResults = new Dictionary<int, string>();

      Log("Loading roles", logtoconsole);
      try
      {
        if (!File.Exists(xmlFile)) throw new InitException("Failed to open role file");

        XDocument document = XDocument.Load(xmlFile, LoadOptions.SetLineInfo);

        #region Version check
        Log("Checking roles.xml file version", logtoconsole);
        if (document.Root.Attribute("version").Value != "1.0")
        {
          throw new InitException("Incorrect role file version");
        }
        Log("File version verified", logtoconsole);
        #endregion

        #region Alignment
        Log("Reading alignments", logtoconsole);
        if (!document.Root.HasElement("Alignments")) throw new InitException("Alignments have not been properly defined");
        foreach (var each in document.Root.Element("Alignments").Elements("Alignment"))
        {
          Team temp = GetTeam(each);
          var align = new Alignment(each.TryGetStringElement("Name"), temp);
          Alignments.Add(Alignments.Count, align);
          Log("Alignment \"" + align.Name + "\" registered", logtoconsole);
        }
       Log("Alignments loaded", logtoconsole);
        #endregion

        #region Roles
        Log("Reading roles", logtoconsole);
        if (!document.Root.HasElement("Roles")) throw new InitException("Roles have not been properly defined");
        foreach (var each in document.Root.Element("Roles").Elements("Role"))
        {
          var team = GetTeam(each);
          var align = new Alignment(each.TryGetStringElement("Alignment"), team);
          var name = each.TryGetStringElement("Name");
          var hasActions = GetHasActionValues(each);

          #region Get Suspicious
          bool suspicious;
          switch (team)
          {
            //Town are all non suspicious
            case Team.Town: { suspicious = false; break; }
            //Maf are all suspicious
            case Team.Mafia: { suspicious = true; break; }
            //The rest check
            default:
              {
                if (align.Name == "Killing") suspicious = true;
                else suspicious = false;
                break;
              }
          }
          #endregion

          //Messages.Add(name + "Assign", each.GetElementValue("OnAssign"));
          Roles.Add(name.ToLower(), new Role
          {
            Name = name,
            team = team,
            Alignment = align,
            HasDayAction = hasActions[0],
            HasNightAction = hasActions[1],
            Description = each.TryGetStringElement("Description"),
            NightImmune = GetNightImmune(each),
            Suspicious = suspicious,
            InvestResult = GetInvestResult(each),
            Instruction = each.TryGetStringElement("Instruct", true),
            AllowOthers = hasActions[2],
            AllowSelf = hasActions[3]
          });
          Log("\"" + name + "\" registered", logtoconsole);
        }
        if (Roles.Count == 0) throw new InitException("There are no roles defined!");

        ConsoleLog("Finished reading roles");
        #endregion

        #region Rolelist
        Log("Loading rolelists", logtoconsole);
        if (!document.Root.HasElement("Rolelists")) throw new InitException("Rolelists are not defined properly!");
        foreach (var rolelist in document.Root.Element("Rolelists").Elements("Rolelist"))
        {
          var listname = rolelist.TryGetStringAttribute("Name");
          foreach (var each in rolelist.Elements("Role"))
          {
            try
            {
              Wrapper To_Add = new Wrapper();
              string value = "";

              if (each.TryGetAttribute("Name", out value))
              { //Normal role definition
                value = value.ToLower();
                try { To_Add = Roles[value]; }
                catch (KeyNotFoundException)
                {
                  throw new InitException("Role \"" + value + "\" does not exist!", each);
                }
              }
              else
              {
                if (each.TryGetAttribute("Alignment", out value))
                { //Alignment defined
                  if (value == "Any") To_Add = new Alignment();
                  else
                  {
                    try { To_Add = Alignment.Parse(value); }
                    catch (ArgumentException)
                    {
                      throw new InitException("Alignment \"" + value + "\" does not exist!", each);
                    }
                  }
                }
                else if (each.TryGetAttribute("Team", out value))
                { //Team defined
                  try { To_Add = new TeamWrapper((Team)Enum.Parse(typeof(Team), value)); }
                  catch (ArgumentException)
                  {
                    throw new InitException("Only \"Town\", \"Mafia\" and \"Neutral\" are acceptable values for \"Team\"", each);
                  }
                }
                else throw new InitException("Unrecognised role definition", each);
              }

              int count = 1;
              //If a count is not defined assume it is one
              try { int.TryParse(each.TryGetStringAttribute("Count", true), out count); }
              catch(Exception) { }

              try { RoleLists[listname].Add(To_Add, count); }
              catch(ArgumentException)
              {
                throw new InitException("\"" + To_Add.Name + "\" has been defined more than once in "
                  + listname);
              }
            }
            catch (InitException e)
            {
              MessageLog(e.Message);
              return;
            }
          }
          Log("Registered new rolelist: " + listname, logtoconsole);
        }
        if (RoleLists.Count == 0) throw new InitException("There are no rolelists defined!");
        ConsoleLog("Finished loading rolelists");
        #endregion

        #region Invest messages
        Log("Loading investigation results", logtoconsole);
        if (!document.Root.HasElement("InvestResults")) throw new InitException("Invest results have not properly been defined");
        foreach (var each in document.Root.Element("InvestResults").Elements("InvestResult"))
        {
          InvestResults.Add(int.Parse(each.TryGetStringAttribute("Key")), each.TryGetStringElement("Value"));
        }
        Log("Invest results loaded", logtoconsole);
        #endregion
      }
      catch(InitException e) when (logtoconsole)
      {
        InitialErr("Failed to load roles.xml, see console for details", e);
      }

      ConsoleLog("Roles loaded");
    }

    public static void InitializeMessages() { InitializeMessages(false); }

    public static void InitializeMessages(bool logtoconsole)
		{
			Log("Loading messages", logtoconsole);
			Messages = new Dictionary<string, string>();

      try
      {
        if (!File.Exists(messageFile)) throw new InitException("Messages", "Missing message file");
        XDocument doc = XDocument.Load(messageFile, LoadOptions.SetLineInfo);

        foreach (var each in doc.Root.Elements("string"))
        {
          string key = each.TryGetStringAttribute("key");
          Messages.Add(key, each.TryGetStringElement("value"));
          Log("Message\"" + key + "\" registered", logtoconsole);
        }
        
      }
      catch(InitException e) when (logtoconsole)
      {
        InitialErr("Failed to load messages.xml, see console for details", e);
      }

  		Log("Loaded messages", logtoconsole);
      //ArrangeXML();
		}
		#endregion

		#region The Properties of Data
		/// <summary>
		/// The number of players currently in the game
		/// </summary>
		public static int PlayerCount { get { return Joined.Count; } }

		/// <summary>
		/// Boolean value indicating whether a game has been started
		/// </summary>
		public static bool GameStarted { 
			get 
			{
				if (GamePhase == GamePhase.Inactive) return false;
				else return true;
			} 
		}

    /// <summary>
    /// The current phase the game is going through
    /// </summary>
    public static GamePhase GamePhase { get; set; } = GamePhase.Inactive;

		/// <summary>
		/// The current group the game is running on
		/// </summary>
		public static long CurrentGroup { get; set; }

    //This time is actually moved back by 8 hours to GMT
		/// <summary>
		/// The time which the bot was started
		/// </summary>
		public static DateTime StartTime { get; set; }

    //Need this to remove stuff for the commands
    public static string BotUsername { get; set; } = "@quiztestbot";

    /// <summary>
    /// The number of players currently alive
    /// </summary>
    public static int AliveCount
    {
      get { return GameData.Joined.Count(x => x.Value.IsAlive); }
    }

    /// <summary>
    /// Boolean value indicatiing if the mayor has revealed himself
    /// </summary>
    public static bool HasRevealed { get; set;}
    #endregion

    #region The Dictionaries of Data
    /// <summary>
    /// Dictionary of all the roles currently defined
    /// </summary>
    public static Dictionary<string, Role> Roles;

		/// <summary>
		/// Contains all the rolelists currently defined
		/// </summary>
		public static Triptionary<string, Wrapper, int> RoleLists;

		public static Dictionary<int, Alignment> Alignments;

		public static Dictionary<int, Player> Joined = new Dictionary<int, Player>();

		/// <summary>
		/// Dictionary containing all the messages
		/// </summary>
		public static Dictionary<string, string> Messages;

    /// <summary>
    /// Dictionary containing all the investigative results
    /// </summary>
    public static Dictionary<int, string> InvestResults;

    public static Dictionary<int, Player> Alive
    {
      get { return Joined.Where(x => x.Value.IsAlive).ToDictionary(x => x.Key, x => x.Value); }
    }
    #endregion

    public static Player GetPlayer(Player test, bool dead = false)
    {
      if (!dead)
      {
        if (!Alive.ContainsValue(test)) return null;
        else
        {
          return Alive.Values.Where(x => x == test).ToArray()[0];
        }
      }
      else
      {
        if (!Joined.ContainsValue(test)) return null;
        else
        {
          return Joined.Values.Where(x => x == test).ToArray()[0];
        }
      }
    }

    private static void ArrangeXML()
    {
      var doc = XDocument.Load(messageFile);
      var elements = new XElement[Messages.Count];
      int i = 0;
      var query = from message in Messages
                  orderby message.Key[0] ascending, message.Key[1] ascending 
                  select message;

      foreach(var message in query)
      {
        elements[i] = new XElement("string", new XElement("value", message.Value));
        elements[i].Add(new XAttribute("key", message.Key));
        i++;
      }
      doc.Save(messageFile);
    }
	}

  /// <summary>
  /// Class containing the settings system
  /// </summary>
	class Settings
	{
		/// <summary>
		/// The maximum number of players allowed per game
		/// </summary>
		public static int MaxPlayers
		{
			get { return Properties.Settings.Default.Max_Users; }
			set { Properties.Settings.Default.Max_Users = value; }
		}

    /// <summary>
    /// The minimum number of players allowed per game
    /// </summary>
    public static int MinPlayers
    {
      get { return Properties.Settings.Default.Min_Users; }
      set { Properties.Settings.Default.Min_Users = value; }
    }

		/// <summary>
		/// The amount of time the join phase is allocated, in seconds
		/// </summary>
		public static int JoinTime
		{
			get { return Properties.Settings.Default.Join_Time; }
			set { Properties.Settings.Default.Join_Time = value; }
		}

		/// <summary>
		/// The amount of time the join phase is allocated, in milliseconds
		/// </summary>
		public static int JoinTimeMili
		{
			get { return Properties.Settings.Default.Join_Time * 1000; }
		}

    /// <summary>
    /// The amount of time the night time phase is allocated, in seconds
    /// </summary>
    public static int NightTime
    {
      get { return Properties.Settings.Default.Night_Cycle; }
      set { Properties.Settings.Default.Night_Cycle = value; }
    }

    /// <summary>
    /// The amount of time the day time phase is allocated, in seconds
    /// </summary>
    public static int DayTime
    {
      get { return Properties.Settings.Default.Day_Cycle; }
      set { Properties.Settings.Default.Day_Cycle = value; }
    }

    /// <summary>
    /// The amount of time the lynch phase is allocated, in seconds
    /// </summary>
    public static int LynchTime
    {
      get { return Properties.Settings.Default.Voting_Cycle; }
      set { Properties.Settings.Default.Voting_Cycle = value; }
    }

    /// <summary>
    /// The currently selected rolelist name
    /// </summary>
    public static string CurrentRoleList
		{
			get { return Properties.Settings.Default.Rolelist; }
			set { Properties.Settings.Default.Rolelist = value; }
		}

		/// <summary>
		/// The currently selected rolelist
		/// </summary>
		public static Dictionary<Wrapper, int> CurrentRoles
		{
			get { return GameData.RoleLists[CurrentRoleList]; }
		}

    /// <summary>
    /// Boolean value indicating if nicknames should be used
    /// </summary>
    public static bool UseNicknames
    {
      get { return Properties.Settings.Default.UseNicknames; }
      set { Properties.Settings.Default.UseNicknames = value; }
    }

    public static bool GetUserId { get; set; } = false;
	}

  static class XmlExtensions
  {
    public static string GetAttributeValue(this XElement x, XName name)
    {
      return x.Attribute(name).Value;
    }

    public static string GetElementValue(this XElement x, XName name)
    {
      return x.Element(name).Value;
    }

    public static bool TryGetAttribute(this XElement x, XName name, out string output)
    {
      try
      {
        output = x.Attribute(name).Value;
        return true;
      }
      catch (NullReferenceException)
      {
        output = null;
        return false;
      }
    }

    public static bool TryGetElement(this XElement x, XName name, out string output)
    {
      try
      {
        output = x.Element(name).Value;
        return true;
      }
      catch (NullReferenceException)
      {
        output = null;
        return false;
      }
    }

    public static string TryGetStringElement(this XElement each, string name, bool allowNull = false)
    {
      string output;
      if (!each.TryGetElement(name, out output))
      {
        if (!allowNull) GameData.Error(name, each);
        else return string.Empty;
      }
      if (string.IsNullOrWhiteSpace(output) && !allowNull) GameData.Error(name, each);
      return output;
    }

    public static string TryGetStringAttribute(this XElement each, string name, bool allowNull = false)
    {
      string output;
      if (!each.TryGetAttribute(name, out output))
      {
        if (!allowNull) GameData.Error(name, each);
        else return string.Empty;
      }
      if (string.IsNullOrWhiteSpace(output) && !allowNull) GameData.Error(name, each);
      return output;
    }

    public static bool HasElement(this XElement element, XName name)
    {
      foreach(var each in element.Elements())
      {
        if (each.Name == name) return true;
      }
      return false;
    }
  }
}
