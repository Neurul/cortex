using CQRSlite.Commands;
using Moq;
using Nancy;
using Nancy.Testing;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Common.Test;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace org.neurul.Cortex.Port.Adapter.In.Api.Test.NeuronModuleFixture.given
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
            protected override string Path => "samplebody/cortex/neurons/ae0df6d0-c5ef-4514-a81d-8950ebf13588";

            protected override Func<string, Action<BrowserContext>, Task<BrowserResponse>> Request => this.sut.Put;
        }

        public class When_body_contains_tag : PutRequestContext
        {
            protected override Action<BrowserContext> BrowserContext => with =>
            {
                with.Body(@"{
                              ""Tag"": ""2017/11/09 22:41"",
                              ""AuthorId"": ""5e5aa54e-d585-4348-9109-4be0b498b232""
                            }"
                );
            };

            [Fact]
            public void Then_should_send_CreateNeuron_command()
            {
                // assert
                Assert.IsAssignableFrom<CreateNeuron>(this.commandSent);
            }

            [Fact]
            public void Then_should_respond_with_ok()
            {
                Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
            }
        }

        public class When_body_is_missing_tag : PutRequestContext
        {
            protected override Action<BrowserContext> BrowserContext => with =>
            {
                with.Body(@"{
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
                Assert.Contains("Tag", this.response.Body.AsString());
            }
        }
    }

    public class When_request_is_patch
    {
        public abstract class PatchRequestContext : Context
        {
            protected override string Path => "/samplebody/cortex/neurons/ae0df6d0-c5ef-4514-a81d-8950ebf13588";

            protected override Func<string, Action<BrowserContext>, Task<BrowserResponse>> Request => this.sut.Patch;
        }

        public class When_header_has_expected_version
        {
            public abstract class HasExpectedVersionContext : PatchRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with => with.Header("ETag", "5");
            }

            public class When_body_contains_tag : HasExpectedVersionContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    base.BrowserContext(with);
                    with.Body(@"{
                                  ""Tag"": ""New Tag"",
                                  ""AuthorId"": ""5e5aa54e-d585-4348-9109-4be0b498b232""
                                }"
                    );    
                };

                [Fact]
                public void Then_should_send_ChangeNeuronTag_command()
                {
                    // assert
                    Assert.IsAssignableFrom<ChangeNeuronTag>(this.commandSent);
                }

                [Fact]
                public void Then_should_respond_with_ok()
                {
                    Assert.Equal(HttpStatusCode.OK, this.response.StatusCode);
                }
            }

            public class When_body_is_missing_tag : HasExpectedVersionContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    base.BrowserContext(with);
                    with.Body(@"{
                        ""Terminals"": [
                        {
                            ""TargetId"": ""5e5aa54e-d585-4348-9109-4be0b498b231"",
                            ""Effect"": ""Excite"",
                            ""Strength"": ""1""
                        }
                        ],
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
                public void Then_should_respond_body_containing_tag_only()
                {
                    // assert
                    Assert.Contains("Tag", this.response.Body.AsString());
                    Assert.DoesNotContain("ExpectedVersion", this.response.Body.AsString());
                }
            }
        }

        public class When_header_expected_version_is_missing
        {
            public class When_body_is_missing_tag : PatchRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{
                                  ""Terminals"": [
                                    {
                                      ""TargetId"": ""5e5aa54e-d585-4348-9109-4be0b498b231"",
                                    ""Effect"": ""Excite"",
                                    ""Strength"": ""1""
                                    }
                                  ],
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
                public void Then_should_respond_with_body_containing_tag_and_expected_version()
                {
                    // assert
                    Assert.Contains("Tag", this.response.Body.AsString());
                    Assert.Contains("ExpectedVersion", this.response.Body.AsString());
                }
            }

            public class When_body_contains_tag : PatchRequestContext
            {
                protected override Action<BrowserContext> BrowserContext => with =>
                {
                    with.Body(@"{
                                  ""Tag"": ""New Tag"",
                                  ""Terminals"": [
                                    {
                                      ""TargetId"": ""5e5aa54e-d585-4348-9109-4be0b498b231"",
                                    ""Effect"": ""Excite"",
                                    ""Strength"": ""1""
                                    }
                                  ],
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
                public void Then_should_respond_with_body_containing_expected_version_only()
                {
                    // assert
                    Assert.Contains("ExpectedVersion", this.response.Body.AsString());
                    Assert.DoesNotContain("Tag", this.response.Body.AsString());
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

        public class When_deactivating_neuron
        {
            public abstract class DeactivateNeuronContext : DeleteRequestContext
            {
                protected override string Path => "/samplebody/cortex/neurons/" + Context.NeuronId;

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
                    // assert
                    Assert.IsAssignableFrom<DeactivateNeuron>(this.commandSent);
                }

                [Fact]
                public void Then_should_send_correct_neuron_id()
                {
                    Assert.Equal(Context.NeuronId, ((DeactivateNeuron)this.commandSent).Id.ToString());
                }

                [Fact]
                public void Then_should_send_correct_version()
                {
                    Assert.Equal(When_header_has_expected_version.ExpectedVersion, ((DeactivateNeuron)this.commandSent).ExpectedVersion);
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
                    // assert
                    Assert.Equal(HttpStatusCode.BadRequest, this.response.StatusCode);
                }

                [Fact]
                public void Then_should_respond_with_body_containing_expected_version()
                {
                    // assert
                    Assert.Contains("ExpectedVersion", this.response.Body.AsString());
                }
            }
        }
    }
}
