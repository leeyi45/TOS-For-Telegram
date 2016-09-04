using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QuizBot
{
  /// <summary>
  /// Base exception class
  /// </summary>
  class GameException : Exception
  {
    public GameException(string message) : base(message) { }
  }

  /// <summary>
  /// Exception class for Initialization errors
  /// </summary>
  class InitException : GameException
  {
    public InitException(string message, XElement each) :
      base("Error while reading roles.xml at line " + (each as System.Xml.IXmlLineInfo).LineNumber + ": ")
    { }

    public InitException(string message) :
      base("Encountered an error while reading roles: " + message)
    { }

    public InitException(string file, string message) :
      base("Encountered an error while reading " + file + " " + message)
    { }
  }
}
