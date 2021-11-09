using DrawClient.MVVM.Core;
using DrawClient.MVVM.Model;
using DrawClient.Net;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace DrawClient.MVVM.ViewModel
{
    class MainViewModel
    {
        private Server _server;
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public StrokeCollection CanvasStrokes { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();
            CanvasStrokes = new StrokeCollection();
            _server = new Server();
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.inkStrokeReceivedEvent += CanvasStrokesReceived;
            _server.userDisconnectEvent += RemoveUser;
            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(UserName), o => !string.IsNullOrEmpty(UserName));
            SendMessageCommand = new RelayCommand(o => _server.SendMessageToServer(Message), o => !string.IsNullOrEmpty(Message));
        }

        public void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            MemoryStream _ms = new MemoryStream();
            CanvasStrokes.Save(_ms);
            _ms.Flush();

            _server.SendCanvasStrokesToServer(_ms);
        }

        private void CanvasStrokesReceived()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var strokes = _server.PacketReader.ReadCanvasStrokes();
                MemoryStream ms = new MemoryStream(strokes);
                CanvasStrokes = new StrokeCollection(ms);
            });
        }
        private void UserConnected()
        {
            var user = new UserModel
            {
                UserName = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage()
            };
            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }

        }

        private void MessageReceived()
        {
            var msg = _server.PacketReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));

        }
        private void RemoveUser()
        {
            var uid = _server.PacketReader.ReadMessage();
            var user = Users.Where(u => u.UID == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

    }
}
