using org.neurul.Common.Events;
using org.neurul.Common.Test;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Linq;
using Xunit;

namespace org.neurul.Cortex.Domain.Model.Test.Neurons.NeuronFixture.given
{
    public abstract class Context : TestContext<Neuron>
    {
        protected Guid id;
        protected string tag;

        protected virtual Guid Id => this.id = this.id == Guid.Empty ? Guid.NewGuid() : this.id;
        protected virtual string Tag => this.tag = this.tag ?? string.Empty;
    }

    public abstract class ConstructingAuthorNeuronContext : Context
    {
        protected override bool InvokeWhenOnConstruct => false;

        protected override void When() => this.sut = new Neuron(this.Id, this.Tag);
    }

    public class When_author_neuron
    {
        public class When_constructing
        {
            public class When_empty_id_specified : ConstructingAuthorNeuronContext
            {
                protected override Guid Id => Guid.Empty;

                [Fact]
                public void Then_should_throw_invalid_operation_exception()
                {
                    Assert.Throws<InvalidOperationException>(() => this.When());
                }
            }

            public class When_null_tag_specified : ConstructingAuthorNeuronContext
            {
                protected override string Tag => null;

                [Fact]
                public void Then_should_throw_argument_null_exception()
                {
                    Assert.Throws<ArgumentNullException>(() => this.When());
                }
            }
        }

        public abstract class ConstructedContext : ConstructingAuthorNeuronContext
        {
            protected override bool InvokeWhenOnConstruct => true;
        }

        public class When_constructed : ConstructedContext
        {
            [Fact]
            public void Then_should_contain_correct_id()
            {
                Assert.Equal(this.Id, this.sut.Id);
            }

            [Fact]
            public void Then_should_contain_empty_tag()
            {
                Assert.Equal(string.Empty, this.sut.Tag);
            }

            [Fact]
            public void Then_should_raise_neuron_created_event()
            {
                Assert.IsAssignableFrom<NeuronCreated>(this.sut.GetUncommittedChanges().Last());
            }

            [Fact]
            public void Then_should_raise_event_with_correct_author()
            {
                Assert.Equal(this.Id.ToString(), ((IAuthoredEvent)this.sut.GetUncommittedChanges().Last()).AuthorId);
            }
        }
    }

    public class When_non_author_neuron
    {
        public abstract class ConstructingNonAuthorNeuronContext : Context
        {
            protected Neuron author;

            protected override bool InvokeWhenOnConstruct => false;

            protected virtual Neuron Author => this.author = this.author ?? new Neuron(Guid.NewGuid(), string.Empty);

            protected override void When() => this.sut = new Neuron(this.Id, this.Tag, this.Author);
        }

        public class When_constructing
        {
            public class When_empty_id_specified : ConstructingNonAuthorNeuronContext
            {
                protected override Guid Id => Guid.Empty;

                [Fact]
                public void Then_should_throw_argument_null_exception()
                {
                    Assert.Throws<InvalidOperationException>(() => this.When());
                }
            }

            public class When_null_tag_specified : ConstructingNonAuthorNeuronContext
            {
                protected override string Tag => null;

                [Fact]
                public void Then_should_throw_argument_null_exception()
                {
                    Assert.Throws<ArgumentNullException>(() => this.When());
                }
            }

            public class When_null_author_specified : ConstructingNonAuthorNeuronContext
            {
                protected override Neuron Author => null;

                [Fact]
                public void Then_should_throw_argument_null_exception()
                {
                    Assert.Throws<ArgumentNullException>(() => this.When());
                }
            }

            public class When_inactive_author_specified : ConstructingNonAuthorNeuronContext
            {
                protected override void When()
                {
                    this.Author.Deactivate(this.Author);
                    base.When();
                }

                [Fact]
                public void Then_should_throw_argument_exception()
                {
                    Assert.Throws<ArgumentException>(() => this.When());
                }
            }

            public class When_empty_author_id_specified : ConstructingNonAuthorNeuronContext
            {
                protected override Neuron Author => new Neuron(Guid.Empty, string.Empty);

                [Fact]
                public void Then_should_throw_invalid_operation_exception()
                {
                    Assert.Throws<InvalidOperationException>(() => this.When());
                }
            }

            public class When_authorId_is_equal_to_neuron_id : ConstructingNonAuthorNeuronContext
            {
                protected override Guid Id => this.Author.Id;

                [Fact]
                public void Then_should_throw_argument_exception()
                {
                    Assert.Throws<ArgumentException>(() => this.When());
                }
            }
        }

        public abstract class ConstructedNonAuthorNeuronContext : ConstructingNonAuthorNeuronContext
        {
            protected override bool InvokeWhenOnConstruct => true;
        }

        public class When_constructed : ConstructedNonAuthorNeuronContext
        {
            [Fact]
            public void Then_should_contain_correct_id()
            {
                Assert.Equal(this.Id, this.sut.Id);
            }

            [Fact]
            public void Then_should_contain_empty_tag()
            {
                Assert.Equal(string.Empty, this.sut.Tag);
            }

            [Fact]
            public void Then_should_raise_neuron_created_event()
            {
                Assert.IsAssignableFrom<NeuronCreated>(this.sut.GetUncommittedChanges().Last());
            }

