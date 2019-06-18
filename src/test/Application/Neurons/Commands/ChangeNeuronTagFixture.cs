using org.neurul.Common.Test;
using org.neurul.Cortex.Application.Neurons;
using org.neurul.Cortex.Application.Neurons.Commands;
using System;
using Xunit;

namespace org.neurul.Cortex.Application.Test.Neurons.Commands.ChangeNeuronTagFixture.given
{
    public abstract class ConstructingContext : TestContext<ChangeNeuronTag>
    {
        protected string avatarId;
        protected Guid id;
        protected string newTag;
        protected string authorId;
        protected int expectedVersion;

        protected override bool InvokeWhenOnConstruct => false;

        protected virtual string AvatarId => this.avatarId = this.avatarId ?? "AvatarId";
        protected virtual Guid Id => this.id = this.id == Guid.Empty ? Guid.NewGuid() : this.id;
        protected virtual string NewTag => this.newTag = this.newTag ?? "Tag";
        protected virtual string AuthorId => this.authorId = this.authorId ?? Guid.NewGuid().ToString();
        protected virtual int ExpectedVersion => this.expectedVersion = this.expectedVersion == 0 ? 1 : this.expectedVersion;

        protected override void When() => this.sut = new ChangeNeuronTag(this.AvatarId, this.Id, this.NewTag, this.AuthorId, this.ExpectedVersion);
    }

    public class When_constructing
    {
        public class When_specified_avatarId_is_null : ConstructingContext
        {
            protected override string AvatarId => null;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentNullException>(() => this.When());
            }
        }

        public class When_specified_id_is_invalid : ConstructingContext
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

        public class When_specified_new_tag_is_null : ConstructingContext
        {
            protected override string NewTag => null;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentNullException>(() => this.When());
            }
        }

        public class When_specified_authorid_is_null : ConstructingContext
        {
            protected override string AuthorId => null;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentException>(() => this.When());
            }
        }

        public class When_specified_authorid_is_invalid : ConstructingContext
        {
            protected override string AuthorId => "invalidguid";

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

        public class When_specified_expected_version_is_zero : ConstructingContext
        {
            protected override int ExpectedVersion => 0;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentException>(() => this.When());
            }
        }

        public class When_specified_expected_version_is_negative_nine : ConstructingContext
        {
            protected override int ExpectedVersion => -9;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentException>(() => this.When());
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
            Assert.Equal(this.NewTag, this.sut.NewTag);
        }

        [Fact]
        public void Then_should_have_correct_author_id()
        {
            Assert.Equal(this.AuthorId, this.sut.AuthorId);
        }

        [Fact]
        public void Then_should_have_correct_expected_version()
        {
            Assert.Equal(this.ExpectedVersion, this.sut.ExpectedVersion);
        }
    }
}
