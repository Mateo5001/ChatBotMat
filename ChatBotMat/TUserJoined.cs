using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotMat
{
  public class TUserJoined :TUser
  {
    public bool isJoined { get; set; }
    public string Channel { get; set; }
  }
}
