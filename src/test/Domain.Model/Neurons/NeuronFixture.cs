using CQRSlite.Events;
using Moq;
using org.neurul.Cortex.Domain.Model.Neurons;
using org.neurul.Common.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace org.neurul.Cortex.Domain.Model.Test.Neurons.NeuronFixture.given
{
    public abstract class Context : TestContext<Neuron>
    {
        private string authorId = Guid.NewGuid().ToString();

        protected override void Given()
        {
            base.Given();

            var id = Guid.NewGuid();
            this.sut = new Neuron(id, string.Empty, authorId);
        }

        protected virtual string AuthorId => this.authorId;
}

    public class When_created_with_id_only : Context
    {
        [Fact]
        public void Should_contain_empty_data()
        {
            Assert.Equal(string.Empty, this.sut.Data);
        }

        [Fact]
        public void Should_have_empty_terminals()
        {
            Assert.Empty(this.sut.Terminals);
        }

        [Fact]
        public void Should_raise_neuron_created_event()
        {
            Assert.IsAssignableFrom<NeuronCreated>(this.sut.GetUncommittedChanges().Last());
        }
    }

    public class When_adding_terminal
    {
        public abstract class InitializeAddingTerminalContext : Context
        {
            protected Terminal[] terminals;

            protected override void Given()
            {
                base.Given();

                this.terminals = this.GenerateTerminals();
            }

            protected virtual int GenerateTerminalsCount => 1;

            protected virtual Terminal[] GenerateTerminals()
            {
                var result = new List<Terminal>();
                for (int i = 0; i < this.GenerateTerminalsCount; i++)
                    result.Add(new Terminal(Guid.NewGuid()));
                return result.ToArray();
            }
        }

        public class When_validating_parameters
        {
            public class When_null_terminal_is_specified : InitializeAddingTerminalContext 
            {
                [Fact]
                public async Task Should_throw_argument_null_exception()
                {
                    await Assert.ThrowsAsync<ArgumentNullException>("terminals", () => this.sut.AddTerminals(this.AuthorId, null));
                }
            }

            public class When_empty_terminal_array_is_specified : InitializeAddingTerminalContext
            {
                [Fact]
                public async Task Should_throw_argument_exception()
                {
                    // Assert
                    await Assert.ThrowsAsync<ArgumentException>("terminals", () => this.sut.AddTerminals(new Terminal[0], this.AuthorId));
                }
            }

            public class When_neuron_is_deactivated : InitializeAddingTerminalContext
            {
                protected override void Given()
                {
                    base.Given();

                    this.sut.Deactivate(this.AuthorId);
                }

                protected override bool InvokeWhenOnConstruct => false;

                [Fact]
                public async Task Should_throw_invalid_operation_exception()
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(() => this.sut.AddTerminals(this.terminals, this.AuthorId));
                }
            }
        }

        public abstract class AddingTerminalContext : InitializeAddingTerminalContext
        {
            protected override void When()
            {
                base.When();

                Task.Run(async () => await this.sut.AddTerminals(this.TerminalsForAdding, this.AuthorId)).Wait();
            }

            protected virtual Func<Terminal, bool> TerminalValidator => t => true;

            protected virtual Terminal[] TerminalsForAdding => new Terminal[] { this.terminals[0] };            
        }

        public class When_terminals_are_empty
        {
            public class When_specified_target_exists : AddingTerminalContext
            {
                [Fact]
                public void Should_add_terminal()
                {
                    Assert.Single(this.sut.Terminals);
                }

                [Fact]
                public void Should_raise_terminals_added_event()
                {
                    Assert.NotEmpty(this.sut.GetUncommittedChanges().OfType<TerminalsAdded>());
                }
            }

            public class When_two_specified_targets_exist : AddingTerminalContext
            {
                protected override int GenerateTerminalsCount => 2;

                protected override Terminal[] TerminalsForAdding => this.terminals.ToArray();

                [Fact]
                public void Should_add_two_terminals()
                {
                    // Assert
                    Assert.Equal(2, this.sut.Terminals.Count);
                }
            }            
        }

        public class When_terminals_have_one_terminal
        {
            public abstract class AddingToTerminalsWithOneTerminalContext : AddingTerminalContext
            {
                protected override int GenerateTerminalsCount => 1;

                protected override void Given()
                {
                    base.Given();

                    Task.Run(async () => await this.sut.AddTerminals(this.AuthorId, this.terminals[0])).Wait();
                }
            }

            public class When_specified_target_exists : AddingToTerminalsWithOneTerminalContext
            {
                protected override int GenerateTerminalsCount => base.GenerateTerminalsCount + 1;

                protected override Terminal[] TerminalsForAdding => new Terminal[] { this.terminals[1] };

                [Fact]
                public void Should_add_terminal()
                {
                    Assert.Equal(2, this.sut.Terminals.Count);
                }

                [Fact]
                public void Should_add_terminal_as_last_added_terminal_in_terminals()
                {
                    Assert.Equal(this.terminals[1], this.sut.Terminals.Last());
                }

                [Fact]
                public void Should_raise_terminals_added_event()
                {
                    Assert.IsAssignableFrom<TerminalsAdded>(this.sut.GetUncommittedChanges().Last());
                }

                [Fact]
                public void Should_reference_terminal_in_last_terminals_added_event()
                {
                    Assert.Equal(this.terminals[1], ((TerminalsAdded)this.sut.GetUncommittedChanges().Last()).Terminals.Last());
                }

                [Fact]
                public void Should_have_three_uncommitted_changes()
                {
                    Assert.Equal(3, this.sut.GetUncommittedChanges().Length);
                }
            }
        }

        public class When_terminals_have_two_terminals
        {
            public abstract class AddingToTerminalsWithTwoTerminalsContext : AddingTerminalContext
            {
                protected override int GenerateTerminalsCount => 2;

                protected override void Given()
                {
                    base.Given();

                    Task.Run(async () => await this.sut.AddTerminals(this.AuthorId, this.terminals[0], this.terminals[1])).Wait();
                }
            }

            public class When_adding_duplicate_of_first_terminals_terminal : AddingToTerminalsWithTwoTerminalsContext
            {
                protected override bool InvokeWhenOnConstruct => false;

                protected override Terminal[] GenerateTerminals()
                {
                    Guid t1 = Guid.NewGuid(),
                        t2 = Guid.NewGuid(),
                        t3 = new Guid(t1.ToString());

                    return new Terminal[] { new Terminal(t1), new Terminal(t2), new Terminal(t3) };
                }

                [Fact]
                public async Task Should_throw_an_argument_exception()
                {
                    // Assert
                    await Assert.ThrowsAsync<ArgumentException>(() => this.sut.AddTerminals(this.AuthorId, this.terminals[2]));
                }
            }            
        }        
    }

    public class When_changing_data
    {
        public class When_neuron_is_inactive : Context
        {
            protected override void Given()
            {
                base.Given();

                this.sut.Deactivate(this.AuthorId);
            }

            [Fact]
            public void Should_throw_invalid_operation_exception()
            {
                Assert.Throws<InvalidOperationException>(() => this.sut.ChangeData(string.Empty, this.AuthorId));
            }
        }

        public abstract class ChangingDataContext : Context
        {
            protected override void Given()
            {
                base.Given();

                this.sut.ChangeData(this.NewData, this.AuthorId);
            }

            protected abstract string NewData
            {
                get;
            }
        }

        public class When_data_is_valid : ChangingDataContext
        {
            protected override string NewData => "Hello World";

            [Fact]
            public void Should_change_data()
            {
                // Assert
                Assert.Equal(this.NewData, ((NeuronDataChanged)this.sut.GetUncommittedChanges().Last()).Data);
                Assert.Equal(this.NewData, this.sut.Data);
            }
        }

        public class When_new_data_is_same_as_current : ChangingDataContext
        {
            protected override string NewData => string.Empty;

            [Fact]
            public void Should_do_nothing()
            {
                // Assert
                Assert.NotEqual(typeof(NeuronDataChanged), this.sut.GetUncommittedChanges().Last().GetType());
                Assert.Equal(string.Empty, this.sut.Data);
            }
        }
    }

    public class When_removing_terminal
    {
        public class When_validating_parameters
        {
            public class When_null_is_specified : Context
            {
                [Fact]
                public void Should_throw_argument_null_exception()
                {
                    Assert.Throws<ArgumentNullException>("terminals", () => this.sut.RemoveTerminals(null, this.AuthorId));
                }
            }

            public class When_empty_terminal_array_is_specified : Context
            {
                [Fact]
                public void Should_throw_argument_exception()
                {
                    Assert.Throws<ArgumentException>("terminals", () => this.sut.RemoveTerminals(this.AuthorId));
                }
            }

            public class When_neuron_is_deactivated : Context
            {
                protected override void Given()
                {
                    base.Given();

                    this.sut.Deactivate(this.AuthorId);
                }

                [Fact]
                public void Should_throw_invalid_operation_exception()
                {
                    Assert.Throws<InvalidOperationException>(() => this.sut.RemoveTerminals(this.AuthorId));
                }
            }
        }

        public abstract class RemovingTerminalContext : Context
        {
            protected Guid[] terminalIds;

            protected override void Given()
            {
                base.Given();

                this.terminalIds = this.GetTerminals();
            }

            protected virtual Guid[] GetTerminals()
            {
                return new Guid[] { Guid.NewGuid() };
            }
        }

        public class When_terminals_are_empty
        {
            public class When_specifying_terminal_not_in_terminals : RemovingTerminalContext
            {
                [Fact]
                public void Should_throw_argument_exception()
                {
                    var ex = Assert.Throws<ArgumentException>(() => this.sut.RemoveTerminals(this.AuthorId, new Terminal(this.terminalIds[0])));
                    Assert.Contains(this.terminalIds[0].ToString(), ex.Message);
                }
            }

            public class When_specifying_terminals_not_in_terminals : RemovingTerminalContext
            {
                [Fact]
                public void Should_throw_argument_exception()
                {
                    var ts = new Terminal[]
                    {
                            new Terminal(Guid.NewGuid()),
                            new Terminal(Guid.NewGuid()),
                            new Terminal(Guid.NewGuid())
                    };
                    var ex = Assert.Throws<ArgumentException>(() => this.sut.RemoveTerminals(this.AuthorId, ts));
                    Assert.Contains(ts[0].TargetId.ToString(), ex.Message);
                }
            }
        }

        public abstract class SingleTerminalInTerminalsContext : RemovingTerminalContext
        {
            protected override void Given()
            {
                base.Given();

                foreach (Guid g in this.terminalIds)
                {
                    Task.Run(() => this.sut.AddTerminals(this.AuthorId, new Terminal(g))).Wait();
                }
            }
        }

        public class When_single_terminal_in_terminals
        {
            public class When_specifying_single_terminal : SingleTerminalInTerminalsContext
            {
                protected override void When()
                {
                    this.sut.RemoveTerminals(this.AuthorId, new Terminal(this.terminalIds[0]));
                }

                [Fact]
                public void Should_remove_terminal()
                {
                    Assert.Empty(this.sut.Terminals);
                }

                [Fact]
                public void Should_raise_terminals_removed_event()
                {
                    Assert.NotEmpty(this.sut.GetUncommittedChanges().OfType<TerminalsRemoved>());
                }
            }

            public class When_specifying_terminal_not_in_terminals : SingleTerminalInTerminalsContext
            {
                [Fact]
                public void Should_throw_argument_exception()
                {
                    Assert.Throws<ArgumentException>("terminals", () => this.sut.RemoveTerminals(this.AuthorId, new Terminal(Guid.NewGuid())));
                }
            }
        }

        public abstract class MultipleTerminalsInTerminalsContext : SingleTerminalInTerminalsContext
        {
            protected override Guid[] GetTerminals()
            {
                return new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            }
        }

        public class When_multiple_terminals_in_terminals
        {
            public class When_specifying_all_terminals_in_terminals : MultipleTerminalsInTerminalsContext
            {
                protected override void When()
                {
                    base.When();

                    var ts = this.terminalIds.Select(g => new Terminal(g));
                    this.sut.RemoveTerminals(ts, this.AuthorId);
                }

                [Fact]
                public void Should_remove_terminals()
                {
                    Assert.Empty(this.sut.Terminals);
                }

                [Fact]
                public void Should_raise_terminals_removed_event()
                {
                    Assert.IsAssignableFrom<TerminalsRemoved>(this.sut.GetUncommittedChanges().Last());
                }
            }

            public class When_specifying_single_terminal_in_terminals : MultipleTerminalsInTerminalsContext
            {
                private int initCount;

                protected override void Given()
                {
                    base.Given();

                    this.initCount = this.terminalIds.Count();
                    this.sut.RemoveTerminals(this.AuthorId, new Terminal(this.terminalIds[1]));
                }

                [Fact]
                public void Should_reduce_terminals_count_by_one()
                {
                    Assert.Equal(initCount - 1, this.sut.Terminals.Count);
                }

                [Fact]
                public void Should_retain_other_terminals()
                {
                    Assert.Contains(this.sut.Terminals, t => t.TargetId == this.terminalIds[0]);
                    Assert.Contains(this.sut.Terminals, t => t.TargetId == this.terminalIds[2]);
                }

                [Fact]
                public void Should_remove_specified_terminal()
                {
                    Assert.DoesNotContain(this.sut.Terminals, t => t.TargetId == this.terminalIds[1]);
                }
            }
        }
    }

    public class When_deactivating_neuron
    {
        public abstract class DeactivatingContext : Context
        {
            protected override void Given()
            {
                base.Given();

                this.sut.Deactivate(this.AuthorId);
            }
        }

        public class When_neuron_is_active : DeactivatingContext
        {
            [Fact]
            public void Should_raise_neuron_deactivated_event()
            {
                Assert.IsAssignableFrom<NeuronDeactivated>(this.sut.GetUncommittedChanges().Last());
            }
        }

        public class When_neuron_is_inactive : DeactivatingContext
        {
            [Fact]
            public void Should_throw_invalid_operation_exception()
            {
                Assert.Throws<InvalidOperationException>(() => this.sut.Deactivate(this.AuthorId));
            }
        }
    }
}
