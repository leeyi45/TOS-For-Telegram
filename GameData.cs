using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;

using RoleList = QuizBot.Triptionary<System.String, QuizBot.Role, System.Int32>;

namespace QuizBot
{
	class GameData
	{
		#region Intialization
		public const string xmlFile = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\QuizBot\QuizBot\Game\Roles.xml";

		public const string messageFile = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\QuizBot\QuizBot\Game\Messages.xml";

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
			Attributes = new Triptionary<Team, int, string>();

			//This whole function can be optimized (lots of repetitive code)
			try { Program.ConsoleLog("Loading roles"); }
			catch { }
			#region XML File Processing
			//Locate the xml file
			if (!File.Exists(xmlFile))
			{
				RoleInitialErr(new Exception("existence"));
				return;
			}

			#endregion

			//try {
				#region Attribute Check
				XmlTextReader reader = new XmlTextReader(xmlFile);
				reader.WhitespaceHandling = WhitespaceHandling.None;

				while(reader.Read()) 
				{ //TODO Remove the goto statement
					if (reader.Name == "Attributes" && reader.NodeType == XmlNodeType.Element)
					{ //Found attributes
						goto AttributesFound;
					}
				}
				throw new Exception("Attribute definitions");
				#endregion

				#region Attribute Reading
				AttributesFound:
				while (reader.Read())
				{
					if (reader.Name == "Attribute" && reader.NodeType == XmlNodeType.Element)
					{
						Team temp = (Team)Enum.Parse(typeof(Team), reader.GetAttribute("Team"));
						Attributes[temp].Add(Attributes[temp].Count, reader.GetAttribute("Name"));
					}
					else if (reader.Name == "Attributes" && reader.NodeType == XmlNodeType.EndElement)
					{
						break;
					}
				}
				#endregion

				#region Version Check
				//Restart Reader because I have no idea how to use XDocument
				//TODO create XDocument implementation
				reader = new XmlTextReader(xmlFile);
				reader.WhitespaceHandling = WhitespaceHandling.None;

				//Check if the file version is correct
				reader.Read(); //Advance to <?xml version="1.0" encoding="utf-8" ?>
				reader.Read(); //Advance to root node
				if (!(reader.Name == "Root" && reader.NodeType == XmlNodeType.Element &&
					reader.GetAttribute(0) == "1.0"))
				{ //Incorrect Version
					throw new Exception("Version");
				}
				#endregion

				#region Roles Check
				while(reader.Read()) 
				{ //TODO Remove the goto statement
					if (reader.Name == "Roles" && reader.NodeType == XmlNodeType.Element)
					{ //Found roles
						goto RolesFound;
					}
				}
				throw new Exception("Role definitions");
				#endregion
	
				#region Role Reading
				RolesFound:
				{ }
				//Begin reading the roles
				while (reader.Read())
				{
					if (reader.Name == "Role" && reader.NodeType == XmlNodeType.Element)
					{
          var team = (Team)Enum.Parse(typeof(Team), reader.GetAttribute("Team"));

            Roles.Add(reader.GetAttribute("Name"), new Role(
              reader.GetAttribute("Name"), 
              team, 
							reader.GetAttribute("Description"), 
              new Attribute(reader.GetAttribute("Attribute"), team), 
							Boolean.Parse(reader.GetAttribute("HasDayAction")), 
							Boolean.Parse(reader.GetAttribute("HasNightAction"))));
					}
					else if (reader.Name == "Roles" && reader.NodeType == XmlNodeType.EndElement)
					{
						break;
					}
				}
				#endregion

