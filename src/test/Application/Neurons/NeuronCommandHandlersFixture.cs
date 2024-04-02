// DEL: using CQRSlite.Commands;
//using CQRSlite.Domain.Exception;
//using CQRSlite.Events;
//using Moq;
//using neurUL.Common;
//using neurUL.Cortex.Application.Neurons;
//using neurUL.Cortex.Application.Neurons.Commands;
//using neurUL.Cortex.Common;
//using neurUL.Cortex.Domain.Model.Neurons;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;

//namespace neurUL.Cortex.Application.Test.Neurons.NeuronCommandHandlersFixture.given
//{
//    public abstract class NeuronCommandHandlerConstructingContext<TCommand> : ConstructingContext<Neuron, NeuronCommandHandlers, TCommand> where TCommand : ICommand
//    {
//        protected override NeuronCommandHandlers BuildHandler() => new NeuronCommandHandlers(this.EventSourceFactory, this.SettingsService);
//    }

//    public class When_constructing
//    {
//        public class When_null_event_store : NeuronCommandHandlerConstructingContext<CreateNeuron>
//        {
//            protected override NeuronCommandHandlers BuildHandler() => new NeuronCommandHandlers(null, this.SettingsService);

//            [Fact]
//            public void Then_should_throw_argument_null_exception()
//            {
//                Assert.Throws<ArgumentNullException>(() => this.BuildHandler());
//            }
//        }

//        public class When_null_session : NeuronCommandHandlerConstructingContext<CreateNeuron>
//        {
//            protected override NeuronCommandHandlers BuildHandler() => new NeuronCommandHandlers(this.EventSourceFactory, null);

//            [Fact]
//            public void Then_should_throw_argument_null_exception()
//            {
//                Assert.Throws<ArgumentNullException>(() => this.BuildHandler());
//            }
//        }
//    }

//    public abstract class ConstructedContext<TCommand> : NeuronCommandHandlerConstructingContext<TCommand> where TCommand : ICommand
//    {
//        protected override bool InvokeBuildWhenOnConstruct => true;
//        protected override bool InvokeWhenOnConstruct => false;
//    }

//    public class When_constructed : ConstructedContext<CreateNeuron>
//    {
//        [Fact]
//        public void Then_should_contain_correct_eventStore()
//        {
//            Assert.Equal(
//                this.EventSourceFactory,
//                ((object)this.handler).GetFieldValue(typeof(NeuronCommandHandlers), "eventSourceFactory")
//                );
//        }

//        [Fact]
//        public void Then_should_contain_correct_session()
//        {
//            Assert.Equal(
//                this.SettingsService,
//                ((object)this.handler).GetFieldValue(typeof(NeuronCommandHandlers), "settingsService")
//                );
//        }
//    }

//    public abstract class CreationPrepareConstructedContext<TCommand> : ConstructedContext<TCommand> where TCommand : ICommand
//    {
//        protected override IEnumerable<IEvent> Given()
//        {
//            var result = new List<IEvent>(base.Given());

//            // TODO: transfer to Sentry
//            //if (this.PreAddAuthor)
//            //    result.Add(new NeuronCreated(this.AuthorId, this.AuthorId) { Version = 1 });
//            if (this.PreAddNeuron)
//                result.Add(new NeuronCreated(this.Id) { Version = 1 });

//            return result.ToArray();
//        }

//        protected Guid id;
        
//        // TODO: transfer to Sentry 
//        //protected virtual bool PreAddAuthor => true;
//        protected virtual bool PreAddNeuron => true;

//        protected virtual Guid Id => this.id = this.id == Guid.Empty ? Guid.NewGuid() : this.id;

//        // TODO: transfer to Cortex.Tag
//        //protected virtual string Tag => this.tag = this.tag ?? "Hello";
//    }

//    public class When_creating_neuron
//    {
//        public class When_null_command : ConstructedContext<CreateNeuron>
//        {
//            protected override CreateNeuron When() => null;

//            [Fact]
//            public async Task Then_should_throw_argument_null_exception()
//            {
//                await Assert.ThrowsAsync<ArgumentNullException>(() => this.InvokeWhen());
//            }
//        }

//        public abstract class CreatingNeuronConstructedContext : CreationPrepareConstructedContext<CreateNeuron>
//        {
//            protected override CreateNeuron When() => new CreateNeuron(this.Id, this.AuthorId);
//        }

//        public class When_neuronId_already_exists : CreatingNeuronConstructedContext
//        {
//            [Fact]
//            public async Task Then_should_throw_concurrency_exception()
//            {
//                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
//            }
//        }

