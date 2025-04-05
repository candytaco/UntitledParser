using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class Geiger : UserMessage {

		public byte GeigerRange;

		public Geiger(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			GeigerRange = bsr.ReadByte();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"geiger range: {GeigerRange}");
		}
		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("Geiger");
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
