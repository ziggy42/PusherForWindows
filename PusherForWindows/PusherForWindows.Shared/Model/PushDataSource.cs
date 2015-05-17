using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using PusherForWindows.Pusher;

namespace PusherForWindows.Model
{
    public class PushDataSource
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
            this.pushes.Remove(push);
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

        public async void Refresh()
        {
            var updatedPushes = await PusherUtils.UpdatePushListAsync();
            foreach(var push in updatedPushes)
            {
                var index = this.pushes.IndexOf(push);
                if(index > -1)
                {
                    if (push.IsActive)
                        this.pushes[index] = push;
                    else
                        this.pushes.RemoveAt(index);
                }
                else
                {
                    this.InserFirst(push);
                }
            }
        }

        private void InserFirst(Push push)
        {
            for (int i = 0; i < this.pushes.Count; i++)
            {
                if (push.Created > this.pushes[i].Created)
                {
                    this.pushes.Insert(i, push);
                    break;
                }
            }
        }
    }
}
