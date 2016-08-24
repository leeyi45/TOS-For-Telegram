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
    #region Intialization
    public const string xmlFile = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\TOS-For-Telegram\Game\Roles.xml";

		public const string messageFile = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\TOS-For-Telegram\Game\Messages.xml";

		public static void RoleInitialErr(Exception e)
		{
			MessageBox.Show("Failed to initialize roles, check file\nCheck for " + e.Message, "Error",
	MessageBoxButtons.OK, MessageBoxIcon.Error);
			foreach (var each in new StackTrace(e).GetFrames())
			{
				Console.WriteLine(each.GetFileLineNumber());
			}
			Console.ReadLine();
		}

    public static void InitializeRoles()
    {
      Roles = new Dictionary<string, Role>();
      RoleLists = new Triptionary<string, Wrapper, int>();
      Alignments = new Dictionary<int, Alignment>();
      InvestResults = new Dictionary<int, string>();

      try { Program.ConsoleLog("Loading roles"); }
      catch { }

      if (!File.Exists(xmlFile)) throw new Exception("file existence");

      XDocument document = XDocument.Load(xmlFile);

      #region Version check
      if (document.Root.Attribute("version").Value != "1.0")
      {
        throw new Exception("Version");
      }
      #endregion

      #region Alignment
      if (!document.HasElement("Alignments")) throw new Exception("Alignment definitions");
      foreach (var each in document.Root.Element("Alignments").Elements("Alignment"))
      {
        Team temp = (Team)Enum.Parse(typeof(Team), each.GetAttributeValue("Team"));
        Alignments.Add(Alignments.Count, new Alignment(each.GetAttributeValue("Name"), temp));
      }
      #endregion

      #region Roles
      if (!document.HasElement("Roles")) throw new Exception("Role definitions");
      foreach (var each in document.Root.Element("Roles").Elements("Role"))
      {
        var team = (Team)Enum.Parse(typeof(Team), each.GetElementValue("Team"));
        var name = each.GetElementValue("Name");
        var align = new Alignment(each.GetElementValue("Alignment"), team);
        bool suspicious;
        bool nightImmune = false;
        switch(team)
        {
          //Town are all non suspicious
          case Team.Town: { suspicious = false; break; }
          //Maf are all suspicious
          case Team.Mafia: { suspicious = true;  break; }
          //The rest check
          default:
            {
              if (align.Name == "Killing") suspicious = true;
              else suspicious = false;
              break;
            }
        }

        string boolParse;
        if (each.TryGetElement("NightImmune", out boolParse)) nightImmune = bool.Parse(boolParse);
        
        //Messages.Add(name + "Assign", each.GetElementValue("OnAssign"));
        Roles.Add(name, new Role
        {
          Name = name,
          team = team,
          Alignment = align,
          HasDayAction = bool.Parse(each.GetElementValue("HasDayAction")),
          HasNightAction = bool.Parse(each.GetElementValue("HasNightAction")),
          Description = each.GetElementValue("Description"),
          NightImmune = nightImmune,
          Suspicious = suspicious,
          InvestResult = int.Parse(each.GetElementValue("Invest"))
        });
      }
      #endregion

      #region Rolelist
      if (!document.HasElement("Rolelists")) throw new Exception("Rolelist definitions");
      foreach (var rolelist in document.Root.Element("Rolelists").Elements("Rolelist"))
      {
        var listname = rolelist.GetAttributeValue("Name");
        foreach (var each in rolelist.Elements("Role"))
        {
          try
          {
            Wrapper To_Add = new Wrapper();
            string value = "";

            if (each.TryGetAttribute("Name", out value))
            { //Normal role definition
              To_Add = Roles[value];
            }
            else if (each.TryGetAttribute("Alignment", out value))
            { //Alignment defined
              if (value == "Any") To_Add = new Alignment();
              else To_Add = Alignment.Parse(value);
            }
            else if (each.TryGetAttribute("Team", out value))
            { //Team defined
              To_Add = new TeamWrapper((Team)Enum.Parse(typeof(Team), value));
            }
            else throw new Exception();

            int count = 1;
            //If a count is not defined assume it is one
            if (each.TryGetAttribute("Count", out value)) int.TryParse(value, out count);

            RoleLists[listname].Add(To_Add, count);
          }
          catch (Exception) { throw new Exception("Role not defined correctly on line " + (each as System.Xml.IXmlLineInfo).LineNumber); }
        }

      }
      #endregion

      #region Invest messages
      if (!document.HasElement("InvestResults")) throw new Exception("Invest result definitions");
      foreach(var each in document.Root.Element("InvestResults").Elements("InvestResult"))
      {
        InvestResults.Add(int.Parse(each.GetAttributeValue("Key")), each.GetElementValue("Value"));
      }
      #endregion

      try { Program.ConsoleLog("Roles loaded"); }
      catch { }
    }

    public static void InitializeMessages()
		{
			try { Program.ConsoleLog("Loading messages"); }
			catch { }
			Messages = new Dictionary<string, string>();

      if (!File.Exists(messageFile)) throw new Exception("Missing message file");
      XDocument doc = XDocument.Load(messageFile);

      foreach(var each in doc.Root.Elements("string"))
      {
        Messages.Add(each.GetAttributeValue("key"), each.GetElementValue("value"));
      }

  		try { Program.ConsoleLog("Loaded messages"); }
			catch { }
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
				if (GamePhase == QuizBot.GamePhase.Inactive) return false;
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

		/// <summary>
		/// The time which the bot was started
		/// </summary>
		public static DateTime StartTime { get; set; }

    //Need this to remove stuff for the commands
    public static string BotUsername { get; set; } = "@quiztestbot";

    public static int AliveCount
    {
      get { return GameData.Joined.Count(x => x.Value.IsAlive); }
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

    public static Player GetPlayer(Player test)
    {
      if (!Joined.ContainsValue(test)) return null;
      else
      {
        return Joined.Values.Where(x => x == test).ToArray()[0];
      }
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
	}
}
