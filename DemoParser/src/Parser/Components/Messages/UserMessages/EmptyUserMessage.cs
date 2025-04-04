using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class EmptyUserMessage : UserMessage {

		public override bool MayContainData => false;

		public EmptyUserMessage(SourceDemo? demoRef, byte value) : base(demoRef, value) {}

		protected override void Parse(ref BitStreamReader bsr) {}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement(this.GetType().Name);
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
