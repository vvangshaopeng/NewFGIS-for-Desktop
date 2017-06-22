using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Common
{
    public class NotifyPropertyChanged: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
