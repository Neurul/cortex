using org.neurul.Common.Test;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Linq;
using Xunit;

namespace org.neurul.Cortex.Application.Test.Neurons.Commands.CreateTerminalFixture.given
{
    public abstract class ConstructingContext : TestContext<CreateTerminal>
    {
        protected string avatarId;
        protected Guid id;
        protected Guid presynapticNeuronId;
        protected Guid postsynapticNeuronId;
        protected NeurotransmitterEffect effect;
        protected float strength;
        protected Guid authorId;

        protected override bool InvokeWhenOnConstruct => false;

        protected virtual string AvatarId => this.avatarId = this.avatarId ?? "AvatarId";
        protected virtual Guid Id => this.id = this.id == Guid.Empty ? Guid.NewGuid() : this.id;
        protected virtual Guid PresynapticNeuronId => this.presynapticNeuronId = this.presynapticNeuronId == Guid.Empty ? Guid.NewGuid() : this.presynapticNeuronId;
        protected virtual Guid PostsynapticNeuronId => this.postsynapticNeuronId = this.postsynapticNeuronId == Guid.Empty ? Guid.NewGuid() : this.postsynapticNeuronId;
        protected virtual NeurotransmitterEffect Effect => this.effect = this.effect == NeurotransmitterEffect.NotSet ? NeurotransmitterEffect.Excite : this.effect;
        protected virtual float Strength => this.strength = this.strength == 0f ? 1f : this.strength;
        protected virtual Guid AuthorId => this.authorId = this.authorId == Guid.Empty ? Guid.NewGuid() : this.authorId;

        protected override void When() => this.sut = new CreateTerminal(this.AvatarId, this.Id, this.PresynapticNeuronId, 
            this.PostsynapticNeuronId, this.Effect, this.Strength, this.AuthorId);
    }

    public class When_constructing
    {
        public class When_avatarId_is_null : ConstructingContext
        {
            protected override string AvatarId => null;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentNullException>(() => this.When());
            }
        }

        public class When_id_is_invalid : ConstructingContext
        {
            protected override Guid Id => Guid.Empty;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentException>(() => this.When());
            }

            [Fact]
            public void Then_should_throw_argument_exception_containing_id_reference()
            {
                var ex = Assert.Throws<ArgumentException>(() => this.When());
                Assert.Contains("id", ex.Message);
            }
        }

        public class When_presynaptic_is_invalid : ConstructingContext
        {
            protected override Guid PresynapticNeuronId => Guid.Empty;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentException>(() => this.When());
            }

            [Fact]
            public void Then_should_throw_argument_exception_containing_presynaptic_reference()
            {
                var ex = Assert.Throws<ArgumentException>(() => this.When());
                Assert.Contains("presynapticNeuronId", ex.Message);
            }
        }

        public class When_postsynaptic_is_invalid : ConstructingContext
        {
            protected override Guid PostsynapticNeuronId => Guid.Empty;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentException>(() => this.When());
            }

            [Fact]
            public void Then_should_throw_argument_exception_containing_postsynaptic_reference()
            {
                var ex = Assert.Throws<ArgumentException>(() => this.When());
                Assert.Contains("postsynapticNeuronId", ex.Message);
            }
        }

        public class When_authorid_is_invalid : ConstructingContext
        {
            protected override Guid AuthorId => Guid.Empty;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentException>(() => this.When());
            }

            [Fact]
            public void Then_should_throw_argument_exception_containing_author_reference()
            {
                var ex = Assert.Throws<ArgumentException>(() => this.When());
                Assert.Contains("author", ex.Message);
            }
        }
    }

    public class ConstructedContext : ConstructingContext
    {
        protected override bool InvokeWhenOnConstruct => true;
    }

    public class When_constructed : ConstructedContext
    {
        [Fact]
        public void Then_should_have_correct_avatar_id()
        {
            Assert.Equal(this.AvatarId, this.sut.AvatarId);
        }

        [Fact]
        public void Then_should_have_correct_id()
        {
            Assert.Equal(this.Id, this.sut.Id);
        }

        [Fact]
        public void Then_should_contain_correct_presynapticNeuronId()
        {
            Assert.Equal(this.PresynapticNeuronId, this.sut.PresynapticNeuronId);
        }

        [Fact]
        public void Then_should_contain_correct_postsynapticNeuronId()
        {
            Assert.Equal(this.PostsynapticNeuronId, this.sut.PostsynapticNeuronId);
        }

        [Fact]
        public void Then_should_contain_correct_effect()
        {
            Assert.Equal(this.Effect, this.sut.Effect);
        }

        [Fact]
        public void Then_should_contain_correct_strength()
        {
            Assert.Equal(this.Strength, this.sut.Strength);
        }

        [Fact]
        public void Then_should_have_correct_author_id()
        {
            Assert.Equal(this.AuthorId, this.sut.SubjectId);
        }
    }
}
