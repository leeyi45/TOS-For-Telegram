using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using static QuizBot.Program;
using System.Reflection;

namespace QuizBot
{
  static class GameData
  {
    #region Intialization
    public static string roleFile { get { return @"Roles.xml"; } }

    public static string MessagesFile { get { return @"Messages.xml"; } }

    public static string ProtocolsFile { get { return @"Protocols.xml"; } }

    public const string xmlLocation = @"C:\Users\Lee Yi\Desktop\Everything, for the moment\Coding\C# Bot\TOS-For-Telegram\Xml\";

    private static void InitialErr(string message, InitException e)
    {
      MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      ConsoleLog(e.Message);
    }

    #region Role creation functions
    public static void Error(string message, XElement each)
    {
      throw new InitException("Roles.xml", "Failed to get " + message, each);
    }

    private static bool[] GetBooleanValues(XElement each, bool defaultVal, params string[] data)
    {
      return (from dat in data
              select each.TryGetElementValue(dat, defaultVal: defaultVal)).ToArray();
    }

    private static Team GetTeam(XElement each)
    {
      try { return (Team)Enum.Parse(typeof(Team), each.TryGetElementValue<string>("Team")); }
      catch (ArgumentException)
      {
        throw new InitException("Roles.xml",
          "Only \"Town\", \"Mafia\" and \"Neutral\" are acceptable values for \"Team\"", each);
      }
    }

    private static Alignment GetAlignment(XElement each, Team team)
    {
      string alignStr;
      if (!each.TryGetElement("Alignment", out alignStr)) Error("alignment", each);
      return new Alignment(each.TryGetElementValue<string>("Name"), team);
    }

    private static int GetInvestResult(XElement each)
    {
      var parse = each.TryGetElementValue<string>("Invest");
      int output;
      if (!int.TryParse(parse, out output)) Error("invest result", each);
      return output;
    }
    #endregion
    public static void Log(string text, bool logtoconsole, int amount)
    {
      if (logtoconsole) ConsoleLog(text);
      else startup.SetExtraInfo(text, amount);
    }

    public static void Log(string text, bool logtoconsole)
    {
      if (logtoconsole) ConsoleLog(text);
      else startup.SetExtraInfo(text);
    }

    #region Roles.xml
    private static void LoadAlignments(XDocument doc, bool logtoconsole)
    {
      Log("Reading alignments", logtoconsole);
      if (!doc.Root.HasElement("Alignments")) throw new InitException("Alignments have not been properly defined");
      foreach (var each in doc.Root.Element("Alignments").Elements("Alignment"))
      {
        Team temp = GetTeam(each);
        var name = each.TryGetElementValue("Roles.xml", "Name");
        var align = new Alignment(name, temp);
        Alignments.Add(align);
        Log("Alignment \"" + align.Name + "\" registered", logtoconsole);
      }
      Log("Alignments loaded", logtoconsole);
    }

    private static void LoadRoles(XDocument doc, bool logtoconsole)
    {
      Log("Reading roles", logtoconsole);
      if (!doc.Root.HasElement("Roles")) throw new InitException("Roles have not been properly defined");
      foreach (var each in doc.Root.Element("Roles").Elements("Role"))
      {
        var team = GetTeam(each);
        var align = new Alignment(each.TryGetElementValue("Roles.xml", "Alignment"), team);
        var name = each.TryGetElementValue("Roles.xml", "Name");
        var truevals = GetBooleanValues(each, true, "HasDayAction", "HasNightAction", "AllowOthers",
          "AllowSelf");
        var falsevals = GetBooleanValues(each, false, "Unique", "NightImmune");

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
          HasDayAction = truevals[0],
          HasNightAction = truevals[1],
          Description = each.TryGetElementValue("Roles.xml", "Description"),
          NightImmune = falsevals[1],
          Suspicious = suspicious,
          InvestResult = GetInvestResult(each),
          Instruction = each.TryGetElementValue<string>("Instruct", ""),
          AllowOthers = truevals[2],
          AllowSelf = truevals[3],
          Unique = falsevals[0]
        }, each, "role: " + name);
        Log("\"" + name + "\" registered", logtoconsole);
      }
      if (Roles.Count == 0) throw new InitException("There are no roles defined!");

