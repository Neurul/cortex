using CQRSlite.Commands;
using Moq;
using Nancy;
using Nancy.Testing;
using org.neurul.Common.Test;
using org.neurul.Cortex.Application.Neurons.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace org.neurul.Cortex.Port.Adapter.In.Api.Test.TerminalModuleFixture.given
{
    public abstract class Context : TestContext<Browser>
    {
        protected const string TerminalId = "1a2157a8-615c-4a83-be49-fde69f4ff123";
        protected ICommand commandSent;
        protected BrowserResponse response;

        protected override void Given()
        {
            base.Given();

            // arrange
            var commandSender = new Mock<ICommandSender> { DefaultValue = DefaultValue.Mock };
            commandSender
                .Setup(e => e.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>()))
                .Callback<ICommand, CancellationToken>((x, y) => this.commandSent = x)
                .Returns(Task.CompletedTask);
            this.sut = new Browser(with => with
                .Module<TerminalModule>()
                .Dependency(commandSender.Object), defaults => defaults.Accept("application/json")
            );
        }

        protected override void When()
        {
            base.When();

            this.response = this.Request(this.Path, this.BrowserContext).Result;
        }

        protected abstract string Path
        {
            get;
        }

        protected abstract Action<BrowserContext> BrowserContext
        {
            get;
        }

        protected abstract Func<string, Action<BrowserContext>, Task<BrowserResponse>> Request
        {
            get;
        }
    }

    public class When_request_is_post
    {
        public abstract class PostRequestContext : Context
        {
            protected override string Path => "samplebody/cortex/terminals/";

            protected override Func<string, Action<BrowserContext>, Task<BrowserResponse>> Request => this.sut.Post;
        }

        public class When_body_contains_all_required_fields : PostRequestContext
        {
            protected override Action<BrowserContext> BrowserContext => with =>
            {
                with.Body(@"{
                              ""Id"": ""ae0df6d0-c5ef-4514-a81d-8950ebf13588"",
                              ""PresynapticNeuronId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"",
                              ""PostsynapticNeuronId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"",
                              ""Effect"": ""Excite"",
                              ""Strength"": ""1"",
                              ""AuthorId"": ""5e5aa54e-d585-4348-9109-4be0b498b232""
                            }"
                );
            };

            [Fact]
            public void Then_should_send_CreateTerminal_command()
            {
                // assert
                Assert.IsAssignableFrom<CreateTerminal>(this.commandSent);
            }

            [Fact]
            public void Then_should_respond_with_ok()
            {
                Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
            }
        }

        public class When_body_is_missing_id : PostRequestContext
        {
            protected override Action<BrowserContext> BrowserContext => with =>
            {
                with.Body(@"{
                              ""PresynapticNeuronId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"",
                              ""PostsynapticNeuronId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"",
                              ""Effect"": ""Excite"",
                              ""Strength"": ""1"",
                              ""AuthorId"": ""5e5aa54e-d585-4348-9109-4be0b498b232""
                            }"
                );
            };

            [Fact]
            public void Then_should_respond_with_bad_request()
            {
                // assert
                Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
            }

            [Fact]
            public void Then_should_respond_with_body_containing_tag()
            {
                // assert
                Assert.Contains("Id", this.response.Body.AsString());
            }
        }

        public class When_body_is_missing_presynapticNeuronId : PostRequestContext
        {
            protected override Action<BrowserContext> BrowserContext => with =>
            {
                with.Body(@"{
                              ""Id"": ""ae0df6d0-c5ef-4514-a81d-8950ebf13588"",
                              ""PostsynapticNeuronId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"",
                              ""Effect"": ""Excite"",
                              ""Strength"": ""1"",
                              ""AuthorId"": ""5e5aa54e-d585-4348-9109-4be0b498b232""
                            }"
                );
            };

            [Fact]
            public void Then_should_respond_with_bad_request()
            {
                // assert
                Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
            }

            [Fact]
            public void Then_should_respond_with_body_containing_tag()
            {
                // assert
                Assert.Contains("PresynapticNeuronId", this.response.Body.AsString());
            }
        }

        public class When_body_is_missing_postsynapticNeuronId : PostRequestContext
        {
            protected override Action<BrowserContext> BrowserContext => with =>
            {
                with.Body(@"{
                              ""Id"": ""ae0df6d0-c5ef-4514-a81d-8950ebf13588"",
                              ""PresynapticNeuronId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"",
                              ""Effect"": ""Excite"",
                              ""Strength"": ""1"",
                              ""AuthorId"": ""5e5aa54e-d585-4348-9109-4be0b498b232""
                            }"
                );
            };

            [Fact]
            public void Then_should_respond_with_bad_request()
            {
                // assert
                Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
            }

            [Fact]
            public void Then_should_respond_with_body_containing_tag()
            {
                // assert
                Assert.Contains("PostsynapticNeuronId", this.response.Body.AsString());
            }
        }

        public class When_body_is_missing_effect : PostRequestContext
        {
            protected override Action<BrowserContext> BrowserContext => with =>
            {
                with.Body(@"{
                              ""Id"": ""ae0df6d0-c5ef-4514-a81d-8950ebf13588"",
                              ""PostsynapticNeuronId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"",
                              ""PresynapticNeuronId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"",
                              ""Strength"": ""1"",
                              ""AuthorId"": ""5e5aa54e-d585-4348-9109-4be0b498b232""
                            }"
                );
            };

            [Fact]
            public void Then_should_respond_with_bad_request()
            {
                // assert
                Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
            }

            [Fact]
            public void Then_should_respond_with_body_containing_tag()
            {
                // assert
                Assert.Contains("Effect", this.response.Body.AsString());
            }
        }

        public class When_body_is_missing_strength : PostRequestContext
        {
            protected override Action<BrowserContext> BrowserContext => with =>
            {
                with.Body(@"{
                              ""Id"": ""ae0df6d0-c5ef-4514-a81d-8950ebf13588"",
                              ""PostsynapticNeuronId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"",
                              ""PresynapticNeuronId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"",
                              ""Effect"": ""Excite"",
                              ""AuthorId"": ""5e5aa54e-d585-4348-9109-4be0b498b232""
                            }"
                );
            };

            [Fact]
            public void Then_should_respond_with_bad_request()
            {
                // assert
                Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
            }

            [Fact]
            public void Then_should_respond_with_body_containing_tag()
            {
                // assert
                Assert.Contains("Strength", this.response.Body.AsString());
            }
        }
    }

    public class When_request_is_delete
    {
        public abstract class DeleteRequestContext : Context
        {
            protected override Func<string, Action<BrowserContext>, Task<BrowserResponse>> Request => this.sut.Delete;
        }

        public class When_deactivating_terminal
        {
            public abstract class DeactivateNeuronContext : DeleteRequestContext
            {
                protected override string Path => "/samplebody/cortex/terminals/" + Context.TerminalId;

                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{ ""AuthorId"": ""5e5aa54e-d585-4348-9109-4be0b498b232"" }");
                };
            }

            public class When_header_has_expected_version : DeactivateNeuronContext
            {
                protected const int ExpectedVersion = 5;

                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    base.BrowserContext(with);
                    with.Header("ETag", When_header_has_expected_version.ExpectedVersion.ToString());
                };

                [Fact]
                public void Then_should_send_deactivate_neuron_command()
                {
                    Assert.IsAssignableFrom<DeactivateTerminal>(this.commandSent);
                }

                [Fact]
                public void Then_should_send_correct_terminal_id()
                {
                    Assert.Equal(Context.TerminalId, ((DeactivateTerminal)this.commandSent).Id.ToString());
                }

                [Fact]
                public void Then_should_send_correct_version()
                {
                    Assert.Equal(When_header_has_expected_version.ExpectedVersion, ((DeactivateTerminal)this.commandSent).ExpectedVersion);
                }

                [Fact]
                public void Then_should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }
            }

            public class When_header_expected_version_is_missing : DeactivateNeuronContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    base.BrowserContext(with);
                };

                [Fact]
                public void Then_should_respond_with_bad_request()
                {
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Then_should_respond_with_body_containing_expected_version()
                {
                    Assert.Contains("ExpectedVersion", this.response.Body.AsString());
                }
            }
        }
    }
}