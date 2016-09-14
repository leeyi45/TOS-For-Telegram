using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading;
using Telegram.Bot.Types;
using System.Reflection;

namespace QuizBot
{
  //This file contains all the stuff pertaining to managing a game
  partial class Game
  {
    private static string instanceFile { get { return "InstanceData.xml"; } }

    public class Settings
    {
      /// <summary>
      /// Constructor to create new settings class
      /// </summary>
      /// <param name="Info">Property values</param>
      public Settings(IEnumerable<PropertyInfo> Info)
      {
        var properties = AllSettings.ToDictionary(x => x.Name, x => x);

        foreach (var info in Info)
        {
          var prop = properties[info.Name];
          prop.SetValue(this, info.GetValue(null));
        }
      }

      /// <summary>
      /// Constructor to create settings class based on existing data
      /// </summary>
      /// <param name="element">Settings XElement</param>
      public Settings(XElement element)
      {
        var properties = AllSettings.ToDictionary(x => x.Name, x => x);
        var quizbotsettings = QuizBot.Settings.AllSettings.ToDictionary(x => x.Name, x => x);
        foreach (var each in element.Elements())
        {
          var prop = properties[each.Name.ToString()];
          string val;
          element.TryGetElement(each.Name, out val);
          if(string.IsNullOrWhiteSpace(val)) val = quizbotsettings[
            each.Name.ToString()].GetValue(null).ToString();
          prop.SetValue(this, Convert.ChangeType(val, prop.PropertyType));
        }
      }

      public int MaxPlayers { get; private set; }

      public int MinPlayers { get; private set; }

      public bool UseNicknames { get; private set; }

      public int LynchTime { get; private set; }

      public int NightTime { get; private set; }

      public int DayTime { get; private set; }

      public int JoinTime { get; private set; }

      public string CurrentRoleList
      {
        get { return rolelist; }
        private set
        {
          if (!GameData.RoleLists.Keys.Contains(value)) throw new InitException("No such rolelist!");
          else rolelist = value;
        }
      }

      private string rolelist;

      public IEnumerable<PropertyInfo> AllSettings
      {
        get {
          return from prop in typeof(Settings).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                 where prop.CanRead && prop.CanWrite
                 select prop; }
      }

      public XElement ToXElement()
      {
        var output = new XElement("Settings");
        foreach(var each in AllSettings)
        {
          output.Add(new XElement(each.Name, each.GetValue(this).ToString()));
        }
        return output;
      }
    }

    private Thread GameStart;

    /// <summary>
    /// The current roles registered
    /// </summary>
    private Dictionary<string, Role> Roles;

    /// <summary>
    /// The current rolelist being used
    /// </summary>
    private Dictionary<Wrapper, int> Rolelist;

    private List<Alignment> Alignments;

    private Dictionary<string, string> GameMessages;

    private Dictionary<string, string> Protocols;

    public Settings settings { get; private set; }

    public string GroupName { get; private set; }

    /// <summary>
    /// List of the all the players that joined this game instance
    /// </summary>
    public List<Player> Joined { get; set; }

    /// <summary>
    /// List of the players currently alive
    /// </summary>
    public List<Player> Alive { get { return Joined.Where(x => x.IsAlive).ToList(); } }

    /// <summary>
    /// Return players with the role specified
    /// </summary>
    /// <param name="condition">The role to search for</param>
    /// <returns>An array containing the players that fit the definition</returns>
    public IEnumerable<Player> GetPlayers(string roleName)
    {
      return Alive.Where(x => x.role == Roles[roleName]);
    }

    #region The Properties of Data
    /// <summary>
    /// The number of players currently in the game
    /// </summary>
    public int PlayerCount { get { return Joined.Count; } }

    /// <summary>
    /// Boolean value indicating whether a game has been started
    /// </summary>
    public bool GameStarted
    {
      get { return GamePhase == GamePhase.Inactive; }
    }

    /// <summary>
    /// The current phase the game is going through
    /// </summary>
    public GamePhase GamePhase { get; set; } = GamePhase.Inactive;

    /// <summary>
    /// The current group the game is running on
    /// </summary>
    public long CurrentGroup { get; set; }

    /// <summary>
    /// The number of players currently alive
    /// </summary>
    public int AliveCount
    {
      get { return Alive.Count; }
    }

    /// <summary>
    /// Boolean value indicatiing if the mayor has revealed himself
    /// </summary>
    public bool HasRevealed { get; set; }
    #endregion

    private async void BotNormalMessage(string text)
    {
      await Program.Bot.SendTextMessageAsync(CurrentGroup, text,
        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
    }

    private async void BotNormalMessage(long id, string text)
    {
      await Program.Bot.SendTextMessageAsync(id, text, 
        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
    }

    private async void BotMessage(string key, params object[] args)
    {
      await Program.Bot.SendTextMessageAsync(CurrentGroup, string.Format(GameMessages[key], args),
        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
    }

    private async void BotMessage(long id, string key, params object[] args)
    {
      await Program.Bot.SendTextMessageAsync(id, string.Format(GameMessages[key], args),
        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
    }

    public static void LoadInstances()
    {
      Commands.GameInstances = new Dictionary<long, Game>();
      var doc = GDExtensions.SafeLoad("InstanceData.xml");
      var defaultSettings = QuizBot.Settings.AllSettings.ToDictionary(x => x.Name, x => x);
      foreach(var each in doc.Root.Elements("Instance"))
      {
        int group;
        string name;
        try
        {
          group = each.TryGetElementValue<int>(instanceFile, "CurrentGroup");
          name = each.TryGetElementValue("InstanceData.xml", "Name");
        }
        catch(InitException) { continue; }
        Commands.GameInstances.Add(group, new Game(name, group, new Settings(each.Element("Settings"))));
      }
    }
  }
}
