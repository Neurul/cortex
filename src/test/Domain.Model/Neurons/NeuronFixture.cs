using CQRSlite.Events;
using neurUL.Common.Test;
using neurUL.Cortex.Domain.Model.Neurons;
using System;
using System.Linq;
using ei8.EventSourcing.Client.In;
using Xunit;

namespace neurUL.Cortex.Domain.Model.Test.Neurons.NeuronFixture.given
{
    public abstract class Context : TestContext<Neuron>
    {
        protected Guid id;

        protected virtual Guid Id => this.id = this.id == Guid.Empty ? Guid.NewGuid() : this.id;
    }
    public abstract class ConstructingNeuronContext : Context
    {
        protected override bool InvokeWhenOnConstruct => false;

        protected override void When() => this.sut = new Neuron(this.Id);
    }

    public class When_constructing
    {
        public class When_empty_id : ConstructingNeuronContext
        {
            protected override Guid Id => Guid.Empty;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentException>(() => this.When());
            }
        }

        // TODO: transfer to cortex.tag
        // public class When_null_tag : ConstructingNonAuthorNeuronContext
        //{
        //    protected override string Tag => null;

        //    [Fact]
        //    public void Then_should_throw_argument_null_exception()
        //    {
        //        Assert.Throws<ArgumentNullException>(() => this.When());
        //    }
        //}

        // TODO: transfer to sentry
        //public class When_inactive_author : ConstructingNonAuthorNeuronContext
        //{
        //    protected override void When()
        //    {
        //        this.AuthorId.Deactivate(this.AuthorId);
        //        base.When();
        //    }

        //    [Fact]
        //    public void Then_should_throw_argument_exception()
        //    {
        //        Assert.Throws<ArgumentException>(() => this.When());
        //    }
        //}            
    }

    public abstract class ConstructedNeuronContext : ConstructingNeuronContext
    {
        protected override bool InvokeWhenOnConstruct => true;
    }

    public class When_constructed : ConstructedNeuronContext
    {
        [Fact]
        public void Then_should_contain_correct_id()
        {
            Assert.Equal(this.Id, this.sut.Id);
        }

        // TODO: transfer to cortex.tag
        //[Fact]
        //public void Then_should_contain_empty_tag()
        //{
        //    Assert.Equal(string.Empty, this.sut.Tag);
        //}

        [Fact]
        public void Then_should_raise_neuron_created_event()
        {
            Assert.IsAssignableFrom<NeuronCreated>(this.sut.GetUncommittedChanges().Last());
        }
    }

    public abstract class DeactivatedConstructedContext : ConstructedNeuronContext
    {
        protected override bool InvokeWhenOnConstruct => false;

        protected override void When()
        {
            base.When();
            this.sut.Deactivate();
        }
    }

    // TODO: transfer to Cortex.tag
    //public class When_changing_tag
    //{
    //    public class When_neuron_is_inactive : DeactivatedConstructedContext
    //    {
    //        protected override void When()
    //        {
    //            base.When();
    //            this.sut.ChangeTag(string.Empty, this.AuthorId);
    //        }

    //        [Fact]
    //        public void Then_should_throw_invalid_operation_exception()
    //        {
    //            Assert.Throws<InvalidOperationException>(() => this.When());
    //        }
    //    }

    //    public abstract class ChangingTagContext : ConstructedNonAuthorNeuronContext
    //    {
    //        protected Neuron authorParameter;

    //        protected override void When()
    //        {
    //            base.When();

    //            this.sut.ChangeTag(this.TagParameter, this.AuthorParameter);
    //        }

    //        protected abstract string TagParameter
    //        {
    //            get;
    //        }

    //        protected virtual Neuron AuthorParameter => this.authorParameter = this.authorParameter ?? this.AuthorId;
    //    }

    //    public class When_tag_is_valid : ChangingTagContext
    //    {
    //        protected override string TagParameter => "Hello World";

    //        [Fact]
    //        public void Then_should_change_tag()
    //        {
    //            // Assert
    //            Assert.Equal(this.TagParameter, ((NeuronTagChanged)this.sut.GetUncommittedChanges().Last()).Tag);
    //            Assert.Equal(this.TagParameter, this.sut.Tag);
    //        }
    //    }

    //    public class When_tag_is_null : ChangingTagContext
    //    {
    //        protected override string TagParameter => null;

    //        protected override bool InvokeWhenOnConstruct => false;

    //        [Fact]
    //        public void Then_should_throw_argument_null_exception()
    //        {
    //            Assert.Throws<ArgumentNullException>(() => this.When());
    //        }
    //    }

    //    public class When_new_tag_is_same_as_current : ChangingTagContext
    //    {
    //        protected override string TagParameter => string.Empty;

    //        [Fact]
    //        public void Then_should_do_nothing()
    //        {
    //            // Assert
    //            Assert.NotEqual(typeof(NeuronTagChanged), this.sut.GetUncommittedChanges().Last().GetType());
    //            Assert.Equal(string.Empty, this.sut.Tag);
    //        }
    //    }

    //    public class When_new_author_is_null : ChangingTagContext
    //    {
    //        protected override string TagParameter => "Hello";

    //        protected override Neuron AuthorParameter => null;

    //        protected override bool InvokeWhenOnConstruct => false;

    //        [Fact]
    //        public void Then_should_throw_argument_null_exception()
    //        {
    //            Assert.Throws<ArgumentNullException>(() => this.When());
    //        }
    //    }

    //    public class When_inactive_author : ChangingTagContext
    //    {
    //        protected override string TagParameter => "Hello";

    //        protected override Neuron AuthorParameter => this.authorParameter = this.authorParameter ?? new Neuron(Guid.NewGuid(), string.Empty);

    //        protected override bool InvokeWhenOnConstruct => false;

    //        protected override void When()
    //        {
    //            this.AuthorParameter.Deactivate(this.AuthorId);
    //            base.When();
    //        }

    //        [Fact]
    //        public void Then_should_throw_argument_exception()
    //        {
    //            Assert.Throws<ArgumentException>(() => this.When());
    //        }
    //    }
    //}

    public class When_deactivating_neuron
    {
        public class When_neuron_is_inactive : DeactivatedConstructedContext
        {
            protected override bool InvokeWhenOnConstruct => false;

            protected override void When()
            {
                base.When();
                this.sut.Deactivate();
            }

            [Fact]
            public void Then_should_throw_invalid_operation_exception()
            {
                Assert.Throws<InvalidOperationException>(() => this.When());
            }
        }

        public abstract class DeactivatingContext : ConstructedNeuronContext
        {
            protected override void When()
            {
                base.When();

                this.sut.Deactivate();
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

        // TODO: transfer to Sentry
        //public class When_inactive_author : DeactivatingContext
        //{
        //    protected override Neuron AuthorParameter => this.authorId = this.authorId ?? new Neuron(Guid.NewGuid(), string.Empty);

        //    protected override bool InvokeWhenOnConstruct => false;

        //    protected override void When()
        //    {
        //        this.AuthorParameter.Deactivate(this.AuthorId);
        //        base.When();
        //    }

        //    [Fact]
        //    public void Then_should_throw_argument_exception()
        //    {
        //        Assert.Throws<ArgumentException>(() => this.When());
        //    }
        //}
    }
}

// TODO: add validators for Neuron events constructors