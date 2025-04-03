using System.Xml.Linq;
using DemoParser.Parser.GameState;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Packets.CustomDataTypes {

	public class UnknownCustomDataMessage : CustomDataMessage {

		public UnknownCustomDataMessage(SourceDemo? demoRef) : base(demoRef) {}


		protected override void Parse(ref BitStreamReader bsr) {}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"unknown custom data of {Reader.BitLength / 8} bytes: {Reader.ToHexString()}");
		}

		public override void XMLWrite(XElement parent)
		{
			parent.Add(new XElement("UnknownCustomData", Reader.ToHexString()));
		}
	}
}
