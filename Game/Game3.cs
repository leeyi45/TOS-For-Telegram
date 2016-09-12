using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

namespace QuizBot
{
  //This file contains all the stuff pertaining to managing a game
  partial class Game
  {
    private class Settings
    {
      public Settings(IEnumerable<PropertyInfo> Info)
      {
        var properties = AllSettings.ToDictionary(x => x.Name, x => x);

        foreach (var info in Info)
        {
          var prop = properties[info.Name];
          prop.SetValue(this, prop.GetValue(null));
        }
      }

      public int MaxPlayers { get; private set; }

      public int MinPlayers { get; private set; }

      public bool UseNicknames { get; private set; }

      public int LynchTime { get; private set; }

      public int NightTime { get; private set; }

      public int DayTime { get; private set; }

      public IEnumerable<PropertyInfo> AllSettings
      {
        get {
          return from prop in typeof(Settings).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                 where prop.CanRead && prop.CanWrite
                 select prop; }
      }
    }

    private Thread GameStart;

    /// <summary>
    /// The current roles being used
    /// </summary>
    private readonly Dictionary<string, Role> Roles;

    /// <summary>
    /// The current rolelist being used
    /// </summary>
    private readonly Dictionary<Wrapper, int> Rolelist;

    private readonly Dictionary<string, string> GameMessages;

    private readonly Dictionary<string, string> Protocols;

    private readonly Settings settings;

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
      await Program.Bot.SendTextMessageAsync(CurrentGroup, text);
    }

    private async void BotNormalMessage(long id, string text)
    {
      await Program.Bot.SendTextMessageAsync(id, text);
    }

    private async void BotMessage(string key, params object[] args)
    {
      await Program.Bot.SendTextMessageAsync(CurrentGroup, string.Format(GameMessages[key], args));
    }

    private async void BotMessage(long id, string key, params object[] args)
    {
      await Program.Bot.SendTextMessageAsync(id, string.Format(GameMessages[key], args));
    }
  }
}
