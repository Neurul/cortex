using Common.Test;
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
    public class When_creating_neuron
    {
        public class When_data_is_specified : Specification<Neuron, NeuronCommandHandlers, CreateNeuron>
        {
            private const string DataValue = "Hello World";
            private const string AvatarIdValue = "samplebody";
            private Guid guid;

            protected override NeuronCommandHandlers BuildHandler()
            {
                var nes = new Mock<INavigableEventStore>();
                return new NeuronCommandHandlers(nes.Object, this.Session);
            }

            protected override IEnumerable<IEvent> Given()
            {
                this.guid = Guid.NewGuid();
                return new List<IEvent>();
            }

            protected override CreateNeuron When()
            {
                return new CreateNeuron(AvatarIdValue, this.guid, DataValue);
            }

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
        public abstract class CreateNeuronWithTerminalsContext : ConditionalWhenSpecification<Neuron, NeuronCommandHandlers, CreateNeuronWithTerminals>
        {
            protected const string DataValue = "Hello World";
            private const string AvatarIdValue = "samplebody";
            protected Guid guid;
            protected Guid targetGuid;

            protected override NeuronCommandHandlers BuildHandler()
            {
                var nes = new Mock<INavigableEventStore>();
                return new NeuronCommandHandlers(nes.Object, this.Session);
            }

            protected override IEnumerable<IEvent> Given()
            {
                this.guid = Guid.NewGuid();
                this.targetGuid = Guid.NewGuid();
                return this.GetEvents();
            }

            protected virtual List<IEvent> GetEvents()
            {
                return new List<IEvent>();
            }

            protected override CreateNeuronWithTerminals When()
            {
                return new CreateNeuronWithTerminals(AvatarIdValue, this.guid, DataValue, new Terminal[] { new Terminal(this.targetGuid) });
            }
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
            protected override List<IEvent> GetEvents()
            {
                return new List<IEvent>() { new NeuronCreated(this.guid, DataValue) { Version = 1 } };
            }

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
        public abstract class ChangeNeuronDataContext : Specification<Neuron, NeuronCommandHandlers, ChangeNeuronData>
        {
            protected const string OrigDataValue = "Hello World";
            private const string AvatarIdValue = "samplebody";
            protected Guid guid;

            protected override NeuronCommandHandlers BuildHandler()
            {
                return new NeuronCommandHandlers(new Mock<INavigableEventStore>().Object, this.Session);
            }

            protected override IEnumerable<IEvent> Given()
            {
                this.guid = Guid.NewGuid();
                return new List<IEvent>
            {
                new NeuronCreated(this.guid, OrigDataValue) { Version = 1}
            };
            }

            protected override ChangeNeuronData When()
            {
                return new ChangeNeuronData(AvatarIdValue, this.guid, NewDataValue, 1);
            }

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
        public abstract class AddTerminalsToNeuronContext : Specification<Neuron, NeuronCommandHandlers, AddTerminalsToNeuron>
        {
            protected const string DataValue = "Hello World";
            private const string AvatarIdValue = "samplebody";
            protected Guid guid;
            protected Guid targetGuid;

            protected override NeuronCommandHandlers BuildHandler()
            {
                return new NeuronCommandHandlers(new Mock<INavigableEventStore>().Object, this.Session);
            }

            protected override IEnumerable<IEvent> Given()
            {
                this.InitFields();
                var l = new List<IEvent>();
                l.AddRange(this.GetEvents());
                return l;
            }

            protected override AddTerminalsToNeuron When()
            {
                var ts = new List<Terminal>();
                foreach (Guid g in this.Targets)
                    ts.Add(new Terminal(g));
                return new AddTerminalsToNeuron(AvatarIdValue, this.guid, ts.ToArray(), this.GivenEventsCount);
            }

            protected virtual int GivenEventsCount
            {
                get;
            }

            protected virtual Guid[] Targets
            {
                get;
            }

            protected virtual void InitFields()
            {
                this.guid = Guid.NewGuid();
                this.targetGuid = Guid.NewGuid();
            }

            protected virtual IEvent[] GetEvents()
            {
                return new IEvent[]
                {
                new NeuronCreated(this.guid, AddTerminalsToNeuronContext.DataValue) { Version = 1}
                };
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

                protected override void InitFields()
                {
                    base.InitFields();
                    this.targetGuid2 = Guid.NewGuid();
                    this.targetGuid3 = Guid.NewGuid();
                }

                protected override int GivenEventsCount => 2;

                protected override Guid[] Targets => new Guid[] { this.targetGuid2, this.targetGuid3 };

                protected override IEvent[] GetEvents()
                {
                    return base.GetEvents().Concat(
                        new IEvent[] { new TerminalsAdded(this.guid, new Terminal[] { new Terminal(this.targetGuid) }) { Version = 2 } }
                    ).ToArray();
                }

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
        public abstract class RemoveTerminalsFromNeuronContext : Specification<Neuron, NeuronCommandHandlers, RemoveTerminalsFromNeuron>
        {
            protected const string DataValue = "Hello World";
            private const string AvatarIdValue = "samplebody";
            protected Guid guid;
            protected Guid targetGuid;

            protected override NeuronCommandHandlers BuildHandler()
            {
                return new NeuronCommandHandlers(new Mock<INavigableEventStore>().Object, this.Session);
            }

            protected override IEnumerable<IEvent> Given()
            {
                this.InitFields();
                var l = new List<IEvent>();
                l.AddRange(this.GetEvents());
                return l;
            }

            protected override RemoveTerminalsFromNeuron When()
            {
                var ts = new List<Terminal>();
                foreach (Guid g in this.CommandTargets)
                    ts.Add(new Terminal(g));
                return new RemoveTerminalsFromNeuron(AvatarIdValue, this.guid, ts.ToArray(), this.GivenEventsCount);
            }

            protected virtual int GivenEventsCount
            {
                get;
            }

            protected virtual Guid[] CommandTargets
            {
                get;
            }

            protected virtual void InitFields()
            {
                this.guid = Guid.NewGuid();
                this.targetGuid = Guid.NewGuid();
            }

            protected virtual IEvent[] GetEvents()
            {
                return new IEvent[]
                {
                new NeuronCreated(this.guid, DataValue) { Version = 1}
                };
            }
        }

        public class When_terminals_have_one_terminal
        {
            public class When_one_terminal_is_specified : RemoveTerminalsFromNeuronContext
            {
                protected override int GivenEventsCount => 2;

                protected override Guid[] CommandTargets => new Guid[] { this.targetGuid };

                protected override IEvent[] GetEvents()
                {
                    return base.GetEvents().Concat(
                        new IEvent[] { new TerminalsAdded(this.guid, new Terminal[] { new Terminal(this.targetGuid) }) { Version = 2 } }
                    ).ToArray();
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
                    Assert.Equal(this.targetGuid, ((TerminalsRemoved)this.PublishedEvents.First()).Terminals.First().TargetId);
                }
            }
        }

        public class When_terminals_have_three_terminals
        {
            public class When_one_terminal_is_specified : RemoveTerminalsFromNeuronContext
            {
                private Guid targetGuid2;
                private Guid targetGuid3;

                protected override void InitFields()
                {
                    base.InitFields();
                    this.targetGuid2 = Guid.NewGuid();
                    this.targetGuid3 = Guid.NewGuid();
                }

                protected override int GivenEventsCount => 2;

                protected override Guid[] CommandTargets => new Guid[] { this.targetGuid2 };

                protected override IEvent[] GetEvents()
                {
                    return base.GetEvents().Concat(
                        new IEvent[] { new TerminalsAdded(this.guid, new Terminal[] {
                    new Terminal(this.targetGuid),
                    new Terminal(this.targetGuid2),
                    new Terminal(this.targetGuid3)
                }) { Version = 2 } }
                    ).ToArray();
                }

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
                    Assert.Single(((TerminalsRemoved)this.PublishedEvents.First()).Terminals);
                    Assert.Equal(this.targetGuid2, ((TerminalsRemoved)this.PublishedEvents.Last()).Terminals.First().TargetId);
                }
            }
        }
    }
    
    public class When_deactivating_neuron
    {
        public abstract class DeactivateNeuronContext : ConditionalWhenSpecification<Neuron, NeuronCommandHandlers, DeactivateNeuron>
        {
            protected const string OrigDataValue = "Hello World";
            private const string AvatarIdValue = "samplebody";
            protected Guid guid;

            protected override NeuronCommandHandlers BuildHandler()
            {
                return new NeuronCommandHandlers(new Mock<INavigableEventStore>().Object, this.Session);
            }

            protected override IEnumerable<IEvent> Given()
            {
                this.guid = Guid.NewGuid();
                return new List<IEvent>
                {
                    new NeuronCreated(this.guid, OrigDataValue) { Version = 1}
                };
            }

            protected override DeactivateNeuron When()
            {
                return new DeactivateNeuron(AvatarIdValue, this.guid, this.Given().Last().Version);
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
                var baseEvents = base.Given().ToList();
                baseEvents.Add(new NeuronDeactivated(this.guid) { Version = 2 });
                return baseEvents;
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
