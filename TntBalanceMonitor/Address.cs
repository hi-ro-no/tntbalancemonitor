// Copyright (C) 2017 valiant1x contact@intensecoin.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TntBalanceMonitor
{
    public class Address : INotifyPropertyChanged
    {
        string _Addr;
        public string Addr
        {
            get { return _Addr; }
            set
            {
                _Addr = value;
                this.NotifyPropertyChanged();
            }
        }

        string _Ip;
        public string Ip
        {
            get { return _Ip; }
            set
            {
                _Ip = value;
                this.NotifyPropertyChanged();
            }
        }

        string _Height;
        public string Height
        {
            get { return _Height; }
            set
            {
                _Height = value;
                this.NotifyPropertyChanged();
            }
        }

        bool _AuditPassed;
        public bool AuditPassed
        {
            get { return _AuditPassed; }
            set
            {
                _AuditPassed = value;
                this.NotifyPropertyChanged();
            }
        }

        float _BalEth;
        public float BalEth
        {
            get { return _BalEth; }
            set
            {
                _BalEth = value;
                this.NotifyPropertyChanged();
            }
        }

        float _BalTnt;
        public float BalTnt
        {
            get { return _BalTnt; }
            set
            {
                _BalTnt = value;
                this.NotifyPropertyChanged();
            }
        }
        DateTime _LastUpdated;
        public DateTime LastUpdated
        {
            get { return _LastUpdated; }
            set
            {
                _LastUpdated = value;
                this.NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Address(string addr, string ip)
        {
            Addr = addr;
            Ip = ip;
        }
    }
}
