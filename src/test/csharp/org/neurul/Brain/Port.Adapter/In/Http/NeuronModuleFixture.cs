using CQRSlite.Commands;
using Moq;
using Nancy;
using Nancy.Testing;
using org.neurul.Brain.Application.Neurons.Commands;
using org.neurul.Common.Test;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace org.neurul.Brain.Port.Adapter.In.Http.Test.NeuronModuleFixture.given
{
    public abstract class Context : TestContext<Browser>
    {
        protected const string NeuronId = "1a2157a8-615c-4a83-be49-fde69f4ff123";
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
                .Module<NeuronModule>()
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

    public class When_request_is_put
    {
        public abstract class PutRequestContext : Context
        {
            protected override string Path => "/brain/neurons/ae0df6d0-c5ef-4514-a81d-8950ebf13588";

            protected override Func<string, Action<BrowserContext>, Task<BrowserResponse>> Request => this.sut.Put;
        }

        public class When_body_contains_data
        {
            public class When_body_contains_terminals : PutRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{
                        ""Data"": ""2017/11/09 22:41"",
                        ""Terminals"": [
                        {
                            ""Target"": ""5e5aa54e-d585-4348-9109-4be0b498b231""
                        }
                        ]
                    }"
                    );
                };

                [Fact]
                public void Should_send_CreateNeuronWithTerminals_command()
                {
                    // assert
                    Assert.IsAssignableFrom<CreateNeuronWithTerminals>(this.commandSent);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }
            }

            public class When_body_is_missing_terminals : PutRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{
                              ""Data"": ""2017/11/09 22:41""
                            }"
                    );
                };

                [Fact]
                public void Should_send_CreateNeuron_command()
                {
                    // assert
                    Assert.IsAssignableFrom<CreateNeuron>(this.commandSent);
                }
            }
        }

        public class When_body_is_missing_data
        {
            public class When_body_contains_terminals : PutRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{
                                  ""Terminals"": [
                                    {
                                      ""Target"": ""5e5aa54e-d585-4348-9109-4be0b498b231""
                                    }
                                  ]
                                }"
                    );
                };

                [Fact]
                public void Should_respond_with_bad_request()
                {
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Should_respond_with_body_containing_data()
                {
                    // assert
                    Assert.Contains("Data", this.response.Body.AsString());
                }
            }

            public class When_body_is_missing_terminals : PutRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{}");
                };

                [Fact]
                public void Should_respond_with_bad_request()
                {
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Should_respond_with_body_containing_data()
                {
                    // assert
                    Assert.Contains("Data", this.response.Body.AsString());
                }
            }
        }
    }

    public class When_request_is_post
    {
        public abstract class PostRequestContext : Context
        {
            protected override string Path => "/brain/neurons/1a2157a8-615c-4a83-be49-fde69f4ff470/terminals";

            protected override Func<string, Action<BrowserContext>, Task<BrowserResponse>> Request => this.sut.Post;
        }

        public class When_header_has_expected_version
        {
            public abstract class HasExpectedVersionContext : PostRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with => with.Header("ETag", "5");
            }

            public class When_body_contains_terminals : HasExpectedVersionContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    base.BrowserContext(with);
                    with.Body(@"{
                    ""Terminals"": [
                    {
                        ""Target"": ""ceeb5d65-4468-437d-8e99-22cfb66a6eca""
                    }
                    ]
                }"
                    );
                };

                [Fact]
                public void Should_send_AddTerminalsToNeuron_command()
                {
                    // assert
                    Assert.IsAssignableFrom<AddTerminalsToNeuron>(this.commandSent);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }
            }

            public class When_body_is_missing_terminals : HasExpectedVersionContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    base.BrowserContext(with);
                    with.Body(@"{ }"
                    );
                };

                [Fact]
                public void Should_respond_with_bad_request()
                {
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Should_respond_with_body_containing_terminals_only()
                {
                    // assert
                    Assert.Contains("Terminals", this.response.Body.AsString());
                    Assert.DoesNotContain("ExpectedVersion", this.response.Body.AsString());
                }
            }
        }

        public class When_header_expected_version_is_missing
        {
            public class When_body_is_missing_terminals : PostRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{
                            ""Data"": ""Hello World""
                        }"
                    );
                };

                [Fact]
                public void Should_respond_with_bad_request()
                {
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Should_respond_with_body_containing_terminals()
                {
                    // assert
                    Assert.Contains("Terminals", this.response.Body.AsString());
                }

                [Fact]
                public void Should_respond_with_body_containing_expected_version()
                {
                    // assert
                    Assert.Contains("ExpectedVersion", this.response.Body.AsString());
                }
            }

            public class When_body_contains_terminals : PostRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{
                              ""Terminals"": [
                                {
                                  ""Target"": ""5e5aa54e-d585-4348-9109-4be0b498b231""
                                }
                              ]
                            }"
                    );
                };

                [Fact]
                public void Should_respond_with_bad_request()
                {
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Should_respond_with_body_containing_expected_version_only()
                {
                    // assert
                    Assert.Contains("ExpectedVersion", this.response.Body.AsString());
                    Assert.DoesNotContain("Terminals", this.response.Body.AsString());
                }
            }
        }
    }

    public class When_request_is_patch
    {
        public abstract class PatchRequestContext : Context
        {
            protected override string Path => "/brain/neurons/ae0df6d0-c5ef-4514-a81d-8950ebf13588";

            protected override Func<string, Action<BrowserContext>, Task<BrowserResponse>> Request => this.sut.Patch;
        }

        public class When_header_has_expected_version
        {
            public abstract class HasExpectedVersionContext : PatchRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with => with.Header("ETag", "5");
            }

            public class When_body_contains_data : HasExpectedVersionContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    base.BrowserContext(with);
                    with.Body(@"{
                                  ""Data"": ""New Data"",
                                }"
                    );    
                };

                [Fact]
                public void Should_send_ChangeNeuronData_command()
                {
                    // assert
                    Assert.IsAssignableFrom<ChangeNeuronData>(this.commandSent);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }
            }

            public class When_body_is_missing_data : HasExpectedVersionContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    base.BrowserContext(with);
                    with.Body(@"{
                        ""Terminals"": [
                        {
                            ""Target"": ""5e5aa54e-d585-4348-9109-4be0b498b231""
                        }
                        ]
                    }"
                    );
                };

                [Fact]
                public void Should_respond_with_bad_request()
                {
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Should_respond_body_containing_data_only()
                {
                    // assert
                    Assert.Contains("Data", this.response.Body.AsString());
                    Assert.DoesNotContain("ExpectedVersion", this.response.Body.AsString());
                }
            }
        }

        public class When_header_expected_version_is_missing
        {
            public class When_body_is_missing_data : PatchRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{
                                  ""Terminals"": [
                                    {
                                      ""Target"": ""5e5aa54e-d585-4348-9109-4be0b498b231""
                                    }
                                  ]
                                }"
                    );
                };

                [Fact]
                public void Should_respond_with_bad_request()
                {
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Should_respond_with_body_containing_data_and_expected_version()
                {
                    // assert
                    Assert.Contains("Data", this.response.Body.AsString());
                    Assert.Contains("ExpectedVersion", this.response.Body.AsString());
                }
            }

            public class When_body_contains_data : PatchRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{
                                  ""Data"": ""New Data"",
                                  ""Terminals"": [
                                    {
                                      ""Target"": ""5e5aa54e-d585-4348-9109-4be0b498b231""
                                    }
                                  ]
                                }"
                    );
                };

                [Fact]
                public void Should_respond_with_bad_request()
                {
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Should_respond_with_body_containing_expected_version_only()
                {
                    // assert
                    Assert.Contains("ExpectedVersion", this.response.Body.AsString());
                    Assert.DoesNotContain("Data", this.response.Body.AsString());
                }
            }
        }
    }

    public class When_request_is_delete
    {
        public abstract class DeleteRequestContext : Context
        {
            protected override Func<string, Action<BrowserContext>, Task<BrowserResponse>> Request => this.sut.Delete;
        }

        public class When_deleting_terminal
        {
            public abstract class DeleteTerminalContext : DeleteRequestContext
            {
                protected const string TerminalId = "123457a8-615c-4a83-be49-fde69f4ff456";

                protected override string Path => "/brain/neurons/" + Context.NeuronId + "/terminals/" + DeleteTerminalContext.TerminalId;
            }

            public class When_header_has_expected_version : DeleteTerminalContext
            {   
                protected override Action<BrowserContext> BrowserContext => with => with.Header("ETag", "5");

                [Fact]
                public void Should_send_RemoveTerminalsFromNeuron_command()
                {
                    // assert
                    Assert.IsAssignableFrom<RemoveTerminalsFromNeuron>(this.commandSent);
                }

                [Fact]
                public void Should_send_correct_neuron_id()
                {
                    Assert.Equal(Context.NeuronId, ((RemoveTerminalsFromNeuron)this.commandSent).Id.ToString());
                }

                [Fact]
                public void Should_send_correct_terminal_id()
                {
                    Assert.Equal(When_header_has_expected_version.TerminalId, ((RemoveTerminalsFromNeuron)this.commandSent).Terminals.ToArray()[0].Target.ToString());
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }
            }

            public class When_header_expected_version_is_missing : DeleteTerminalContext
            {
                protected override Action<BrowserContext> BrowserContext => with => { };

                [Fact]
                public void Should_respond_with_bad_request()
                {
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Should_respond_with_body_containing_expected_version()
                {
                    // assert
                    Assert.Contains("ExpectedVersion", this.response.Body.AsString());
                }
            }
        }

        public class When_deactivating_neuron
        {
            public abstract class DeactivateNeuronContext : DeleteRequestContext
            {
                protected override string Path => "/brain/neurons/" + Context.NeuronId;
            }

            public class When_header_has_expected_version : DeactivateNeuronContext
            {
                protected const int ExpectedVersion = 5;

                protected override Action<BrowserContext> BrowserContext => with => { with.Header("ETag", When_header_has_expected_version.ExpectedVersion.ToString()); };

                [Fact]
                public void Should_send_deactivate_neuron_command()
                {
                    // assert
                    Assert.IsAssignableFrom<DeactivateNeuron>(this.commandSent);
                }

                [Fact]
                public void Should_send_correct_neuron_id()
                {
                    Assert.Equal(Context.NeuronId, ((DeactivateNeuron)this.commandSent).Id.ToString());
                }

                [Fact]
                public void Should_send_correct_version()
                {
                    Assert.Equal(When_header_has_expected_version.ExpectedVersion, ((DeactivateNeuron)this.commandSent).ExpectedVersion);
                }

                [Fact]
                public void Should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }
            }

            public class When_header_expected_version_is_missing : DeactivateNeuronContext
            {
                protected override Action<BrowserContext> BrowserContext => with => { };

                [Fact]
                public void Should_respond_with_bad_request()
                {
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Should_respond_with_body_containing_expected_version()
                {
                    // assert
                    Assert.Contains("ExpectedVersion", this.response.Body.AsString());
                }
            }
        }
    }
}