//        public class When_neuronId_is_preexisting_terminalId : CreatingNeuronConstructedContext
//        {
//            protected Guid author2Id;
//            protected virtual Guid Author2Id => this.author2Id = this.author2Id == Guid.Empty ? Guid.NewGuid() : this.author2Id;

//            protected override IEnumerable<IEvent> Given() => base.Given().Concat(new IEvent[]
//            {
//                new NeuronCreated(this.Author2Id) { Version = 1 },
//                new TerminalCreated(this.Id, this.Author2Id, this.AuthorId, NeurotransmitterEffect.Excite, 1f) { Version = 1 }
//            });

//            protected override bool PreAddNeuron => false;

//            [Fact]
//            public async Task Then_should_throw_concurrency_exception()
//            {
//                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
//            }
//        }

//        // TODO: transfer to Sentry
//        // public class When_author_does_not_exist : CreatingNeuronConstructedContext
//        //{
//        //    protected override bool PreAddAuthor => false;

//        //    [Fact]
//        //    public async Task Then_should_throw_argument_exception()
//        //    {
//        //        await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
//        //    }

//        //    [Fact]
//        //    public async Task Then_should_throw_argument_exception_containing_author_reference()
//        //    {
//        //        var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
//        //        Assert.Contains("author", ex.Message);
//        //    }
//        //}

//        public class CreatedNeuronConstructedContext : CreatingNeuronConstructedContext
//        {
//            protected override bool PreAddNeuron => false;

//            protected override bool InvokeWhenOnConstruct => true;
//        }

//        public class When_requirements_are_met : CreatedNeuronConstructedContext
//        {
//            [Fact]
//            public void Then_should_create_one_event()
//            {
//                Assert.Equal(1, this.PublishedEvents.Count);
//            }

//            [Fact]
//            public void Then_should_create_correct_event()
//            {
//                Assert.IsType<NeuronCreated>(this.PublishedEvents.First());
//            }

//            [Fact]
//            public void Then_should_have_correct_id()
//            {
//                Assert.Equal(this.Id, ((NeuronCreated)this.PublishedEvents.First()).Id);
//            }

//            // TODO: transfer to cortex.tag
//            // [Fact]
//            //public void Then_should_have_correct_tag()
//            //{
//            //    Assert.Equal(this.Tag, ((NeuronCreated)this.PublishedEvents.First()).Tag);
//            //}
//        }
//    }

//    public abstract class ModificationPrepareConstructedContext<T> : CreationPrepareConstructedContext<T> where T : ICommand
//    {
//        protected int expectedVersion;

//        protected virtual int ExpectedVersion => this.expectedVersion = this.expectedVersion == 0 ? 1 : this.expectedVersion;
//    }

//    // TODO: transfer to cortex tag
//    //public class When_changing_tag
//    //{
//    //    public class When_null_command : ConstructedContext<ChangeNeuronTag>
//    //    {
//    //        protected override ChangeNeuronTag When() => null;

//    //        [Fact]
//    //        public async Task Then_should_throw_argument_null_exception()
//    //        {
//    //            await Assert.ThrowsAsync<ArgumentNullException>(() => this.InvokeWhen());
//    //        }
//    //    }

//    //    public abstract class ChangingTagConstructedContext : ModificationPrepareConstructedContext<ChangeNeuronTag>
//    //    {
//    //        protected string newTag;
//    //        protected virtual string NewTag => this.newTag = this.newTag ?? "Hello Worlds";
//    //        protected override ChangeNeuronTag When() => new ChangeNeuronTag(this.AvatarId, this.Id, this.NewTag, this.AuthorId, this.ExpectedVersion);
//    //    }

//    //    public class When_neuronId_does_not_exist : ChangingTagConstructedContext
//    //    {
//    //        protected override bool PreAddNeuron => false;

//    //        [Fact]
//    //        public async Task Then_should_throw_argument_exception()
//    //        {
//    //            await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
//    //        }

//    //        [Fact]
//    //        public async Task Then_should_throw_argument_exception_containing_neuron_reference()
//    //        {
//    //            var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
//    //            Assert.Contains("neuron", ex.Message);
//    //        }
//    //    }

//    //    public class When_author_does_not_exist : ChangingTagConstructedContext
//    //    {
//    //        protected override bool PreAddAuthor => false;

//    //        [Fact]
//    //        public async Task Then_should_throw_argument_exception()
//    //        {
//    //            await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
//    //        }

//    //        [Fact]
//    //        public async Task Then_should_throw_argument_exception_containing_author_reference()
//    //        {
//    //            var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
//    //            Assert.Contains("author", ex.Message);
//    //        }
//    //    }

//    //    public class When_expected_version_is_incorrect : ChangingTagConstructedContext
//    //    {
//    //        protected override int ExpectedVersion => 2;

