using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot
{
	class Player_Mgt
	{

    /// <summary>
    /// Return players with the role specified
    /// </summary>
    /// <param name="condition">The role to search for</param>
    /// <returns>An array containing the players that fit the definition</returns>
    public static Player[] ReturnPlayers(Role condition)
    {
      return GameData.Joined.Values.Where(x => x.role == condition).ToArray();
    }


    public static void ProcessHeals()
    {
      foreach(var player in ReturnPlayers(GameData.Roles["Doctor"]))
      {
        GameData.GetPlayer(player.ActionTarget).Healed = true;
      }
    }

    public static void ProcessEscort()
    {
      foreach (var player in ReturnPlayers(GameData.Roles["Escort"]))
      {
        if(player.role == GameData.Roles["Doctor"])
        { //Reset doctor RB
          player.ActionTarget.Healed = false;
        }
        else if(player.ActionTarget.role == GameData.Roles["Serial Killer"])
        { //Escort visiting SK kills them
          player.Kill(player.ActionTarget);
        }
        GameData.GetPlayer(player.ActionTarget).IsRoleBlocked = true;
      }

      foreach (var player in ReturnPlayers(GameData.Roles["Consort"]))
      {
        if (player.role == GameData.Roles["Doctor"])
        {
          player.ActionTarget.Healed = false;
        }
        else if (player.ActionTarget.role == GameData.Roles["Serial Killer"])
        { 
          player.Kill(player.ActionTarget);
        }
        GameData.GetPlayer(player.ActionTarget).IsRoleBlocked = true;
      }
    }

    public static void ProcessSK()
    {
      foreach(var player in ReturnPlayers(GameData.Roles["Serial Killer"]))
      {
        if(player.role.NightImmune)
        { //Inform the player
          
        }
        else
        {
          player.ActionTarget.Kill(player);
        }
      }
    }
    //Perhaps combine the two functions idk
    public static void ProcessMafioso()
    {
      foreach (var player in ReturnPlayers(GameData.Roles["Mafioso"]))
      {
        if (player.role.NightImmune)
        { //Inform the player

        }
        else
        {
          player.ActionTarget.Kill(player);
        }
      }
    }

    public static void UpdateRolesAfterNight()
    {
      //Process the doctors' first
      ProcessHeals();

      //Process the roleblocks next
      ProcessEscort();

      //Process the SKs next
      ProcessSK();

      //Process the mafioso
      ProcessMafioso();

    }
  }
}
