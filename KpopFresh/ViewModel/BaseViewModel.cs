using CommunityToolkit.Mvvm; 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KpopFresh.ViewModel
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        [ObservableProperty]
        string title;
        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        int scrollIndex;

        [ObservableProperty]
        DateOnly todayDate;
    }
}