            [Fact]
            public void Then_should_raise_event_with_correct_author()
            {
                Assert.Equal(this.Author.Id.ToString(), ((IAuthoredEvent)this.sut.GetUncommittedChanges().Last()).AuthorId);
            }
        }

        public abstract class DeactivatedConstructedContext : ConstructedNonAuthorNeuronContext
        {
            protected override bool InvokeWhenOnConstruct => false;

            protected override void When()
            {
                base.When();
                this.sut.Deactivate(this.Author);
            }
        }

        public class When_changing_tag
        {
            public class When_neuron_is_inactive : DeactivatedConstructedContext
            {
                protected override void When()
                {
                    base.When();
                    this.sut.ChangeTag(string.Empty, this.Author);
                }

                [Fact]
                public void Then_should_throw_invalid_operation_exception()
                {
                    Assert.Throws<InvalidOperationException>(() => this.When());
                }
            }

            public abstract class ChangingTagContext : ConstructedNonAuthorNeuronContext
            {
                protected Neuron authorParameter;

                protected override void When()
                {
                    base.When();

                    this.sut.ChangeTag(this.TagParameter, this.AuthorParameter);
                }

                protected abstract string TagParameter
                {
                    get;
                }

                protected virtual Neuron AuthorParameter => this.authorParameter = this.authorParameter ?? this.Author;
            }

            public class When_tag_is_valid : ChangingTagContext
            {
                protected override string TagParameter => "Hello World";

                [Fact]
                public void Then_should_change_tag()
                {
                    // Assert
                    Assert.Equal(this.TagParameter, ((NeuronTagChanged)this.sut.GetUncommittedChanges().Last()).Tag);
                    Assert.Equal(this.TagParameter, this.sut.Tag);
                }
            }

            public class When_tag_is_null : ChangingTagContext
            {
                protected override string TagParameter => null;

                protected override bool InvokeWhenOnConstruct => false;

                [Fact]
                public void Then_should_throw_argument_null_exception()
                {
                    Assert.Throws<ArgumentNullException>(() => this.When());
                }
            }

            public class When_new_tag_is_same_as_current : ChangingTagContext
            {
                protected override string TagParameter => string.Empty;

                [Fact]
                public void Then_should_do_nothing()
                {
                    // Assert
                    Assert.NotEqual(typeof(NeuronTagChanged), this.sut.GetUncommittedChanges().Last().GetType());
                    Assert.Equal(string.Empty, this.sut.Tag);
                }
            }

            public class When_new_author_is_null : ChangingTagContext
            {
                protected override string TagParameter => "Hello";

                protected override Neuron AuthorParameter => null;

                protected override bool InvokeWhenOnConstruct => false;

                [Fact]
                public void Then_should_throw_argument_null_exception()
                {
                    Assert.Throws<ArgumentNullException>(() => this.When());
                }
            }

            public class When_inactive_author_specified : ChangingTagContext
            {
                protected override string TagParameter => "Hello";

                protected override Neuron AuthorParameter => this.authorParameter = this.authorParameter ?? new Neuron(Guid.NewGuid(), string.Empty);

                protected override bool InvokeWhenOnConstruct => false;

                protected override void When()
                {
                    this.AuthorParameter.Deactivate(this.Author);
                    base.When();
                }

                [Fact]
                public void Then_should_throw_argument_exception()
                {
                    Assert.Throws<ArgumentException>(() => this.When());
                }
            }
        }

        public class When_deactivating_neuron
        {
            public class When_neuron_is_inactive : DeactivatedConstructedContext
            {
                protected override bool InvokeWhenOnConstruct => false;

                protected override void When()
                {
                    base.When();
                    this.sut.Deactivate(this.Author);
                }

                [Fact]
                public void Then_should_throw_invalid_operation_exception()
                {
                    Assert.Throws<InvalidOperationException>(() => this.When());
                }
            }

            public abstract class DeactivatingContext : ConstructedNonAuthorNeuronContext
            {
                protected Neuron authorParameter;

                protected virtual Neuron AuthorParameter => this.authorParameter = this.authorParameter ?? this.Author;

                protected override void When()
                {
                    base.When();

                    this.sut.Deactivate(this.AuthorParameter);
                }
            }

            public class When_neuron_is_active : DeactivatingContext
            {
                [Fact]
                public void Then_should_raise_neuron_deactivated_event()
                {
                    Assert.IsAssignableFrom<NeuronDeactivated>(this.sut.GetUncommittedChanges().Last());
                }
            }

            public class When_specified_author_is_null : DeactivatingContext
            {
                protected override Neuron AuthorParameter => null;

                protected override bool InvokeWhenOnConstruct => false;

                [Fact]
                public void Then_should_throw_argument_null_exception()
                {
                    Assert.Throws<ArgumentNullException>(() => this.When());
                }
            }

            public class When_inactive_author_specified : DeactivatingContext
            {
                protected override Neuron AuthorParameter => this.authorParameter = this.authorParameter ?? new Neuron(Guid.NewGuid(), string.Empty);

                protected override bool InvokeWhenOnConstruct => false;

                protected override void When()
                {
                    this.AuthorParameter.Deactivate(this.Author);
                    base.When();
                }

                [Fact]
                public void Then_should_throw_argument_exception()
                {
                    Assert.Throws<ArgumentException>(() => this.When());
                }
            }
        }
    }
}

// TODO: add validators for Neuron events constructors