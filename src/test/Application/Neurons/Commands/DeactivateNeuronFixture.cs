using neurUL.Common.Test;
using neurUL.Cortex.Application.Neurons;
using neurUL.Cortex.Application.Neurons.Commands;
using System;
using Xunit;

namespace neurUL.Cortex.Application.Test.Neurons.Commands.DeactivateNeuronFixture.given
{
    public abstract class ConstructingContext : TestContext<DeactivateNeuron>
    {
        protected Guid id;
        protected Guid authorId;
        protected int expectedVersion;

        protected override bool InvokeWhenOnConstruct => false;

        protected virtual Guid Id => this.id = this.id == Guid.Empty ? Guid.NewGuid() : this.id;
        protected virtual Guid AuthorId => this.authorId = this.authorId == Guid.Empty ? Guid.NewGuid() : this.authorId;
        protected virtual int ExpectedVersion => this.expectedVersion = this.expectedVersion == 0 ? 1 : this.expectedVersion;

        protected override void When() => this.sut = new DeactivateNeuron(this.Id, this.AuthorId, this.ExpectedVersion);
    }

    public class When_constructing
    {
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

        public class When_authorid_is_invalid : ConstructingContext
        {
            protected override Guid AuthorId => Guid.Empty;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentException>(() => this.When());
            }

            [Fact]
            public void Then_should_throw_argument_exception_containing_authorid_reference()
            {
                var ex = Assert.Throws<ArgumentException>(() => this.When());
                Assert.Contains("authorId", ex.Message);
            }
        }

        public class When_expected_version_is_zero : ConstructingContext
        {
            protected override int ExpectedVersion => 0;

            [Fact]
            public void Then_should_throw_argument_exception()
            {
                Assert.Throws<ArgumentException>(() => this.When());
            }
        }

        public class When_expected_version_is_negative_nine : ConstructingContext
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
        public void Then_should_have_correct_id()
        {
            Assert.Equal(this.Id, this.sut.Id);
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
