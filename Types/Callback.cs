using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot.Types;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace QuizBot
{
  public class Callback
  {
    public Callback(long playerId, string protocol, string data)
    {
      Protocol = protocol;
      Data = data;
      From = playerId;
    }

    public Callback(int group, long playerId, string protocol, string data)
    {
      Protocol = protocol;
      Data = data;
      From = playerId;
      Group = group;
    }

    /// <summary>
    /// The callback protocol
    /// </summary>
    public string Protocol { get; set; }

    /// <summary>
    /// The callback data
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// The Id of the user who sent the callback
    /// </summary>
    public long From { get; set; }

    public int Group { get; set; }

    public static implicit operator string(Callback it)
    {
      return JsonConvert.SerializeObject(it);
    }

  }
}
