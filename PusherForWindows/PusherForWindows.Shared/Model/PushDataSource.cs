using PusherForWindows.Pusher;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PusherForWindows.Model
{
    class PushDataSource
    {
        private ObservableCollection<Push> pushes = new ObservableCollection<Push>();
        public ObservableCollection<Push> Items
        {
            get { return this.pushes; }
            set { this.pushes = value; }
        }

        public void Add(Push push)
        {
            this.pushes.Insert(0, push);
        }

        public void Remove(Push push)
        {
            pushes.Remove(push);
        }

        public PushDataSource()
        {
            
        }

        public PushDataSource(IList<Push> pushes)
        {
            foreach (var p in pushes)
            {
                this.pushes.Add(p);
            }
        }

        public async void Populate()
        {
            var newPushes = await PusherUtils.GetPushListAsync();
            foreach (var p in newPushes)
            {
                this.pushes.Add(p);
            }
        }

        public void Refresh()
        {
            pushes.Clear();
            Populate();
        }
    }
}
