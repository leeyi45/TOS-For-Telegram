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
  class Callback
  {
    /*
    public Callback(string protocol, string data)
    {
      Protocol = protocol;
      Data = data;
    }*/

    public Callback(Player from, string protocol, string data)
    {
      Protocol = protocol;
      Data = data;
      From = from;
    }

    public Callback(string JSON)
    {
      var data = JObject.Parse(JSON);
      Data = (string)data["Data"];
      Protocol = (string)data["Protocol"];
      From = new Player(int.Parse((string)data["From"]["Id"]), (string)data["From"]["Username"],
        (string)data["From"]["FirstName"], (string)data["From"]["LastName"]);
    }

    public string Protocol { get; set; }

    public string Data { get; set; }

    public Player From { get; set; }

    public static implicit operator string(Callback it)
    {
      return JsonConvert.SerializeObject(it);
    }

  }
}
