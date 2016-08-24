﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
  }
}
