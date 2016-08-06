using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QuizBot
{
	enum Cycle
	{
		Day = 1,
		Night = 2,
		Lynch = 3
	}

	enum Team
	{
		Neutral = 3,
		Evil = 2,
		Town = 1
	}

	class Role
	{
		public Role(string _name, Team _team, string d, string a)
		{
			Name = _name;
			team = _team;
			attribute = a;
			description = d;
		}

		public string Name { get; private set; }

		public Team team { get; private set; }

		public string description { get; private set; }

		public string attribute { get; private set; }
	}

	class Player : User
	{
		public Role role { get; set; }
	}
}
