using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Client.Enums;
using System.Timers;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using System.Net;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using ChatBotMat.Util;

namespace ChatBotMat
{
  internal class MatBot
  {
    public bool isInLive { get; set; }
    public List<MessageAliasTimer> listaTimers { get; set; }
    public List<ChatCommand> listaComandos { get { return obternetListaComandos(); } }
    public List<TUserSaludo> listaSaludosMod { get { return obternetSaludos(); } }
    public List<TUserJoined> listaUsuariosxStream { get; set; }
    public int messagesCount { get; set; }
    public EventHandler onSalir;
    public readonly ConnectionCredentials credentials = new ConnectionCredentials(con.BotName, con.Key);
    TwitchClient client;
    //public static Credentials con = new Credentials()
    //{
    //  BotName = "matew5001Bot",
    //  Key = "itr7dwzqzobobl5307ekgh6qeddz7d",
    //  Channel = "matew5001"
    //};
    public static Credentials con = new Credentials()
    {
      BotName = "pudindroid",
      Key = "nnaq51v4l7xxu326oq1yu0r5ioir65",
      Channel = "LydiaPudin"
    };
    //pudindroid
    public static string Channel { get; set; }

    public static DateTime LastSendedIMG { get; set; }

    public MatBot()
    {
      Channel = con.Channel;//"matew5001";

      //TwitchLib.Api.TwitchAPI api = new TwitchLib.Api.TwitchAPI();
      //TwitchLib.Api.Services.LiveStreamMonitorService clientApi = new TwitchLib.Api.Services.LiveStreamMonitorService(api);
      //clientApi.SetChannelsByName(new List<string>() { Channel });
      //clientApi.OnStreamOnline += onStreamStarts;
      //clientApi.OnStreamOffline += onStreamOffLine;
      //clientApi.Start();


      client = new TwitchClient();
      client.Initialize(credentials, Channel);
      client.OnJoinedChannel += joinedChanel;
      client.OnMessageReceived += mensajeRecivido;
      client.OnRaidNotification += raidRecivido;
      client.OnModeratorJoined += joinModeratos;
      client.OnChannelStateChanged += ChanelChangedState;
      listaUsuariosxStream = new List<TUserJoined>();
      listaTimers = obternetTimers();
      foreach (var item in listaTimers)
      {
        item.aTimer.Interval = item.Time * 60000;
        item.aTimer.Elapsed += (sender, e) => { OnTimePass(item); };
        item.aTimer.Enabled = true;
      }
      messagesCount = 0;
      client.Connect();

    }

    private void onStreamOffLine(object sender, OnStreamOfflineArgs e)
    {
      sendMessage("me retiro cuidense");
      Salir(null);
    }

    private void onStreamStarts(object sender, OnStreamOnlineArgs e)
    {
      listaUsuariosxStream = new List<TUserJoined>();
    }

    private void ChanelChangedState(object sender, OnChannelStateChangedArgs e)
    {
      //throw new NotImplementedException();
    }

    private void OnTimePass(MessageAliasTimer item)
    {
      if (messagesCount > 0)
        sendMessage(string.IsNullOrEmpty(item.value) ? item.command.Value : item.value);
    }


