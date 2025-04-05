using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages.Haptic {

	public class HapSetDrag : UserMessage {

		public float Unk;


		public HapSetDrag(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Unk = bsr.ReadFloat();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"unknown float: {Unk}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("HapticSetDrag");
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
