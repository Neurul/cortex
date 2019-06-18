using CQRSlite.Commands;
using CQRSlite.Domain.Exception;
using CQRSlite.Events;
using Moq;
using org.neurul.Common.Events;
using org.neurul.Cortex.Application.Neurons;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace org.neurul.Cortex.Application.Test.Neurons.NeuronCommandHandlersFixture.given
{
    public abstract class NeuronCommandHandlerConstructingContext<TCommand> : ConstructingContext<Neuron, NeuronCommandHandlers, TCommand> where TCommand : ICommand
    {
        protected override NeuronCommandHandlers BuildHandler() => new NeuronCommandHandlers(this.EventStore, this.Session);
    }

    public class When_constructing
    {
        public class When_null_event_store_specified : NeuronCommandHandlerConstructingContext<CreateNeuron>
        {
            protected override NeuronCommandHandlers BuildHandler() => new NeuronCommandHandlers(null, this.Session);

            [Fact]
            public void Then_should_throw_argument_null_exception()
            {
                Assert.Throws<ArgumentNullException>(() => this.BuildHandler());
            }
        }

        public class When_null_session_specified : NeuronCommandHandlerConstructingContext<CreateNeuron>
        {
            protected override NeuronCommandHandlers BuildHandler() => new NeuronCommandHandlers(new Mock<INavigableEventStore>().Object, null);

            [Fact]
            public void Then_should_throw_argument_null_exception()
            {
                Assert.Throws<ArgumentNullException>(() => this.BuildHandler());
            }
        }
    }

    public abstract class ConstructedContext<TCommand> : NeuronCommandHandlerConstructingContext<TCommand> where TCommand : ICommand
    {
        protected override bool InvokeBuildWhenOnConstruct => true;
        protected override bool InvokeWhenOnConstruct => false;
    }

    public class When_constructed : ConstructedContext<CreateNeuron>
    {
        [Fact]
        public void Then_should_contain_correct_eventStore()
        {
            Assert.Equal(
                this.EventStore,
                ((object)this.handler).GetFieldValue(typeof(NeuronCommandHandlers), "eventStore")
                );
        }

        [Fact]
        public void Then_should_contain_correct_session()
        {
            Assert.Equal(
                this.Session,
                ((object)this.handler).GetFieldValue(typeof(NeuronCommandHandlers), "session")
                );
        }
    }

    public abstract class CreationPrepareConstructedContext<TCommand> : ConstructedContext<TCommand> where TCommand : ICommand
    {
        protected override IEnumerable<IEvent> Given()
        {
            var result = new List<IEvent>(base.Given());

            if (this.PreAddAuthor)
                result.Add(new NeuronCreated(Guid.Parse(this.AuthorId), "Author", this.AuthorId.ToString()) { Version = 1 });
            if (this.PreAddNeuron)
                result.Add(new NeuronCreated(this.Id, this.Tag, this.AuthorId.ToString()) { Version = 1 });

            return result.ToArray();
        }

        protected string avatarId;
        protected Guid id;
        protected string tag;
        protected string authorId;

        protected virtual bool PreAddAuthor => true;
        protected virtual bool PreAddNeuron => true;

        protected virtual string AvatarId => this.avatarId = this.avatarId ?? "samplebody";
        protected virtual Guid Id => this.id = this.id == Guid.Empty ? Guid.NewGuid() : this.id;
        protected virtual string Tag => this.tag = this.tag ?? "Hello";
        protected virtual string AuthorId => this.authorId = this.authorId ?? Guid.NewGuid().ToString();
    }

    public class When_creating_neuron
    {
        public class When_null_command_specified : ConstructedContext<CreateNeuron>
        {
            protected override CreateNeuron When() => null;

            [Fact]
            public async Task Then_should_throw_argument_null_exception()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => this.InvokeWhen());
            }
        }

        public abstract class CreatingNeuronConstructedContext : CreationPrepareConstructedContext<CreateNeuron>
        {
            protected override CreateNeuron When() => new CreateNeuron(this.AvatarId, this.Id, this.Tag, this.AuthorId);
        }

        public class When_neuronId_already_exists : CreatingNeuronConstructedContext
        {
            [Fact]
            public async Task Then_should_throw_concurrency_exception()
            {
                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
            }
        }

        public class When_neuronId_is_authorId : CreatingNeuronConstructedContext
        {
            protected override bool PreAddNeuron => false;

            protected override Guid Id => Guid.Parse(this.AuthorId);

            [Fact]
            public async Task Then_should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
            }
        }

        public class When_neuronId_is_preexisting_terminalId : CreatingNeuronConstructedContext
        {
            protected Guid author2Id;
            protected virtual Guid Author2Id => this.author2Id = this.author2Id == Guid.Empty ? Guid.NewGuid() : this.author2Id;

            protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[]
            {
                new NeuronCreated(this.Author2Id, "Author2", this.AuthorId.ToString()) { Version = 1 },
                new TerminalCreated(this.Id, this.Author2Id, Guid.Parse(this.AuthorId), NeurotransmitterEffect.Excite, 1f, this.AuthorId.ToString()) { Version = 1 }
            });

            protected override bool PreAddNeuron => false;

            [Fact]
            public async Task Then_should_throw_concurrency_exception()
            {
                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
            }
        }

        public class When_specified_author_does_not_exist : CreatingNeuronConstructedContext
        {
            protected override bool PreAddAuthor => false;

            [Fact]
            public async Task Then_should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
            }

            [Fact]
            public async Task Then_should_throw_argument_exception_containing_author_reference()
            {
                var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
                Assert.Contains("author", ex.Message);
            }
        }

        public class CreatedNeuronConstructedContext : CreatingNeuronConstructedContext
        {
            protected override bool PreAddNeuron => false;

            protected override bool InvokeWhenOnConstruct => true;
        }

        public class When_requirements_are_met : CreatedNeuronConstructedContext
        {
            [Fact]
            public void Then_should_create_one_event()
            {
                Assert.Equal(1, this.PublishedEvents.Count);
            }

            [Fact]
            public void Then_should_create_correct_event()
            {
                Assert.IsType<NeuronCreated>(this.PublishedEvents.First());
            }

            [Fact]
            public void Then_should_have_correct_id()
            {
                Assert.Equal(this.Id, ((NeuronCreated)this.PublishedEvents.First()).Id);
            }

            [Fact]
            public void Then_should_have_correct_tag()
            {
                Assert.Equal(this.Tag, ((NeuronCreated)this.PublishedEvents.First()).Tag);
            }

            [Fact]
            public void Then_should_have_correct_authorId()
            {
                Assert.Equal(this.AuthorId, ((NeuronCreated)this.PublishedEvents.First()).AuthorId);
            }
        }
    }

    public class When_creating_author_neuron
    {
        public class When_null_command_specified : ConstructedContext<CreateAuthorNeuron>
        {
            protected override CreateAuthorNeuron When() => null;

            [Fact]
            public async Task Then_should_throw_argument_null_exception()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => this.InvokeWhen());
            }
        }

        public abstract class CreatingAuthorNeuronConstructedContext : CreationPrepareConstructedContext<CreateAuthorNeuron>
        {
            protected override CreateAuthorNeuron When() => new CreateAuthorNeuron(this.AvatarId, this.Id, this.Tag);
        }

        public class When_neuronId_already_exists : CreatingAuthorNeuronConstructedContext
        {
            [Fact]
            public async Task Then_should_throw_concurrency_exception()
            {
                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
            }
        }

        public class When_neuronId_is_preexisting_terminalId : CreatingAuthorNeuronConstructedContext
        {
            protected Guid author2Id;
            protected virtual Guid Author2Id => this.author2Id = this.author2Id == Guid.Empty ? Guid.NewGuid() : this.author2Id;

            protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[]
            {
                new NeuronCreated(this.Author2Id, "Author2", this.AuthorId.ToString()) { Version = 1 },
                new TerminalCreated(this.Id, this.Author2Id, Guid.Parse(this.AuthorId), NeurotransmitterEffect.Excite, 1f, this.AuthorId.ToString()) { Version = 1 }
            });

            protected override bool PreAddNeuron => false;

            [Fact]
            public async Task Then_should_throw_concurrency_exception()
            {
                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
            }
        }

        public class CreatedNeuronConstructedContext : CreatingAuthorNeuronConstructedContext
        {
            protected override bool PreAddNeuron => false;

            protected override bool InvokeWhenOnConstruct => true;
        }

        public class When_requirements_are_met : CreatedNeuronConstructedContext
        {
            [Fact]
            public void Then_should_create_one_event()
            {
                Assert.Equal(1, this.PublishedEvents.Count);
            }

            [Fact]
            public void Then_should_create_correct_event()
            {
                Assert.IsType<NeuronCreated>(this.PublishedEvents.First());
            }

            [Fact]
            public void Then_should_have_correct_tag()
            {
                Assert.Equal(this.Tag, ((NeuronCreated)this.PublishedEvents.First()).Tag);
            }
        }
    }

    public abstract class ModificationPrepareConstructedContext<T> : CreationPrepareConstructedContext<T> where T : ICommand
    {
        protected int expectedVersion;

        protected virtual int ExpectedVersion => this.expectedVersion = this.expectedVersion == 0 ? 1 : this.expectedVersion;
    }

    public class When_changing_tag
    {
        public class When_null_command_specified : ConstructedContext<ChangeNeuronTag>
        {
            protected override ChangeNeuronTag When() => null;

            [Fact]
            public async Task Then_should_throw_argument_null_exception()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => this.InvokeWhen());
            }
        }

        public abstract class ChangingTagConstructedContext : ModificationPrepareConstructedContext<ChangeNeuronTag>
        {
            protected string newTag;
            protected virtual string NewTag => this.newTag = this.newTag ?? "Hello Worlds";
            protected override ChangeNeuronTag When() => new ChangeNeuronTag(this.AvatarId, this.Id, this.NewTag, this.AuthorId, this.ExpectedVersion);
        }

        public class When_neuronId_does_not_exist : ChangingTagConstructedContext
        {
            protected override bool PreAddNeuron => false;

            [Fact]
            public async Task Then_should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
            }

            [Fact]
            public async Task Then_should_throw_argument_exception_containing_neuron_reference()
            {
                var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
                Assert.Contains("neuron", ex.Message);
            }
        }

        public class When_specified_author_does_not_exist : ChangingTagConstructedContext
        {
            protected override bool PreAddAuthor => false;

            [Fact]
            public async Task Then_should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
            }

            [Fact]
            public async Task Then_should_throw_argument_exception_containing_author_reference()
            {
                var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
                Assert.Contains("author", ex.Message);
            }
        }

        public class When_expected_version_is_incorrect : ChangingTagConstructedContext
        {
            protected override int ExpectedVersion => 2;

            [Fact]
            public async Task Then_should_throw_concurrency_exception()
            {
                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
            }
        }

        public abstract class ChangedNeuronTagContext : ChangingTagConstructedContext
        {
            protected override bool InvokeWhenOnConstruct => true;
        }

        public class When_new_tag_is_specified : ChangedNeuronTagContext
        {
            [Fact]
            public void Then_should_create_one_event()
            {
                Assert.Equal(1, this.PublishedEvents.Count);
            }

            [Fact]
            public void Then_should_create_correct_event()
            {
                Assert.IsType<NeuronTagChanged>(this.PublishedEvents.First());
            }

            [Fact]
            public void Then_should_have_correct_tag()
            {
                Assert.Equal(this.NewTag, ((NeuronTagChanged)this.PublishedEvents.First()).Tag);
            }
        }

        public class When_new_tag_specified_is_equal_to_original_tag : ChangedNeuronTagContext
        {
            protected override string NewTag => this.Tag;

            [Fact]
            public void Then_should_not_create_event()
            {
                Assert.Equal(0, this.PublishedEvents.Count);
            }
        }
    }

    public class When_deactivating_neuron
    {
        public class When_null_command_specified : ConstructedContext<DeactivateNeuron>
        {
            protected override DeactivateNeuron When() => null;

            [Fact]
            public async Task Then_should_throw_argument_null_exception()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => this.InvokeWhen());
            }
        }

        public abstract class DeactivatingNeuronConstructedContext : ModificationPrepareConstructedContext<DeactivateNeuron>
        {
            protected override DeactivateNeuron When() => new DeactivateNeuron(this.AvatarId, this.Id, this.AuthorId, this.ExpectedVersion);
        }

        public class When_neuronId_does_not_exist : DeactivatingNeuronConstructedContext
        {
            protected override bool PreAddNeuron => false;

            [Fact]
            public async Task Then_should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
            }

            [Fact]
            public async Task Then_should_throw_argument_exception_containing_neuron_reference()
            {
                var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
                Assert.Contains("neuron", ex.Message);
            }
        }

        public class When_specified_author_does_not_exist : DeactivatingNeuronConstructedContext
        {
            protected override bool PreAddAuthor => false;

            [Fact]
            public async Task Then_should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
            }

            [Fact]
            public async Task Then_should_throw_argument_exception_containing_author_reference()
            {
                var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
                Assert.Contains("author", ex.Message);
            }
        }

        public class When_expected_version_is_incorrect : DeactivatingNeuronConstructedContext
        {
            protected override int ExpectedVersion => 2;

            [Fact]
            public async Task Then_should_throw_concurrency_exception()
            {
                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
            }
        }

        public abstract class DeactivatedNeuronContext : DeactivatingNeuronConstructedContext
        {
            protected override bool InvokeWhenOnConstruct => true;
        }

        public class When_neuron_is_active : DeactivatedNeuronContext
        {
            [Fact]
            public void Then_should_create_one_event()
            {
                Assert.Equal(1, this.PublishedEvents.Count);
            }

            [Fact]
            public void Then_should_create_correct_event()
            {
                Assert.IsAssignableFrom<NeuronDeactivated>(this.PublishedEvents.First());
            }
        }

        public class When_neuron_is_inactive : DeactivatedNeuronContext
        {
            protected override IEnumerable<IEvent> Given()
            {
                return base.Given().Concat(new IEvent[]
                {
                    new NeuronDeactivated(this.Id, this.AuthorId.ToString()) { Version = 2 }
                });
            }

            protected override bool InvokeWhenOnConstruct => false;
            protected override int ExpectedVersion => 2;

            [Fact]
            public async Task Then_should_throw_invalid_operation_exception()
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => this.InvokeWhen());
            }
        }
    }
}