    private void joinModeratos(object sender, OnModeratorJoinedArgs e)
    {
      foreach (var item in listaSaludosMod)
      {
        if (item.Name.Equals(e.Username))
        {
          sendMessage(item.Saludo);
        }
      }
    }
    private void raidRecivido(object sender, OnRaidNotificationArgs e)
    {
      client.SendMessage(e.Channel, "Te recomendamos ver el contenido de @" + e.RaidNotificaiton.DisplayName + " en https://www.Twitch.com/" + e.RaidNotificaiton.DisplayName);
    }
    string limpiarMensaje(string msjIn)
    {
      return msjIn.Replace("@", "");
    }
    private void mensajeRecivido(object sender, OnMessageReceivedArgs e)
    {
      TUser us = verifiUserJoined(e);
      messagesCount++;
      commandRecived(e, out ChatCommandRequest cmd, out string[] arg);
      cmd.userRequest = us;
      #region comandos personalizados
      foreach (var item in listaComandos)
      {
        if (item.Key.Equals(cmd.Key))
        {
          sendMessage(item.Value);
          return;
        }
      }
      #endregion

      if (arg[0].Equals("!pop") || arg[0].Equals("!addtimer") || arg[0].Equals("!addcom") || arg[0].Equals("!editcom") || arg[0].Equals("!delcom") || arg[0].Equals("!adduser") || arg[0].Equals("!edituser") || arg[0].Equals("!deluser") || arg[0].Equals("!end"))
      {

        if (arg[0].Equals("!end"))
        {
          salir(e);
        }

        if (string.IsNullOrEmpty(cmd.argumentos[0].Trim()))
        {
          sendMessage("mal uso de el comando por favor leer la documentacion");
          return;
        }

        if (cmd.Key.Equals("!pop"))
        {
          if (IsImageUrl(cmd.argumentos[0]))
          {
            bool res = esModerada(cmd.argumentos[0]);


            if (res)
            {
              if (LastSendedIMG == null || DateTime.Now - LastSendedIMG > new TimeSpan(0, 0, 30))
              {
                LastSendedIMG = DateTime.Now;
                string archivo = HTMLGeneratedIMG(cmd);

                using (StreamWriter sw = new StreamWriter("Design/source.html"))
                {
                  sw.WriteLine(archivo);
                  sw.Close();
                }
                return;
              }
              else
              {
                sendMessage("@" + cmd.userRequest.Name + " , el spam es malo aunque divertido controlate... NotLikeThis");
                return;
              }
            }
            else
            {
              sendMessage("@" + cmd.userRequest.Name + " , Este es un canal cristiano, ¿que es eso?... NotLikeThis NotLikeThis NotLikeThis");
              return;
            }

          }
          else
          {
            sendMessage("@" + cmd.userRequest.Name + " estas seguro que lo que envias es una imagen Kappa");
            return;
          }
        }

        if (e.ChatMessage.UserType == UserType.Moderator || e.ChatMessage.UserType == UserType.Broadcaster)
        {
          #region comandos agregar
          if ((cmd.Key.Equals("!addtimer")))
          {
            agregarTimer(cmd);
            sendMessage("agregado satisfactorio");
          }
          //if ((cmd.Key.Equals("!edittimer")))
          //{
          //  editarTimer(cmd);
          //}
          //if ((cmd.Key.Equals("!deltimer")))
          //{
          //  eliminarTimer(cmd);
          //}
          #endregion
          #region comandos agregar
          if ((cmd.Key.Equals("!addcom")))
          {
            agregarComando(cmd);
            sendMessage("agregado satisfactorio");
          }
          if ((cmd.Key.Equals("!editcom")))
          {
            editarComando(cmd);
            sendMessage("agregado satisfactorio");
          }
          if ((cmd.Key.Equals("!delcom")))
          {
            eliminarComando(cmd);
            sendMessage("agregado satisfactorio");
          }
          #endregion
          #region saludos para mod
          if ((cmd.Key.Equals("!adduser")))
          {
            agregarSaludoMod(cmd);
            sendMessage("agregado satisfactorio");
          }
          if ((cmd.Key.Equals("!edituser")))
          {
            editarSaludoMod(cmd);
            sendMessage("agregado satisfactorio");
          }
          if ((cmd.Key.Equals("!deluser")))
          {
            eliminarSaludoMod(cmd);
            sendMessage("agregado satisfactorio");
          }
          #endregion
        }
        else
        {
          sendMessage("@" + e.ChatMessage.Username + ", solo los moderadores pueden usar este comando");
        }
      }

    }

    private bool esModerada(string v)
    {
      try
      {
        ResponseDatosmoderacionImagen img = null;
        string url = "https://api.sightengine.com/1.0/check.json?models=nudity,wad,offensive&api_user=812851622&api_secret=BSmcaeNnkcriSWef2ny8&url=" + v;

        var req = (HttpWebRequest)HttpWebRequest.Create(url);
        using (var resp = req.GetResponse())
        {
          Stream receiveStream = resp.GetResponseStream();
          StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
          string content = reader.ReadToEnd();
          img = JsonConvert.DeserializeObject<ResponseDatosmoderacionImagen>(content);
        }
        // Get the response.

        return img.nudity.safe > 0.60 && img.weapon < 0.2 && img.drugs < 0.2;
      }
      catch (Exception)
      {
        return false;
      }
    }

