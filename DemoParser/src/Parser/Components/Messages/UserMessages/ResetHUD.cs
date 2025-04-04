using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class ResetHUD : UserMessage {

		public byte Unknown;


		public ResetHUD(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Unknown = bsr.ReadByte();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"unknown byte: {Unknown}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("ResetHUD");
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
