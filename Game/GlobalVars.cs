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

    public static bool GettingNicknames { get; set; } = false;

    public static bool GettingConfigOption { get; set; } = false;

    public static Dictionary<int, Tuple<bool, string>> ReceivingVals { get; set; }

    public static bool Connected { get; set; } = false;
  }

  /// <summary>
  /// Class containing the settings system
  /// </summary>
  static class Settings
  {
    [SettingDetail("Max Player Count")]
    /// <summary>
    /// The maximum number of players allowed per game
    /// </summary>
    public static int MaxPlayers
    {
      get { return Properties.Settings.Default.Max_Users; }
      set { Properties.Settings.Default.Max_Users = value; }
    }

    [SettingDetail("Min Player Count", "Recommended not to change")]
    /// <summary>
    /// The minimum number of players allowed per game
    /// </summary>
    public static int MinPlayers
    {
      get { return Properties.Settings.Default.Min_Users; }
      set { Properties.Settings.Default.Min_Users = value; }
    }

    [SettingDetail("Join Time", "Not currently in use")]
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

    [SettingDetail("Night Duration")]
    /// <summary>
    /// The amount of time the night time phase is allocated, in seconds
    /// </summary>
    public static int NightTime
    {
      get { return Properties.Settings.Default.Night_Cycle; }
      set { Properties.Settings.Default.Night_Cycle = value; }
    }

    [SettingDetail("Day Duration")]
    /// <summary>
    /// The amount of time the day time phase is allocated, in seconds
    /// </summary>
    public static int DayTime
    {
      get { return Properties.Settings.Default.Day_Cycle; }
      set { Properties.Settings.Default.Day_Cycle = value; }
    }

    [SettingDetail("Lynch Duration")]
    /// <summary>
    /// The amount of time the lynch phase is allocated, in seconds
    /// </summary>
    public static int LynchTime
    {
      get { return Properties.Settings.Default.Voting_Cycle; }
      set { Properties.Settings.Default.Voting_Cycle = value; }
    }

    [SettingDetail("Rolelist")]
    /// <summary>
    /// The currently selected rolelist name
    /// </summary>
    public static string CurrentRoleList
    {
      get { return Properties.Settings.Default.Rolelist; }
      set
      {
        if(GameData.RoleLists.Keys.Contains(value))
        {
          Properties.Settings.Default.Rolelist = value;
        }
        else
        {
          throw new ArgumentException("No such rolelist");
        }
      }
    }

    [SettingDetail("Nicknames")]
    /// <summary>
    /// Boolean value indicating if nicknames should be used
    /// </summary>
    public static bool UseNicknames
    {
      get { return Properties.Settings.Default.UseNicknames; }
      set { Properties.Settings.Default.UseNicknames = value; }
    }

    /// <summary>
    /// The currently selected rolelist
    /// </summary>
    public static Dictionary<Wrapper, int> CurrentRoles
    {
      get { return GameData.RoleLists[CurrentRoleList]; }
    }

    public static Dictionary<string, SettingInfo> SetPropertyValue { get; private set; }

    /// <summary>
    /// Returns an enumerator to iterate through all the settings
    /// </summary>
    public static IEnumerable<PropertyInfo> AllSettings
    {
      get
      {
        return from each in typeof(Settings).GetProperties(BindingFlags.Static | BindingFlags.Public)
               where each.GetCustomAttribute(typeof(SettingDetail)) != null
               select each;
      }
    }

    public static int SettingCount { get; private set; }

    /// <summary>
    /// Loads all the properties into a dictionary
    /// </summary>
    public static void LoadProperties()
    {
      SettingCount = AllSettings.Count();
      SetPropertyValue = AllSettings.ToDictionary(x => x.Name, x =>
          {
            return new SettingInfo((SettingDetail)x.GetCustomAttribute(typeof(SettingDetail)), x);
          });
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

  class SettingDetail : Attribute
  {
    public SettingDetail(string displayName, string onselect = "", string extramsg = "")
    {
      DisplayName = displayName;
      OnSelect = onselect;
      ExtraMessage = extramsg;
    }

    /// <summary>
    /// Name to display on the config screen
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Any extra messages to send the user
    /// </summary>
    public string ExtraMessage { get; set; }

    /// <summary>
    /// Message to send on option select
    /// </summary>
    public string OnSelect { get; set; }
  }

  class SettingInfo
  {
    public SettingInfo(SettingDetail details, PropertyInfo info)
    {
      this.details = details;
      Info = info;
    }

    private SettingDetail details;

    public string DisplayName
    {
      get { return details.DisplayName; }
      set { details.DisplayName = value; }
    }

    public string ExtraMessage
    {
      get { return details.ExtraMessage; }
      set { details.ExtraMessage = value; }
    }

    public string OnSelect
    {
      get { return details.OnSelect; }
      set { details.OnSelect = value; }
    }

    public PropertyInfo Info { get; set; }

    public void SetValue(object val)
    {
      Info.SetValue(null, Convert.ChangeType(val, Info.PropertyType));
    }
  }
}
