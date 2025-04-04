using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	// I know how to skip user messages, but the byte array that this prints can be extremely helpful
	public sealed class UnknownUserMessage : UserMessage {

		public UnknownUserMessage(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"{Reader.BitLength / 8} bytes: {Reader.ToHexString()}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("UnknownUserMessage");
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
