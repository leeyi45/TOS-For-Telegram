using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading;
using Telegram.Bot.Types;
using System.Reflection;
using System.Threading.Tasks;

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
      public Settings(IEnumerable<SettingDetail> Info, Game parent)
      {
        CreateProperties();
        foreach (var info in Info)
        {
          var prop = SetPropertyValue[info.Name.ToString().ToLower()];
          prop.SetValue(this, info.GetValue(null));
        }
        this.parent = parent;
      }

      /// <summary>
      /// Constructor to create settings class based on existing data
      /// </summary>
      /// <param name="element">Settings XElement</param>
      public Settings(XElement element, Game parent)
      {
        CreateProperties();
        var quizbotsettings = QuizBot.Settings.SetPropertyValue;
        foreach (var each in element.Elements())
        {
          var prop = SetPropertyValue[each.Name.ToString().ToLower()];
          string val;
          element.TryGetElement(each.Name, out val);
          if(string.IsNullOrWhiteSpace(val)) val = quizbotsettings[
            each.Name.ToString().ToLower()].GetValue(null).ToString();
          prop.SetValue(this, Convert.ChangeType(val, prop.Info.PropertyType));
        }
        this.parent = parent;
      }

      private void CreateProperties()
      {
        SettingCount = AllSettings.Count();
        SetPropertyValue = AllSettings.ToDictionary(x => x.Name.ToLower(), x => x);
      }

      private Game parent;

      public int SettingCount { get; private set; }

      [SettingDetail("Max Player Count")]
      public int MaxPlayers
      {
        get { return maxplayers; }
        set
        {
          if (maxplayers <= minplayers)
          {
            throw new ConfigException("Max player count cannot be less than or equal to min player count!");
          }
          else maxplayers = value;
        }
      }

      [SettingDetail("Min Player Count")]
      public int MinPlayers
      {
        get { return minplayers; }
        set
        {
          if (maxplayers <= minplayers)
          {
            throw new ConfigException("Min player count cannot be greater than or equal to max player count!");
          }
          else minplayers = value;
        }
      }

      [SettingDetail("Use Nicknames")]
      public bool UseNicknames { get; private set; }

      [SettingDetail("Lynch Duration")]
      public int LynchTime { get; private set; }

      [SettingDetail("Night Duration")]
      public int NightTime { get; private set; }

      [SettingDetail("Day Duration")]
      public int DayTime { get; private set; }

      [SettingDetail("Join Duration", ExtraMessage = "Currently not in use")]
      public int JoinTime { get; private set; }

      [SettingDetail("Rolelist")]
      public string CurrentRoleList
      {
        get { return rolelist; }
        private set
        {
          if (!GameData.RoleLists.Keys.Contains(value)) throw new ConfigException("No such rolelist!");
          else rolelist = value;
        }
      }

      private string rolelist;

      private int maxplayers = 30;

      private int minplayers = 5;

      public IEnumerable<SettingDetail> AllSettings
      {
        get
        {
          foreach (var each in typeof(Settings).GetProperties(BindingFlags.Instance | BindingFlags.Public))
          {
            var attri = each.GetCustomAttribute<SettingDetail>();
            if (attri != null && each.CanRead && each.CanWrite)
            {
              attri.Info = each;
              var defaultVal = QuizBot.Settings.SetPropertyValue[attri.Name.ToLower()];
              attri.MaxValue = defaultVal.MaxValue;
              attri.MinValue = defaultVal.MinValue;
              yield return attri;
            }
          }
        }
      }

      public Dictionary<string, SettingDetail> SetPropertyValue { get; set; }

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

    private BackgroundWorker GameStart;

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

    public bool RefreshQueued { get; set; }

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
    /// Boolean value indicating whether a lobby has been created
    /// </summary>
    public bool LobbyCreated { get; set; }

    /// <summary>
    /// Boolean value indicating whether a game has been started
    /// </summary>
    public bool GameStarted
    {
      get { return (int)GamePhase > 1; }
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

    private void BotNormalMessage(string text)
    {
      BotNormalMessage(CurrentGroup, text);
    }

    private async void BotNormalMessage(long id, string text)
    {
     await Program.Bot.SendTextMessageAsync(id, text, 
        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
    }

    private void BotMessage(string key, params object[] args)
    {
      BotMessage(CurrentGroup, key, args);
    }

    private void BotMessage(long id, string key, params object[] args)
    {
      BotNormalMessage(id, string.Format(GameMessages[key], args));
    }

    public static void LoadInstances()
    {
      Commands.GameInstances = new Dictionary<long, Game>();
      var doc = GDExtensions.SafeLoad("InstanceData.xml");
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
        Commands.GameInstances.Add(group, new Game(name, group, each.Element("Settings")));
      }
    }
  }
}
