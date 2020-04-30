using neurUL.Common.Test;
using neurUL.Cortex.Application.Neurons;
using neurUL.Cortex.Application.Neurons.Commands;
using System;
using Xunit;

namespace neurUL.Cortex.Application.Test.Neurons.Commands.CreateNeuronFixture.given
{
    public abstract class ConstructingContext : TestContext<CreateNeuron>
    {
        protected Guid id;
        protected Guid authorId;

        protected override bool InvokeWhenOnConstruct => false;

        protected virtual Guid Id => this.id = this.id == Guid.Empty ? Guid.NewGuid() : this.id;
        protected virtual Guid AuthorId => this.authorId = this.authorId == Guid.Empty ? Guid.NewGuid() : this.authorId;

        protected override void When() => this.sut = new CreateNeuron(this.Id, this.AuthorId);
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
        public void Then_should_have_correct_id()
        {
            Assert.Equal(this.Id, this.sut.Id);
        }

        [Fact]
        public void Then_should_have_correct_author_id()
        {
            Assert.Equal(this.AuthorId, this.sut.AuthorId);
        }
    }
}
