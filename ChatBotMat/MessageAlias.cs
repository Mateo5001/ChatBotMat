using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotMat
{
  class MessageAlias
  {
    public ChatCommand command { get; set; }
    public int Time { get; set; }
    public string value { get; set; }
    public string Channel { get; set; }
  }
}
