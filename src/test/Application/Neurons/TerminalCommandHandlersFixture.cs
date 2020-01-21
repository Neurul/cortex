using CQRSlite.Commands;
using CQRSlite.Domain.Exception;
using CQRSlite.Events;
using Moq;
using org.neurul.Common;
using org.neurul.Cortex.Application.Neurons;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Common;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace org.neurul.Cortex.Application.Test.Neurons.TerminalCommandHandlersFixture.given
{
    public abstract class TerminalCommandHandlerConstructingContext<TCommand> : ConstructingContext<Terminal, TerminalCommandHandlers, TCommand> where TCommand : ICommand
    {
        protected override TerminalCommandHandlers BuildHandler() => new TerminalCommandHandlers(this.EventSourceFactory, this.SettingsService);
    }

    public class When_constructing
    {
        public class When_null_event_store : TerminalCommandHandlerConstructingContext<CreateTerminal>
        {
            protected override TerminalCommandHandlers BuildHandler() => new TerminalCommandHandlers(null, this.SettingsService);

            [Fact]
            public void Then_should_throw_argument_null_exception()
            {
                Assert.Throws<ArgumentNullException>(() => this.BuildHandler());
            }
        }

        public class When_null_session : TerminalCommandHandlerConstructingContext<CreateTerminal>
        {
            protected override TerminalCommandHandlers BuildHandler() => new TerminalCommandHandlers(this.EventSourceFactory, null);

            [Fact]
            public void Then_should_throw_argument_null_exception()
            {
                Assert.Throws<ArgumentNullException>(() => this.BuildHandler());
            }
        }
    }

    public abstract class ConstructedContext<TCommand> : TerminalCommandHandlerConstructingContext<TCommand> where TCommand : ICommand
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
                this.EventSourceFactory,
                ((object)this.handler).GetFieldValue(typeof(TerminalCommandHandlers), "eventSourceFactory")
                );
        }

        [Fact]
        public void Then_should_contain_correct_session()
        {
            Assert.Equal(
                this.SettingsService,
                ((object)this.handler).GetFieldValue(typeof(TerminalCommandHandlers), "settingsService")
                );
        }
    }

    public abstract class CreationPrepareConstructedContext<TCommand> : ConstructedContext<TCommand> where TCommand : ICommand
    {
        protected override IEnumerable<IEvent> Given()
        {
            var result = new List<IEvent>(base.Given());

            if (this.PreAddAuthor)
                result.Add(new NeuronCreated(this.AuthorId) { Version = 1 });
            if (this.PreAddPresynapticNeuron)
                result.Add(new NeuronCreated(this.PresynapticNeuronId) { Version = 1 });
            if (this.PreAddPostsynapticNeuron)
                result.Add(new NeuronCreated(this.PostsynapticNeuronId) { Version = 1 });
            if (this.PreAddTerminal)
                result.Add(new TerminalCreated(this.Id, this.PresynapticNeuronId, this.PostsynapticNeuronId, this.Effect, this.Strength) { Version = 1 });

            return result.ToArray();
        }

        protected string avatarId;
        protected Guid id;
        protected Guid presynapticNeuronId;
        protected Guid postsynapticNeuronId;
        protected NeurotransmitterEffect effect;
        protected float strength;

        protected virtual bool PreAddAuthor => true;
        protected virtual bool PreAddPresynapticNeuron => true;
        protected virtual bool PreAddPostsynapticNeuron => true;
        protected virtual bool PreAddTerminal => true;

        protected virtual string AvatarId => this.avatarId = this.avatarId ?? "samplebody";
        protected virtual Guid Id => this.id = this.id == Guid.Empty ? Guid.NewGuid() : this.id;
        protected virtual Guid PresynapticNeuronId => this.presynapticNeuronId = this.presynapticNeuronId == Guid.Empty ? Guid.NewGuid() : this.presynapticNeuronId;
        protected virtual Guid PostsynapticNeuronId => this.postsynapticNeuronId = this.postsynapticNeuronId == Guid.Empty ? Guid.NewGuid() : this.postsynapticNeuronId;
        protected virtual NeurotransmitterEffect Effect => this.effect = this.effect == NeurotransmitterEffect.NotSet ? NeurotransmitterEffect.Excite : this.effect;
        protected virtual float Strength => this.strength = this.strength == 0f ? 1f : this.strength;
    }

    public class When_creating_terminal
    {
        public class When_null_command : ConstructedContext<CreateTerminal>
        {
            protected override CreateTerminal When() => null;

            [Fact]
            public async Task Then_should_throw_argument_null_exception()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => this.InvokeWhen());
            }
        }

        public abstract class CreatingTerminalConstructedContext : CreationPrepareConstructedContext<CreateTerminal>
        {
            protected override CreateTerminal When() => new CreateTerminal(this.AvatarId, this.Id, this.PresynapticNeuronId,
                this.PostsynapticNeuronId, this.Effect, this.Strength, this.AuthorId);
        }

        public class When_terminalId_already_exists : CreatingTerminalConstructedContext
        {
            [Fact]
            public async Task Then_should_throw_concurrency_exception()
            {
                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
            }
        }

        public class When_terminalId_is_preexisting_neuronId : CreatingTerminalConstructedContext
        {
            protected Guid author2Id;
            protected virtual Guid Author2Id => this.author2Id = this.author2Id == Guid.Empty ? Guid.NewGuid() : this.author2Id;

            protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[]
            {
                new NeuronCreated(this.Id) { Version = 1 },
            });

            protected override bool PreAddTerminal => false;

            [Fact]
            public async Task Then_should_throw_concurrency_exception()
            {
                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
            }
        }

        public class When_presynaptic_does_not_exist : CreatingTerminalConstructedContext
        {
            protected override bool PreAddPresynapticNeuron => false;

            [Fact]
            public async Task Then_should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
            }

            [Fact]
            public async Task Then_should_throw_argument_exception_containing_presynaptic_reference()
            {
                var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
                Assert.Contains("presynaptic", ex.Message);
            }
        }

        public class When_postsynaptic_does_not_exist : CreatingTerminalConstructedContext
        {
            protected override bool PreAddPostsynapticNeuron => false;

            [Fact]
            public async Task Then_should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
            }

            [Fact]
            public async Task Then_should_throw_argument_exception_containing_postsynaptic_reference()
            {
                var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
                Assert.Contains("postsynaptic", ex.Message);
            }
        }

        // TODO: transfer to sentry
        //public class When_author_does_not_exist : CreatingTerminalConstructedContext
        //{
        //    protected override bool PreAddAuthor => false;

        //    [Fact]
        //    public async Task Then_should_throw_argument_exception()
        //    {
        //        await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
        //    }

        //    [Fact]
        //    public async Task Then_should_throw_argument_exception_containing_author_reference()
        //    {
        //        var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
        //        Assert.Contains("author", ex.Message);
        //    }
        //}

        public class CreatedNeuronConstructedContext : CreatingTerminalConstructedContext
        {
            protected override bool PreAddTerminal => false;

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
                Assert.IsType<TerminalCreated>(this.PublishedEvents.First());
            }

            [Fact]
            public void Then_should_have_correct_id()
            {
                Assert.Equal(this.Id, ((TerminalCreated)this.PublishedEvents.First()).Id);
            }

            [Fact]
            public void Then_should_have_correct_presynapticNeuronId()
            {
                Assert.Equal(this.PresynapticNeuronId, ((TerminalCreated)this.PublishedEvents.First()).PresynapticNeuronId);
            }

            [Fact]
            public void Then_should_have_correct_postsynapticNeuronId()
            {
                Assert.Equal(this.PostsynapticNeuronId, ((TerminalCreated)this.PublishedEvents.First()).PostsynapticNeuronId);
            }

            [Fact]
            public void Then_should_have_correct_effect()
            {
                Assert.Equal(this.Effect, ((TerminalCreated)this.PublishedEvents.First()).Effect);
            }

            [Fact]
            public void Then_should_have_correct_strength()
            {
                Assert.Equal(this.Strength, ((TerminalCreated)this.PublishedEvents.First()).Strength);
            }
        }
    }

    public abstract class ModificationPrepareConstructedContext<T> : CreationPrepareConstructedContext<T> where T : ICommand
    {
        protected int expectedVersion;

        protected virtual int ExpectedVersion => this.expectedVersion = this.expectedVersion == 0 ? 1 : this.expectedVersion;
    }

    public class When_deactivating_terminal
    {
        public class When_null_command : ConstructedContext<DeactivateTerminal>
        {
            protected override DeactivateTerminal When() => null;

            [Fact]
            public async Task Then_should_throw_argument_null_exception()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => this.InvokeWhen());
            }
        }

        public abstract class DeactivatingTerminalConstructedContext : ModificationPrepareConstructedContext<DeactivateTerminal>
        {
            protected override DeactivateTerminal When() => new DeactivateTerminal(this.AvatarId, this.Id, this.AuthorId, this.ExpectedVersion);
        }

        public class When_terminalId_does_not_exist : DeactivatingTerminalConstructedContext
        {
            protected override bool PreAddTerminal => false;

            [Fact]
            public async Task Then_should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
            }

            [Fact]
            public async Task Then_should_throw_argument_exception_containing_terminal_reference()
            {
                var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
                Assert.Contains("terminal", ex.Message);    
            }
        }

        public class When_terminalId_is_equal_to_preexisting_neuron_id : DeactivatingTerminalConstructedContext
        {
            protected override DeactivateTerminal When() => new DeactivateTerminal(this.AvatarId, this.PresynapticNeuronId, this.AuthorId, this.ExpectedVersion);

            [Fact]
            public async Task Then_should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
            }

            [Fact]
            public async Task Then_should_throw_argument_exception_with_correct_message()
            {
                var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
                Assert.Contains("not a recognized event", ex.ToString());
            }
        }

        // TODO: transfer to sentry
        // public class When_author_does_not_exist : DeactivatingTerminalConstructedContext
        //{
        //    protected override bool PreAddAuthor => false;

        //    [Fact]
        //    public async Task Then_should_throw_argument_exception()
        //    {
        //        await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
        //    }

        //    [Fact]
        //    public async Task Then_should_throw_argument_exception_containing_author_reference()
        //    {
        //        var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
        //        Assert.Contains("author", ex.Message);
        //    }
        //}

        public class When_expected_version_is_incorrect : DeactivatingTerminalConstructedContext
        {
            protected override int ExpectedVersion => 2;

            [Fact]
            public async Task Then_should_throw_concurrency_exception()
            {
                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
            }
        }

        public abstract class DeactivatedTerminalContext : DeactivatingTerminalConstructedContext
        {
            protected override bool InvokeWhenOnConstruct => true;
        }

        public class When_terminal_is_active : DeactivatedTerminalContext
        {
            [Fact]
            public void Then_should_create_one_event()
            {
                Assert.Equal(1, this.PublishedEvents.Count);
            }

            [Fact]
            public void Then_should_create_correct_event()
            {
                Assert.IsAssignableFrom<TerminalDeactivated>(this.PublishedEvents.First());
            }
        }

        public class When_terminal_is_inactive : DeactivatedTerminalContext
        {
            protected override IEnumerable<IEvent> Given()
            {
                return base.Given().Concat(new IEvent[]
                {
                    new TerminalDeactivated(this.Id) { Version = 2 }
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

// TODO: add tests
    //- terminal
	   // - create
		  //  \ When_terminalId_is_equal_to_preexisting_neuron_id
			 //   - already exists
		  //  ] When_presynapticNeuronId_is_equal_to_preexisting_terminal_id
		  //  ] When_postsynapticNeuronId_is_equal_to_preexisting_terminal_id
		  //  ] When_authorId_is_equal_to_preexisting_terminal_id
	   // - deactivate
		  //  / When_terminalId_is_equal_to_preexisting_neuron_id
		  //  ] When_authorId_is_equal_to_preexisting_terminal_id
    //- neuron
	   // / formulate test cases
	   // - create neuron
		  //  ] When_neuronId_is_equal_to_preexisting_terminal_id
		  //  ] When_authorId_is_equal_to_preexisting_terminal_id
	   // - create author neuron
		  //  ] When_neuronId_is_equal_to_preexisting_terminal_id
	   // - change tag
		  //  ] When_neuronId_is_equal_to_preexisting_terminal_id
		  //  ] When_authorId_is_equal_to_preexisting_terminal_id
	   // - deactivate neuron
		  //  ] When_neuronId_is_equal_to_preexisting_terminal_id
		  //  ] When_authorId_is_equal_to_preexisting_terminal_id