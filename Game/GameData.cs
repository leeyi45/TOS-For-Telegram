﻿using System;
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
			RoleLists = new Triptionary<string, Role, int>();
			Attributes = new Triptionary<Team, int, string>();

			//This whole function can be optimized (lots of repetitive code)

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
						Roles.Add(reader.GetAttribute(0), new Role(reader.GetAttribute(0), (Team)Enum.Parse(typeof(Team), reader.GetAttribute(1)), 
							reader.GetAttribute(2), reader.GetAttribute(3), 
							Boolean.Parse(reader.GetAttribute(4)), 
							Boolean.Parse(reader.GetAttribute(5))));
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
						while (reader.Read())
						{
							if (reader.Name == "Role" && reader.NodeType == XmlNodeType.Element)
							{
								RoleLists[listname].Add(Roles[reader.GetAttribute("Name")], int.Parse(reader.GetAttribute("Count")));
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
		}
		#endregion

		public static Dictionary<int, Player> Joined = new Dictionary<int, Player>();

		public static int PlayerCount { get { return Joined.Count; } }

		public static bool GameStarted { get; set; }

		public static GamePhase GamePhase { get; set; }

		public static long CurrentGroup { get; set; }

		public static DateTime StartTime { get; set; }

		//Need this to remove stuff for the commands
		public const string WeirdThing = "@quiztestbot";

		#region The Dictionaries of Data
		public static Dictionary<string, Role> Roles;

		public static Triptionary<string, Role, int> RoleLists;

		public static Triptionary<Team, int, string> Attributes;

		public static Dictionary<string, Action> DayRoleActions;

		public static Dictionary<string, Action> NightRoleActions;
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
		public static Dictionary<Role, int> CurrentRoles
		{
			get { return GameData.RoleLists[CurrentRoleList]; }
		}
	}
}