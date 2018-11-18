using Common.Test;
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
    public abstract class NonexistentAuthorContext<TCommand> : ConditionalWhenSpecification<Neuron, NeuronCommandHandlers, TCommand> where TCommand : ICommand
    {
        protected static readonly string DataValue = "Hello World";
        protected static readonly string AvatarIdValue = "samplebody";
        protected Guid guid;
        protected Guid authorId;

        protected override bool InvokeWhenOnConstruct => false;

        protected override NeuronCommandHandlers BuildHandler() => new NeuronCommandHandlers(new Mock<INavigableEventStore>().Object, this.Session);

        protected override IEnumerable<IEvent> Given()
        {
            this.guid = Guid.NewGuid();
            this.authorId = Guid.NewGuid();
            return new IEvent[0];
        }
    }

    public abstract class ExistingAuthorContext<TCommand> : NonexistentAuthorContext<TCommand> where TCommand : ICommand
    {
        protected override bool InvokeWhenOnConstruct => true;
        protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[] {
            new NeuronCreated(this.authorId, "Author", this.authorId.ToString()) { Version = 1 }
        });
    }

    public abstract class ExistingDeactivatedAuthorContext<TCommand> : ExistingAuthorContext<TCommand> where TCommand : ICommand
    {
        protected override bool InvokeWhenOnConstruct => false;
        protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[] {
            new NeuronDeactivated(this.authorId, this.authorId.ToString()) { Version = 2 }
        });
    }

    public class When_creating_neuron
    {
        public class When_specified_author_does_not_exist : NonexistentAuthorContext<CreateNeuron>
        {
            protected override CreateNeuron When() => new CreateNeuron(AvatarIdValue, this.guid, DataValue, this.authorId.ToString());

            [Fact]
            public async Task Should_throw_aggregate_not_found_exception()
            {
                await Assert.ThrowsAsync<AggregateNotFoundException>(() => this.InvokeWhen());
            }
        }

        public class When_specified_author_is_neuron : NonexistentAuthorContext<CreateNeuron>
        {
            protected override bool InvokeWhenOnConstruct => true;
            protected override CreateNeuron When() => new CreateNeuron(AvatarIdValue, this.authorId, DataValue, this.authorId.ToString());

            [Fact]
            public void Should_create_one_event()
            {
                Assert.Equal(1, this.PublishedEvents.Count);
            }

            [Fact]
            public void Should_create_correct_event()
            {
                Assert.IsType<NeuronCreated>(this.PublishedEvents.First());
            }

            [Fact]
            public void Should_have_correct_data()
            {
                Assert.Equal(DataValue, ((NeuronCreated)this.PublishedEvents.First()).Data);
            }
        }

        public class When_specified_author_is_deactivated : ExistingDeactivatedAuthorContext<CreateNeuron>
        {
            protected override CreateNeuron When() => new CreateNeuron(AvatarIdValue, this.guid, DataValue, this.authorId.ToString());
        
            [Fact]
            public async Task Should_throw_invalid_operation_exception()
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => this.InvokeWhen());
            }
        }

        public class CreateNeuronWithDataContext : ExistingAuthorContext<CreateNeuron>
        {
            protected override CreateNeuron When() => new CreateNeuron(AvatarIdValue, this.guid, DataValue, this.authorId.ToString());
        }

        public class When_data_is_specified : CreateNeuronWithDataContext
        { 
            [Fact]
            public void Should_create_one_event()
            {
                Assert.Equal(1, this.PublishedEvents.Count);
            }

            [Fact]
            public void Should_create_correct_event()
            {
                Assert.IsType<NeuronCreated>(this.PublishedEvents.First());
            }

            [Fact]
            public void Should_have_correct_data()
            {
                Assert.Equal(DataValue, ((NeuronCreated)this.PublishedEvents.First()).Data);
            }
        }
    }

    public class When_creating_neuron_with_terminals
    {
        public class When_specified_author_does_not_exist : NonexistentAuthorContext<CreateNeuronWithTerminals>
        {
            protected override CreateNeuronWithTerminals When() => new CreateNeuronWithTerminals(AvatarIdValue, this.guid, DataValue, new Terminal[] { new Terminal(Guid.NewGuid(), NeurotransmitterEffect.Excite, 1) }, this.authorId.ToString());

            [Fact]
            public async Task Should_throw_aggregate_not_found_exception()
            {
                await Assert.ThrowsAsync<AggregateNotFoundException>(() => this.InvokeWhen());
            }
        }

        public class When_specified_author_is_neuron : NonexistentAuthorContext<CreateNeuronWithTerminals>
        {
            protected override bool InvokeWhenOnConstruct => true;
            protected override CreateNeuronWithTerminals When() => new CreateNeuronWithTerminals(AvatarIdValue, this.authorId, DataValue, new Terminal[] { new Terminal(Guid.NewGuid(), NeurotransmitterEffect.Excite, 1) }, this.authorId.ToString());

            [Fact]
            public void Should_create_one_event()
            {
                Assert.Equal(2, this.PublishedEvents.Count);
            }

            [Fact]
            public void Should_create_correct_first_event()
            {
                Assert.IsType<NeuronCreated>(this.PublishedEvents.First());
            }

            [Fact]
            public void Should_have_correct_data()
            {
                Assert.Equal(DataValue, ((NeuronCreated)this.PublishedEvents.First()).Data);
            }
        }

        public class When_specified_author_is_deactivated : ExistingDeactivatedAuthorContext<CreateNeuronWithTerminals>
        {
            protected override CreateNeuronWithTerminals When() => new CreateNeuronWithTerminals(AvatarIdValue, this.guid, DataValue, new Terminal[] { new Terminal(Guid.NewGuid(), NeurotransmitterEffect.Excite, 1) }, this.authorId.ToString());

            [Fact]
            public async Task Should_throw_invalid_operation_exception()
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => this.InvokeWhen());
            }
        }

        public abstract class CreateNeuronWithTerminalsContext : ExistingAuthorContext<CreateNeuronWithTerminals>
        {
            protected Guid targetGuid;

            protected override CreateNeuronWithTerminals When() => new CreateNeuronWithTerminals(AvatarIdValue, this.guid, DataValue, new Terminal[] { new Terminal(this.targetGuid, NeurotransmitterEffect.Excite, 1) }, this.authorId.ToString());
        }

        public class When_data_and_single_terminal_are_specified : CreateNeuronWithTerminalsContext
        {
            [Fact]
            public void Should_contain_one_terminal()
            {
                var n = this.Session.Get<Neuron>(this.guid);
                Assert.Single(n.Result.Terminals);
            }

            [Fact]
            public void Should_contain_correct_terminal_data()
            {
                var n = this.Session.Get<Neuron>(this.guid);
                Assert.Equal(this.targetGuid, n.Result.Terminals.Last().TargetId);
            }

            [Fact]
            public void Should_create_two_events()
            {
                Assert.Equal(2, this.PublishedEvents.Count);
            }

            [Fact]
            public void Should_create_correct_events()
            {
                Assert.IsType<NeuronCreated>(this.PublishedEvents.First());
                Assert.IsType<TerminalsAdded>(this.PublishedEvents.Skip(1).First());
            }

            [Fact]
            public void Should_have_correct_data()
            {
                Assert.Equal(DataValue, ((NeuronCreated)this.PublishedEvents.First()).Data);
                Assert.Equal(this.targetGuid, ((TerminalsAdded)this.PublishedEvents.Skip(1).First()).Terminals.First().TargetId);
            }
        }

        public class When_identical_neuron_exists : CreateNeuronWithTerminalsContext
        {
            protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[] {
                new NeuronCreated(this.guid, DataValue, this.authorId.ToString()) { Version = 1 }
            });

            protected override bool InvokeWhenOnConstruct => false;

            [Fact]
            public async Task Should_throw_concurrency_exception()
            {
                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
            }
        }
    }

    public class When_changing_data
    {
        public class When_specified_author_does_not_exist : NonexistentAuthorContext<ChangeNeuronData>
        {
            protected override ChangeNeuronData When() => new ChangeNeuronData(AvatarIdValue, this.guid, string.Empty, this.authorId.ToString(), 1);

            protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[] {
                new NeuronCreated(this.guid, string.Empty, this.authorId.ToString()) { Version = 1}
            });

            [Fact]
            public async Task Should_throw_aggregate_not_found_exception()
            {
                await Assert.ThrowsAsync<AggregateNotFoundException>(() => this.InvokeWhen());
            }
        }

        public class When_specified_author_is_deactivated : ExistingDeactivatedAuthorContext<ChangeNeuronData>
        {
            protected override ChangeNeuronData When() => new ChangeNeuronData(AvatarIdValue, this.guid, string.Empty, this.authorId.ToString(), 1);

            [Fact]
            public async Task Should_throw_invalid_operation_exception()
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => this.InvokeWhen());
            }
        }

        public abstract class ChangeNeuronDataContext : ExistingAuthorContext<ChangeNeuronData>
        {
            protected const string OrigDataValue = "Hello World";

            protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[] {
                new NeuronCreated(this.guid, OrigDataValue, this.authorId.ToString()) { Version = 1}
            });

            protected override ChangeNeuronData When() => new ChangeNeuronData(AvatarIdValue, this.guid, NewDataValue, this.authorId.ToString(), 1);

            protected virtual string NewDataValue
            {
                get { return "How are you"; }
            }
        }

        public class When_new_data_is_specified : ChangeNeuronDataContext
        {
            [Fact]
            public void Should_create_one_event()
            {
                Assert.Equal(1, this.PublishedEvents.Count);
            }

            [Fact]
            public void Should_create_correct_event()
            {
                Assert.IsType<NeuronDataChanged>(this.PublishedEvents.First());
            }

            [Fact]
            public void Should_have_correct_data()
            {
                Assert.Equal(NewDataValue, ((NeuronDataChanged)this.PublishedEvents.First()).Data);
            }
        }

        public class When_new_data_specified_is_equal_to_original_data : ChangeNeuronDataContext
        {
            protected override string NewDataValue => OrigDataValue;

            [Fact]
            public void Should_not_create_event()
            {
                Assert.Equal(0, this.PublishedEvents.Count);
            }
        }
    }

    public class When_adding_terminal
    {
        public class When_specified_author_does_not_exist : NonexistentAuthorContext<AddTerminalsToNeuron>
        {
            protected override AddTerminalsToNeuron When() => new AddTerminalsToNeuron(AvatarIdValue, this.guid, new Terminal[0], this.authorId.ToString(), 0);

            protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[] {
                new NeuronCreated(this.guid, string.Empty, this.authorId.ToString()) { Version = 1}
            });

            [Fact]
            public async Task Should_throw_aggregate_not_found_exception()
            {
                await Assert.ThrowsAsync<AggregateNotFoundException>(() => this.InvokeWhen());
            }
        }

        public class When_specified_author_is_deactivated : ExistingDeactivatedAuthorContext<AddTerminalsToNeuron>
        {
            protected override AddTerminalsToNeuron When() => new AddTerminalsToNeuron(AvatarIdValue, this.guid, new Terminal[0], this.authorId.ToString(), 0);

            [Fact]
            public async Task Should_throw_invalid_operation_exception()
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => this.InvokeWhen());
            }
        }

        public abstract class AddTerminalsToNeuronContext : ExistingAuthorContext<AddTerminalsToNeuron>
        {            
            protected Guid targetGuid;            
         
            protected override IEnumerable<IEvent> Given()
            {
                this.targetGuid = Guid.NewGuid();
                return base.Given().Concat(new IEvent[]
                {
                    new NeuronCreated(this.guid, AddTerminalsToNeuronContext.DataValue, this.authorId.ToString()) { Version = 1}
                });
            }

            protected override AddTerminalsToNeuron When()
            {
                var ts = new List<Terminal>();
                foreach (Guid g in this.Targets)
                    ts.Add(new Terminal(g, NeurotransmitterEffect.Excite, 1));
                return new AddTerminalsToNeuron(AvatarIdValue, this.guid, ts.ToArray(), this.authorId.ToString(), this.GivenEventsCount);
            }

            protected virtual int GivenEventsCount
            {
                get;
            }

            protected virtual Guid[] Targets
            {
                get;
            }
        }

        public class When_terminals_are_empty
        {
            public class When_one_terminal_is_specified : AddTerminalsToNeuronContext
            {
                protected override int GivenEventsCount => 1;

                protected override Guid[] Targets => new Guid[] { this.targetGuid };

                [Fact]
                public void Should_contain_one_terminal()
                {
                    var n = this.Session.Get<Neuron>(this.guid);
                    Assert.Single(n.Result.Terminals);
                }

                [Fact]
                public void Should_create_one_event()
                {
                    Assert.Single(this.PublishedEvents);
                }

                [Fact]
                public void Should_create_correct_event()
                {
                    Assert.IsType<TerminalsAdded>(this.PublishedEvents.First());
                }

                [Fact]
                public void Should_have_correct_data()
                {
                    Assert.Equal(this.targetGuid, ((TerminalsAdded)this.PublishedEvents.First()).Terminals.First().TargetId);
                }
            }
        }

        public class When_terminals_have_one_terminal
        {
            public class When_two_terminals_are_specified : AddTerminalsToNeuronContext
            {
                private Guid targetGuid2;
                private Guid targetGuid3;

                protected override IEnumerable<IEvent> Given()
                {
                    this.targetGuid2 = Guid.NewGuid();
                    this.targetGuid3 = Guid.NewGuid();
                    return base.Given().Concat(
                        new IEvent[] { new TerminalsAdded(this.guid, new Terminal[] { new Terminal(this.targetGuid, NeurotransmitterEffect.Excite, 1) }, this.authorId.ToString()) { Version = 2 } }
                    );
                }
                    
                protected override int GivenEventsCount => 2;

                protected override Guid[] Targets => new Guid[] { this.targetGuid2, this.targetGuid3 };

                [Fact]
                public void Should_contain_three_terminals()
                {
                    var n = this.Session.Get<Neuron>(this.guid);
                    Assert.Equal(3, n.Result.Terminals.Count);
                }

                [Fact]
                public void Should_create_one_event()
                {
                    Assert.Equal(1, this.PublishedEvents.Count);
                }

                [Fact]
                public void Should_create_correct_event()
                {
                    Assert.IsType<TerminalsAdded>(this.PublishedEvents.First());
                }

                [Fact]
                public void Should_have_correct_data()
                {
                    Assert.Equal(2, ((TerminalsAdded)this.PublishedEvents.First()).Terminals.Count());
                    Assert.Equal(this.targetGuid2, ((TerminalsAdded)this.PublishedEvents.First()).Terminals.First().TargetId);
                    Assert.Equal(this.targetGuid3, ((TerminalsAdded)this.PublishedEvents.First()).Terminals.Skip(1).First().TargetId);
                }
            }
        }
    }

    public class When_removing_terminal
    {
        public class When_specified_author_does_not_exist : NonexistentAuthorContext<RemoveTerminalsFromNeuron>
        {
            protected override RemoveTerminalsFromNeuron When() => new RemoveTerminalsFromNeuron(AvatarIdValue, this.guid, new string[0], this.authorId.ToString(), 0);

            protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[] {
                new NeuronCreated(this.guid, string.Empty, this.authorId.ToString()) { Version = 1}
            });

            [Fact]
            public async Task Should_throw_aggregate_not_found_exception()
            {
                await Assert.ThrowsAsync<AggregateNotFoundException>(() => this.InvokeWhen());
            }
        }

        public class When_specified_author_is_deactivated : ExistingDeactivatedAuthorContext<RemoveTerminalsFromNeuron>
        {
            protected override RemoveTerminalsFromNeuron When() => new RemoveTerminalsFromNeuron(AvatarIdValue, this.guid, new string[0], this.authorId.ToString(), 0);

            [Fact]
            public async Task Should_throw_invalid_operation_exception()
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => this.InvokeWhen());
            }
        }

        public abstract class RemoveTerminalsFromNeuronContext : ExistingAuthorContext<RemoveTerminalsFromNeuron>
        {
            protected Guid targetGuid;
            
            protected override IEnumerable<IEvent> Given()
            {
                this.targetGuid = Guid.NewGuid();
                return base.Given().Concat(new IEvent[]
                {
                    new NeuronCreated(this.guid, DataValue, this.authorId.ToString()) { Version = 1}
                });
            }

            protected override RemoveTerminalsFromNeuron When()
            {
                var ts = new List<string>();
                foreach (Guid g in this.CommandTargets)
                    ts.Add(g.ToString());
                return new RemoveTerminalsFromNeuron(AvatarIdValue, this.guid, ts.ToArray(), this.authorId.ToString(), this.GivenEventsCount);
            }

            protected virtual int GivenEventsCount
            {
                get;
            }

            protected virtual Guid[] CommandTargets
            {
                get;
            }
        }

        public class When_terminals_have_one_terminal
        {
            public class When_one_terminal_is_specified : RemoveTerminalsFromNeuronContext
            {
                protected override int GivenEventsCount => 2;

                protected override Guid[] CommandTargets => new Guid[] { this.targetGuid };

                protected override IEnumerable<IEvent> Given()
                {
                    return base.Given().Concat(new IEvent[] {
                        new TerminalsAdded(this.guid, new Terminal[] { new Terminal(this.targetGuid, NeurotransmitterEffect.Excite, 1) }, this.authorId.ToString()) { Version = 2 }
                    });
                }
                
                [Fact]
                public void Should_have_empty_terminals()
                {
                    var n = this.Session.Get<Neuron>(this.guid);
                    Assert.Empty(n.Result.Terminals);
                }

                [Fact]
                public void Should_create_one_event()
                {
                    Assert.Single(this.PublishedEvents);
                }

                [Fact]
                public void Should_create_correct_event()
                {
                    Assert.IsType<TerminalsRemoved>(this.PublishedEvents.First());
                }

                [Fact]
                public void Should_have_correct_data()
                {
                    Assert.Equal(this.targetGuid.ToString(), ((TerminalsRemoved)this.PublishedEvents.First()).TargetIds.First());
                }
            }
        }

        public class When_terminals_have_three_terminals
        {
            public class When_one_terminal_is_specified : RemoveTerminalsFromNeuronContext
            {
                private Guid targetGuid2;
                private Guid targetGuid3;

                protected override IEnumerable<IEvent> Given()
                {
                    this.targetGuid2 = Guid.NewGuid();
                    this.targetGuid3 = Guid.NewGuid();
                    return base.Given().Concat(
                        new IEvent[] {
                            new TerminalsAdded(
                                this.guid,
                                new Terminal[] {
                                    new Terminal(this.targetGuid, NeurotransmitterEffect.Excite, 1),
                                    new Terminal(this.targetGuid2, NeurotransmitterEffect.Excite, 1),
                                    new Terminal(this.targetGuid3, NeurotransmitterEffect.Excite, 1)
                                },
                                this.authorId.ToString()
                            ) { Version = 2 }
                        }
                    );
                }

                protected override int GivenEventsCount => 2;

                protected override Guid[] CommandTargets => new Guid[] { this.targetGuid2 };

                [Fact]
                public void Should_contain_two_terminals()
                {
                    var n = this.Session.Get<Neuron>(this.guid);
                    Assert.Equal(2, n.Result.Terminals.Count);
                }

                [Fact]
                public void Should_create_one_event()
                {
                    Assert.Equal(1, this.PublishedEvents.Count);
                }

                [Fact]
                public void Should_create_correct_event()
                {
                    Assert.IsType<TerminalsRemoved>(this.PublishedEvents.First());
                }

                [Fact]
                public void Should_have_correct_data()
                {
                    Assert.Single(((TerminalsRemoved)this.PublishedEvents.First()).TargetIds);
                    Assert.Equal(this.targetGuid2.ToString(), ((TerminalsRemoved)this.PublishedEvents.Last()).TargetIds.First());
                }
            }
        }
    }
    
    public class When_deactivating_neuron
    {
        public class When_specified_author_does_not_exist : NonexistentAuthorContext<DeactivateNeuron>
        {
            protected override DeactivateNeuron When() => new DeactivateNeuron(AvatarIdValue, this.guid, this.authorId.ToString(), this.Given().Last().Version);

            protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[] {
                new NeuronCreated(this.guid, string.Empty, this.authorId.ToString()) { Version = 1}
            });

            [Fact]
            public async Task Should_throw_aggregate_not_found_exception()
            {
                await Assert.ThrowsAsync<AggregateNotFoundException>(() => this.InvokeWhen());
            }
        }

        public class When_specified_author_is_deactivated : ExistingDeactivatedAuthorContext<DeactivateNeuron>
        {
            protected override DeactivateNeuron When() => new DeactivateNeuron(AvatarIdValue, this.guid, this.authorId.ToString(), this.Given().Last().Version);

            [Fact]
            public async Task Should_throw_invalid_operation_exception()
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => this.InvokeWhen());
            }
        }

        public abstract class DeactivateNeuronContext : ExistingAuthorContext<DeactivateNeuron>
        {
            protected const string OrigDataValue = "Hello World";

            protected override IEnumerable<IEvent> Given()
            {
                return base.Given().Concat(new IEvent[]
                {
                    new NeuronCreated(this.guid, OrigDataValue, this.authorId.ToString()) { Version = 1}
                });
            }

            protected override DeactivateNeuron When()
            {
                return new DeactivateNeuron(AvatarIdValue, this.guid, this.authorId.ToString(), this.Given().Last().Version);
            }
        }

        public class When_neuron_is_active : DeactivateNeuronContext
        {
            [Fact]
            public void Should_create_one_event()
            {
                Assert.Equal(1, this.PublishedEvents.Count);
            }

            [Fact]
            public void Should_create_correct_event()
            {
                Assert.IsAssignableFrom<NeuronDeactivated>(this.PublishedEvents.First());
            }
        }

        public class When_neuron_is_inactive : DeactivateNeuronContext
        {
            protected override IEnumerable<IEvent> Given()
            {
                return base.Given().Concat(new IEvent[]
                {
                    new NeuronDeactivated(this.guid, this.authorId.ToString()) { Version = 2 }
                });
            }

            protected override bool InvokeWhenOnConstruct => false;

            [Fact]
            public async Task Should_throw_invalid_operation_exception()
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => this.InvokeWhen());
            }
        }
    }
}
