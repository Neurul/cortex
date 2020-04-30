using Common.Test;
using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Client.Out;
using ei8.EventSourcing.Common;

namespace neurUL.Cortex.Application.Test.Neurons
{
    public abstract class ConstructingContext<TAggregate, THandler, TCommand> : ConditionalWhenSpecification<TAggregate, THandler, TCommand> where TAggregate : AggregateRoot where THandler : class where TCommand : ICommand
    {
        protected IEventSourceFactory eventSourceFactory;
        protected ISettingsService settingsService;

        protected Guid authorId;
        protected virtual Guid AuthorId => this.authorId = this.authorId == Guid.Empty ? Guid.NewGuid() : this.authorId;

        protected override IEnumerable<IEvent> Given() => new IEvent[0];

        protected override TCommand When() => default(TCommand);

        protected virtual IEventSourceFactory EventSourceFactory
        {
            get
            {
                if (this.eventSourceFactory == null)
                {
                    var mockEsf = new Mock<IEventSourceFactory>();
                    var mockEs = new Mock<IEventSource>();
                    var mockNc = new Mock<INotificationClient>();

                    mockNc.Setup(nc => nc.GetNotificationLog(string.Empty, string.Empty, CancellationToken.None)).Returns(Task.FromResult(new NotificationLog(
                        new NotificationLogId(21, 40),
                        new NotificationLogId(1, 20),
                        new NotificationLogId(41, 60),
                        new NotificationLogId(21, 40),
                        new Notification[0],
                        false,
                        60)));

                    mockEs.SetupGet(es => es.Session).Returns(this.Session);
                    mockEs.SetupGet(es => es.NotificationClient).Returns(mockNc.Object);
                    mockEsf.Setup(esf => esf.Create("http://192.168.8.100:60000/samplebody/", "http://192.168.8.100:60001/samplebody/", this.AuthorId)).Returns(mockEs.Object);
                    this.eventSourceFactory = mockEsf.Object;
                }
                return this.eventSourceFactory;
            }
        }

        protected virtual ISettingsService SettingsService
        {
            get
            {
                if (this.settingsService == null)
                {
                    var mockSs = new Mock<ISettingsService>();
                    mockSs.SetupGet(ss => ss.EventSourcingInBaseUrl).Returns("http://192.168.8.100:60000");
                    mockSs.SetupGet(ss => ss.EventSourcingOutBaseUrl).Returns("http://192.168.8.100:60001");
                    this.settingsService = mockSs.Object;
                }
                return this.settingsService;
            }
        }

        protected override bool InvokeBuildWhenOnConstruct => false;
    }
}
