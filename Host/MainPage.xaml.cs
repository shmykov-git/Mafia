using Mafia;
using System.Diagnostics;
using Mafia.Model;
using Host.Mafia;

namespace Host
{
    public partial class MainPage : ContentPage, ITextBuilder
    {
        private readonly IHost host;
        private readonly Game game;

        public MainPage(IHost host, Game game)
        {
            InitializeComponent();

            this.host = host;
            this.game = game;
        }

        public void WriteLine(string text)
        {
            Message.Text += $"{text}\r\n";
        }

        private void OnClicked(object sender, EventArgs e)
        {
            var seed = 1;
            WriteLine($"\r\nGame {seed}");
            host.ChangeSeed(seed);
            game.Start();

            //SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
