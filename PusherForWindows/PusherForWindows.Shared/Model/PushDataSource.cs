using System.Collections.Generic;
using System.Collections.ObjectModel;
using PusherForWindows.Pusher;
using Windows.UI.Xaml;

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
            var newPushes = new ObservableCollection<Push>(await ((App)Application.Current).GetAllPushes());
            foreach (var p in newPushes)
            {
                this.InserFirst(p);
            }

            this.Refresh();
        }

        public async void Refresh()
        {
            var updatedPushes = await Pushbullet.UpdatePushListAsync();
            foreach (var push in updatedPushes)
            {
                var index = this.pushes.IndexOf(push);
                if (index > -1)
                {
                    if (push.IsActive)
                        this.pushes[index] = push;
                    else
                        this.pushes.RemoveAt(index);
                }
                else
                {
                    if (push.IsActive)
                        this.InserFirst(push);
                }
            }
        }

        private void InserFirst(Push push)
        {
            if (this.pushes.Count == 0)
            {
                this.Add(push);
            }
            else
            {
                var isAdded = false;
                for (int i = 0; i < this.pushes.Count; i++)
                {
                    if (push.Created > this.pushes[i].Created)
                    {
                        this.pushes.Insert(i, push);
                        isAdded = true;
                        break;
                    }
                }

                if (!isAdded)
                    this.pushes.Add(push);
            }
        }
    }
}
