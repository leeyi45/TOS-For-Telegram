using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace QuizBot
{
	class Game
	{
		public static void RoleInitialErr(Exception e)
		{
			MessageBox.Show("Failed to initialize roles, check file\nCheck for " + e.Message, "Error",
	MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void InitializeRoles()
		{
			//Temp variables
			string[] keys = new string[] {"Name", "Team", "Description", "Attribute"};
			Role temp;

			Dictionary<string, string> temps = new Dictionary<string,string>();

			XmlTextReader reader = 
				new XmlTextReader(@"Roles.xml");
			reader.WhitespaceHandling = WhitespaceHandling.None;

			//Perform the check for correct version
			try
			{
				#region Attempt version checking
				while (reader.Read())
				{
					//check the version
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "Stuff" &&
						reader.GetAttribute(0) == "1.0")
					{ //version if valid
						#region Locate Roles
						while (reader.Read())
						{
							//Locate role table
							if (reader.NodeType == XmlNodeType.Element && reader.Name == "Roles")
							{
								#region Read the roles
								while (reader.Read())
								{
									if (reader.NodeType == XmlNodeType.Element && reader.Name == "Role")
									{ //Located a role
										reader.ReadToDescendant("Name");
										Console.WriteLine("Line 53 " + reader.Name);
										for (int i = 0; i < keys.Length; i++ )
										{
											temps[keys[i]] = reader.ReadElementContentAsString();
											Console.WriteLine("Line 56 " + reader.Name);
											try { Console.WriteLine(reader.ReadToNextSibling(keys[i+1], "default")); }
											catch (IndexOutOfRangeException) { }
											//if (!) throw new Exception(keys[i+1] + " node failure");
											Console.ReadLine();
										}
									}
									else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "/Role")
									{
										MessageBox.Show("DOne");
										return;
									}
								}
								#endregion
							}
						}
						//Role table not found
						throw new Exception("Role table failure");
						#endregion
					}
				}
				//Version is invalid
				throw new Exception("Version faliure");
				#endregion
			}
			catch (Exception e)
			{
				RoleInitialErr(e);
				return;
			}
		}

		public Dictionary<string, Role> Roles;
	}
}