    private static string HTMLGeneratedIMG(ChatCommandRequest cmd)
    {
      return @"<!DOCTYPE html>
 <html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"" >
    <head>
        <meta charset = ""utf-8"" />
  <link href=""style.css"" rel=""stylesheet"" />
  <link href=""https://cdnjs.cloudflare.com/ajax/libs/animate.css/3.7.0/animate.min.css"" rel=""stylesheet"" />
   <script src=""https://code.jquery.com/jquery-3.2.1.js""></script>
<script type=""text/javascript"">
$(document).ready(function() {
    setTimeout(function() {
        $("".content"").fadeOut(500);
    },12500);
	})
	</script>
            <title></title>
     </head>
     <body>
<div class=""content"">
       <img class=""animated rollIn"" width=""720"" height=""480"" src=""" + cmd.argumentos[0] + @""" />
</div>      
</body>
      </html>";
    }
    bool IsImageUrl(string URL)
    {
      try
      {

        var req = (HttpWebRequest)HttpWebRequest.Create(URL);
        req.Method = "HEAD";
        using (var resp = req.GetResponse())
        {
          return resp.ContentType.ToLower(CultureInfo.InvariantCulture)
                     .StartsWith("image/");
        }
      }
      catch (Exception)
      {
        return false;
      }
    }
    private void commandRecived(OnMessageReceivedArgs e, out ChatCommandRequest cmd, out string[] arg)
    {
      cmd = new ChatCommandRequest();
      arg = e.ChatMessage.Message.Split(" ");
      cmd.Key = arg[0];
      cmd.Value = e.ChatMessage.Message.Replace(cmd.Key + " ", "");
      cmd.argumentos = limpiarMensaje(e.ChatMessage.Message.Replace(cmd.Key + " ", "")).Split(" ");
      cmd.Channel = Channel;
    }

    private TUser verifiUserJoined(OnMessageReceivedArgs e)
    {
      TUserJoined user = new TUserJoined() { Name = e.ChatMessage.Username, isJoined = true, Nivel = e.ChatMessage.UserType, Channel = Channel };
      var foundit = listaUsuariosxStream.Find(x => x.Name.Equals(user.Name, StringComparison.CurrentCultureIgnoreCase) && user.Channel == Channel);
      if (foundit == null)
      {
        listaUsuariosxStream.Add(user);
        OnUserJoined(user);
      }
      return user;
    }

    private void OnUserJoined(TUserJoined user)
    {
      List<TUserSaludo> listasaludos = obternetSaludos();
      var userSaludo = listasaludos.Find(x => x.Name == user.Name && x.Channel == Channel);
      if (userSaludo != null)
      {
        sendMessage(userSaludo.Saludo);
      }
      else
      {
        switch (user.Nivel)
        {
          case UserType.Viewer:
            onUserJoined(user);
            break;
          case UserType.Moderator:
            onModJoined(user);
            break;
          default:
            break;
        }
      }
    }

    private void onModJoined(TUserJoined user)
    {
      //sendMessage("cuidado todos, acaba de llegar la espada de @" + user.Name + ", pero olvido afilarla Kappa");
    }

    private void onUserJoined(TUserJoined user)
    {
      sendMessage("Bienvenido @" + user.Name + " disfruta del stream en compañia de todos BloodTrail");
    }

    private void eliminarTimer(ChatCommandRequest cmd)
    {
      List<MessageAlias> mensages = obternetTimers<MessageAlias>();
    }

    private void agregarTimer(ChatCommandRequest cmd)
    {
      MessageAlias timer = new MessageAlias();
      timer.Time = int.Parse(cmd.argumentos[0]);
      if (cmd.argumentos[1].StartsWith('!'))
      {
        timer.command = listaComandos.Find(x => x.Key.Equals(cmd.argumentos[1]));
      }
      else
      {
        timer.value = cmd.Value.Substring(cmd.argumentos[0].Length + 1);
      }
      MessageAliasTimer timerT = new MessageAliasTimer();
      timerT.Time = int.Parse(cmd.argumentos[0]);
      if (cmd.argumentos[1].StartsWith('!'))
      {
        timerT.command = listaComandos.Find(x => x.Key.Equals(cmd.argumentos[1]));
      }
      else
      {
        timerT.value = cmd.Value.Substring(cmd.argumentos[0].Length + 1);
      }

      timerT.aTimer.Interval = timerT.Time * 60000;
      timerT.aTimer.Elapsed += (sender, e) => { OnTimePass(timerT); };
      timerT.aTimer.Enabled = true;
      timerT.Channel = Channel;
      listaTimers.Add(timerT);

      List<MessageAlias> mensages = obternetTimers<MessageAlias>();
      mensages.Add(timer);
      escribirListaTimers(mensages);


    }

