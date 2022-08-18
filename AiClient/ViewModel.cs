using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiClient
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private IEnumerable<RoomInfo> roomList = RoomInfo.ReloadRoom;
        private string userName;
        private Boolean isGameStart = false;

        public IEnumerable<RoomInfo> RoomList
        {
            get => roomList; set
            {
                roomList = value;
                OnPropertyChanged(nameof(RoomList));
            }
        }

        public string UserName
        {
            get => userName; set
            {
                userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public bool IsGameStart {
            get => isGameStart; set
            {
                isGameStart = value;
                OnPropertyChanged(nameof(IsGameStart));
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
