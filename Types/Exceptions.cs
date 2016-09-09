using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QuizBot
{
  [Serializable]
  /// <summary>
  /// Base exception class
  /// </summary>
  class GameException : Exception
  {
    public GameException(string message) : base(message) { }
  }

  [Serializable]
  /// <summary>
  /// Exception class for Initialization errors
  /// </summary>
  class InitException : GameException
  {
    public InitException(string file, string message, XElement each) :
      base("Error while reading " + file + " at line " + (each as System.Xml.IXmlLineInfo).LineNumber + ": ")
    { }

    public InitException(string message) :
      base("Encountered an error while reading roles: " + message)
    { }
  }

  [Serializable]
  /// <summary>
  /// Exception class for errors during role assignment
  /// </summary>
  class AssignException : GameException
  {
    public AssignException(string message) : 
      base ("Error occurred while assigning roles:" + message) { }
  }
}
