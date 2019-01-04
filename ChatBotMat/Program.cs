using System;
using System.Windows.Input;
using System.Timers;
using System.Collections.Generic;

namespace ChatBotMat
{
  class Program
  {
    public static bool Continuar { get; set; }
    static void Main(string[] args)
    {
      
      try
      {
        MatBot chat = new MatBot();
        chat.onSalir += salir;
        Continuar = true;
        while (Continuar)
        {

        }
      }
      catch (Exception e)
      {

        throw;
      }
     
    }

    private static void salir(object sender, EventArgs e)
    {
        Console.WriteLine("saliendo del bot");
        Continuar = false;
    }
    
  }
}
