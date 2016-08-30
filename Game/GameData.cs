using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace QuizBot
{
  class GameData
  {
    private class InitException : Exception
    {
      public InitException(string message, XElement each) :
        base("Error while reading roles.xml at line " + (each as System.Xml.IXmlLineInfo).LineNumber + ": ")
      { }

      public InitException(string message) : 
        base("Encountered an error while reading roles: " + message) { }

      public InitException(string file, string message) :
        base("Encountered an error while reading " + file + " " + message)
      { }
    }

    #region Intialization
    public const string xmlFile = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\TOS-For-Telegram\Game\Roles.xml";

		public const string messageFile = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\TOS-For-Telegram\Game\Messages.xml";

		private static void InitialErr(string message, InitException e)
		{
      MessageBox.Show(message);
      Program.ConsoleLog(e.Message);
    }

    #region Role creation functions
    public static void Error(string message, XElement each)
    {
      throw new InitException("Failed to get " + message, each);
    }

    private static Tuple<bool, bool> GetHasActionValues(XElement each)
    {
      string parse;
      if (!each.TryGetElement("HasDayAction", out parse)) Error("HasDayAction", each);
      bool dayAction;
      if (!bool.TryParse(parse, out dayAction)) Error("HasDayAction", each);

      if(!each.TryGetElement("HasNightAction", out parse)) Error("HasNightAction", each);
      bool nightAction;
      if(!bool.TryParse(parse, out nightAction)) Error("HasNightAction", each);
      return new Tuple<bool, bool>(dayAction, nightAction);
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
      string parse;
      if (!each.TryGetElement("Invest", out parse)) Error("invest result", each);
      int output;
      if (!int.TryParse(parse, out output)) Error("invest result", each);
      return output;
    }
    #endregion
    public static void InitializeRoles()
    {
      Roles = new Dictionary<string, Role>();
      RoleLists = new Triptionary<string, Wrapper, int>();
      Alignments = new Dictionary<int, Alignment>();
      InvestResults = new Dictionary<int, string>();

      try { Program.ConsoleLog("Loading roles"); }
      catch { }

      try
      {
        if (!File.Exists(xmlFile)) throw new InitException("Failed to open role file");

        XDocument document = XDocument.Load(xmlFile);

        #region Version check
        Program.ConsoleLog("Checking roles.xml file version");
        if (document.Root.Attribute("version").Value != "1.0")
        {
          throw new InitException("Incorrect role file version");
        }
        Program.ConsoleLog("File version verified");
        #endregion

        #region Alignment
        Program.ConsoleLog("Reading alignments");
        if (!document.HasElement("Alignments")) throw new InitException("Alignments have not been properly defined");
        foreach (var each in document.Root.Element("Alignments").Elements("Alignment"))
        {
          Team temp = GetTeam(each);
          var align = new Alignment(each.TryGetStringElement("Name"), temp);
          Alignments.Add(Alignments.Count, align);
          Program.ConsoleLog("Alignment \"" + align.Name + "\" registered");
        }
        Program.ConsoleLog("Alignments loaded");
        #endregion

        #region Roles
        Program.ConsoleLog("Reading roles");
        if (!document.HasElement("Roles")) throw new InitException("Roles have not been properly defined");
        foreach (var each in document.Root.Element("Roles").Elements("Role"))
        {
          var team = GetTeam(each);
          var align = GetAlignment(each, team);
          var name = each.TryGetStringElement("Name");

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

          #region Get Instruction Value
          string instruction = string.Empty;
          each.TryGetElement("Instruct", out instruction);
          #endregion

          var hasActions = GetHasActionValues(each);

          //Messages.Add(name + "Assign", each.GetElementValue("OnAssign"));
          Roles.Add(name, new Role
          {
            Name = name,
            team = team,
            Alignment = align,
            HasDayAction = hasActions.Item1,
            HasNightAction = hasActions.Item2,
            Description = each.TryGetStringElement("Description"),
            NightImmune = GetNightImmune(each),
            Suspicious = suspicious,
            InvestResult = GetInvestResult(each),
            Instruction = instruction
          });
          Program.ConsoleLog("\"" + name + "\" registered");
        }
        Program.ConsoleLog("Finished reading roles");
        #endregion

        #region Rolelist
        Program.ConsoleLog("Loading rolelists");
        if (!document.HasElement("Rolelists")) throw new InitException("Rolelists are not defined properly!");
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
              if (each.TryGetAttribute("Count", out value)) int.TryParse(value, out count);

              RoleLists[listname].Add(To_Add, count);
            }
            catch (InitException e)
            {
              Program.MessageLog(e.Message);
              return;
            }
          }
          Program.ConsoleLog("Registered new rolelist: " + listname);
        }
        Program.ConsoleLog("Finished loading rolelists");
        #endregion

        #region Invest messages
        Program.ConsoleLog("Loading investigation results");
        if (!document.HasElement("InvestResults")) throw new InitException("Invest results have not properly been defined");
        foreach (var each in document.Root.Element("InvestResults").Elements("InvestResult"))
        {
          InvestResults.Add(int.Parse(each.TryGetStringAttribute("Key")), each.TryGetStringElement("Value"));
        }
        Program.ConsoleLog("Invest results loaded");
        #endregion
      }
      catch(InitException e)
      {
        InitialErr("Failed to load roles.xml, see console for details", e);
        return;
      }

      try { Program.ConsoleLog("Roles loaded"); }
      catch { }
    }

    public static void InitializeMessages()
		{
			try { Program.ConsoleLog("Loading messages"); }
			catch { }
			Messages = new Dictionary<string, string>();

      try
      {
        if (!File.Exists(messageFile)) throw new InitException("Messages", "Missing message file");
        XDocument doc = XDocument.Load(messageFile);

        foreach (var each in doc.Root.Elements("string"))
        {
          Messages.Add(each.TryGetStringAttribute("key"), each.TryGetStringElement("value"));
        }
      }
      catch(InitException e)
      {
        InitialErr("Failed to load messages.xml, see console for details", e);
      }

  		try { Program.ConsoleLog("Loaded messages"); }
			catch { }
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

    public static Dictionary<int, Player> Alive
    {
      get { return Joined.Where(x => x.Value.IsAlive).ToDictionary(x => x.Key, x => x.Value); }
    }
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

    public static void ArrangeXML()
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

    public static bool GetUserId { get; set; } = false;
	}
}