      ConsoleLog("Finished reading roles");
    }

    private static void LoadRolelists(XDocument doc, bool logtoconsole)
    {
      Log("Loading rolelists", logtoconsole);
      if (!doc.Root.HasElement("Rolelists")) throw new InitException("Rolelists are not defined properly!");
      foreach (var rolelist in doc.Root.Element("Rolelists").Elements("Rolelist"))
      {
        var listname = rolelist.TryGetAttributeValue("Roles.xml", "Name");
        RoleLists.Add(listname, new Dictionary<Wrapper, int>());
        foreach (var each in rolelist.Elements("Role"))
        {
          #region Deal with the count
          var count = each.TryGetAttributeValue("Count", 1);
          //If a count is not defined assume it is one
          if (count < 1) throw new InitException("Roles.xml", "The count must be greater than one!", each);
          #endregion

          var To_Add = new Wrapper();
          string value = "";

          #region Normal Role Definition
          if (each.TryGetAttribute("Name", out value))
          { //Normal role definition
            value = value.ToLower();
            try
            {
              To_Add = Roles[value];
              if ((To_Add as Role).Unique && count != 1)
                throw new InitException("Roles.xml",
                  "The role " + To_Add.Name + " is unique! (There cannot be more than one)", each);
            }
            catch (KeyNotFoundException)
            {
              throw new InitException("Roles.xml", "Role \"" + value + "\" does not exist!", each);
            }
          }
          #endregion
          else
          {
            #region Alignment Defined
            if (each.TryGetAttribute("Alignment", out value))
            { //Alignment defined
              if (value == "Any") To_Add = new Alignment();
              else
              {
                try { To_Add = Alignment.Parse(value); }
                catch (ArgumentException)
                {
                  throw new InitException("Roles.xml", "Alignment \"" + value + "\" does not exist!", each);
                }
              }
            }
            #endregion
            #region Team Defined
            else if (each.TryGetAttribute("Team", out value))
            { //Team defined
              try { To_Add = new TeamWrapper((Team)Enum.Parse(typeof(Team), value)); }
              catch (ArgumentException)
              {
                throw new InitException("Roles.xml",
                  "Only \"Town\", \"Mafia\" and \"Neutral\" are acceptable values for \"Team\"", each);
              }
            }
            #endregion
            else throw new InitException("Roles.xml", "Unrecognised role definition", each);
          }
          RoleLists[listname].Add(To_Add, count, each, "definition in " + listname + ": " + To_Add.Name);
        }
        Log("Registered new rolelist: " + listname, logtoconsole);
      }
      if (RoleLists.Count == 0) throw new InitException("There are no rolelists defined!");
      Log("Finished loading rolelists", logtoconsole);
    }

    private static void LoadInvestResults(XDocument doc, bool logtoconsole)
    {
      Log("Loading investigation results", logtoconsole);
      if (!doc.Root.HasElement("InvestResults")) throw new InitException("Invest results have not properly been defined");
      foreach (var each in doc.Root.Element("InvestResults").Elements("InvestResult"))
      {
        InvestResults.Add(each.TryGetAttributeValue<int>("Key"), each.TryGetElementValue("Roles.xml", "Value"), each,
    "Invest Result");
      }
      Log("Invest results loaded", logtoconsole);
    }
    #endregion

    public static void InitializeRoles(bool logtoconsole)
    {
      CommandVars.RolesLoaded = false;
      var loading = new Dictionary<string, Action<XDocument, bool>>();
      Alignments = new List<Alignment>();
      Roles = new Dictionary<string, Role>();
      RoleLists = new Dictionary<string, Dictionary<Wrapper, int>>();
      InvestResults = new Dictionary<int, string>();

      if (!File.Exists(xmlLocation + roleFile)) throw new InitException("Missing Roles.xml!");
      var doc = GDExtensions.SafeLoad(roleFile);

      loading.Add("Alignments", LoadAlignments);
      loading.Add("Roles", LoadRoles);
      loading.Add("Rolelist", LoadRolelists);
      loading.Add("Invest Results", LoadInvestResults);

      foreach (var each in loading)
      {
        goto bypass;
        retry:
        {
          doc = GDExtensions.SafeLoad(roleFile);
        }
        bypass:
        try
        {
          Log("Loading " + each.Key, logtoconsole);
          each.Value(doc, logtoconsole);
        }
        catch (InitException e) when (!logtoconsole)
        {
          switch (ErrorShow(e.Message))
          {
            case DialogResult.Ignore: { continue; }
            case DialogResult.Retry:
              {
                doc = XDocument.Load(roleFile, LoadOptions.SetLineInfo);
                goto retry;
              }
            case DialogResult.Abort: { Application.Exit(); break; }
          }
        }
        catch (InitException e) when (logtoconsole)
        {
          InitialErr("Failed to load " + each.Key, e);
        }
      }
      CommandVars.RolesLoaded = true;
    }

    public static void InitializeMessages(bool logtoconsole)
    {
      try
      {
        Log("Loading messages", logtoconsole);
        CommandVars.messagesLoaded = false;
        Messages = new Dictionary<string, string>();
        retry:
        if (!File.Exists(xmlLocation + MessagesFile)) throw new InitException("Missing Messages.xml");
        var doc = GDExtensions.SafeLoad(MessagesFile);

        foreach (var each in doc.Root.Elements("string"))
        {
          try
          {
            var key = each.TryGetAttributeValue("Messages.xml", "key");
            Messages.Add(key, each.TryGetElementValue("Messages.xml", "value"), each, "message: " + key,
              "Messages.xml");
            Log("Message\"" + key + "\" registered", logtoconsole);
          }
          catch (InitException e) when (!logtoconsole)
          {
            switch (ErrorShow(e.Message))
            {
              case DialogResult.Ignore: { continue; }
              case DialogResult.Retry:
                {
                  doc = XDocument.Load(MessagesFile, LoadOptions.SetLineInfo);
                  goto retry;
                }
              case DialogResult.Abort: { Application.Exit(); break; }
            }
          }
        }

        Log("Loaded messages", logtoconsole);
        CommandVars.messagesLoaded = true;
        //ArrangeXML();
      }
      catch (InitException e) when (logtoconsole)
      {
        InitialErr("Failed to load Messages.xml, see console for details", e);
      }
    }

    //May be possible to combine with InitializeMessages
    public static void InitializeProtocols(bool logtoconsole)
    {
      try
      {
        CommandVars.protocolsLoaded = false;
        Protocols = new Dictionary<string, string>();
        retry:
        if (!File.Exists(xmlLocation + ProtocolsFile)) throw new InitException("Missing Protocols.xml");
        var doc = GDExtensions.SafeLoad(ProtocolsFile);

        foreach (var each in doc.Root.Elements("protocol"))
        {
          try
          {
            var key = each.TryGetAttributeValue("Protocols.xml", "key");
            Protocols.Add(key, each.TryGetElementValue("Protocols.xml", "value"),
              each, "protocol: " + key, "Protocols.xml");
            Log("Registered protocol: " + key, logtoconsole);
          }
          catch (InitException e) when (!logtoconsole)
          {
            switch (ErrorShow(e.Message))
            {
              case DialogResult.Ignore: { continue; }
              case DialogResult.Retry:
                {
                  doc = XDocument.Load(ProtocolsFile, LoadOptions.SetLineInfo);
                  goto retry;
                }
              case DialogResult.Abort: { Application.Exit(); break; }
            }
          }
        }
        CommandVars.protocolsLoaded = true;
      }
      catch (InitException e) when (logtoconsole)
      {
        InitialErr("Failed to load Protocols.xml, see console for details", e);
      }
    }

    public static void InitializeOthers(bool logtoconsole)
    {
      Messages = new Dictionary<string, string>();
      Protocols = new Dictionary<string, string>();

      var dictionaries = typeof(GameData).GetProperties(BindingFlags.Static | BindingFlags.Public).
        Where(x => x.PropertyType == typeof(Dictionary<string, string>)).
        ToDictionary(x => x.Name, x => x.GetValue(null) as Dictionary<string, string>);
      foreach (var each in dictionaries)
      {
        var filename = typeof(GameData).GetProperty(each.Key + "File", BindingFlags.Static | BindingFlags.Public).
          GetValue(null).ToString();
        if (!File.Exists(xmlLocation + filename)) throw new InitException("Missing " + filename + "!");
        Log("Loading " + filename, logtoconsole);
        var doc = GDExtensions.SafeLoad(filename);
        var keyword = each.Key.TrimEnd('s').ToLower();
        foreach (var element in doc.Root.Elements(keyword))
        {
          var key = element.TryGetAttributeValue(filename, "key");
          each.Value.Add(key, element.TryGetElementValue(filename, "value"), element, keyword + ": " + key,
            filename);
          Log("Registered " + keyword + ": " + key, logtoconsole);
        }
        typeof(CommandVars).GetProperty(keyword + "sLoaded", BindingFlags.Static | BindingFlags.Public)
          .SetValue(null, true);
      }
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
      get { return GamePhase == GamePhase.Inactive; }
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
      get { return Alive.Count; }
    }

    /// <summary>
    /// Boolean value indicatiing if the mayor has revealed himself
    /// </summary>
    public static bool HasRevealed { get; set; }
    #endregion

    #region The Dictionaries of Data
    /// <summary>
    /// Dictionary of all the roles currently defined
    /// </summary>
    public static Dictionary<string, Role> Roles { get; set; }

    /// <summary>
    /// Contains all the rolelists currently defined
    /// </summary>
    public static Dictionary<string, Dictionary<Wrapper, int>> RoleLists { get; set; }

    /// <summary>
    /// List of all the alignment currently registered
    /// </summary>
    public static List<Alignment> Alignments { get; set; }

    /// <summary>
    /// List containing all the players who have joined the game
    /// </summary>
    public static List<Player> Joined { get; set; } = new List<Player>();

    /// <summary>
    /// Dictionary containing all the protocols
    /// </summary>
    public static Dictionary<string, string> Protocols { get; set; }

		/// <summary>
		/// Dictionary containing all the messages
		/// </summary>
		public static Dictionary<string, string> Messages { get; set; }

    /// <summary>
    /// Dictionary containing all the investigative results
    /// </summary>
    public static Dictionary<int, string> InvestResults { get; set; }

    public static IEnumerable<KeyValuePair<string, Dictionary<string, string>>> Dictionaries
    {
      get
      {
        yield return new KeyValuePair<string, Dictionary<string, string>>("roles", Roles.ValuesToString());
        yield return new KeyValuePair<string, Dictionary<string, string>>("messages", Messages);
        yield return new KeyValuePair<string, Dictionary<string, string>>("protocols", Protocols);
      }
    }

    /// <summary>
    /// List containing all the players in the game who are currently alive
    /// </summary>
    public static List<Player> Alive
    {
      get { return Joined.Where(x => x.IsAlive).ToList(); }
    }
    #endregion

    private static void ArrangeXML()
    {
      var doc = XDocument.Load(MessagesFile);
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
      doc.Save(MessagesFile);
    }

    public static DialogResult ErrorShow(string text)
    {
      return MessageBox.Show(text, "Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error,
        MessageBoxDefaultButton.Button3);
    }
	}

  static class GDExtensions
  {
    public static bool HasElement(this XElement element, XName name)
    {
      foreach (var each in element.Elements())
      {
        if (each.Name == name) return true;
      }
      return false;
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

    //Null values are not allowed
    /// <summary>
    /// Returns the value of the element, converted to the specified type
    /// </summary>
    /// <typeparam name="T">The type to return</typeparam>
    /// <param name="x">The element to get the value from</param>
    /// <param name="name">The name of the value to get</param>
    /// <param name="handle">Boolean value indicating if exceptions should be handled</param>
    /// <returns>The value of the element</returns>
    public static T TryGetElementValue<T>(this XElement x, string name, bool handle = true)
    {
      string message;
      try { return (T)Convert.ChangeType(x.Element(name).Value, typeof(T)); }
      catch(NullReferenceException) when (handle) { message = "Failed to get " + name; }
      catch(FormatException) when (handle)
      { message = "Invalid value for " + name + ", " + typeof(T).Name + " expected!"; }
      catch(InvalidCastException) when (handle)
      { message = "Invalid value for " + name + ", " + typeof(T).Name + " expected!"; }
      throw new InitException("Roles.xml", message, x);
    }

    //Null values are not allowed
    /// <summary>
    /// Returns the value of the element, converted to the specified type
    /// </summary>
    /// <typeparam name="T">The type to return</typeparam>
    /// <param name="x">The element to get the value from</param>
    /// <param name="name">The name of the value to get</param>
    /// <param name="handle">Boolean value indicating if exceptions should be handled</param>
    /// <param name="file">File name</param>
    /// <returns>The value of the element</returns>
    public static T TryGetElementValue<T>(this XElement x, string file, string name, bool handle = true)
    {
      string message;
      try { return (T)Convert.ChangeType(x.Element(name).Value, typeof(T)); }
      catch (NullReferenceException) when (handle) { message = "Failed to get " + name; }
      catch (FormatException) when (handle)
      { message = "Invalid value for " + name + ", " + typeof(T).Name + " expected!"; }
      catch (InvalidCastException) when (handle)
      { message = "Invalid value for " + name + ", " + typeof(T).Name + " expected!"; }
      throw new InitException(file, message, x);
    }

    //Null values are allowed
    /// <summary>
    /// Returns the value of the element, converted to the specified type, allowing for a default
    /// value to be returned if an exception occurs
    /// </summary>
    /// <typeparam name="T">The type to return</typeparam>
    /// <param name="x">The element to get the value from</param>
    /// <param name="name">The name of the value to get</param>
    /// <param name="defaultVal">The default value to return</param>
    /// <returns>The value of the element</returns>
    public static T TryGetElementValueSpecial<T>(this XElement x, string name, T defaultVal)
    {
      return (T)Convert.ChangeType(x.Element(name).Value, typeof(T));
    }

    //Null values are allowed
    /// <summary>
    /// Returns the value of the element, converted to the specified type, allowing for a default
    /// value to be returned if an exception occurs
    /// </summary>
    /// <typeparam name="T">The type to return</typeparam>
    /// <param name="x">The element to get the value from</param>
    /// <param name="name">The name of the value to get</param>
    /// <param name="defaultVal">The default value to return</param>
    /// <returns>The value of the element</returns>
    public static T TryGetElementValue<T>(this XElement x, string name, T defaultVal)
    {
      try { return (T)Convert.ChangeType(x.Element(name).Value, typeof(T)); }
      catch { return defaultVal; }
    }

    //No conversion is required (string value)
    /// <summary>
    /// Returns the string value of the element
    /// </summary>
    /// <param name="x">The parent element</param>
    /// <param name="file">The name of the file</param>
    /// <param name="name">The name of the element</param>
    /// <returns>The string value of the element</returns>
    public static string TryGetElementValue(this XElement x, string file, string name)
    {
      string output;
      if (!x.TryGetElement(name, out output))
        throw new InitException(file, "Failed to get " + name, x);
      return output;
    }

    //Null values are not allowed
    /// <summary>
    /// Returns the value of the attribute, converted to the specified type
    /// </summary>
    /// <typeparam name="T">The type to return</typeparam>
    /// <param name="x">The attribute to get the value from</param>
    /// <param name="name">The name of the value to get</param>
    /// <param name="handle">Boolean value indicating if exceptions should be handled</param>
    /// <returns>The value of the attribute</returns>
    public static T TryGetAttributeValue<T>(this XElement x, string name, bool handle = true)
    {
      string message;
      try { return (T)Convert.ChangeType(x.Attribute(name).Value, typeof(T)); }
      catch (NullReferenceException) when (handle) { message = "Failed to get " + name; }
      catch (FormatException) when (handle)
      { message = "Invalid value for " + name + ", " + nameof(T) + " expected!"; }
      catch (InvalidCastException) when (handle)
      { message = "Invalid value for " + name + ", " + nameof(T) + " expected!"; }
      throw new InitException("Roles.xml", message, x);
    }

    //Null values are allowed
    /// <summary>
    /// Returns the value of the attribute, converted to the specified type, allowing for a default
    /// value to be returned if an exception occurs
    /// </summary>
    /// <typeparam name="T">The type to return</typeparam>
    /// <param name="x">The element to get the value from</param>
    /// <param name="name">The name of the attribute to get</param>
    /// <param name="defaultVal">The default value to return</param>
    /// <returns>The value of the attribute</returns>
    public static T TryGetAttributeValue<T>(this XElement x, string name, T defaultVal)
    {
      try { return (T)Convert.ChangeType(x.Attribute(name).Value, typeof(T)); }
      catch { return defaultVal; }
    }

    //No conversion is required (string value)
    /// <summary>
    /// Returns the string value of the attribute
    /// </summary>
    /// <param name="x">The parent element</param>
    /// <param name="file">The name of the file</param>
    /// <param name="name">The name of the attribute</param>
    /// <returns>The string value of the attribute</returns>
    public static string TryGetAttributeValue(this XElement x, string file, string name)
    {
      string output;
      if (!x.TryGetAttribute(name, out output)) throw new InitException(file, "Failed to get " + name, x);
      return output;
    }

    //Safe add
    /// <summary>
    /// Adds an element to the dictionary, throwing an InitException if a
    /// duplicate element is received
    /// </summary>
    /// <typeparam name="TKey">The Key Type</typeparam>
    /// <typeparam name="TValue">The Value Type</typeparam>
    /// <param name="it">The Dictionary to add to</param>
    /// <param name="key">The key for the value to add</param>
    /// <param name="value">the value to add</param>
    /// <param name="each">The XElement from which data is being obtained</param>
    /// <param name="message">The error message</param>
    /// <param name="file">The file the XElement belongs to</param>
    public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> it, TKey key, TValue value,
      XElement each, string message, string file = "Roles.xml")
    {
      try { it.Add(key, value); }
      catch(ArgumentException)
      {
        throw new InitException(file, "Duplicate " + message + "!", each);
      }
    }

    /// <summary>
    /// Loads an XDocumentusing the specified file, handling any Xml Exceptions
    /// </summary>
    /// <param name="uri">URI of the file</param>
    /// <returns>XDocument object</returns>
    public static XDocument SafeLoad(string uri)
    {
      try { return XDocument.Load(GameData.xmlLocation + uri, LoadOptions.SetLineInfo); }
      catch(System.Xml.XmlException)
      {
        throw new InitException("Xml Error with " + uri);
      }
    }

    public static Dictionary<T, object> ValuesToObject<T, U>(this Dictionary<T, U> it)
    {
      return it.ToDictionary(x => x.Key, x => (object)x.Value);
    }

    public static Dictionary<T, string> ValuesToString<T, U>(this Dictionary<T, U> it)
    {
      return it.ToDictionary(x => x.Key, x => x.Value.ToString());
    }

    public static string ToUpperFirst(this string it)
    {
      if (string.IsNullOrWhiteSpace(it)) return String.Empty;
      if (it.Length == 1) return it[0].ToString().ToUpper();
      return it[0].ToString().ToUpper() + it.Substring(1, it.Length-1);
    }
  }
}
