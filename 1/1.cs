using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Numerics;
using System.Security;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Vector = Jypeli.Vector;

namespace _1;

/// @author Oliver Kandén
/// @version 22.10.2023
/// <summary>
/// 
/// </summary>
public class _1 : PhysicsGame
{
    DoubleMeter alaspainlaskuri;
    Timer aikalaskuri;
    private IntMeter pistelaskuri;
    private PhysicsObject kori1;
    private PhysicsObject kori2;
    private PhysicsObject koripallo;
    private PhysicsObject laskuri;
    private string[] vaihtoehdot = { "Aloita peli", "Näytä ohjeet", "Lopeta peli" };

    private string[] randomtekstit =
        { "Wau, älä lopeta", "Heität kuin Michael Jordan!", "Miten voit olla noin tulessa!!" };
    
    
    public override void Begin()
    {
        Level.Background.Color = Color.Black;
        Level.Background.Image = LoadImage("ht_suunnitelma_luonnos_kuva.png");
        Camera.ZoomToLevel();
        Level.CreateLeftBorder();
        Level.CreateRightBorder();
        Level.CreateBottomBorder();
        LuoAikalaskuri();
        LaskeAlaspain();
        LuoPisteLaskuri();
        LuoKoritJaLaskuri();
        Nappaimet();
        //NaytaTekstiKorinJalkeen();
        Gravity = new Vector(0, -200);

        MultiSelectWindow alkuvalikko = new MultiSelectWindow("Pelin alkuvalikko", vaihtoehdot);
        alkuvalikko.AddItemHandler(0, LuoPallo);
        alkuvalikko.AddItemHandler(1, ShowControlHelp);
        alkuvalikko.AddItemHandler(2, ConfirmExit);

        alkuvalikko.Color = Color.Orange;
        alkuvalikko.SetButtonColor(Color.Black);
        alkuvalikko.SetButtonTextColor(Color.White);

        PushButton[] nappulat = alkuvalikko.Buttons;
        Add(alkuvalikko);
    }

    
    
    
    /// <summary>
    /// Aliohjelma, jossa pelissä käytettävät näppäimet.
    /// </summary>
    private void Nappaimet()
    {
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");


        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Apua");
        Keyboard.Listen(Key.Up, ButtonState.Pressed, LyoPalloa, "Lyö palloa ylöspäin", new Vector(0, 250));
        Keyboard.Listen(Key.Left, ButtonState.Pressed, LyoPalloa, "Lyö palloa vasemmalle", new Vector(-250, 0));
        Keyboard.Listen(Key.Space, ButtonState.Pressed, LuoPallo, "Luo uusi pallo");
    }



    /// <summary>
    /// Aliohjelma, jolla palloa pystytään ohjaamaan.
    /// </summary>
    /// <param name="suunta">Suunta, johon näppäimistöllä ohjataan.</param>
    private void LyoPalloa(Vector suunta)
    {
        koripallo.Hit(suunta);
    }


    /// <summary>
    /// Luo uuden koripallon, kun painaa näppäimistöstä SPACE-painik+etta. 
    /// </summary>
    private void LuoPallo()
    {
        koripallo = new PhysicsObject(50, 50, Shape.Circle);
        koripallo.Image = LoadImage("purepng.com-basketballbasketballgamebasketball-1701528096435dvnur.png");
        koripallo.Position = new Vector(300, 150);
        Add(koripallo);
        AddCollisionHandler(koripallo, laskuri, LaskePiste);
        koripallo.LifetimeLeft = TimeSpan.FromSeconds(10.0);
    }


    /// <summary>
    /// Aliohjelma luo kaksi koria "tolppaa" ja laskurin, joka laskee tehtyjen pisteiden määrän. 
    /// </summary>
    private void LuoKoritJaLaskuri()
    {
        kori1 = new PhysicsObject(40, 40, Shape.Rectangle);
        kori2 = new PhysicsObject(40, 40, Shape.Rectangle);
        laskuri = new PhysicsObject(40, 40, Shape.Rectangle);
        kori1.Color = Color.Orange;
        kori2.Color = Color.Orange;
        laskuri.Color = Color.White;
        kori1.Position = new Vector(-360, 140);
        kori2.Position = new Vector(-250, 140);
        laskuri.Position = new Vector(-300, 37);
        kori1.IgnoresGravity = true;
        kori2.IgnoresGravity = true;
        laskuri.IgnoresGravity = true;
        kori1.Mass = 10000000;
        kori2.Mass = 10000000;
        laskuri.Mass = 10000000;
        Add(kori1);
        Add(kori2);
        Add(laskuri);
    }



    /// <summary>
    /// Aliohjelma, joka laskee pisteen, kun pallo on mennyt koriin. 
    /// </summary>
    /// <param name="koripallo">Koripallo, jota yritetään heittää koriin.</param>
    /// <param name="laskuri">Objekti, johon koripallon osuessa, laskee se pisteen.</param>
    void LaskePiste(PhysicsObject koripallo, PhysicsObject laskuri)
    {   
        koripallo.Destroy();
        pistelaskuri.Value += 1;
        int luku = RandomGen.NextInt(3);
        RandomGen.Shuffle(randomtekstit);
        MessageDisplay.Add(randomtekstit[luku]);
        MessageDisplay.Color = Color.Orange;
        MessageDisplay.MessageTime = new TimeSpan(0, 0, 2);
        MessageDisplay.MaxMessageCount = 5;   
    }



    /// <summary>
    /// Aliohjelma, joka luo aikalaskurin.
    /// </summary>
    void LuoAikalaskuri()
    {
        alaspainlaskuri = new DoubleMeter(60);

        aikalaskuri = new Timer();
        aikalaskuri.Interval = 0.1;
        aikalaskuri.Timeout += LaskeAlaspain;
        aikalaskuri.Start();

        Label aikanaytto = new Label();
        aikanaytto.TextColor = Color.Black;
        aikanaytto.DecimalPlaces = 1;
        aikanaytto.Position = new Vector(0, 200);
        aikanaytto.BindTo(alaspainlaskuri);
        Add(aikanaytto);
    }


    /// <summary>
    /// Aliohjelma, jolla asetetaan aikalaskuri laskemaan alaspäin. 
    /// </summary>
    void LaskeAlaspain()
    {
        alaspainlaskuri.Value -= 0.1;

        if (alaspainlaskuri.Value <= 0)
        {
            MessageDisplay.Add("Aika loppui...");
            aikalaskuri.Stop();

            // täydennä mitä tapahtuu, kun aika loppuu
            // TODO: Tee aika loppui tekstistä siistimpi. 
        }
    }



    /// <summary>
    /// Aliohjelma, joka luo pistelaskurin, mikä näyttää pelaajalle tehtyjen pisteiden lukumäärän. 
    /// </summary>
    void LuoPisteLaskuri()
    {
        pistelaskuri = new IntMeter(0);

        Label pistenaytto = new Label();
        pistenaytto.X = Screen.Left + 100;
        pistenaytto.Y = Screen.Top - 100;
        pistenaytto.TextColor = Color.Black;
        pistenaytto.Color = Color.White;
        pistenaytto.Title = "Pisteitä: ";

        pistenaytto.BindTo(pistelaskuri);
        Add(pistenaytto);
    }
    
    // TODO: Ehkä jokin jossa lasketaan nopeiten tehty kori ja hitaiten, johon pystyisi käyttämään taulukkoa ja silmukkaa. 

}
