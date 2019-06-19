using org.neurul.Common.Test;
using org.neurul.Cortex.Application.Neurons;
using org.neurul.Cortex.Application.Neurons.Commands;
using System;
using Xunit;

namespace org.neurul.Cortex.Application.Test.Neurons.Commands.CreateNeuronFixture.given
{
    public abstract class ConstructingContext : TestContext<CreateNeuron>
    {
        protected string avatarId;
        protected Guid id;
        protected string tag;
        protected Guid authorId;

        protected override bool InvokeWhenOnConstruct => false;

        protected virtual string AvatarId => this.avatarId = this.avatarId ?? "AvatarId";
        protected virtual Guid Id => this.id = this.id == Guid.Empty ? Guid.NewGuid() : this.id;
        protected virtual string Tag => this.tag = this.tag ?? "Tag";
        protected virtual Guid AuthorId => this.authorId = this.authorId == Guid.Empty ? Guid.NewGuid() : this.authorId;

        protected override void When() => this.sut = new CreateNeuron(this.AvatarId, this.Id, this.Tag, this.AuthorId);
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

        public class When_tag_is_null : ConstructingContext
        {
            protected override string Tag => null;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentNullException>(() => this.When());
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
            public void Then_should_throw_argument_exception_containing_id_reference()
            {
                var ex = Assert.Throws<ArgumentException>(() => this.When());
                Assert.Contains("authorId", ex.Message);
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
        public void Then_should_have_correct_tag()
        {
            Assert.Equal(this.Tag, this.sut.Tag);
        }

        [Fact]
        public void Then_should_have_correct_author_id()
        {
            Assert.Equal(this.AuthorId, this.sut.AuthorId);
        }
    }
}
