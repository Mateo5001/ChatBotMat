using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotMat
{
  public class ChatCommand
  {
    public string Key { get; set; }
    public string Value { get; set; }
    public string[] argumentos { get; set; }
    public string Channel { get; set; }
  }
}
