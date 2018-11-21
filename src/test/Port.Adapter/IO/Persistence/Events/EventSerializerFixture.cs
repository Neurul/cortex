using org.neurul.Cortex.Domain.Model.Neurons;
using org.neurul.Common.Test;
using Xunit;

namespace org.neurul.Cortex.Port.Adapter.IO.Persistence.Events.Test.EventSerializerFixture.given
{
    public class Context : TestContext<EventSerializer>
    {
        protected override void Given()
        {
            base.Given();

            this.sut = new EventSerializer();
        }
    }

    public class When_deserializing_NeuronTagChanged_event : Context
    {
        private NeuronTagChanged deserializedEvent;
        private const string Tag = "Hello Worlds";
        private const string TestGuid = "4eb5c455-3785-4ef0-b73e-eefb61d31e8a";

        protected override void When()
        {
            base.When();

            this.deserializedEvent = this.sut.Deserialize(
                "org.neurul.Cortex.Domain.Model.Neurons.NeuronTagChanged, org.neurul.Cortex.Domain.Model, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                @"{
                  ""Tag"": """ + Tag + @""",
                  ""Id"": """ + TestGuid + @""",
                  ""Version"": 6,
                  ""TimeStamp"": ""2017-11-24T12:20:38.851535+00:00""
                }"
            ) as NeuronTagChanged;
        }

        [Fact]
        public void Should_deserialize_successfully()
        {
            Assert.NotNull(this.deserializedEvent);
        }

        [Fact]
        public void Should_contain_correct_tag()
        {
            Assert.Equal(Tag, this.deserializedEvent.Tag);
        }

        [Fact]
        public void Should_contain_correct_id()
        {
            Assert.Equal(TestGuid, this.deserializedEvent.Id.ToString());
        }
    }
}