				#region Alternate Implementation
				/* To be implemented in the future
				var persons = XDocument.Load(xmlFile)
								.Root
								.Elements("Roles")
								.Elements("Role")
								.Select(x => new Role { Name = x.Element("Name").Value, team = (Team)Enum.Parse(typeof(Team), (string)x.Element("Team").Value), 
									attribute = x.Element("attribute").Value,
								description = x.Element("Description").Value, HasDayAction = Boolean.Parse(x.Element("HasDayAction").Value),
								HasNightAction = Boolean.Parse(x.Element("HasNightAction").Value)})
								.ToArray();

				MessageBox.Show("got to line 82");
				
				foreach (var x in XDocument.Load(xmlFile).Root.Element("Roles").Elements())
				{
					Roles.Add(x.Element("Name").Value, new Role(x.Element("Name").Value, (Team)int.Parse(x.Element("Team").Value),
						x.Element("Description").Value, x.Element("attribute").Value, Boolean.Parse(x.Element("HasDayAction").Value),
						Boolean.Parse(x.Element("HasNightAction").Value)));
				}

				
				foreach (var each in persons)
				{
					Console.WriteLine(each.Name);
				}
				Console.WriteLine("Persons length is " + persons.Length);
				Console.ReadLine();
				*/
				#endregion

				#region Rolelist Check
				reader = new XmlTextReader(xmlFile);
				reader.WhitespaceHandling = WhitespaceHandling.None;
				while (reader.Read())
				{ //TODO Remove the goto statement
					if (reader.Name == "Rolelists" && reader.NodeType == XmlNodeType.Element)
					{ //Found roles
						goto RoleListsFound;
					}
				}
				throw new Exception("Rolelist definitions");
				#endregion

				#region Rolelist Reading
				RoleListsFound:
				while (reader.Read())
				{
					if (reader.Name == "Rolelist" && reader.NodeType == XmlNodeType.Element)
					{
						string listname = reader.GetAttribute("Name");
          Wrapper To_Add;
						while (reader.Read())
						{
              if (reader.Name == "Role" && reader.NodeType == XmlNodeType.Element)
              { //Check what kind of role definition this is
                string name = reader.GetAttribute("Name");
                if (string.IsNullOrWhiteSpace(name))
                { //If there is no role defined
                  string attri = reader.GetAttribute("Attribute");
                  if (attri == "Any") To_Add = new Attribute();
                  else To_Add = Attribute.Parse(attri);
                }
                else To_Add = Roles[name];
                RoleLists[listname].Add(To_Add, int.Parse(reader.GetAttribute("Count")));
              }
              else if (reader.Name == "Rolelist" && reader.NodeType == XmlNodeType.EndElement)
              {
                break;
              }
						}
					}
					else if (reader.Name == "Rolelists" && reader.NodeType == XmlNodeType.EndElement)
					{
						break;
					}
				}
				#endregion
			try {}
			catch (Exception e)
			{
				RoleInitialErr(e);
			}
			try { Program.ConsoleLog("Roles loaded"); }
			catch { }
		}

		public static void InitializeMessages()
		{
			try { Program.ConsoleLog("Loading messages"); }
			catch { }
			Messages = new Dictionary<string, string>();
			XmlTextReader reader = new XmlTextReader(messageFile);
			reader.WhitespaceHandling = WhitespaceHandling.None;
			while (reader.Read())
			{
				if (reader.Name == "string" && reader.NodeType == XmlNodeType.Element)
				{
					string key = reader.GetAttribute("key");
					reader.Read();
					if (reader.Name == "value" && reader.NodeType == XmlNodeType.Element)
					{
						reader.Read();
						Messages.Add(key, reader.Value);
						//Program.PrintWait("Element name is " + reader.Name + " and value is " + reader.Value);
					}
				}
				else if (reader.Name == "strings" && reader.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
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
		public const string WeirdThing = "@quiztestbot";
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

		public static Triptionary<Team, int, string> Attributes;

		public static Dictionary<string, Action> DayRoleActions;

		public static Dictionary<string, Action> NightRoleActions;

		public static Dictionary<int, Player> Joined = new Dictionary<int, Player>();

		/// <summary>
		/// Dictionary containing all the messages
		/// </summary>
		public static Dictionary<string, string> Messages;
		#endregion
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
