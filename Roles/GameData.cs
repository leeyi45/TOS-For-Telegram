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

namespace QuizBot
{
	class GameData
	{
		public const string xmlFile = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\QuizBot\QuizBot\Roles\Roles.xml";

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

		public static Dictionary<string, Role> Roles;

		public static Triptionary<string, Role, int> RoleLists;

		public static Dictionary<string, string> Attributes;

		public static void InitializeRoles()
		{
			Roles = new Dictionary<string, Role>();
			RoleLists = new Triptionary<string, Role, int>();
			Attributes = new Dictionary<string, string>();

			//This whole function can be optimized (lots of repetitive code)

			#region XML File Processing
			//Locate the xml file
			if (!File.Exists(xmlFile))
			{
				RoleInitialErr(new Exception("existence"));
				return;
			}

			#endregion

			try
			{
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
						Attributes.Add(reader.GetAttribute(0), reader.GetAttribute(1));
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
						reader.Read();
						string listname = reader.Value;
						reader.Read(); //Read element value
						reader.Read(); //Read end tag
						reader.Read();
						RoleLists.Add(listname, Roles[reader.GetAttribute(0)], int.Parse(reader.GetAttribute(1)));
					}
					else if (reader.Name == "Rolelists" && reader.NodeType == XmlNodeType.EndElement)
					{
						break;
					}
				}
				#endregion

			}
			catch (Exception e)
			{
				RoleInitialErr(e);
			}
		}
	}
}
