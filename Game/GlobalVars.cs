using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace QuizBot
{
  static class CommandVars
  {
    public static bool GetUserId { get; set; } = false;

    public static List<Player> PlayersInGame { get; set; } = new List<Player>();

    public static bool GettingConfigOption { get; set; } = false;

    public static Dictionary<int, Tuple<bool, string>> ReceivingVals { get; set; }

    public static bool Connected
    {
      get { return connected; }
      set
      {
        try
        {
          Program.startup.ConsoleForm.SwitchLabelState("connect", value);
        }
        catch(Exception) { }
        connected = value;
      }
    }

    public static bool RolesLoaded
    {
      get { return rolesloaded; }
      set
      {
        try
        {
          Program.startup.ConsoleForm.SwitchLabelState("role", value);
        }
        catch (Exception) { }
        rolesloaded = value;
      }
    }

    public static bool messagesLoaded
    {
      get { return messagesloaded; }
      set
      {
        try
        {
          Program.startup.ConsoleForm.SwitchLabelState("message", value);
        }
        catch (Exception) { }
        messagesloaded = value;
      }
    }

    public static bool protocolsLoaded
    {
      get { return protocolsloaded; }
      set
      {
        try
        {
          Program.startup.ConsoleForm.SwitchLabelState("protocol", value);
        }
        catch (Exception) { }
        protocolsloaded = value;
      }
    }

    #region Hidden fields
    private static bool connected = false;

    private static bool rolesloaded = false;

    private static bool messagesloaded = false;

    private static bool protocolsloaded = false;
    #endregion
  }

  /// <summary>
  /// Class containing the settings system
  /// </summary>
  static class Settings
  {
    [SettingDetail("Max Player Count", 2)]
    /// <summary>
    /// The maximum number of players allowed per game
    /// </summary>
    public static int MaxPlayers
    {
      get { return Properties.Settings.Default.Max_Users; }
    }

    [SettingDetail("Min Player Count", 2, ExtraMessage = "Not recommended to change")]
    /// <summary>
    /// The minimum number of players allowed per game
    /// </summary>
    public static int MinPlayers
    {
      get { return Properties.Settings.Default.Min_Users; }
    }

    [SettingDetail("Join Time", 20, ExtraMessage = "Not Currently In Use")]
    /// <summary>
    /// The amount of time the join phase is allocated, in seconds
    /// </summary>
    public static int JoinTime
    {
      get { return Properties.Settings.Default.Join_Time; }
    }

    /// <summary>
    /// The amount of time the join phase is allocated, in milliseconds
    /// </summary>
    public static int JoinTimeMili
    {
      get { return Properties.Settings.Default.Join_Time * 1000; }
    }

    [SettingDetail("Night Duration", 20)]
    /// <summary>
    /// The amount of time the night time phase is allocated, in seconds
    /// </summary>
    public static int NightTime
    {
      get { return Properties.Settings.Default.Night_Cycle; }
    }

    [SettingDetail("Day Duration", 20)]
    /// <summary>
    /// The amount of time the day time phase is allocated, in seconds
    /// </summary>
    public static int DayTime
    {
      get { return Properties.Settings.Default.Day_Cycle; }
    }

    [SettingDetail("Lynch Duration", 20)]
    /// <summary>
    /// The amount of time the lynch phase is allocated, in seconds
    /// </summary>
    public static int LynchTime
    {
      get { return Properties.Settings.Default.Voting_Cycle; }
    }

    [SettingDetail("Rolelist")]
    /// <summary>
    /// The currently selected rolelist name
    /// </summary>
    public static string CurrentRoleList
    {
      get { return Properties.Settings.Default.Rolelist; }
    }

    [SettingDetail("Nicknames")]
    /// <summary>
    /// Boolean value indicating if nicknames should be used
    /// </summary>
    public static bool UseNicknames
    {
      get { return Properties.Settings.Default.UseNicknames; }
    }

    /// <summary>
    /// The currently selected rolelist
    /// </summary>
    public static Dictionary<Wrapper, int> CurrentRoles
    {
      get { return GameData.RoleLists[CurrentRoleList]; }
    }

    /// <summary>
    /// Dictionary containing the data of all the current settings
    /// </summary>
    public static Dictionary<string, SettingDetail> SetPropertyValue { get; private set; }

    /// <summary>
    /// Returns an enumerator to iterate through all the settings
    /// </summary>
    public static IEnumerable<SettingDetail> AllSettings
    {
      get
      {
        foreach(var each in typeof(Settings).GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
          var attri = each.GetCustomAttribute<SettingDetail>();
          if(attri != null)
          {
            attri.Info = each;
            yield return attri;
          }
        }
      }
    }

    /// <summary>
    /// Integer value indicating the number of settings currently defined
    /// </summary>
    public static int SettingCount { get; private set; }

    /// <summary>
    /// Integer value indicating the smallest amount of time permissible between messages
    /// in milliseconds
    /// </summary>
    public static int MaxMessage
    {
      get { return Properties.Settings.Default.MaxMessage; }
    }

    /// <summary>
    /// Loads all the properties into a dictionary
    /// </summary>
    public static void LoadProperties()
    {
      SettingCount = AllSettings.Count();
      SetPropertyValue = AllSettings.ToDictionary(x => x.Name.ToLower(), x => x);
    }

    /// <summary>
    /// Returns the value of the property with the given key
    /// </summary>
    /// <param name="key">The name of the property</param>
    /// <returns>The value of the property as a string</returns>
    public static string GetPropertyValue(string key)
    {
      return SetPropertyValue[key].Info.GetValue(null).ToString();
    }
  }

  public class SettingDetail : Attribute
  {
    public SettingDetail(string displayName, string onselect = "", string extramsg = "")
    {
      DisplayName = displayName;
      OnSelect = onselect;
      ExtraMessage = extramsg;
      MinValue = -1;
      MaxValue = -1;
    }

    public SettingDetail(string displayName, int minValue, int maxValue = -1)
    {
      DisplayName = displayName;
      MinValue = minValue;
      MaxValue = maxValue;
    }

    /// <summary>
    /// Name to display on the config screen
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Name of the underlying PropertyInfo
    /// </summary>
    public string Name { get { return Info.Name; } }

    /// <summary>
    /// Any extra messages to send the user
    /// </summary>
    public string ExtraMessage { get; set; }

    /// <summary>
    /// Message to send on option select
    /// </summary>
    public string OnSelect { get; set; }

    public PropertyInfo Info { get; set; }

    public int MinValue { get; set; }

    public int MaxValue { get; set; }

    public void SetValue(object val) { SetValue(null, val); }

    public void SetValue(object target, object val)
    {
      var value = Convert.ChangeType(val, Info.PropertyType);
      if (Info.PropertyType == typeof(int))
      {
        if (MinValue != -1 && (int)value < MinValue)
        {
          throw new ConfigException(value + " is too small for " + DisplayName);
        }
        else if (MaxValue != -1 && (int)value > MaxValue)
        {
          throw new ConfigException(value + " is too large for " + DisplayName);
        }
      }
      Info.SetValue(target, value);
    }

    public object GetValue(object target)
    {
      return Info.GetValue(target);
    }

  }
}
