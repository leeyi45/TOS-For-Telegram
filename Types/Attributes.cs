using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QuizBot
{
  //Class originally defined in the werewolf for telegram
  public class Command : Attribute
  {
    /// <summary>
    /// The string to trigger the command
    /// </summary>
    public string Trigger { get; set; }

    /// <summary>
    /// Is this command limited to bot admins only
    /// </summary>
    public bool GlobalAdminOnly { get; set; } = false;

    /// <summary>
    /// Is this command limited to group admins only
    /// </summary>
    public bool GroupAdminOnly { get; set; } = false;

    /// <summary>
    /// Developer only command
    /// </summary>
    public bool DevOnly { get; set; } = false;

    /// <summary>
    /// Marks the command as something to block (for example, in support chat)
    /// </summary>
    public bool Blockable { get; set; } = false;

    /// <summary>
    /// Marks the command as to be used within a group only
    /// </summary>
    public bool InGroupOnly { get; set; } = false;

    /// <summary>
    /// Marks the command as to be used within a private chat only
    /// </summary>
    public bool InPrivateOnly { get; set; } = false;

    [Obsolete("Use GameInstance", true)]
    /// <summary>
    /// Marks the command as one to be used after a lobby has been created
    /// </summary>
    public bool GameStartOnly { get; set; } = false;

    /// <summary>
    /// Marks the command to be used after a lobby has been created
    /// </summary>
    public bool GameInstance { get; set; } = false;

    private bool testmode = false;

    /// <summary>
    /// Boolean value indicating if the command is to be used in test mode only
    /// </summary>
    public bool TestModeOnly
    { get { return testmode; }
      set
      { //Test commands can only be used by devs
        if (value) DevOnly = true;
        testmode = value;
      }
    }
  }

  //My own work tyvm
  public class ConsoleCommand : Attribute
  {
    public ConsoleCommand(string trigger, params string[] args)
    {
      Trigger = trigger;
      Args = args;
      IsMetaData = false;
    }

    public ConsoleCommand(bool IsMetaData = true)
    {
      this.IsMetaData = IsMetaData;
    }

    public string Trigger { get; set; }

    public string[] Args { get; set; }

    public bool IsMetaData { get; set; }
  }
}
