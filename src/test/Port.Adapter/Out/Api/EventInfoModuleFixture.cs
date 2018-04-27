using Moq;
using Nancy;
using Nancy.Testing;
using Newtonsoft.Json;
using org.neurul.Cortex.Application.EventInfo;
using org.neurul.Common;
using org.neurul.Common.Events;
using org.neurul.Common.Test;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace org.neurul.Cortex.Port.Adapter.Out.Api.Test.EventInfoModuleFixture.given
{
    public abstract class Context : TestContext<Browser>
    {
        protected BrowserResponse response;
        protected bool invoked;

        protected override void Given()
        {
            base.Given();

            var appService = new Mock<IEventInfoApplicationService> { DefaultValue = DefaultValue.Mock };
            this.SetupMock(appService);
            this.sut = new Browser(with => with
                .Module<EventInfoModule>()
                .Dependency(appService.Object), defaults => defaults.Accept("application/json")
            );
        }

        protected override void When()
        {
            base.When();
            this.response = this.sut.Get(this.Path).Result;
        }

        protected abstract void SetupMock(Mock<IEventInfoApplicationService> mock);

        protected abstract EventInfoLog EventInfoLogResult
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
            protected override string Path => "/samplebody/cortex/events";

            protected override void SetupMock(Mock<IEventInfoApplicationService> mock)
            {
                mock.Setup(e => e.GetCurrentEventInfoLog(It.IsAny<string>()))
                    .Callback<string>(s => this.invoked = true)
                    .Returns(Task.FromResult(this.EventInfoLogResult));
            }
        }

        public class When_store_is_empty : GettingCurrentLogContext
        {
            protected override EventInfoLog EventInfoLogResult => new EventInfoLog(new EventInfoLogId(0,0), null, null, null, new EventInfo[0], false);
            
            [Fact]
            public void Should_invoke_method()
            {
                Assert.True(this.invoked);
            }

            [Fact]
            public void Should_respond_with_ok()
            {
                Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
            }

            [Fact]
            public void Should_have_empty_event_info_in_response_body()
            {
                var eil = JsonConvert.DeserializeObject<EventInfo[]>(this.response.Body.AsString());
                Assert.Empty(eil);
            }

            [Fact]
            public void Should_have_log_id()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[Common.Constants.Response.Header.Link.Key],
                    Common.Constants.Response.Header.Link.Relation.Self,
                    out string link
                    );
                Assert.True(hasLogId);
            }

            [Fact]
            public void Should_have_no_first_id()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[Common.Constants.Response.Header.Link.Key],
                    Common.Constants.Response.Header.Link.Relation.First,
                    out string link
                    );
                Assert.False(hasLogId);
            }

            [Fact]
            public void Should_have_correct_self_link()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[Common.Constants.Response.Header.Link.Key], 
                    Common.Constants.Response.Header.Link.Relation.Self,
                    out string link
                    );
                Assert.Equal("http:///samplebody/cortex/events/0,0", link);
            }
        }

        public class When_log_has_next
        {
            public abstract class HasNextContext : GettingCurrentLogContext
            {
                protected override EventInfoLog EventInfoLogResult =>
                    new EventInfoLog(
                        new EventInfoLogId(6,10),
                        new EventInfoLogId(1,20),
                        new EventInfoLogId(11,15),
                        this.PreviousId,
                        new EventInfo[]
                        {
                        new EventInfo() { Id = "6" },
                        new EventInfo() { Id = "7" },
                        new EventInfo() { Id = "8" },
                        new EventInfo() { Id = "9" },
                        new EventInfo() { Id = "10" },
                        },
                        true
                        );

                protected abstract EventInfoLogId PreviousId { get; }
            }

            public class When_log_has_previous : HasNextContext
            {
                protected override EventInfoLogId PreviousId => new EventInfoLogId(1,5);

                [Fact]
                public void Should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Should_have_correct_event_info_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<EventInfo[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/6,10", link);
                }

                [Fact]
                public void Should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,20", link);
                }

                [Fact]
                public void Should_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_next_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/11,15", link);
                }

                [Fact]
                public void Should_have_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_previous_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,5", link);
                }
            }

            public class When_log_has_no_previous : HasNextContext
            {
                protected override EventInfoLogId PreviousId => null;

                [Fact]
                public void Should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Should_have_correct_event_info_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<EventInfo[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/6,10", link);
                }

                [Fact]
                public void Should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,20", link);
                }

                [Fact]
                public void Should_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_next_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/11,15", link);
                }

                [Fact]
                public void Should_have_no_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
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
                protected override EventInfoLog EventInfoLogResult =>
                    new EventInfoLog(
                        new EventInfoLogId(6, 10), 
                        new EventInfoLogId(1, 20),
                        null,
                        this.PreviousId,
                        new EventInfo[]
                        {
                        new EventInfo() { Id = "6" },
                        new EventInfo() { Id = "7" },
                        new EventInfo() { Id = "8" },
                        new EventInfo() { Id = "9" },
                        new EventInfo() { Id = "10" },
                        },
                        true
                        );

                protected abstract EventInfoLogId PreviousId { get; }
            }

            public class When_log_has_previous : HasNoNextContext
            {
                protected override EventInfoLogId PreviousId => new EventInfoLogId(1,5);

                [Fact]
                public void Should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Should_have_correct_event_info_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<EventInfo[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/6,10", link);
                }

                [Fact]
                public void Should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,20", link);
                }

                [Fact]
                public void Should_have_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_previous_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,5", link);
                }

                [Fact]
                public void Should_not_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.False(hasLogId);
                }
            }

            public class When_log_has_no_previous : HasNoNextContext
            {
                protected override EventInfoLogId PreviousId => null;

                [Fact]
                public void Should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Should_have_correct_event_info_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<EventInfo[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/6,10", link);
                }

                [Fact]
                public void Should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,20", link);
                }

                [Fact]
                public void Should_not_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.False(hasLogId);
                }

                [Fact]
                public void Should_not_have_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
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

            protected override string Path => "/samplebody/cortex/events/" + this.UriLogId;

            protected override void SetupMock(Mock<IEventInfoApplicationService> mock)
            {
                mock.Setup(e => e.GetEventInfoLog(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback<string, string>((s, e)  => { this.invoked = true; this.gettingLogId = e; })
                    .Returns(Task.FromResult(this.EventInfoLogResult));
            }
        }

        public class When_store_is_empty : GettingLogContext
        {
            protected override EventInfoLog EventInfoLogResult => new EventInfoLog(new EventInfoLogId(1,5), null, null, null, new EventInfo[0], false);

            public override string UriLogId => "1,5";

            [Fact]
            public void Should_invoke_method()
            {
                Assert.True(this.invoked);
            }

            [Fact]
            public void Should_process_correct_log_id()
            {
                Assert.Equal(this.UriLogId, this.gettingLogId);
            }

            [Fact]
            public void Should_respond_with_ok()
            {
                Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
            }

            [Fact]
            public void Should_have_empty_event_info_in_response_body()
            {
                var eil = JsonConvert.DeserializeObject<EventInfo[]>(this.response.Body.AsString());
                Assert.Empty(eil);
            }

            [Fact]
            public void Should_have_log_id()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[Common.Constants.Response.Header.Link.Key],
                    Common.Constants.Response.Header.Link.Relation.Self,
                    out string link
                    );
                Assert.True(hasLogId);
            }

            [Fact]
            public void Should_have_correct_self_link()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[Common.Constants.Response.Header.Link.Key],
                    Common.Constants.Response.Header.Link.Relation.Self,
                    out string link
                    );
                Assert.Equal("http:///samplebody/cortex/events/1,5", link);
            }

            [Fact]
            public void Should_not_have_first_id()
            {
                var hasLogId = ResponseHelper.Header.Link.TryGet(
                    this.response.Headers[Common.Constants.Response.Header.Link.Key],
                    Common.Constants.Response.Header.Link.Relation.First,
                    out string link
                    );
                Assert.False(hasLogId);
            }
        }

        public class When_log_has_next
        {
            public abstract class HasNextContext : GettingLogContext
            {
                protected override EventInfoLog EventInfoLogResult =>
                    new EventInfoLog(
                        new EventInfoLogId(6,10),
                        new EventInfoLogId(1,20),
                        new EventInfoLogId(11,15),
                        this.PreviousId,
                        new EventInfo[]
                        {
                        new EventInfo() { Id = "6" },
                        new EventInfo() { Id = "7" },
                        new EventInfo() { Id = "8" },
                        new EventInfo() { Id = "9" },
                        new EventInfo() { Id = "10" },
                        },
                        true
                        );

                protected abstract EventInfoLogId PreviousId { get; }
            }

            public class When_log_has_previous : HasNextContext
            {
                protected override EventInfoLogId PreviousId => new EventInfoLogId(1,5);

                public override string UriLogId => "6,10";

                [Fact]
                public void Should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Should_process_correct_log_id()
                {
                    Assert.Equal(this.UriLogId, this.gettingLogId);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Should_have_correct_event_info_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<EventInfo[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/6,10", link);
                }

                [Fact]
                public void Should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,20", link);
                }

                [Fact]
                public void Should_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_next_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/11,15", link);
                }

                [Fact]
                public void Should_have_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_previous_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,5", link);
                }
            }

            public class When_log_has_no_previous : HasNextContext
            {
                protected override EventInfoLogId PreviousId => null;

                public override string UriLogId => "6,10";

                [Fact]
                public void Should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Should_process_correct_log_id()
                {
                    Assert.Equal(this.UriLogId, this.gettingLogId);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Should_have_correct_event_info_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<EventInfo[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/6,10", link);
                }

                [Fact]
                public void Should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,20", link);
                }

                [Fact]
                public void Should_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_next_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/11,15", link);
                }

                [Fact]
                public void Should_have_no_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
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
                protected override EventInfoLog EventInfoLogResult =>
                    new EventInfoLog(
                        new EventInfoLogId(6,10),
                        new EventInfoLogId(1,20),
                        null,
                        this.PreviousId,
                        new EventInfo[]
                        {
                        new EventInfo() { Id = "6" },
                        new EventInfo() { Id = "7" },
                        new EventInfo() { Id = "8" },
                        new EventInfo() { Id = "9" },
                        new EventInfo() { Id = "10" },
                        },
                        true
                        );

                protected abstract EventInfoLogId PreviousId { get; }
            }

            public class When_log_has_previous : HasNoNextContext
            {
                protected override EventInfoLogId PreviousId => new EventInfoLogId(1,5);

                public override string UriLogId => "6,10";

                [Fact]
                public void Should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Should_process_correct_log_id()
                {
                    Assert.Equal(this.UriLogId, this.gettingLogId);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Should_have_correct_event_info_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<EventInfo[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/6,10", link);
                }

                [Fact]
                public void Should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,20", link);
                }

                [Fact]
                public void Should_have_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_previous_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,5", link);
                }

                [Fact]
                public void Should_not_have_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.False(hasLogId);
                }
            }

            public class When_log_has_no_previous : HasNoNextContext
            {
                protected override EventInfoLogId PreviousId => null;

                public override string UriLogId => "6,10";

                [Fact]
                public void Should_invoke_method()
                {
                    Assert.True(this.invoked);
                }

                [Fact]
                public void Should_process_correct_log_id()
                {
                    Assert.Equal(this.UriLogId, this.gettingLogId);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }

                [Fact]
                public void Should_have_correct_event_info_count_in_response_body()
                {
                    var eil = JsonConvert.DeserializeObject<EventInfo[]>(this.response.Body.AsString());
                    Assert.Equal(5, eil.Length);
                }

                [Fact]
                public void Should_have_log_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_self_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Self,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/6,10", link);
                }

                [Fact]
                public void Should_have_first_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.True(hasLogId);
                }

                [Fact]
                public void Should_have_correct_first_link()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.First,
                        out string link
                        );
                    Assert.Equal("http:///samplebody/cortex/events/1,20", link);
                }

                [Fact]
                public void Should_have_no_previous_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Previous,
                        out string link
                        );
                    Assert.False(hasLogId);
                }

                [Fact]
                public void Should_have_no_next_id()
                {
                    var hasLogId = ResponseHelper.Header.Link.TryGet(
                        this.response.Headers[Common.Constants.Response.Header.Link.Key],
                        Common.Constants.Response.Header.Link.Relation.Next,
                        out string link
                        );
                    Assert.False(hasLogId);
                }
            }
        }
    }
}
