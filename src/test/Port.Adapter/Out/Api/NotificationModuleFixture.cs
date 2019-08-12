using Moq;
using Nancy;
using Nancy.Testing;
using Newtonsoft.Json;
using org.neurul.Cortex.Application.Notification;
using org.neurul.Common;
using org.neurul.Common.Events;
using org.neurul.Common.Test;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace org.neurul.Cortex.Port.Adapter.Out.Api.Test.NotificationModuleFixture.given
{
    public abstract class Context : TestContext<Browser>
    {
        protected BrowserResponse response;
        protected bool invoked;

        protected override void Given()
        {
            base.Given();

            var appService = new Mock<INotificationApplicationService> { DefaultValue = DefaultValue.Mock };
            this.SetupMock(appService);
            this.sut = new Browser(with => with
                .Module<NotificationModule>()
                .Dependency(appService.Object), defaults => defaults.Accept("application/json")
            );
        }

        protected override void When()
        {
            base.When();
            this.response = this.sut.Get(this.Path).Result;
        }

        protected abstract void SetupMock(Mock<INotificationApplicationService> mock);

        protected abstract NotificationLog NotificationLogResult
        {
            get;
        }

        protected abstract string Path
        {
            get;
        }
    }

    public class When_getting_current_log
    {
        public abstract class GettingCurrentLogContext : Context
        {
            protected override string Path => "/samplebody/cortex/notifications";

            protected override void SetupMock(Mock<INotificationApplicationService> mock)
            {
                mock.Setup(e => e.GetCurrentNotificationLog(It.IsAny<string>()))
                    .Callback<string>(s => this.invoked = true)
                    .Returns(Task.FromResult(this.NotificationLogResult));
            }
        }

        public class When_store_is_empty : GettingCurrentLogContext
        {
            protected override NotificationLog NotificationLogResult => new NotificationLog(new NotificationLogId(0,0), null, null, null, new Notification[0], false);
            
            [Fact]
            public void Then_should_invoke_method()
            {
                Assert.True(this.invoked);
            }

            [Fact]
            public void Then_should_respond_with_ok()
            {
                Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
            }

            [Fact]
            public void Then_should_have_empty_notification_in_response_body()
            {
                var eil = JsonConvert.DeserializeObject<Notification[]>(this.response.Body.AsString());
                Assert.Empty(eil);
            }

            [Fact]
            public void Then_should_have_log_id()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                    org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                    out string link
                    );
                Assert.True(hasLogId);
            }

            [Fact]
            public void Then_should_have_no_first_id()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                    org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                    out string link
                    );
                Assert.False(hasLogId);
            }

            [Fact]
            public void Then_should_have_correct_self_link()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key], 
                    org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                    out string link
                    );
                Assert.Equal("http:///samplebody/cortex/notifications/0,0", link);
            }
        }

        public class When_log_has_next
        {
            public abstract class HasNextContext : GettingCurrentLogContext
            {
                protected override NotificationLog NotificationLogResult =>
                    new NotificationLog(
                        new NotificationLogId(6,10),
                        new NotificationLogId(1,20),
                        new NotificationLogId(11,15),
                        this.PreviousId,
                        new Notification[]
                        {
                        new Notification() { Id = "6" },
                        new Notification() { Id = "7" },
                        new Notification() { Id = "8" },
                        new Notification() { Id = "9" },
                        new Notification() { Id = "10" },
                        },
                        true
                        );

                protected abstract NotificationLogId PreviousId { get; }
            }

            public class When_log_has_previous : HasNextContext
            {
                protected override NotificationLogId PreviousId => new NotificationLogId(1,5);

                [Fact]
                public void Then_should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Then_should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Then_should_have_correct_notification_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<Notification[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Then_should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/6,10", link);
                }

                [Fact]
                public void Then_should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,20", link);
                }

                [Fact]
                public void Then_should_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_next_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/11,15", link);
                }

                [Fact]
                public void Then_should_have_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_previous_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,5", link);
                }
            }

            public class When_log_has_no_previous : HasNextContext
            {
                protected override NotificationLogId PreviousId => null;

                [Fact]
                public void Then_should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Then_should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Then_should_have_correct_notification_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<Notification[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Then_should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/6,10", link);
                }

                [Fact]
                public void Then_should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,20", link);
                }

                [Fact]
                public void Then_should_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_next_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/11,15", link);
                }

                [Fact]
                public void Then_should_have_no_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.False(hasLogId);
                }
            }
        }

        public class When_log_has_no_next
        {
            public abstract class HasNoNextContext : GettingCurrentLogContext
            {
                protected override NotificationLog NotificationLogResult =>
                    new NotificationLog(
                        new NotificationLogId(6, 10), 
                        new NotificationLogId(1, 20),
                        null,
                        this.PreviousId,
                        new Notification[]
                        {
                        new Notification() { Id = "6" },
                        new Notification() { Id = "7" },
                        new Notification() { Id = "8" },
                        new Notification() { Id = "9" },
                        new Notification() { Id = "10" },
                        },
                        true
                        );

                protected abstract NotificationLogId PreviousId { get; }
            }

            public class When_log_has_previous : HasNoNextContext
            {
                protected override NotificationLogId PreviousId => new NotificationLogId(1,5);

                [Fact]
                public void Then_should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Then_should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Then_should_have_correct_notification_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<Notification[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Then_should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/6,10", link);
                }

                [Fact]
                public void Then_should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,20", link);
                }

                [Fact]
                public void Then_should_have_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_previous_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,5", link);
                }

                [Fact]
                public void Then_should_not_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.False(hasLogId);
                }
            }

            public class When_log_has_no_previous : HasNoNextContext
            {
                protected override NotificationLogId PreviousId => null;

                [Fact]
                public void Then_should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Then_should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Then_should_have_correct_notification_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<Notification[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Then_should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/6,10", link);
                }

                [Fact]
                public void Then_should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,20", link);
                }

                [Fact]
                public void Then_should_not_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.False(hasLogId);
                }

                [Fact]
                public void Then_should_not_have_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.False(hasLogId);
                }
            }
        }
    }

    public class When_getting_log
    {
        public abstract class GettingLogContext : Context
        {
            public abstract string UriLogId { get; }

            protected string gettingLogId;

            protected override string Path => "/samplebody/cortex/notifications/" + this.UriLogId;

            protected override void SetupMock(Mock<INotificationApplicationService> mock)
            {
                mock.Setup(e => e.GetNotificationLog(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback<string, string>((s, e)  => { this.invoked = true; this.gettingLogId = e; })
                    .Returns(Task.FromResult(this.NotificationLogResult));
            }
        }

        public class When_store_is_empty : GettingLogContext
        {
            protected override NotificationLog NotificationLogResult => new NotificationLog(new NotificationLogId(1,5), null, null, null, new Notification[0], false);

            public override string UriLogId => "1,5";

            [Fact]
            public void Then_should_invoke_method()
            {
                Assert.True(this.invoked);
            }

            [Fact]
            public void Then_should_process_correct_log_id()
            {
                Assert.Equal(this.UriLogId, this.gettingLogId);
            }

            [Fact]
            public void Then_should_respond_with_ok()
            {
                Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
            }

            [Fact]
            public void Then_should_have_empty_notification_in_response_body()
            {
                var eil = JsonConvert.DeserializeObject<Notification[]>(this.response.Body.AsString());
                Assert.Empty(eil);
            }

            [Fact]
            public void Then_should_have_log_id()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                    org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                    out string link
                    );
                Assert.True(hasLogId);
            }

            [Fact]
            public void Then_should_have_correct_self_link()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                    org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                    out string link
                    );
                Assert.Equal("http:///samplebody/cortex/notifications/1,5", link);
            }

            [Fact]
            public void Then_should_not_have_first_id()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                    org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                    out string link
                    );
                Assert.False(hasLogId);
            }
        }

        public class When_log_has_next
        {
            public abstract class HasNextContext : GettingLogContext
            {
                protected override NotificationLog NotificationLogResult =>
                    new NotificationLog(
                        new NotificationLogId(6,10),
                        new NotificationLogId(1,20),
                        new NotificationLogId(11,15),
                        this.PreviousId,
                        new Notification[]
                        {
                        new Notification() { Id = "6" },
                        new Notification() { Id = "7" },
                        new Notification() { Id = "8" },
                        new Notification() { Id = "9" },
                        new Notification() { Id = "10" },
                        },
                        true
                        );

                protected abstract NotificationLogId PreviousId { get; }
            }

            public class When_log_has_previous : HasNextContext
            {
                protected override NotificationLogId PreviousId => new NotificationLogId(1,5);

                public override string UriLogId => "6,10";

                [Fact]
                public void Then_should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Then_should_process_correct_log_id()
                {
                    Assert.Equal(this.UriLogId, this.gettingLogId);
                }

                [Fact]
                public void Then_should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Then_should_have_correct_notification_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<Notification[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Then_should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/6,10", link);
                }

                [Fact]
                public void Then_should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,20", link);
                }

                [Fact]
                public void Then_should_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_next_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/11,15", link);
                }

                [Fact]
                public void Then_should_have_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_previous_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,5", link);
                }
            }

            public class When_log_has_no_previous : HasNextContext
            {
                protected override NotificationLogId PreviousId => null;

                public override string UriLogId => "6,10";

                [Fact]
                public void Then_should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Then_should_process_correct_log_id()
                {
                    Assert.Equal(this.UriLogId, this.gettingLogId);
                }

                [Fact]
                public void Then_should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Then_should_have_correct_notification_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<Notification[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Then_should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/6,10", link);
                }

                [Fact]
                public void Then_should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,20", link);
                }

                [Fact]
                public void Then_should_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_next_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/11,15", link);
                }

                [Fact]
                public void Then_should_have_no_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.False(hasLogId);
                }
            }
        }

        public class When_log_has_no_next
        {
            public abstract class HasNoNextContext : GettingLogContext
            {
                protected override NotificationLog NotificationLogResult =>
                    new NotificationLog(
                        new NotificationLogId(6,10),
                        new NotificationLogId(1,20),
                        null,
                        this.PreviousId,
                        new Notification[]
                        {
                        new Notification() { Id = "6" },
                        new Notification() { Id = "7" },
                        new Notification() { Id = "8" },
                        new Notification() { Id = "9" },
                        new Notification() { Id = "10" },
                        },
                        true
                        );

                protected abstract NotificationLogId PreviousId { get; }
            }

            public class When_log_has_previous : HasNoNextContext
            {
                protected override NotificationLogId PreviousId => new NotificationLogId(1,5);

                public override string UriLogId => "6,10";

                [Fact]
                public void Then_should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Then_should_process_correct_log_id()
                {
                    Assert.Equal(this.UriLogId, this.gettingLogId);
                }

                [Fact]
                public void Then_should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Then_should_have_correct_notification_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<Notification[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Then_should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/6,10", link);
                }

                [Fact]
                public void Then_should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,20", link);
                }

                [Fact]
                public void Then_should_have_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_previous_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,5", link);
                }

                [Fact]
                public void Then_should_not_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.False(hasLogId);
                }
            }

            public class When_log_has_no_previous : HasNoNextContext
            {
                protected override NotificationLogId PreviousId => null;

                public override string UriLogId => "6,10";

                [Fact]
                public void Then_should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Then_should_process_correct_log_id()
                {
                    Assert.Equal(this.UriLogId, this.gettingLogId);
                }

                [Fact]
                public void Then_should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Then_should_have_correct_notification_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<Notification[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Then_should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/6,10", link);
                }

                [Fact]
                public void Then_should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Then_should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/notifications/1,20", link);
                }

                [Fact]
                public void Then_should_have_no_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.False(hasLogId);
                }

                [Fact]
                public void Then_should_have_no_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[org.neurul.Common.Constants.Response.Header.Link.Key],
                        org.neurul.Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.False(hasLogId);
                }
            }
        }
    }
}