//    //        [Fact]
//    //        public async Task Then_should_throw_concurrency_exception()
//    //        {
//    //            await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
//    //        }
//    //    }

//    //    public abstract class ChangedNeuronTagContext : ChangingTagConstructedContext
//    //    {
//    //        protected override bool InvokeWhenOnConstruct => true;
//    //    }

//    //    public class When_new_tag : ChangedNeuronTagContext
//    //    {
//    //        [Fact]
//    //        public void Then_should_create_one_event()
//    //        {
//    //            Assert.Equal(1, this.PublishedEvents.Count);
//    //        }

//    //        [Fact]
//    //        public void Then_should_create_correct_event()
//    //        {
//    //            Assert.IsType<NeuronTagChanged>(this.PublishedEvents.First());
//    //        }

//    //        [Fact]
//    //        public void Then_should_have_correct_tag()
//    //        {
//    //            Assert.Equal(this.NewTag, ((NeuronTagChanged)this.PublishedEvents.First()).Tag);
//    //        }
//    //    }

//    //    public class When_new_tag_is_equal_to_original_tag : ChangedNeuronTagContext
//    //    {
//    //        protected override string NewTag => this.Tag;

//    //        [Fact]
//    //        public void Then_should_not_create_event()
//    //        {
//    //            Assert.Equal(0, this.PublishedEvents.Count);
//    //        }
//    //    }
//    //}

//    public class When_deactivating_neuron
//    {
//        public class When_null_command : ConstructedContext<DeactivateNeuron>
//        {
//            protected override DeactivateNeuron When() => null;

//            [Fact]
//            public async Task Then_should_throw_argument_null_exception()
//            {
//                await Assert.ThrowsAsync<ArgumentNullException>(() => this.InvokeWhen());
//            }
//        }

//        public abstract class DeactivatingNeuronConstructedContext : ModificationPrepareConstructedContext<DeactivateNeuron>
//        {
//            protected override DeactivateNeuron When() => new DeactivateNeuron(this.Id, this.AuthorId, this.ExpectedVersion);
//        }

//        public class When_neuronId_does_not_exist : DeactivatingNeuronConstructedContext
//        {
//            protected override bool PreAddNeuron => false;

//            [Fact]
//            public async Task Then_should_throw_argument_exception()
//            {
//                await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
//            }

//            [Fact]
//            public async Task Then_should_throw_argument_exception_containing_neuron_reference()
//            {
//                var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
//                Assert.Contains("neuron", ex.Message);
//            }
//        }

//        // TODO: transfer to Sentry
//        //public class When_author_does_not_exist : DeactivatingNeuronConstructedContext
//        //{
//        //    protected override bool PreAddAuthor => false;

//        //    [Fact]
//        //    public async Task Then_should_throw_argument_exception()
//        //    {
//        //        await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
//        //    }

//        //    [Fact]
//        //    public async Task Then_should_throw_argument_exception_containing_author_reference()
//        //    {
//        //        var ex = await Assert.ThrowsAsync<ArgumentException>(() => this.InvokeWhen());
//        //        Assert.Contains("author", ex.Message);
//        //    }
//        //}

//        public class When_expected_version_is_incorrect : DeactivatingNeuronConstructedContext
//        {
//            protected override int ExpectedVersion => 2;

//            [Fact]
//            public async Task Then_should_throw_concurrency_exception()
//            {
//                await Assert.ThrowsAsync<ConcurrencyException>(() => this.InvokeWhen());
//            }
//        }

//        public abstract class DeactivatedNeuronContext : DeactivatingNeuronConstructedContext
//        {
//            protected override bool InvokeWhenOnConstruct => true;
//        }

//        public class When_neuron_is_active : DeactivatedNeuronContext
//        {
//            [Fact]
//            public void Then_should_create_one_event()
//            {
//                Assert.Equal(1, this.PublishedEvents.Count);
//            }

//            [Fact]
//            public void Then_should_create_correct_event()
//            {
//                Assert.IsAssignableFrom<NeuronDeactivated>(this.PublishedEvents.First());
//            }
//        }

//        public class When_neuron_is_inactive : DeactivatedNeuronContext
//        {
//            protected override IEnumerable<IEvent> Given()
//            {
//                return base.Given().Concat(new IEvent[]
//                {
//                    new NeuronDeactivated(this.Id) { Version = 2 }
//                });
//            }

//            protected override bool InvokeWhenOnConstruct => false;
//            protected override int ExpectedVersion => 2;

//            [Fact]
//            public async Task Then_should_throw_invalid_operation_exception()
//            {
//                await Assert.ThrowsAsync<InvalidOperationException>(() => this.InvokeWhen());
//            }
//        }
//    }
//}