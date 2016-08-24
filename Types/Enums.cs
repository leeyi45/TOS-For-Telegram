using System;
using System.Collections;
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
  public enum Cycle
	{
		Day = 1,
		Night = 2,
		Lynch = 3
	}

	public enum Team
	{
		Neutral = 3,
		Mafia = 2,
		Town = 1
	}

	public enum GamePhase
	{
		Inactive,
		Joining,
		Assigning,
		Running
	}
}