    private void editarSaludoMod(ChatCommandRequest cmd)
    {
      eliminarSaludoMod(cmd);
      agregarSaludoMod(cmd);
    }
    private void eliminarSaludoMod(ChatCommandRequest cmd)
    {
      List<TUserSaludo> ms = obternetSaludos();
      ms.Remove(ms.Find(x => x.Name == cmd.argumentos[0]));
      escribirListaSaludos(ms);
    }
    private void agregarSaludoMod(ChatCommandRequest cmd)
    {
      TUserSaludo salud = obtenerSaludo(cmd);
      List<TUserSaludo> ms = obternetSaludos();
      ms.Add(salud);
      escribirListaSaludos(ms);
    }
    private static TUserSaludo obtenerSaludo(ChatCommandRequest cmd)
    {
      TUserSaludo salud = new TUserSaludo();
      salud.Name = cmd.argumentos[0];
      salud.Channel = Channel;
      salud.Saludo = cmd.Value.Substring(cmd.argumentos[0].Length + 1);
      return salud;
    }
    public void sendMessage(string pMsg)
    {
      client.SendMessage(Channel, pMsg);
    }
    private void eliminarComando(ChatCommandRequest cmd)
    {
      List<ChatCommand> lc = obternetListaComandos();
      lc.Remove(lc.Find(x => x.Key == cmd.argumentos[0]));
      escribirListaComandos(lc);
    }
    private void editarComando(ChatCommandRequest cmd)
    {
      ChatCommand newCmd = newCommand(cmd);
      List<ChatCommand> lc = obternetListaComandos();
      lc.Remove(lc.Find(x => x.Key == newCmd.Key));
      lc.Add(newCmd);
      escribirListaComandos(lc);
    }
    private void salir(OnMessageReceivedArgs e)
    {
      sendMessage("me retiro cuidense");
      Salir(null);
    }
    private void agregarComando(ChatCommandRequest cmd)
    {
      ChatCommand newCmd = newCommand(cmd);
      List<ChatCommand> lc = obternetListaComandos();
      lc.Add(newCmd);
      escribirListaComandos(lc);
    }
    private static ChatCommand newCommand(ChatCommandRequest cmd)
    {
      return new ChatCommand()
      {
        Key = cmd.argumentos[0],
        Value = cmd.Value.Substring(cmd.argumentos[0].Length + 1),
        argumentos = cmd.argumentos
      };
    }
    private static void escribirListaComandos(List<ChatCommand> lc)
    {
      string comandos = JsonConvert.SerializeObject(lc);
      using (StreamWriter sw = new StreamWriter("listaComandos.json"))
      {
        sw.WriteLine(comandos);
        sw.Close();
      }
    }
    private static void escribirListaSaludos(List<TUserSaludo> mj)
    {
      string comandos = JsonConvert.SerializeObject(mj);
      using (StreamWriter sw = new StreamWriter("listaSaludos.json"))
      {
        sw.WriteLine(comandos);
        sw.Close();
      }
    }
    private static void escribirListaTimers(List<MessageAlias> mj)
    {
      string comandos = JsonConvert.SerializeObject(mj);
      using (StreamWriter sw = new StreamWriter("listaTimers.json"))
      {
        sw.WriteLine(comandos);
        sw.Close();
      }
    }

    private List<ChatCommand> obternetListaComandos()
    {
      try
      {
        string comandos = string.Empty;
        using (StreamReader reader = new StreamReader("listaComandos.json"))
        {
          comandos = reader.ReadToEnd();
        }
        List<ChatCommand> listaComandos = JsonConvert.DeserializeObject<List<ChatCommand>>(comandos);
        return listaComandos;
      }
      catch
      {
        return new List<ChatCommand>();
      }
    }

    private List<MessageAlias> obternetTimers<MessageAlias>()
    {
      try
      {
        string comandos = string.Empty;
        using (StreamReader reader = new StreamReader("listaTimers.json"))
        {
          comandos = reader.ReadToEnd();
        }
        List<MessageAlias> listaComandos = JsonConvert.DeserializeObject<List<MessageAlias>>(comandos);
        return listaComandos;
      }
      catch
      {
        return new List<MessageAlias>();
      }
    }
    private List<MessageAliasTimer> obternetTimers()
    {
      try
      {
        string comandos = string.Empty;
        using (StreamReader reader = new StreamReader("listaTimers.json"))
        {
          comandos = reader.ReadToEnd();
        }
        List<MessageAliasTimer> listaComandos = JsonConvert.DeserializeObject<List<MessageAliasTimer>>(comandos);
        return listaComandos.FindAll(x => x.Channel == Channel);
      }
      catch
      {
        return new List<MessageAliasTimer>();
      }
    }
    private List<TUserSaludo> obternetSaludos()
    {
      try
      {
        string comandos = string.Empty;
        using (StreamReader reader = new StreamReader("listaSaludos.json"))
        {
          comandos = reader.ReadToEnd();
        }
        List<TUserSaludo> listaComandos = JsonConvert.DeserializeObject<List<TUserSaludo>>(comandos);
        return listaComandos.FindAll(x => x.Channel == Channel);
      }
      catch
      {
        return new List<TUserSaludo>();
      }
    }

    private void joinedChanel(object sender, OnJoinedChannelArgs e)
    {
      sendMessage("El PudinDroid se acaba de unir Kappa PogChamp");
    }
    protected virtual void Salir(EventArgs e)
    {
      onSalir?.Invoke(this, e);
    }
  }
}