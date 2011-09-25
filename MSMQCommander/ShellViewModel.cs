﻿using System.Collections.ObjectModel;
using System.Messaging;
using Caliburn.Micro;
using MSMQCommander.Contex;
using MSMQCommander.Events;
using MSMQCommander.Utils;
using MSMQCommander.Views;
using System.Linq;

namespace MSMQCommander 
{
    public class ShellViewModel : 
        PropertyChangedBase, 
        IShell,
        IHandle<QueueSelectedEvent>,
        IHandle<QueueClosedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly CurrentSelectedQueueContext _currentSelectedQueueContext;
        private readonly ObservableCollection<DetailsView> _detailsViews = new ObservableCollection<DetailsView>();

        public ShellViewModel(IEventAggregator eventAggregator, CurrentSelectedQueueContext currentSelectedQueueContext)
        {
            _eventAggregator = eventAggregator;
            _currentSelectedQueueContext = currentSelectedQueueContext;
            _eventAggregator.Subscribe(this);
        }

        public ObservableCollection<DetailsView> DetailsViews
        {
            get { return _detailsViews; }
        }

        public void Handle(QueueSelectedEvent queueSelectedEvent)
        {
            _currentSelectedQueueContext.CurrentSelectedMessageQueue = queueSelectedEvent.MessageQueue;
            var existingViewForQueue = GetExistingViewForQueue(queueSelectedEvent.MessageQueue);
            if (existingViewForQueue != null)
            {
                existingViewForQueue.Activate();
            }
            else
            {
                var newDetailsView = new DetailsView();
                newDetailsView.CloseAction = OnViewClosed;
                DetailsViews.Add(newDetailsView);
                NotifyOfPropertyChange(() => DetailsViews);
                newDetailsView.Activate();
            }
        }

        private void OnViewClosed(DetailsView view)
        {
            DetailsViews.Remove(view);
        }

        private DetailsView GetExistingViewForQueue(MessageQueue queue)
        {
            var existingViewForQueue = DetailsViews.SingleOrDefault(d => d.Equals(queue));
            return existingViewForQueue;
        }

        public void RefreshQueues()
        {
            _eventAggregator.Publish(new RefreshQueuesEvent());
        }

        public void Handle(QueueClosedEvent queueClosedEvent)
        {
            var existingViewForQueue = GetExistingViewForQueue(queueClosedEvent.MessageQueue);
            if (existingViewForQueue == null)
                return;
            existingViewForQueue.Close();
            DetailsViews.Remove(existingViewForQueue);
        }

        public string Title
        {
            get
            {
                return string.Format("MSMQ Commander (version {0}, {1})", 
                    VersionInformation.GetMajorAndMinorVersion(), 
                    VersionInformation.GetBuildDate().ToShortDateString());
            }
        }
    }
}
