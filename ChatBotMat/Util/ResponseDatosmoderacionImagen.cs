using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotMat.Util
{
  public class ResponseDatosmoderacionImagen
  {
    public string status { get; set; }
    public RequestDatosMod request { get; set; }
    public float weapon { get; set; }
    public float alcohol { get; set; }
    public float drugs { get; set; }
    public Nudity nudity { get; set; }

    public Ofensive offensive { get; set; }
    public Media media { get; set; }

  }

  public class Ofensive
  {
    public float prob { get; set; }

  }

  public class Media
  {
    public string id { get; set; }
    public string uri { get; set; }

  }

  public class Nudity
  {
    public float raw { get; set; }
    public float safe { get; set; }
    public float partial { get; set; }

  }

  public class RequestDatosMod
  {
    public string id { get; set; }
    public float timestamp { get; set; }
    public int operations { get; set; }

  }
}
